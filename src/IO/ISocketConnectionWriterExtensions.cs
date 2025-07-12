using Sufficit.Asterisk.FastAGI.Command;
using Sufficit.Asterisk.FastAGI;
using System;
using System.IO;
using Sufficit.Asterisk.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sufficit.Asterisk.IO
{
    public static class ISocketConnectionWriterExtensions
    {
        public static Task SendCommandAsync(this ISocketConnection socket, AGICommand command, CancellationToken cancellationToken)
            => socket.WriteAsync(command.BuildCommand() + "\n", cancellationToken);

        public static void SendCommand(this ISocketConnection socket, AGICommand command)
            => socket.SendCommand(command.BuildCommand() + "\n");

        /// <summary>
        /// I Hope you know what u are doing
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="AGINetworkException"></exception>
        internal static void SendCommand(this ISocketConnection socket, string buffer)
        {
            try
            {
                socket.WriteAsync(buffer, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (IOException e)
            {
                throw new AGINetworkException("Unable to send command to Asterisk: " + e.Message, e);
            }
        }
    }
}
