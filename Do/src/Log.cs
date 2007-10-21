/* ${FileName}
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
using System.Diagnostics;
using System.Collections.Generic;

namespace Do
{
	
	public enum LogLevel {
			Debug,
			Info,
			Warn,
			Error,
			Fatal,
	}
	
	public interface ILog
	{
		void Log (LogLevel level, string msg, params object[] args);
	}
	
	public class ConsoleLog : ILog
	{
		public void Log (LogLevel level, string msg, params object[] args)
		{
			Console.WriteLine ("{0} [{1}]: {2}",
			                   DateTime.Now,
			                   Enum.GetName (typeof(LogLevel), level),
			                   string.Format (msg, args));
		}
	}
	
	public class Log
	{
		
		
		static List<ILog> logs;
		static LogLevel level;
		
		public static void Initialize ()
		{
			AddLog (new ConsoleLog ());
		}
		
		static Log ()
		{
			level = LogLevel.Info;
			logs = new List<ILog> ();
		}
		
		public LogLevel Level {
			get { return level; }
			set {
				level = value;
			}
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
			Write (LogLevel.Debug, msg, args);
		}
		
		public static void Info (string msg, params object[] args)
		{
			Write (LogLevel.Info, msg, args);
		}
		
		public static void Warn (string msg, params object[] args)
		{
			Write (LogLevel.Warn, msg, args);
		}
		
		public static void Error (string msg, params object[] args)
		{
			Write (LogLevel.Error, msg, args);
		}
		
		public static void LogFatal (string msg, params object[] args)
		{
			Write (LogLevel.Fatal, msg, args);
		}
		
		static void Write (LogLevel lvl, string msg, params object[] args)
		{
			string caller_info;
			StackTrace stack;
			StackFrame[] frames;
			
			stack = new StackTrace (true);
			frames = stack.GetFrames ();
			caller_info = "";
			if (frames.Length > 2) {
				try {
					caller_info = string.Format (" (from {0}.{1}:{2})",
					                         frames[2].GetMethod ().DeclaringType.Name,
				                             frames[2].GetMethod ().Name,
				                             frames[2].GetFileLineNumber ());
				} catch {
					caller_info = "";
				}
			}
			if (lvl >= level) {
				foreach (ILog log in logs) {
					log.Log (lvl, msg + caller_info, args);
				}
			}
		}
	}
}
