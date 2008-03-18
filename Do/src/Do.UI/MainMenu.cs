/* MainMenu.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Reflection;
using Gtk;
using Mono.Unix;

using Do;

namespace Do.UI
{
	public class MainMenu
	{
		static MainMenu instance;

		static MainMenu ()
		{
			instance = new MainMenu ();
		}

		public static MainMenu Instance
		{
			get { return instance; }
		}

		Menu menu;
		AboutDialog about;
		int mainMenuX, mainMenuY;

		public MainMenu ()
		{
			MenuItem item;

			about = null;
			menu = new Menu();

			// Preferences menu item
			item = new ImageMenuItem  ("_Preferences");
			(item as ImageMenuItem).Image = new Image (Stock.Preferences, IconSize.Menu);
			// menu.Add (item);
			item.CanFocus = false;
			item.Sensitive = false;

			// Refresh catalog menu item
			item = new ImageMenuItem ("_Refresh Catalog");
			(item as ImageMenuItem).Image = new Image (Stock.Refresh, IconSize.Menu);
			// menu.Add (item);
			item.Activated += OnMainMenuRefreshCatalogClicked;

			// Separator
			// menu.Add (new SeparatorMenuItem ());

			// About menu item
			item = new ImageMenuItem  (Catalog.GetString ("_About Do"));
			(item as ImageMenuItem).Image = new Image (Stock.About, IconSize.Menu);
			menu.Add (item);
			item.CanFocus = false;
			item.Activated += OnMainMenuAboutClicked;

			// Open plugin folder
			item = new ImageMenuItem  (Catalog.GetString ("_Open Plugins Folder"));
			(item as ImageMenuItem).Image = new Image (Stock.Open, IconSize.Menu);
			menu.Add (item);
			item.CanFocus = false;
			item.Activated += OnMainMenuOpenPluginFolderClicked;

			// Quit menu item
			item = new ImageMenuItem (Catalog.GetString ("_Quit"));
			(item as ImageMenuItem).Image = new Image (Stock.Quit, IconSize.Menu);
			menu.Add (item);
			item.Activated += OnMainMenuQuitClicked;

			menu.ShowAll();
		}

		public AboutDialog AboutDialog
		{
			get { return about; }
		}

		protected void OnMainMenuQuitClicked (object o, EventArgs args)
		{
			Do.Controller.Vanish ();
			Application.Quit ();
		}

		protected void OnMainMenuRefreshCatalogClicked (object o, EventArgs args)
		{
		}

		protected void OnMainMenuOpenPluginFolderClicked (object o, EventArgs args)
		{
			Do.Controller.Vanish ();
			Util.Environment.Open (Paths.UserPlugins);
		}

		protected void OnMainMenuAboutClicked (object o, EventArgs args)
		{
			string[] authors;
			string[] logos;
			string logo;

			Do.Controller.Vanish ();

			authors = new string[] {
				"Chris Halse Rogers <chalserogers@gmail.com>",
				"David Siegel <djsiegel@gmail.com>",
				"DR Colkitt <douglas.colkitt@gmail.com>",
				"James Walker",
				"Jason Smith",
				"Miguel de Icaza",
				"Rick Harding",
				"Thomsen Anders",
				"Volker Braun"
			};

			about = new AboutDialog ();
			about.Name = "GNOME Do";

			try {
				AssemblyName name = Assembly.GetEntryAssembly ().GetName ();
				about.Version = String.Format ("{0}.{1}.{2}.{3}", 
				                               name.Version.Major,
				                               name.Version.Minor,
				                               name.Version.Build,
				                               name.Version.Revision);
			} catch {
				about.Version = Catalog.GetString ("Unknown");
			}
			
			logos = new string[] {
				"/usr/share/icons/gnome/scalable/actions/search.svg",
			};

			logo = "gnome-run";
			foreach (string l in logos) {
				if (!System.IO.File.Exists (l)) continue;
				logo = l;
			}

			about.Logo = UI.IconProvider.PixbufFromIconName (logo, 140);
			about.Copyright = "Copyright \xa9 2008 GNOME Do Developers";
			about.Comments = "Do things as quickly as possible\n" +
				"(but no quicker) with your files, bookmarks,\n" +
				"applications, music, contacts, and more!";
			about.Website = "http://do.davebsd.com/";
			about.WebsiteLabel = "Visit Homepage";
			about.Authors = authors;
			about.IconName = "gnome-run";

			if (null != about.Screen.RgbaColormap) {
				Gtk.Widget.DefaultColormap = about.Screen.RgbaColormap;
			}

			about.Run ();
			about.Destroy ();
			about = null;
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
