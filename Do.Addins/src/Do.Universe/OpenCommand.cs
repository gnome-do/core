using System;

using Do.Addins;

namespace Do.Universe
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
					typeof (IOpenableItem),
					typeof (IURIItem),
				};
			}
		}
		
		public Type[] SupportedModifierTypes {
			get {
				return null;
			}
		}

		public bool SupportsItem (IItem item) {
			return true;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			string open_item;
			string error_message;
			
			open_item = null;
			foreach (IItem item in items) {
				if (item is IOpenableItem) {
					(item as IOpenableItem).Open ();
					continue;
				}
				else if (item is IURIItem) {
					open_item = (item as IURIItem).URI;
				}
				Util.Environment.Open (open_item, out error_message);
			}
		}
		
	}
}
