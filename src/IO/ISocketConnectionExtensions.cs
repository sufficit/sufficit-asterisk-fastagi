using Sufficit.Asterisk.FastAGI.Command;
using Sufficit.Asterisk.FastAGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Sufficit.Asterisk.IO;

namespace Sufficit.Asterisk.FastAGI.IO
{
    public static class ISocketConnectionReaderExtensions
    {
        public static async Task<AGIRequest?> GetRequest(this ISocketConnection socket, CancellationToken cancellationToken)
        {
            try
            {
                // Convert IAsyncEnumerable to array with proper error handling
                var requestLinesList = new List<string>();
                await foreach (var line in socket.ReadRequest(cancellationToken))
                {
                    requestLinesList.Add(line);
                }
                var requestLines = requestLinesList.ToArray();
                
                // Validate request lines before creating AGIRequest
                if (requestLines == null || requestLines.Length == 0)
                {
                    // Log connection details for debugging
                    var remoteEndpoint = socket.RemoteAddress != null ? $"{socket.RemoteAddress}:{socket.RemotePort}" : "unknown";
                    var localEndpoint = socket.LocalAddress != null ? $"{socket.LocalAddress}:{socket.LocalPort}" : "unknown";
                    
                    // This helps identify if it's a telnet connection, abandoned call, or network issue
                    System.Diagnostics.Debug.WriteLine($"Empty AGI environment received from {remoteEndpoint} to {localEndpoint} - possible telnet connection or abandoned call");
                    
                    // Return null for empty requests (e.g., telnet connections, abandoned calls)
                    return null;
                }

                return new AGIRequest(requestLines)
                {
                    LocalAddress = socket.LocalAddress,
                    LocalPort = socket.LocalPort,
                    RemoteAddress = socket.RemoteAddress,
                    RemotePort = socket.RemotePort
                };
            }
            catch (ArgumentException ex)
            {
                // Log more details about the invalid environment for debugging
                var remoteEndpoint = socket.RemoteAddress != null ? $"{socket.RemoteAddress}:{socket.RemotePort}" : "unknown";
                System.Diagnostics.Debug.WriteLine($"Invalid AGI environment from {remoteEndpoint}: {ex.Message}");
                
                // Invalid AGI request - environment data is null or empty
                return null;
            }
            catch (IOException ex) when (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.ConnectionReset)
            {
                // Connection reset by peer - normal during service restarts
                return null;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                // Connection reset by peer - normal during service restarts
                return null;
            }
        }

        /// <summary>
        /// Helper extension method to convert an IAsyncEnumerable into a Task<T[]>.
        /// </summary>
        private static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> source)
        {
            var list = new List<T>();
            await foreach (var item in source)
            {
                list.Add(item);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Synchronously gets a reply from the socket connection by blocking the current thread.
        /// WARNING: This is a "sync over async" bridge and can cause deadlocks in some environments.
        /// Use with caution and prefer asynchronous alternatives for new code.
        /// </summary>
        public static AGIReply GetReply(this ISocketConnection socket, uint? timeoutms = null)
        {
            // 1. Call the new async method to get the async stream
            var asyncReplyStream = socket.ReadReplyAsync(timeoutms);

            // 2. Convert the async stream to a Task that will eventually hold the array
            var linesTask = asyncReplyStream.ToArrayAsync();

            // 3. Block the thread and wait for the Task to complete to get the result.
            // This is the "sync over async" part.
            var lines = linesTask.GetAwaiter().GetResult();

            // 4. Return the new AGIReply
            return new AGIReply(lines);
        }
    }
}
