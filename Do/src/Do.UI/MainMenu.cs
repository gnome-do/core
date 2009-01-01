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
using Mono.Unix;

using Do;
using Do.Core;
using Do.Platform;

namespace Do.UI
{
	public class MainMenu : Gtk.Menu
	{
		static MainMenu instance;

		public static MainMenu Instance {
			get {
				if (instance == null)
					instance = new MainMenu ();
				return instance;
			}
		}

		int mainMenuX, mainMenuY;

		protected MainMenu ()
		{
			MenuItem item;

			// About menu item
			item = new ImageMenuItem (Catalog.GetString ("_About Do"));
			(item as ImageMenuItem).Image = new Image (Stock.About, IconSize.Menu);
			Add (item);
			item.CanFocus = false;
			item.Activated += OnMainMenuAboutClicked;
			
			// Do.Preferences menu item
			item = new ImageMenuItem (Catalog.GetString ("_Preferences"));
			(item as ImageMenuItem).Image = new Image (Stock.Preferences, IconSize.Menu);
			Add (item);
			item.CanFocus = false;
			item.Activated += OnMainMenuPreferencesClicked;
			
			// Quit menu item
			item = new ImageMenuItem (Catalog.GetString ("_Quit"));
			(item as ImageMenuItem).Image = new Image (Stock.Quit, IconSize.Menu);
			Add (item);
			item.Activated += OnMainMenuQuitClicked;

			ShowAll ();
		}

		protected void OnMainMenuQuitClicked (object o, EventArgs args)
		{
			Do.Controller.Vanish ();
			Application.Quit ();
		}
		
		protected void OnMainMenuPreferencesClicked (object o, EventArgs args)
		{
			Do.Controller.ShowPreferences ();
		}

		protected void OnMainMenuAboutClicked (object o, EventArgs args)
		{
			Do.Controller.ShowAbout ();
		}

		public void PopupAtPosition (int x, int y)
		{
			menuPositioner = null;
			mainMenuX = x;
			mainMenuY = y;	
			Popup (null, null, PositionMainMenu, 3, Gtk.Global.CurrentEventTime);
		}

		private void PositionMainMenu (Menu menu, out int x, out int y, out bool push_in)
		{
			if (menuPositioner == null) {
				x = mainMenuX;
				y = mainMenuY;
			} else {
				Requisition menuReq = menu.SizeRequest ();
				menuPositioner (menuReq.Height, menuReq.Width, out x, out y);
			}
			push_in = true;
		}

		PositionMenu menuPositioner;
		
		public void PopupWithPositioner (PositionMenu menuPositioner)
		{
			this.menuPositioner = menuPositioner;
			Popup (null, null, PositionMainMenu, 3, Gtk.Global.CurrentEventTime);
		}
	}
}
