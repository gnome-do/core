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
		private StatusIcon tray;
		private bool updates_available;
		private Pixbuf normal_icon = IconProvider.PixbufFromIconName 
			("gnome-run", (int)IconSize.Menu);
		private Pixbuf alert_icon = IconProvider.PixbufFromIconName 
				("dialog-information", (int)IconSize.Menu);
				
		public NotificationIcon()
		{
			tray = new StatusIcon ();
			tray.FromPixbuf = normal_icon;
			tray.Tooltip = "Summon GNOME Do with " + 
				Do.Preferences.SummonKeyBinding;
			tray.Activate += new EventHandler (OnActivateSummonDo);			
			tray.PopupMenu += new PopupMenuHandler (OnTrayIconPopup);
			
			if (Do.Preferences.StatusIconVisible)
				Show ();
			else
				Hide ();
			
			updates_available = false;
			Addins.NotificationBridge.MessageRequested += SendNotification;
		}
		
		public void Show ()
		{
			tray.Visible = true;
		}
		
		public void Hide ()
		{
			if (!updates_available)
				tray.Visible = false;
		}
		
		/// <summary>
		/// Sets some properties when new plugin updates are available
		/// </summary>
		public void NotifyUpdatesAvailable ()
		{
			Show ();
			tray.Activate -= OnActivateSummonDo;
			tray.Activate += OnActivateStartUpdates;
			tray.FromPixbuf = alert_icon;
			if (!updates_available)
				SendNotification ("Plugin updates are available, "
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
		
		public static void SendNotification (string title, string message, string icon)
		{
			Do.NotificationIcon.Show ();
			Gtk.Application.Invoke (delegate {
				int x, y;
				Gdk.Screen screen;

				Do.NotificationIcon.GetLocationOnScreen (
					out screen, out x, out y);

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
			
			tray.GetGeometry (out screen, out area, out orien);
			x = area.X + area.Width / 2;
			y = area.Y + area.Height - 5;
		}
		
		protected void OnActivateSummonDo (object sender, EventArgs args)
		{
			if (!Do.Controller.IsSummoned)
				Do.Controller.Summon ();
		}
		
		protected void OnActivateStartUpdates (object sender, EventArgs args)
		{
			tray.Activate -= OnActivateStartUpdates;
			try {
				PluginManager.InstallAvailableUpdates (true);
			} catch (Exception e){
				Log.Error ("{0}: {1}", e.GetType (), e.Message);
				Log.Debug (e.StackTrace);
			}
			updates_available = false;
			tray.Pixbuf = normal_icon;
			SendNotification ("Plugins successfully updated. " +
				"Please restart GNOME Do.");
			
			if (!Do.Preferences.StatusIconVisible)
				Hide ();
		}
				
		protected void OnTrayIconPopup (object o, EventArgs args) 
		{
			int x, y;
			Gdk.Screen screen;

			GetLocationOnScreen (out screen, out x, out y);
			MainMenu.Instance.PopupAtPosition (x, y);
		}
		
		private static void OnNotificationClosed (object sender, EventArgs args)
		{
			if (!Do.Preferences.StatusIconVisible)
				Do.NotificationIcon.Hide ();
		}
	}
}
