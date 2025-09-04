using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Sufficit.Asterisk.FastAGI.Command;
using Sufficit.Asterisk.FastAGI.IO;
using Microsoft.Extensions.Logging;
using Sufficit.Asterisk.IO;

namespace Sufficit.Asterisk.FastAGI
{
    /// <summary>
    ///     An AGIConnectionHandler is created and run by the AGIServer whenever a new
    ///     socket connection from an Asterisk Server is received.<br />
    ///     It reads the request using an AGIReader and runs the AGIScript configured to
    ///     handle this type of request. Finally it closes the socket connection.
    /// </summary>
    public class AGIConnectionHandler
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger _logger;
        private readonly IMappingStrategy mappingStrategy;
        private readonly bool _SC511_CAUSES_EXCEPTION;

        private readonly ISocketConnection _socket;

        #region AGIConnectionHandler(socket, mappingStrategy)

        /// <summary>
        ///     Creates a new AGIConnectionHandler to handle the given socket connection.
        /// </summary>
        /// <param name="socket">the socket connection to handle.</param>
        /// <param name="mappingStrategy">the strategy to use to determine which script to run.</param>
        public AGIConnectionHandler(ILoggerFactory loggerFactory, ISocketConnection socket, IMappingStrategy mappingStrategy, bool SC511_CAUSES_EXCEPTION)
        {
            this.loggerFactory = loggerFactory;
            
            _socket = socket;
            _socket.OnDisposing += OnSocketDisposing;
            _socket.OnDisconnected += OnSocketDisconnected;

            this.mappingStrategy = mappingStrategy;
            _SC511_CAUSES_EXCEPTION = SC511_CAUSES_EXCEPTION;

            _logger = loggerFactory.CreateLogger<AGIConnectionHandler>();            
        }

        ~AGIConnectionHandler()
            => OnSocketEvent();

        #endregion

        private void OnSocketDisconnected(object? sender, AGISocketReason reason)
            => OnSocketEvent();

        private void OnSocketDisposing(object? sender, EventArgs e)
            => OnSocketEvent();

        private void OnSocketEvent()
        {
            if (_socket != null)
            {
                _socket.OnDisposing -= OnSocketDisposing;
                _socket.OnDisconnected -= OnSocketDisconnected;
            }
        }

        public async ValueTask Run (CancellationToken cancellationToken)
        {
            using (_logger.BeginScope<string>($"[HND:{new Random().Next()}]"))
            {
                string? statusMessage = null;
                try
                {
                    if (_socket == null)
                    {
                        _logger.LogWarning("trying to run with null or disposed socket");
                        return;
                    }

                    var request = await _socket.GetRequest(cancellationToken);
                    
                    // Check if request is valid (null means invalid/empty request)
                    if (request == null)
                    {
                        _logger.LogInformation("Received invalid or empty AGI request - connection may be from telnet or connection was reset");
                        return;
                    }

                    // Added check for when the request is empty
                    // eg. telnet to the service 
                    if (request.Request.Count > 0)
                    {
                        using var script = mappingStrategy.DetermineScript(request);
                        if (script != null)
                        {
                            // TODO: add to session monitor

                            var loggerChannel = loggerFactory.CreateLogger<AGIChannel>();
                            var channel = new AGIChannel(loggerChannel, _socket, _SC511_CAUSES_EXCEPTION);

                            _logger.LogTrace("Begin AGIScript " + script.GetType().FullName + " on " + Thread.CurrentThread.Name);

                            var parameters = new AGIScriptParameters(request, channel);
                            await script.ExecuteAsync(parameters, cancellationToken);
                            statusMessage = "SUCCESS";

                            _logger.LogTrace("End AGIScript " + script.GetType().FullName + " on " + Thread.CurrentThread.Name);
                        }
                        else
                        {
                            statusMessage = "No script configured for URL '" + request.RequestURL + "' (script '" + request.Script + "')";
                            throw new FileNotFoundException(statusMessage);
                        }
                    }
                    else
                    {
                        statusMessage = "A connection was made with no requests";
                        _logger.LogInformation(statusMessage);
                    }
                }
                // expected behavior
                catch (OperationCanceledException ex)
                {
                    statusMessage = ex.Message;
                    _logger.LogDebug(ex, statusMessage);
                }
                catch (SocketException ex)
                {
                    statusMessage = ex.Message;

                    // cleanup socket if aborted
                    //if (ex.ErrorCode == 103) _socket = null;

                    // just log if not 103 => connection aborted
                    //else 
                        _logger.LogError(ex, "IDX00006(SOCKET): {message}", statusMessage);
                        
                }
                catch (AGIHangupException ex)
                {
                    statusMessage = ex.Message;
                    _logger.LogError(ex, "IDX00004(AGIHangup): {message}", statusMessage);
                }
                catch (IOException ex)
                {
                    statusMessage = ex.Message;
                    _logger.LogError(ex, "IDX00003(IO): {message}", statusMessage);
                }
                catch (AGIException ex)
                {
                    statusMessage = ex.Message;
                    _logger.LogError(ex, "IDX00002(AGI): {message}", statusMessage);
                }
                catch (Exception ex) // exception at script level
                {
                    statusMessage = ex.Message;
                    _logger.LogError(ex, "IDX00001(Unexpected): {message}", statusMessage);
                }

                // testing if connection was aborted, before sending back status msgs
                if (_socket != null)
                {
                    try
                    {
                        // avoid disposing socket because it was not created by this handler
                        // using (_socket)
                        {
                            if (_socket.IsConnected)
                            {
                                if (!string.IsNullOrWhiteSpace(statusMessage) && !cancellationToken.IsCancellationRequested)
                                {
                                    var command = new SetVariableCommand(Common.AGI_DEFAULT_RETURN_STATUS, statusMessage);
                                    await _socket.SendCommandAsync(command, cancellationToken);
                                }
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "IDX00000(IOClosing): {message}", ex.Message);
                    }
                    catch (Exception ex) { _logger.LogError(ex, "IDX00005(Unknown): {message}", ex.Message); }
                }
            }
        }
    }
}