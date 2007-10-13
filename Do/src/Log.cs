// Log.cs created with MonoDevelop
// User: dave at 2:51 PM 10/11/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

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
