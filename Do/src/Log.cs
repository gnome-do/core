/* Log.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
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
using System.Collections.Generic;

namespace Do {

	public interface ILog {
		void Log (Log.Level level, string msg);
	}
	
	public class ConsoleLog : ILog {

		public void Log (Log.Level level, string msg)
		{
			Console.WriteLine ("{0} [{1}]: {2}",
               DateTime.Now, Enum.GetName (typeof (Log.Level), level), msg);
		}
	}
	
	public static class Log {

        public enum Level {
            Debug,
            Info,
            Warn,
            Error,
            Fatal,
        }
	
		static List<ILog> logs;
		static Level level;

		public static void Initialize ()
		{
			AddLog (new ConsoleLog ());
		}
		
		static Log ()
		{
			level = Level.Info;
			logs = new List<ILog> ();
		}
		
		public static Level LogLevel {
			get { return level; }
			set { level = value; }
		}
		
		public static void AddLog (ILog log)
		{
			logs.Add (log);
		}
		
		public static void RemoveLog (ILog log)
		{
			logs.Remove (log);
		}
		
		public static void Debug (string msg, params object[] args)
		{
			Write (Level.Debug, msg, args);
		}
		
		public static void Info (string msg, params object[] args)
		{
			Write (Level.Info, msg, args);
		}
		
		public static void Warn (string msg, params object[] args)
		{
			Write (Level.Warn, msg, args);
		}
		
		public static void Error (string msg, params object[] args)
		{
			Write (Level.Error, msg, args);
		}
		
		public static void LogFatal (string msg, params object[] args)
		{
			Write (Level.Fatal, msg, args);
		}
		
		static void Write (Level lvl, string msg, params object[] args)
		{
			msg = string.Format (msg, args);
			if (lvl >= level) {
				foreach (ILog log in logs) {
                    try {
                        log.Log (lvl, msg);
                    } catch (Exception e) {
                        Console.Error.WriteLine ("Logger {0} encountered an error: {1}",
                            log, e.Message);
                    }
				}
			}
		}
	}
}
