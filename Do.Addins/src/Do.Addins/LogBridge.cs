/* LogBridge.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using System.Diagnostics;

namespace Do.Addins
{
	/// <summary>
	/// Provides a way for plugins to offer debugging info the Do log.
	/// Uses reflection to determine which plugin is requesting a log write.
	/// </summary>
	public static class LogBridge
	{
		/// <summary>
		/// Print a Log message with Debug severity.
		/// </summary>
		/// <param name="msg">
		/// A <see cref="System.String"/> containing the message to be printed
		/// </param>
		/// <param name="args">
		/// A <see cref="System.Object"/>  params array with any stacktraces, 
		/// variables, etc.
		/// </param>
		public static void Debug (string msg, params object [] args)
		{
			string message = new StackTrace ().GetFrame (1).GetMethod ().DeclaringType.Namespace 
				+ ": " + msg;
			DebugLogRequested (message, args);
		}
		
		/// <summary>
		/// Print a Log message with Info severity.
		/// </summary>
		/// <param name="msg">
		/// A <see cref="System.String"/> containing the message to be printed
		/// </param>
		/// <param name="args">
		/// A <see cref="System.Object"/>  params array with any stacktraces, 
		/// variables, etc.
		/// </param>
		public static void Info (string msg, params object [] args)
		{
			string message = new StackTrace ().GetFrame (1).GetMethod ().DeclaringType.Namespace 
				+ ": " + msg;
			InfoLogRequested (message, args);
		}
		
		/// <summary>
		/// Print a Log message with Warn severity.
		/// </summary>
		/// <param name="msg">
		/// A <see cref="System.String"/> containing the message to be printed
		/// </param>
		/// <param name="args">
		/// A <see cref="System.Object"/>  params array with any stacktraces, 
		/// variables, etc.
		/// </param>
		public static void Warn (string msg, params object [] args)
		{
			string message = new StackTrace ().GetFrame (1).GetMethod ().DeclaringType.Namespace 
				+ ": " + msg;
			WarnLogRequested (message, args);
		}
		
		/// <summary>
		/// Print a Log message with Error severity.
		/// </summary>
		/// <param name="msg">
		/// A <see cref="System.String"/> containing the message to be printed
		/// </param>
		/// <param name="args">
		/// A <see cref="System.Object"/>  params array with any stacktraces, 
		/// variables, etc.
		/// </param>
		public static void Error (string msg, params object [] args)
		{
			string message = new StackTrace ().GetFrame (1).GetMethod ().DeclaringType.Namespace
				+ ": " + msg;
			ErrorLogRequested (message, args);
		}
		
		/// <summary>
		/// Print a Log message with Fatal severity.
		/// </summary>
		/// <param name="msg">
		/// A <see cref="System.String"/> containing the message to be printed
		/// </param>
		/// <param name="args">
		/// A <see cref="System.Object"/>  params array with any stacktraces, 
		/// variables, etc.
		/// </param>
		public static void Fatal (string msg, params object [] args)
		{
			string message = new StackTrace ().GetFrame (1).GetMethod ().DeclaringType.Namespace 
				+ ": " + msg;
			FatalLogRequested (message, args);
		}
		
		public static event RequestLogHandler DebugLogRequested;
		public static event RequestLogHandler InfoLogRequested;
		public static event RequestLogHandler WarnLogRequested;
		public static event RequestLogHandler ErrorLogRequested;
		public static event RequestLogHandler FatalLogRequested;

		public delegate void RequestLogHandler (string msg, params object [] args);
	}
}
