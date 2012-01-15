// Log.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Do.Universe;

namespace Do.Platform
{
	public enum LogLevel {
		Debug,
		Info,
		Warn,
		Error,
		Fatal,
	}
	
	public abstract class LogBase
	{
		class LogCall
		{
			public readonly LogLevel Level;
			public readonly string Message;

			public LogCall (LogLevel level, string message)
			{
				Level = level;
				Message = message;
			}
		}

		static object write_lock = new object ();
		static bool writing = false;
		public static LogLevel DisplayLevel { get; set; }

		public static void Write (LogLevel level, string msg, params object[] args)
		{
			if (level < DisplayLevel)
				return;

			lock (write_lock) {
				msg = string.Format (msg, args);

				if (writing) {
					throw new InvalidOperationException (String.Format ("Logger implementation attempted to call Log from its own Log method.\n" +
						"Message was:\n" +
						"***************************\n" +
						"{0}\n" +
						"***************************", msg));
				}
				writing = true;

				try {
					// Log message.
					foreach (ILogService log in Services.Logs)
						log.Log (level, msg);
				} finally {
					writing = false;
				}
			}
		}
	}
}
