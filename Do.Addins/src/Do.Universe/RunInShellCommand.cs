// GCRunCommand.cs created with MonoDevelop
// User: dave at 12:54 AM 8/18/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Do.Universe
{
	
	public class RunInShellCommand : ICommand
	{
	
		public static bool CommandLineIsFoundOnPath (string command_line)
		{
			string path, command, command_file;
			int space_position;
			
			if (command_line == null) {
				return false;
			}
			
			command = command_line.Trim ();			
			space_position = command.IndexOf (" ");
			if (space_position > 0) {
				command = command.Substring (0, space_position);
			}
			
			path = System.Environment.GetEnvironmentVariable ("PATH");
			if (path == null) {
				return false;
			}
			
			// If the command is found, fine.
			if (System.IO.File.Exists (command)) {
				return true;
			}
			// Otherwise, try to find the command file in path.
			foreach (string part in path.Split (':')) {
				command_file = System.IO.Path.Combine (part, command);
				if (System.IO.File.Exists (command_file)) {
					return true;
				}
			}
			return false;
		}
		
		public string Name {
			get { return "Run in Shell"; }
		}
		
		public string Description {
			get { return "Run a command in a shell."; }
		}
		
		public string Icon {
			get { return "gnome-terminal"; }
		}
		
		public Type[] SupportedTypes {
			get {
				return new Type[] {
					typeof (ITextItem),
				};
			}
		}
		
		public Type[] SupportedModifierTypes {
			get { return null; }
		}

		public bool SupportsItem (IItem item) {
			string command_line;
			
			command_line = null;
			if (item is ITextItem) {
				command_line = (item as ITextItem).Text;
			}
			
			if (command_line != null) {
				return CommandLineIsFoundOnPath (command_line);
			}
			return false;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			string cmd = null;
			foreach (IItem item in items) {
				if (item is ITextItem) {
					cmd = (item as ITextItem).Text;
				}
				
				Console.WriteLine (cmd);
				try {
					System.Diagnostics.Process.Start (cmd);
				} catch (Exception e) {
					Console.WriteLine ("Failed to run command in shell \"{0}\": ", e.Message);
				}
			}
		}
	
		
	}
	
}
