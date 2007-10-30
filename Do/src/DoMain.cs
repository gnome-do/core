// Main.cs created with MonoDevelop
// User: dave at 12:14 AM 8/22/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using Do.Core;
using Do.DBusLib;

namespace Do
{
	
	public class DoMain {
		
		static Commander commander;
	
		public static void Main (string[] args) {
			
			Log.Initialize ();

			Gtk.Application.Init ();
						
			commander = DBusRegistrar.GetCommanderInstance () as Commander;
			if (commander != null) {
				commander.Show ();
				System.Environment.Exit (0);
			}
			
			Util.Initialize ();
			
			commander = DBusRegistrar.RegisterCommander (new DefaultCommander ()) as Commander;
			commander.Show ();
			
			Gtk.Application.Run ();
		}	
		
		public static Commander Commander {
			get { return commander; }
		}
	}
}
