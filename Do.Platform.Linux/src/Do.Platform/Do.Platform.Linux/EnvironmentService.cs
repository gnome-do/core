// EnvironmentImplementation.cs
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
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Mono.Unix;

using Do.Platform;


namespace Do.Platform.Linux
{
	public class EnvironmentService : IEnvironmentService
	{

		static string last_command_found;

		#region IEnvironmentService

		public void OpenEmail (IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc,
			string subject, string body, IEnumerable<string> attachments)
		{
			Execute (string.Format ("xdg-email {0} {1} {2} {3} {4} {5}",
				to.Aggregate ("", (es, e) => string.Format ("{0} '{1}'", es, e)),
				cc.Aggregate ("", (es, e) => string.Format ("{0} --cc '{1}'", es, e)),
				bcc.Aggregate ("", (es, e) => string.Format ("{0} --bcc '{1}'", es, e)),
				subject,
				body,
				attachments.Aggregate ("", (es, e) => string.Format ("{0} --attach '{1}'", es, e))
			));
		}
		
		public void OpenUrl (string url)
		{
			if (!url.Contains ("://"))
				url = "http://" + url;
			Open (url);
		}

		public void OpenPath (string path)
		{
			Open (path.Replace ("~", Services.Paths.GetUserHomeDirectory ()));
		}

		public bool IsExecutable (string line)
		{
			line = line.Replace ("~", Services.Paths.GetUserHomeDirectory ());
			return IsExecutableFile (line) || CommandLineIsFoundOnPath (line);
		}

		public void Execute (string line)
		{
			line = line.Replace ("~", Services.Paths.GetUserHomeDirectory ());

			Log.Info ("Executing \"{0}\"", line);
			if (File.Exists (line)) {
				Process proc = new Process ();
				proc.StartInfo.FileName = line;
				proc.StartInfo.UseShellExecute = false;
				proc.Start ();
			} else {
				Process.Start (line);
			}
		}

		#endregion

		static void Open (string open)
		{
			using (Process p = new Process ()) {
				p.StartInfo.FileName = open;
				p.StartInfo.UseShellExecute = true;
				try {
					Log.Info ("Opening \"{0}\"...", open);
					p.Start ();
				} catch (Exception e) {
					Log.Error ("Failed to open {0}: {1} \"{2}\"", open, e.GetType ().Name, e.Message);
					Log.Debug (e.StackTrace);
				}
			}
		}

		static bool IsExecutableFile (string path)
		{
			if (path == null) throw new ArgumentNullException ("path");

			if (!File.Exists (path)) return false;

			UnixFileInfo info = new UnixFileInfo (path);
			return 0 !=
				(info.FileAccessPermissions & FileAccessPermissions.UserExecute);
		}

		static bool CommandLineIsFoundOnPath (string line)
		{
			string command;
			
			if (line == null) throw new ArgumentNullException ("line");
			
			int space = line.IndexOf (" ");
			if (0 < space)
				command = line.Substring (0, space);
			else
				command = line;

			// If this command is the same as the last, yes.
			if (command == last_command_found) return true;

			// Otherwise, try to find the command file in path.
			string PATH = Environment.GetEnvironmentVariable ("PATH") ?? "";
			command = PATH
				.Split (':')
				.Select (path => Path.Combine (path, command))
				.FirstOrDefault (IsExecutableFile);
			if (command != null) {
				last_command_found = command;
				return true;
			}
			return false;
		}
		
	}
}
