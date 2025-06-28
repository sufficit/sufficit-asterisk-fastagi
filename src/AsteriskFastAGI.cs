using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsterNET.FastAGI.MappingStrategies;
using AsterNET.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sufficit.Asterisk.FastAGI;
using Sufficit.Asterisk.IO;

namespace AsterNET.FastAGI
{
    public class AsteriskFastAGI
    {
        #region Variables

        private readonly FastAGIOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly AGIServerSocketHandler _socketHandler;

        /// <summary>
        ///     The strategy to use for bind AGIRequests to AGIScripts that serve them.
        /// </summary>
        public IMappingStrategy Strategy { get; }

        /// <summary> True while this server is shut down. </summary>
        private bool stopped;

        private Encoding socketEncoding = Encoding.ASCII;

        public Encoding SocketEncoding
        {
            get { return socketEncoding; }
            set { socketEncoding = value; }
        }

        #endregion
        #region CONSTRUCTORS

        public AsteriskFastAGI(IServiceProvider provider) : this(provider, provider.GetService<IMappingStrategy>() ?? new ResourceMappingStrategy()) { }

        public AsteriskFastAGI(IServiceProvider provider, IMappingStrategy strategy)
        {
            _serviceProvider = provider;
            Strategy = strategy; // setting mapping strategy

            _options = _serviceProvider.GetRequiredService<IOptions<FastAGIOptions>>().Value;
            _logger = _serviceProvider.GetRequiredService<ILogger<AsteriskFastAGI>>();
            _loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

            var ipAddress = IPAddress.Parse(_options.Address);
            var logger = _serviceProvider.GetRequiredService<ILogger<AGIServerSocketHandler>>();
            var options = new ListenerOptions() { Port = _options.Port, Address = ipAddress, Encoding = SocketEncoding }; 
            
            _socketHandler = new AGIServerSocketHandler(_loggerFactory, Options.Create<ListenerOptions>(options));
            _socketHandler.OnRequest = OnRequest;
        }

        private ValueTask OnRequest(ISocketConnection e, CancellationToken cancellationToken)
        {
            _logger.LogDebug("received connection");
            var connectionHandler = new AGIConnectionHandler(_loggerFactory, e, Strategy, _options.SC511_CAUSES_EXCEPTION);
            return connectionHandler.Run(cancellationToken);
        }

        #endregion
        #region Start() 

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            stopped = false;
            Strategy.Load();

            try
            {
                _logger.LogInformation("listening on " + _options.Address + ":" + _options.Port + " ...");
                await _socketHandler.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    _logger.LogError(ex, "unable start AGI Server: cannot to bind to " + _options.Address + ":" + _options.Port + ".");
                }
                               
                _socketHandler.Stop();
                

               //pool.Shutdown();
                _logger.LogInformation("AGI Server shut down.");

                throw;
            }

            try
            {
                _socketHandler.Stop();
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IOException while waiting for connections (2).");
            }
			catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown exception on stopping socket, not so important to handle.");
            }

            _logger.LogInformation("AGI Server shut down.");            
        }

        #endregion
        #region Stop() 

        public void Stop()
        {
            stopped = true;
            _socketHandler.Stop();
        }

        #endregion
    }
}