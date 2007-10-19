// MainMenu.cs created with MonoDevelop
// User: dave at 12:17 PM 10/19/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;

using Do;

namespace Do.UI
{
	
	
	public class MainMenu
	{
		static MainMenu instance;
		
		public static MainMenu Instance {
			get { 
				if (instance == null) {
					instance = new MainMenu ();
				}
				return instance;
			}
		}
		
		Menu menu;
		int mainMenuX, mainMenuY;
		
		public MainMenu ()
		{
			MenuItem item;
			
			menu = new Menu();
			
			// Preferences menu item
		    item = new ImageMenuItem  ("Preferences");
      		(item as ImageMenuItem).Image = new Image(Stock.Preferences, IconSize.Menu);
      		menu.Add (item);
			item.CanFocus = false;
      		item.Sensitive = false;
			
			// Refresh catalog menu item
		    item = new ImageMenuItem ("Refresh Catalog");
      		(item as ImageMenuItem).Image = new Image(Stock.Refresh, IconSize.Menu);
      		menu.Add (item);
      		item.Activated += OnMainMenuRefreshCatalogClicked;
			
			// Separator
			menu.Add (new SeparatorMenuItem ());
			
			// About menu item
		    item = new ImageMenuItem  ("About GNOME Do");
      		(item as ImageMenuItem).Image = new Image(Stock.About, IconSize.Menu);
      		menu.Add (item);
			item.CanFocus = false;
      		item.Activated += OnMainMenuAboutClicked;
			
			// Quit menu item
		    item = new ImageMenuItem ("Quit");
      		(item as ImageMenuItem).Image = new Image(Stock.Quit, IconSize.Menu);
      		menu.Add (item);
      		item.Activated += OnMainMenuQuitClicked;
					
			menu.ShowAll();
		}
		
		
		protected void OnMainMenuQuitClicked (object o, EventArgs args)
		{
			Application.Quit();
		}
		
		protected void OnMainMenuRefreshCatalogClicked (object o, EventArgs args)
		{
			Do.Commander.ItemManager.UpdateItemSources ();
		}
		
		protected void OnMainMenuAboutClicked (object o, EventArgs args)
		{
			AboutDialog about;
			string [] authors;
			
			authors = new string[] {
				"David Siegel <djsiegel@gmail.com>",
				"DR Colkitt <douglas.colkitt@gmail.com>",
				"Ian Cohen <ianrcohen@gmail.com>",
				"James Walker <mr.j.s.walker@gmail.com>",
			};
			
			about = new AboutDialog ();
			about.Name = "GNOME Do";
			about.Version = "0.1";
			about.Logo = Util.Appearance.PixbufFromIconName ("gnome-run", 80);
			// about.Copyright = "Copyright \xa9 2008 David Siegel";
			about.Comments = "Do things as quickly as possible\nin your GNOME desktop environment.";
			about.Website = "http://launchpad.net/gc";
			about.WebsiteLabel = "Visit Homepage";
			about.Authors = authors;
			about.IconName = "gnome-run";
			about.Run ();
			about.Destroy ();
		}
		
		public void PopupAtPosition (int x, int y)
		{
			mainMenuX = x;
			mainMenuY = y;	
			menu.Popup (null, null, PositionMainMenu, 3, Gtk.Global.CurrentEventTime);
		}
		
		private void PositionMainMenu (Menu menu, out int x, out int y, out bool push_in)
		{
			x = mainMenuX;
			y = mainMenuY;
			push_in = true;
		}

	}
}
