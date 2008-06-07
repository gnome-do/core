/* TrayNotificationArea.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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
using Do.Core;

namespace Do.UI
{
	public class TrayNotificationArea
	{
		private StatusIcon trayIcon;
		
		public TrayNotificationArea()
		{
			trayIcon = new StatusIcon (
				IconProvider.PixbufFromIconName ("gnome-run", 32));
			trayIcon.Visible = true;
			trayIcon.Tooltip = "GNOME Do\n" + Do.Preferences.SummonKeyBinding;
			trayIcon.Activate += delegate {
				Controller c = Do.Controller;
				if (!c.IsSummoned)
					c.Summon ();
				else
					c.Vanish ();
			};
			
			trayIcon.PopupMenu += OnTrayIconPopup;
		}
		
		void OnTrayIconPopup (object o, EventArgs args) {
			Menu popupMenu = new Menu ();
			
			ImageMenuItem menuItemAbout = new ImageMenuItem ("_About");
			menuItemAbout.Image = new Image (Stock.About, IconSize.Menu);
			menuItemAbout.Activated += OnAboutClicked;
			popupMenu.Add (menuItemAbout);
			
			ImageMenuItem menuItemPrefs = new ImageMenuItem ("_Preferences");
			menuItemPrefs.Image = new Image (Stock.Preferences, IconSize.Menu);
			menuItemPrefs.Activated += OnPreferencesClicked;
			popupMenu.Add (menuItemPrefs);
			
			ImageMenuItem menuItemQuit = new ImageMenuItem ("_Quit");
			menuItemQuit.Image = new Image (Stock.Quit, IconSize.Menu);
			menuItemQuit.Activated += OnQuitClicked;
			popupMenu.Add (menuItemQuit);
			
			popupMenu.ShowAll ();
			popupMenu.Popup ();
		}
		
		protected void OnAboutClicked (object o, EventArgs args)
		{
			Do.Controller.ShowAbout ();
		}
		
		protected void OnPreferencesClicked (object o, EventArgs args)
		{
			Do.Controller.ShowPreferences ();
		}
		
		protected void OnQuitClicked (object o, EventArgs args)
		{
			Do.Controller.Vanish ();
			Application.Quit ();
		}
	}
}
