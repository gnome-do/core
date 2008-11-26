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
using System.Diagnostics;

using Mono.Unix;

namespace Do.Platform
{
	public class EnvironmentImplementation : Environment.Implementation
	{

		static string last_command_found;

		#region Environment.Implementation
		
		public void OpenURL (string url)
		{
			Open (url);
		}

		public void OpenPath (string path)
		{
			Open (path);
		}

		public bool IsExecutable (string line)
		{
			if (null == line) throw new ArgumentNullException ();

			if (CommandLineIsFoundOnPath (line)) return true;
			if (!File.Exists (line) || Directory.Exists (line)) return false;

			UnixFileInfo info = new UnixFileInfo (line);
			return (info.FileAccessPermissions & FileAccessPermissions.UserExecute) != 0;
		}

		public void Execute (string line)
		{
			if (System.IO.File.Exists (line)) {
				System.Diagnostics.Process proc;
				
				proc = new System.Diagnostics.Process ();
				proc.StartInfo.FileName = line;
				proc.StartInfo.UseShellExecute = false;
				proc.Start ();
			} else {
				System.Diagnostics.Process.Start (line);
			}
		}

		#endregion

		static void Open (string open_item)
		{
			if (open_item == null) return;

			using (Process start_proc = new Process ())
			{
				// start_proc.StartInfo.FileName = open_item;
				// start_proc.StartInfo.UseShellExecute = true;
				start_proc.StartInfo.FileName = "xdg-open";
				start_proc.StartInfo.Arguments = open_item;
				try {
					Log.Debug ("Opening \"{0}\"...", open_item);
					start_proc.Start ();
				} catch (Exception e) {
					Log.Error ("Failed to open {0}: {1}", open_item, e.Message);
				}
			}
		}

		static bool CommandLineIsFoundOnPath (string line)
		{
			string path, command, command_file;
			
			if (line == null) return false;
			
			command = line.Trim ();			
			int space = command.IndexOf (" ");
			if (space > 0) {
				command = command.Substring (0, space);
			}

			// If this command is the same as the last, yes.
			if (command == last_command_found) return true;
			
			// If the command is found, fine.
			if (System.IO.File.Exists (command)) {
				last_command_found = command;
				return true;
			}
			
			// Otherwise, try to find the command file in path.
			path = System.Environment.GetEnvironmentVariable ("PATH");
			if (path != null) {
				foreach (string part in path.Split (':')) {
					command_file = System.IO.Path.Combine (part, command);
					if (System.IO.File.Exists (command_file)) {
						last_command_found = command;
						return true;
					}
				}
			}
			return false;
		}
		
	}
}
