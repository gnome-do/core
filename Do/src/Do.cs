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
		
		static ICommander commander;
		
		static Menu mainMenu;
		
		public static void Main (string[] args) {
			
			Log.Initialize ();

			Application.Init ();
						
			commander = DBusRegistrar.GetCommanderInstance ();
			if (commander != null) {
				commander.Show ();
				System.Environment.Exit (0);
			}
			
			Util.Initialize ();
			InitializeMainMenu ();
			
			commander = DBusRegistrar.RegisterCommander (new DefaultCommander ());
			commander.Show ();
			
			Application.Run ();
		}
		
		static void InitializeMainMenu ()
		{
			
			mainMenu = new Menu();
			
		    ImageMenuItem quit_item = new ImageMenuItem ("Quit");
      		quit_item.Image = new Image(Stock.Quit, IconSize.Menu);
      		mainMenu.Add (quit_item);
      		quit_item.Activated += new EventHandler (OnMainMenuQuitClicked);
			mainMenu.ShowAll();
		}
		
		
		static void OnMainMenuQuitClicked (object o, EventArgs args)
		{
			Application.Quit();
		}
		
		public static Menu MainMenu {
			get { return mainMenu; }
		}

	}
}
