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
using Do.Platform.Linux;
using Do.Universe;
using Do.Interface;

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

		private MainMenu ()
		{
			foreach (IRunnableItem item in Services.Application.MainMenuItems)
				Add (MenuItemFromRunnableItem (item));
			ShowAll ();
		}

		MenuItem MenuItemFromRunnableItem (IRunnableItem item)
		{
			int iconSize;
			Gdk.Pixbuf icon;
			ImageMenuItem menuItem;
			
			menuItem = new ImageMenuItem (item.Name);
			Icon.SizeLookup (IconSize.Menu, out iconSize, out iconSize);
			icon = IconProvider.PixbufFromIconName (item.Icon, iconSize);
			menuItem.Image = new Image (icon);
			menuItem.CanFocus = false;
			menuItem.Activated += (sender, e) => item.Run ();
			return menuItem;
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
