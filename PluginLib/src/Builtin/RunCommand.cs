// GCRunCommand.cs created with MonoDevelop
// User: dave at 12:54 AM 8/18/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Do.PluginLib;

namespace Do.PluginLib.Builtin
{
	
	public class RunCommand : ICommand
	{
	
		public string Name {
			get { return "Run"; }
		}
		
		public string Description {
			get { return "Run an application, script, or other executable."; }
		}
		
		public string Icon {
			get { return "gnome-run"; }
		}
		
		public Type[] SupportedTypes {
			get {
				return new Type[] {
					typeof (IRunnableItem),
				};
			}
		}
		
		public Type[] SupportedModifierTypes {
			get { return null; }
		}

		public bool SupportsItem (IItem item) {
			return true;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			foreach (IItem item in items) {
				if (item is IRunnableItem) {
					(item as IRunnableItem).Run ();
				}
			}
		}
	
		
	}
}
