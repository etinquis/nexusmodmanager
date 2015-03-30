using System;

namespace Nexus.Client.Games.Gamebryo.PluginManagement.LoadOrder
{
	/// <summary>
	/// The exception that is thrown if an error occurs with SORTER.
	/// </summary>
	public class LoadOrderException : Exception
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public LoadOrderException()
		{
		}

		/// <summary>
		/// A simple contructor that sets the exception's message.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		public LoadOrderException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// A simple constructor the sets the exception's message and inner exception.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		/// <param name="inner">The ineer exception.</param>
		public LoadOrderException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
