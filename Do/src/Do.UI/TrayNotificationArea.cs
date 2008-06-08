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
using Notifications;

namespace Do.UI
{
	/// <summary>
	/// Provides a notification area icon for GNOME Do allowing easy
	/// access to menus, and a way for GNOME Do to alert users of status changes.
	/// </summary>
	public class TrayNotificationArea
	{
		private StatusIcon trayIcon;
		
		public TrayNotificationArea()
		{
			trayIcon = new StatusIcon (
				IconProvider.PixbufFromIconName ("gnome-run", 32));
			trayIcon.Activate += delegate {
				Controller c = Do.Controller;
				if (!c.IsSummoned)
					c.Summon ();
				else
					c.Vanish ();
			};
			
			trayIcon.PopupMenu += OnTrayIconPopup;
		}
		
		/// <summary>
		/// Makes the icon visible in the notification area
		/// </summary>
		public void Show ()
		{
			trayIcon.Visible = true;
		}
		
		/// <summary>
		/// Makes the icon invisible in the notifcation area
		/// </summary>
		public void Hide ()
		{
			trayIcon.Visible = false;
		}
		
		/// <summary>
		/// Alerts the user of something by making the icon blink.
		/// Passing 'true' starts the blinking, and 'false' will stop blinking.
		/// </summary>
		/// <param name="notify">
		/// A <see cref="System.Boolean"/>
		/// </param>
		public void IconNotify (bool notify)
		{
			trayIcon.Blinking = notify;
		}
		
		/// <summary>
		/// Sends a message via libnotify
		/// </summary>
		/// <param name="title">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="message">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="icon">
		/// A <see cref="Gdk.Pixbuf"/>
		/// </param>
		public void SendNotification (string title, string message, Gdk.Pixbuf icon)
		{
			//Get the x and y coordinates of the icon
			Gdk.Screen screen;
			Gdk.Rectangle area;
			GetGeometry (out screen, out area);
			
			Notification n = new Notification (title, message, icon);
			n.Timeout = 8000; //show for 8 seconds
			n.SetGeometryHints (screen, area.Left, area.Bottom);
			n.Show ();
		}
				
		protected void OnTrayIconPopup (object o, EventArgs args) 
		{
			//Get the x and y coordinates of the icon
			Gdk.Screen screen;
			Gdk.Rectangle area;
			GetGeometry (out screen, out area);
			
			MainMenu menu = MainMenu.Instance;
			menu.PopupAtPosition (area.Left,area.Bottom + 1);
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
		
		private void GetGeometry (out Gdk.Screen screen, out Gdk.Rectangle area)
		{
			Gtk.Orientation orientation;
			trayIcon.GetGeometry (out screen, out area, out orientation);
		}
	}
}
