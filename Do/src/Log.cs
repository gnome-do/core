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
		void Log (LogEntryType level, string msg);
	}
	
	public class ConsoleLog : ILog {

		public void Log (LogEntryType type, string msg)
		{
		    string time = string.Format ("{0:00}:{1:00}:{2:00}.{3:000}",
				DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second,
				DateTime.Now.Millisecond);
			string stype  = Enum.GetName (typeof (LogEntryType), type);
			string prompt = string.Format ("[{0} {1}]", stype, time);

			switch (type) {
			case LogEntryType.Fatal:
				ConsoleCrayon.BackgroundColor = ConsoleColor.Red;
				ConsoleCrayon.ForegroundColor = ConsoleColor.White;
				break;
			case LogEntryType.Error:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Red;
				break;
			case LogEntryType.Warn:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Yellow;
				break;
			case LogEntryType.Info:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Green;
				break;
			case LogEntryType.Debug:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Blue;
				break;
			}
			Console.Write (prompt);
			ConsoleCrayon.ResetColor ();
			Console.Write (" ");
			Console.WriteLine (AlignMessage (msg, prompt.Length + 1));
		}

		string AlignMessage (string msg, int margin)
		{
			int maxWidth   = 90;
			int lineWidth  = 0;
			string aligned = "";
			string padding = "";
			string[] words = msg.Split (' ');

			while (padding.Length < margin)
				padding += " ";

			lineWidth = margin;
			foreach (string word in words) {
				if (lineWidth + word.Length < maxWidth) {
					aligned = string.Format ("{0}{1} ", aligned, word);
					lineWidth += word.Length + 1;
				} else {
					aligned = string.Format ("{0}\n{1}{2} ", aligned, padding, word);
					lineWidth = margin + word.Length + 1;
				}
			}
			return aligned;
		}
	}
	
	public enum LogEntryType {
		Debug,
		Info,
		Warn,
		Error,
		Fatal,
	}

	public static class Log {
	
		static List<ILog> logs;
		static LogEntryType level;

		public static void Initialize ()
		{
			AddLog (new ConsoleLog ());
		}
		
		static Log ()
		{
			if (Do.Preferences.BeQuiet)
				level = LogEntryType.Error;
			else
				level = LogEntryType.Info;
			logs = new List<ILog> ();
		}
		
		public static LogEntryType LogLevel {
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
			Write (LogEntryType.Debug, msg, args);
		}
		
		public static void Info (string msg, params object[] args)
		{
			Write (LogEntryType.Info, msg, args);
		}
		
		public static void Warn (string msg, params object[] args)
		{
			Write (LogEntryType.Warn, msg, args);
		}
		
		public static void Error (string msg, params object[] args)
		{
			Write (LogEntryType.Error, msg, args);
		}
		
		public static void LogFatal (string msg, params object[] args)
		{
			Write (LogEntryType.Fatal, msg, args);
		}
		
		static void Write (LogEntryType lvl, string msg, params object[] args)
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
