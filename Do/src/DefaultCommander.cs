// DefaultCommandInterface.cs created with MonoDevelop
// User: dave at 6:14 PM 8/22/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Do.Core;
using Do.UI;

namespace Do
{	

	public class DefaultCommander : Commander
	{
		
		private Gtk.Window window;
		
		public DefaultCommander ()
		{
			window = new LBWindow (this, true);
			
			State = CommanderState.Default;
		}
		
		protected override void OnVisibilityChanged (bool visible)
		{
				if (visible) {
					// For some reason we need to call this twice... FIX
					Util.PresentWindow (window);
					Util.PresentWindow (window);
				}
		}
	}
}
