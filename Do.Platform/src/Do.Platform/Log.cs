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
using System.Collections.Generic;

using Do.Universe;

namespace Do.Platform
{
	public static class Log
	{

		public interface Implementation
		{
			void Log (Level level, string msg);
		}
	
		public enum Level {
			Debug,
			Info,
			Warn,
			Error,
			Fatal,
		}

		public static Level LogLevel { get; set; }
		static ICollection<Implementation> imps;
		
		static ICollection<Implementation> Imps {
			get {
				return imps;
			}
		}

		public static void Initialize ()
		{
			imps = new List<Implementation> ();
		}
		
		public static void AddImplementation (Implementation imp)
		{
			if (imp == null)
				throw new ArgumentNullException ("Implementation may not be null");
			
			Imps.Add (imp);
		}
		
		public static void RemoveImplementation (Implementation imp)
		{
			Imps.Remove (imp);
		}

		public static string AlignMessage (string msg)
		{
			int lineWidth  = 0;
			const int maxWidth = 80;
			const string tab = "    ";
			string aligned = string.Empty;
			IEnumerable<string> words = msg.Split (' ');

			foreach (string word in words) {
				if (lineWidth + word.Length < maxWidth) {
					aligned = string.Format ("{0}{1} ", aligned, word);
					lineWidth += word.Length + 1;
				} else {
					aligned = string.Format ("{0}\n{1}{2} ", aligned, tab, word);
					lineWidth = tab.Length + word.Length + 1;
				}
			}
			return aligned;
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
		
		public static void Fatal (string msg, params object[] args)
		{
			Write (Level.Fatal, msg, args);
		}
		
		static void Write (Level lvl, string msg, params object[] args)
		{
			msg = string.Format (msg, args);
			if (lvl >= LogLevel) {
				foreach (Implementation imp in Imps) {
                    try {
                        imp.Log (lvl, msg);
                    } catch (Exception e) {
                        Console.Error.WriteLine ("Logger {0} encountered an error: {1}", imp, e.Message);
                    }
				}
			}
		}
	}
}
