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
using System.Text.RegularExpressions;

using Mono.Unix;
using Mono.Unix.Native;

using Do.Platform;
using Do.Universe;

namespace Do.Platform.Linux
{
	public class EnvironmentService : IEnvironmentService
	{
		const string PathPattern = @"^~([^\-\/][^:\s\/]*)?(\/.*)?$";
		
		string last_command_found;
		readonly Regex path_matcher;
		
		public EnvironmentService ()
		{
			 path_matcher = new Regex (PathPattern, RegexOptions.Compiled);
		}
		
#region IEnvironmentService
		
		public void OpenEmail (IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc,
			string subject, string body, IEnumerable<string> attachments)
		{
			IEnumerable<string> arguments;
			arguments = cc.SelectMany (address => new string[] { "--cc", address });
			arguments = arguments.Concat (bcc.SelectMany (address => new string[] { "--bcc", address }));
			if (!String.IsNullOrEmpty (subject)) {
				arguments = arguments.Concat (new string[] { "--subject", subject });
			}
			if (!String.IsNullOrEmpty (body)) {
				arguments = arguments.Concat (new string[] { "--body", body });
			}
			arguments = arguments.Concat (attachments.SelectMany (attachment => new string[] { "--attach", attachment }));
			arguments = arguments.Concat (to);
			using (Process emailer = ExecuteWithArguments ("xdg-email", arguments)) {
			}
		}
		
		string UserHome {
			get { return Environment.GetFolderPath (Environment.SpecialFolder.Personal); }
		}
		
		public void OpenUrl (string url)
		{
			if (!url.Contains ("://"))
				url = "http://" + url;
			Open (url);
		}
		
		public void OpenPath (string path)
		{
			Open (ExpandPath (path));
		}
		
		public bool IsExecutable (string line)
		{
			line = ExpandPath (line);
			return IsExecutableFile (line) || CommandLineIsFoundOnPath (line);
		}
		
		public void Execute (string line)
		{
			line = ExpandPath (line);
			
			Log<EnvironmentService>.Info ("Executing \"{0}\"", line);
			if (File.Exists (line)) {
				Process proc = new Process ();
				proc.StartInfo.FileName = line;
				proc.StartInfo.UseShellExecute = false;
				proc.Start ();
			} else {
				Process.Start (line);
			}
		}

		public Process ExecuteWithArguments (string command, params string[] arguments)
		{
			return ExecuteWithArguments (command, arguments.AsEnumerable ());
		}

		public Process ExecuteWithArguments (string command, IEnumerable<string> arguments)
		{
			// This requires us to work around the GODDAMNED BRAINDEAD System.Diagnostics.Process
			// API which unavoidably splits on space (and interprets a bunch of other characters).
			// We do this by spawning xargs and passing the arguments through stdin.  Since this is
			// in Do.Platform.Linux the lack of portability is not an issue.
			Process executor = new Process ();
			//FIXME: xargs is actually kinda crappy for this
			//Really, we should be fork/execing.
			executor.StartInfo.FileName = "xargs";
			executor.StartInfo.Arguments = "--null " + command;
			executor.StartInfo.UseShellExecute = false;
			executor.StartInfo.RedirectStandardInput = true;
			executor.Start ();
			foreach (string argument in arguments) {
				executor.StandardInput.Write ("{0}\0", argument);
			}
			executor.StandardInput.Close ();
			Log<EnvironmentService>.Debug ("Executing command “{0} {1}”", command, arguments.Aggregate ((cur, item) => cur + " " + item));
			return executor;
		}

		public void CopyToClipboard (Item item)
		{
			string text = "";
			
			try {			
				// TODO: this is absolute garbage
				if (item is ITextItem)
					text = (item as ITextItem).Text;
				else if (item is IFileItem)
					text = (item as IFileItem).Path;
				else if (item is IUriItem)
					text = (item as IUriItem).Uri;
				else if (item is IUrlItem)
					text = (item as IUrlItem).Url;
				else if (item is IContactDetailItem)
					text = (item as IContactDetailItem).Value;
				else if (item is ContactItem)
					text = (item as ContactItem).Name;
				else					
					text = string.Format ("{0} - {1}", item.Name, item.Description);
				
				Log<EnvironmentService>.Debug ("Copying \"{0}\" to clipboard.", text);
				
				Gtk.Clipboard.Get (Gdk.Selection.Clipboard).Text =
					Gtk.Clipboard.Get (Gdk.Selection.Primary).Text = text;
			} catch (Exception e) {
				Log<EnvironmentService>.Error ("Copy to clipboard failed: {0}", e.Message);
				Log<EnvironmentService>.Debug (e.StackTrace);
			}
		}
		
		public string ExpandPath (string path)
		{
			Match m = path_matcher.Match (path);
			if (!m.Success) 
				return path;
			
			if (String.IsNullOrEmpty (m.Groups[1].Value)) {
				return UserHome + m.Groups[2].Value;
			} else {
				Passwd pw = Syscall.getpwnam (m.Groups[1].Value);
				return (pw == null) ? path : pw.pw_dir + m.Groups[2].Value;
			}
		}
#endregion		
		
		void Open (string open)
		{
			try {
				Log<EnvironmentService>.Info ("Opening \"{0}\"...", open);
				using (Process executor = ExecuteWithArguments ("xdg-open", open)) {
				}
			} catch (Exception e) {
				Log<EnvironmentService>.Error ("Failed to open {0}: {1}", open, e.Message);
				Log<EnvironmentService>.Debug (e.StackTrace);
			}
		}

		bool IsExecutableFile (string path)
		{
			if (path == null) throw new ArgumentNullException ("path");

			if (!File.Exists (path)) return false;

			UnixFileInfo info = new UnixFileInfo (path);
			return 0 !=
				(info.FileAccessPermissions & FileAccessPermissions.UserExecute);
		}

		bool CommandLineIsFoundOnPath (string line)
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
