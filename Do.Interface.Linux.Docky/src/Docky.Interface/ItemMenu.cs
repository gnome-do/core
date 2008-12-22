/* ItemMenu.cs
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
using System.Collections.Generic;
using Gtk;
using Mono.Unix;

namespace Docky.Interface
{
	public class ItemMenu : Gtk.Menu
	{
		static ItemMenu instance;

		public static ItemMenu Instance {
			get {
				if (instance == null)
					instance = new ItemMenu ();
				return instance;
			}
		}

		int mainMenuX, mainMenuY;

		public ItemMenu ()
		{
			
		}

		public void PopupAtPosition (IEnumerable<MenuArgs> items, int x, int y)
		{
			foreach (Gtk.Widget widget in AllChildren) {
				Remove (widget);
				widget.Dispose ();
			}
			
			mainMenuX = x;
			mainMenuY = y;
			foreach (MenuArgs arg in items) {
				if (arg is SeparatorMenuArgs) {
					Add (new SeparatorMenuItem ());
					continue;
				}
				
				MenuItem item = new ImageMenuItem (arg.Description);
				(item as ImageMenuItem).Image = new Image (arg.Icon, IconSize.Menu);
				Add (item);
				item.CanFocus = false;
				item.Activated += arg.Handler;
				item.Sensitive = arg.Sensitive;
			}
			ShowAll ();
			
			Popup (null, null, PositionMainMenu, 3, Gtk.Global.CurrentEventTime);
		}

		private void PositionMainMenu (Menu menu, out int x, out int y, out bool push_in)
		{
			Gtk.Requisition req = SizeRequest ();
			x = mainMenuX - req.Width / 2;
			y = mainMenuY - req.Height;
			push_in = true;
		}
	}
}