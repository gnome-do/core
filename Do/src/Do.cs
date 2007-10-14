// Main.cs created with MonoDevelop
// User: dave at 12:14 AM 8/22/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;

using Do.Core;
using Do.DBusLib;

namespace Do
{
	
	public class Do {
		
		public static void Main (string[] args) {
			ICommander commander;
			
			Log.Initialize ();

			Application.Init ();
						
			commander = DBusRegistrar.GetCommanderInstance ();
			if (commander != null) {
				commander.Show ();
				System.Environment.Exit (0);
			}
			
			Util.Initialize ();
			
			commander = DBusRegistrar.RegisterCommander (new DefaultCommander ());
			commander.Show ();
			
			Application.Run ();
		}

	}
}
