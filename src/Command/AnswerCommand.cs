using System;

namespace Sufficit.Asterisk.FastAGI.Command
{
	public class AnswerCommand : AGICommand
	{
		public AnswerCommand()
		{
		}
		public override string BuildCommand()
		{
			return "ANSWER";
		}
	}
}