using System;

namespace Sufficit.Asterisk.FastAGI.Command
{
	/// <summary>
	/// Sets the given channel variable to the given value.
	/// </summary>
	public class SetVariableCommand : AGICommand
	{
		/// <summary> The name of the variable to set.</summary>
		private string varName;
		/// <summary> The value to set.</summary>
		private string varValue;

		/// <summary>
		/// Get/Set the name of the variable to set.
		/// </summary>
		public string Variable
		{
			get { return varName; }
			set { this.varName = value; }
		}
		/// <summary>
		/// Get/Set the value to set.
		/// </summary>
		public string Value
		{
			get { return varValue; }
			set { this.varValue = value; }
		}

        /// <summary>
        /// Creates a new GetVariableCommand.
        /// </summary>
        /// <param name="name">the name of the variable to set.</param>
        /// <param name="value">the value to set. null to clear value</param>
        public SetVariableCommand(string name, string? value = default)
		{
			this.varName = name;
			this.varValue = value ?? string.Empty;
		}
		
		public override string BuildCommand()
		{
			return "SET VARIABLE " + EscapeAndQuote(varName) + " " + EscapeAndQuote(varValue);
		}
	}
}