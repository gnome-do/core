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
using Mono.Unix;
using Gtk;
using Gdk;

using Notifications;

using Do.Core;
using Do.Platform;

namespace Do.UI
{
	/// <summary>
	/// Provides a notification area icon for GNOME Do allowing easy
	/// access to menus, and a way for GNOME Do to alert users of status changes.
	/// </summary>
	public class NotificationIcon : StatusIcon
	{
		private const int IconSize = 24;
		private bool updates_available;
		private Pixbuf normal_icon = IconProvider.PixbufFromIconName 
			("gnome-do", IconSize);
		private Pixbuf update_icon = IconProvider.PixbufFromIconName
			("software-update-available", IconSize);
				
		public NotificationIcon()
		{
			FromPixbuf = normal_icon;
			Tooltip = Catalog.GetString ("Summon GNOME Do with " + CorePreferences.SummonKeyBinding);
			Activate += new EventHandler (OnActivateSummonDo);			
			PopupMenu += new PopupMenuHandler (OnTrayIconPopup);
			
			if (CorePreferences.StatusIconVisible)
				Show ();
			else
				Hide ();
			
			updates_available = false;
			Addins.NotificationBridge.MessageRequested += SendNotification;
		}
		
		public void Show ()
		{
			Visible = true;
		}
		
		public void Hide ()
		{
			if (!updates_available)
				Visible = false;
		}
		
		/// <summary>
		/// Sets some properties when new plugin updates are available
		/// </summary>
		public void NotifyUpdatesAvailable ()
		{
			Show ();
			Activate -= OnActivateSummonDo;
			Activate += OnActivateStartUpdates;
			FromPixbuf = update_icon;
			if (!updates_available)
				Notifications.SendNotification ("GNOME Do",
					"Updated plugins are available. Click here to update.",
					"software-update-available");
			updates_available = true;
		}
		
		public void GetLocationOnScreen (out Gdk.Screen screen, out int x, out int y)
		{
			Gdk.Rectangle area;
			Gtk.Orientation orien;
			
			GetGeometry (out screen, out area, out orien);
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
			try {
				PluginManager.InstallAvailableUpdates (true);
				Activate -= OnActivateStartUpdates;
				updates_available = false;
				Pixbuf = normal_icon;
				SendNotification ("Plugins successfully updated. " +
				"Please restart GNOME Do.");
			} catch (Exception e) {
				Log.Error ("{0}: {1}", e.GetType (), e.Message);
				//I removed these due to a bug in Mono.Addins, once that bug
				//is fixed i will reinstate user error reporting.
				//Log.Debug (e.StackTrace);
				//SendNotification ("Plugin update failed.");
			}
			if (!CorePreferences.StatusIconVisible)
				Hide ();
		}
				
		protected void OnTrayIconPopup (object o, EventArgs args) 
		{
			int x, y;
			bool push_in;
			StatusIcon.PositionMenu (MainMenu.Instance, out x, out y, out push_in, Handle);
			MainMenu.Instance.PopupAtPosition (x, y);
		}
		
		private static void OnNotificationClosed (object sender, EventArgs args)
		{
			if (!CorePreferences.StatusIconVisible)
				Do.NotificationIcon.Hide ();
		}
	}
}
