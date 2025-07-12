using Sufficit.Asterisk.FastAGI.Command;
using AsterNET.IO;
using Microsoft.Extensions.Logging;
using Sufficit.Asterisk.IO;
using System;

namespace Sufficit.Asterisk.FastAGI
{
    /// <summary>
    ///     Default implementation of the AGIChannel interface.
    /// </summary>
    public class AGIChannel
    {
        #region HANGUP CONTROL

        /// <summary>
        ///  Indicates that hangup message is received
        /// </summary>
        public bool IsHangUp => Socket.IsHangUp;

        #endregion

        public ISocketConnection Socket { get; }

        private readonly ILogger _logger;
        private readonly bool _SC511_CAUSES_EXCEPTION;

        public AGIChannel(ILogger<AGIChannel> logger, ISocketConnection socket, bool SC511_CAUSES_EXCEPTION)
        {
            _logger = logger;
            _logger.BeginScope(this);

            Socket = socket;
            _logger.LogTrace("agi channel socket id: {id}", socket.Handle);

            _SC511_CAUSES_EXCEPTION = SC511_CAUSES_EXCEPTION;
        }

        public AGIChannel(ISocketConnection socket, bool SC511_CAUSES_EXCEPTION)
            : this(new LoggerFactory().CreateLogger<AGIChannel>(), socket, SC511_CAUSES_EXCEPTION)
        {
            
        }

        /// <summary>
		/// Sends the given command to the channel attached to the current thread.
		/// </summary>
		/// <param name="command">the command to send to Asterisk</param>
		/// <returns> the reply received from Asterisk</returns>
		/// <throws>  AGIException if the command could not be processed properly </throws>
        public AGIReply SendCommand(AGICommand command)
        {
            Socket.SendCommand(command);
            var agiReply = Socket.GetReply(command.ReadTimeOut);
            int status = agiReply.GetStatus();
            if (status == (int) AGIReplyStatuses.SC_INVALID_OR_UNKNOWN_COMMAND)
                throw new InvalidOrUnknownCommandException(command.BuildCommand());
            if (status == (int) AGIReplyStatuses.SC_INVALID_COMMAND_SYNTAX)
                throw new InvalidCommandSyntaxException(agiReply.GetSynopsis(), agiReply.GetUsage());

            if (_SC511_CAUSES_EXCEPTION)
            {
                if (IsHangUp || status == (int)AGIReplyStatuses.SC_DEAD_CHANNEL)
                    throw new AGIHangupException();
            }
            return agiReply;
        }

        /// <summary>
        /// Recover the underlaying log system to use on extensions
        /// </summary>
        /// <returns></returns>
        public ILogger GetLogger() => _logger;
    }
}