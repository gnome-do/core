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
		
		public void PerformOnItem (IItem item)
		{
			if (item is IOpenableItem) {
				(item as IOpenableItem).Open ();
			}
		}
		
		public void PerformOnItemWithIndirectItem (IItem item, IItem iitem)
		{
			throw new NotImplementedException ();
		}
		
	}
}
