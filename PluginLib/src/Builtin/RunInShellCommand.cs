// GCRunCommand.cs created with MonoDevelop
// User: dave at 12:54 AM 8/18/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Do.PluginLib;

namespace Do.PluginLib.Builtin
{
	
	public class RunInShellCommand : ICommand
	{
	
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
		
		public Type[] SupportedIndirectTypes {
			get { return null; }
		}

		public bool SupportsItem (IItem item) {
			return true;
		}
		
		public void Perform (IItem[] items, IItem[] indirectItems)
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
