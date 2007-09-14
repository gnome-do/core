// GCLaunchCommand.cs created with MonoDevelop
// User: dave at 12:54 AM 8/18/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using Do.PluginLib;

namespace Do.PluginLib.Builtin
{
	
	public class VoidCommand : ICommand
	{
		
		public string Name {
			get { return "Do Nothing"; }
		}
		
		public string Icon {
			get { return "gtk-stop"; }
		}
		
		public Type[] SupportedTypes {
			get {
				return new Type[] {
					typeof (IItem)
				};
			}
		}
		
		public Type[] SupportedIndirectTypes {
			get { return null; }
		}

		public bool SupportsItem (IItem item)
		{
				return true;
		}
		
		public void PerformOnItem (IItem item)
		{
		}
		
		public void PerformOnItemWithIndirectItem (IItem item, IItem iitem)
		{
			throw new NotImplementedException ();
		}
		
	}
}
