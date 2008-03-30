/* RunInTerminalAction.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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
using Mono.Unix;

using Do.Addins;

namespace Do.Universe
{
	/// <summary>
	/// Runs text commands in a terminal.
	/// </summary>
	public class RunInTerminalAction : AbstractAction
	{

		static string last_command_found;
		static Dictionary<string, string> terminals;

		static RunInTerminalAction ()
		{
			terminals = new Dictionary<string, string> ();
			terminals["gnome-terminal"] = "-x";
			terminals["xterm"] = "-e";
			terminals["konsole"] = "-e";
			terminals["xfce4-terminal"] = "-x";
		}

		public static bool CommandLineIsFoundOnPath (string command_line)
		{
			string path, command, command_file;
			int space_position;
			
			if (command_line == null) return false;
			
			command = command_line.Trim ();			
			space_position = command.IndexOf (" ");
			if (space_position > 0) {
				command = command.Substring (0, space_position);
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

		static bool GetTerminalSettings (out string program, out string args)
		{
			GConf.Client client;

			client = new GConf.Client();
			try {
				program = client.Get ("/desktop/gnome/applications/terminal/exec") as string;
				args = client.Get ("/desktop/gnome/applications/terminal/exec_arg") as string;
				if (!CommandLineIsFoundOnPath (program))
					program = args = null;
			} catch {
				program = args = null;
			}
			
			// No settings found or the program cannot be found. Try to find a
			// suitable terminal manually.
			if (string.IsNullOrEmpty (program)) {
				foreach (string terminal in terminals.Keys) {
					if (CommandLineIsFoundOnPath (terminal)) {
						program = terminal;
						args = terminals[terminal];
						break;
					}
				}
			}
			return program != null;
		}
		
		public static void RunCommandlineInTerminal (string commandline)
		{
			string command, args;
			string terminal, terminal_args;
			Process proc;
			
			// Split commandline into command and arguments.
			if (commandline.Contains (" ")) {
				command = commandline.Substring (0, commandline.IndexOf (" "));
				args = commandline.Substring (commandline.IndexOf (" ")+1);
			} else {
				command = commandline;
				args = "";
			}
			
			proc = new Process ();
			// Get settings for running command in a terminal.
			if (GetTerminalSettings (out terminal, out terminal_args)) {
				proc.StartInfo.FileName = terminal;
				proc.StartInfo.Arguments = string.Format ("{0} {1}", terminal_args, commandline);
			} else {
				// No settings found - just run command as a process.
				proc.StartInfo.FileName = command;
				proc.StartInfo.Arguments = args;
			}
			Console.WriteLine (proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
			try {
				proc.Start ();
			} catch (Exception e) {
				Console.Error.WriteLine ("Failed to run command in terminal \"{0}\": ", e.Message);
			}
		}

		public override string Name
		{
			get { return Catalog.GetString ("Run in Terminal"); }
		}
		
		public override string Description
		{
			get { return Catalog.GetString ("Run a command in a terminal."); }
		}
		
		public override string Icon
		{
			get { return "terminal"; }
		}
		
		public override Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (ITextItem),
					typeof (FileItem),
				};
			}
		}
		
		public override bool SupportsItem (IItem item)
		{
			if (item is ITextItem) {
				return CommandLineIsFoundOnPath ((item as ITextItem).Text);
			} else if (item is FileItem) {
				return FileItem.IsExecutable (item as FileItem);
			}
			return false;
		}

		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{
			string commandline;

			foreach (IItem item in items) {
				commandline = null;
				if (item is ITextItem) {
					commandline = (item as ITextItem).Text;
				}
				else if (item is FileItem) {
					// Format the filename so the terminal doesn't choke on it.
					commandline = (item as FileItem).Path.Replace (" ", "\\ ");
				}
				if (commandline == null) continue;
				RunCommandlineInTerminal (commandline);
			}
			return null;
		}

	}
}
