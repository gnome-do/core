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
using Gtk;

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
		int mainMenuX, mainMenuY;

		public MainMenu ()
		{
			MenuItem item;

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
			item = new ImageMenuItem  ("_About Do");
			(item as ImageMenuItem).Image = new Image (Stock.About, IconSize.Menu);
			menu.Add (item);
			item.CanFocus = false;
			item.Activated += OnMainMenuAboutClicked;

			// Quit menu item
			item = new ImageMenuItem ("_Quit");
			(item as ImageMenuItem).Image = new Image (Stock.Quit, IconSize.Menu);
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
			Do.UniverseManager.AwakeIndexThread ();
		}

		protected void OnMainMenuAboutClicked (object o, EventArgs args)
		{
			AboutDialog about;
			string [] authors;

			Do.Commander.Hide ();

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
