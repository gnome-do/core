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
using System.IO;

using Do.Addins;
using System.Collections.Generic;

namespace Do {

	public interface ILog {
		void Log (LogEntryType level, string msg);
	}
	
	public enum LogEntryType {
		Debug,
		Info,
		Warn,
		Error,
		Fatal,
	}
	
	public abstract class AbstractLog : ILog {
		/// <value>
		/// A string to make printing the current time simpler
		/// </value>
		protected const string Timef   = "{0:00}:{1:00}:{2:00}.{3:000}";
		
		/// <value>
		/// A consistent way of printing [Time LogLevel]
		/// </value>
		protected const string Promptf = "[{0} {1}]";
		
		/// <value>
		/// the current time using the Timef format.
		/// </value>
		protected string Time {
			get { 
				return string.Format (Timef, DateTime.Now.Hour, DateTime.Now.Minute,
					DateTime.Now.Second, DateTime.Now.Millisecond);
			}
		}
		
		protected string AlignMessage (string msg, int margin)
		{
			int maxWidth   = 80;
			int lineWidth  = 0;
			string aligned = string.Empty;
			string padding = string.Empty;
			string[] words = msg.Split (' ');

			while (padding.Length < margin)
				padding += " ";

			lineWidth = margin;
			foreach (string word in words) {
				if (lineWidth + word.Length < maxWidth) {
					aligned = string.Format ("{0}{1} ", aligned, word);
					lineWidth += word.Length + 1;
				} else {
					aligned = string.Format ("{0}\n    {1} ", aligned, word);
					lineWidth = 4 + word.Length + 1;
				}
			}
			return aligned;
		}
		
		abstract public void Log (LogEntryType level, string msg);
	}
	
	public class FileLog : AbstractLog {
		public override void Log (LogEntryType level, string msg) {
			string stype = Enum.GetName (typeof (LogEntryType), level);
			string prompt = string.Format (Promptf, stype, Time);
			
			TextWriter writer = new StreamWriter (Paths.Log, true);
			writer.WriteLine (prompt + " " + AlignMessage (msg, prompt.Length + 1));
			writer.Close ();
		}
	}


	public class ConsoleLog : AbstractLog {
		public override void Log (LogEntryType type, string msg)
		{
			string stype  = Enum.GetName (typeof (LogEntryType), type);
			string prompt = string.Format (Promptf, stype, Time);

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
	}

	public static class Log {
	
		static List<ILog> logs;
		static LogEntryType level;

		public static void Initialize ()
		{
			AddLog (new ConsoleLog ());
			
			if (File.Exists (Paths.Log))
				File.Delete (Paths.Log);
			AddLog (new FileLog ());
		}
		
		static Log ()
		{
			level = Do.Preferences.QuietStart ? LogEntryType.Error
											  : LogEntryType.Info;
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
