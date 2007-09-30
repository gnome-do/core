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
			
			open_item = null;
			foreach (IItem item in items) {
				if (item is IOpenableItem) {
					(item as IOpenableItem).Open ();
					continue;
				}

				if (item is IURIItem) {
					open_item = (item as IURIItem).URI;
				}

				// Use gnome-open to open the open_item
				if (open_item != null) {
					Console.WriteLine ("Opening \"{0}\"...", open_item);
					try {
						System.Diagnostics.Process.Start ("gnome-open", string.Format ("\"{0}\"", open_item));
					} catch (Exception e) {
						Console.WriteLine ("Failed to open \"{0}\": ", e.Message);
					}
				}
			}
		}
		
	}
}
