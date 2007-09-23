using System;
using Do.PluginLib;

namespace Do.PluginLib.Builtin
{
	
	public class OpenCommand : ICommand
	{
	
		public OpenCommand ()
		{
		}
		
		public string Name {
			get { return "Open"; }
		}
		
		public string Description {
			get { return "Opens many kinds of items."; }
		}
		
		public string Icon {
			get { return "gtk-open"; }
		}
		
		public Type[] SupportedTypes {
			get {
				return new Type[] {
					typeof (IOpenableItem)
				};
			}
		}
		
		public Type[] SupportedIndirectTypes {
			get {
				return null;
			}
		}

		public bool SupportsItem (IItem item) {
			return true;
		}
		
		public void Perform (IItem[] items, IItem[] indirectItems)
		{
			foreach (IItem item in items) {
				if (item is IOpenableItem) {
					(item as IOpenableItem).Open ();
				}
			}
		}
		
	}
}
