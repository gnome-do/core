/* NotificationIcon.cs
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
using Gdk;
using Do.Core;
using Notifications;

namespace Do.UI
{
	/// <summary>
	/// Provides a notification area icon for GNOME Do allowing easy
	/// access to menus, and a way for GNOME Do to alert users of status changes.
	/// </summary>
	public class NotificationIcon
	{
		private StatusIcon trayIcon;
		private bool updates_available;
		private Pixbuf normal_icon = IconProvider.PixbufFromIconName 
			("gnome-run", (int)IconSize.Menu);
		private Pixbuf alert_icon = IconProvider.PixbufFromIconName 
				("dialog-information", (int)IconSize.Menu);
				
		public NotificationIcon()
		{
			trayIcon = new StatusIcon ();
			trayIcon.FromPixbuf = normal_icon;
			trayIcon.Tooltip = "Summon GNOME Do with " + 
				Do.Preferences.SummonKeyBinding;
			trayIcon.Activate += new EventHandler (OnActivateSummonDo);			
			trayIcon.PopupMenu += new PopupMenuHandler (OnTrayIconPopup);
			
			if (Do.Preferences.StatusIconVisible)
				Show ();
			else
				Hide ();
			
			updates_available = false;
			Addins.NotificationBridge.MessageRequested += SendNotification;
		}
		
		public void Show ()
		{
			trayIcon.Visible = true;
		}
		
		public void Hide ()
		{
			if (!updates_available)
				trayIcon.Visible = false;
		}
		
		/// <summary>
		/// Sets some properties when new plugin updates are available
		/// </summary>
		public void NotifyUpdatesAvailable ()
		{
			Show ();
			trayIcon.Activate -= OnActivateSummonDo;
			trayIcon.Activate += new EventHandler (OnActivateStartUpdates);
			trayIcon.FromPixbuf = alert_icon;
			if (!updates_available)
				SendNotification ("Plugin updates are available for download, "
					+ "Click here to update.", "Updates Available",
					"software-update-available");
			updates_available = true;
		}
		
		public static void SendNotification (string message)
		{
			SendNotification ("Gnome-Do", message, null);
		}
		
		public static void SendNotification (string title, string message)
		{
			SendNotification (title, message, null);
		}
		
		//I took some of this from DBO's branch.
		public static void SendNotification (string title, string message, string icon)
		{
			NotificationIcon trayIcon = Do.NotificationIcon;
			trayIcon.Show ();
			
			Gtk.Application.Invoke (delegate {
				Gdk.Screen screen;
				int x, y;
				trayIcon.GetLocationOnScreen (out screen, out x, out y);

				Notification msg = new Notification ();
				msg.Closed += new EventHandler (OnNotificationClosed); 
				msg.Summary = title;
				msg.Body = message;
				if (icon != null)
					msg.Icon = IconProvider.PixbufFromIconName (icon,
						(int)IconSize.Menu);
				msg.Timeout = 5000;
				msg.SetGeometryHints (screen, x, y);
				msg.Show ();
			});
		}
		
		public void GetLocationOnScreen (out Gdk.Screen screen, out int x, out int y)
		{
			Gdk.Rectangle area;
			Gtk.Orientation orien;
			
			trayIcon.GetGeometry (out screen, out area, out orien);
			x = area.X + area.Width / 2;
			y = area.Y + area.Height - 5;
		}
		
		protected void OnActivateSummonDo (object sender, EventArgs args)
		{
			Controller c = Do.Controller;
			if (!c.IsSummoned)
				c.Summon ();
			else
				c.Vanish ();
		}
		
		protected void OnActivateStartUpdates (object sender, EventArgs args)
		{
			try {
				PluginManager.InstallAvailableUpdates (true);
				trayIcon.Activate -= new EventHandler (OnActivateStartUpdates);
				updates_available = false;
				trayIcon.Pixbuf = normal_icon;
				SendNotification ("Please restart Do after installing updated plugins.");
			} catch { }
			
			if (!Do.Preferences.StatusIconVisible)
				Hide ();
		}
				
		protected void OnTrayIconPopup (object o, EventArgs args) 
		{
			//Get the x and y coordinates of the icon
			Gdk.Screen screen;
			int x, y;
			GetLocationOnScreen (out screen, out x, out y);
			
			MainMenu menu = MainMenu.Instance;
			menu.PopupAtPosition (x,y);
		}
		
		private static void OnNotificationClosed (object sender, EventArgs args)
		{
			NotificationIcon trayIcon = Do.NotificationIcon;
			if (!Do.Preferences.StatusIconVisible)
				trayIcon.Hide ();
		}
	}
}