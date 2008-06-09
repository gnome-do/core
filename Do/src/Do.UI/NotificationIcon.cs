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
		
		public NotificationIcon()
		{
			trayIcon = new StatusIcon (
				IconProvider.PixbufFromIconName ("gnome-run", (int)IconSize.Menu));
			trayIcon.Tooltip = "Summon GNOME Do with " + 
				Do.Preferences.SummonKeyBinding;
	
			trayIcon.Activate += new EventHandler (OnActivateSummonDo);			
			trayIcon.PopupMenu += new PopupMenuHandler (OnTrayIconPopup);
			
			if (Do.Preferences.StatusIconVisible)
				Show ();
			else
				Hide ();
				
			updates_available = false;
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
		/// Sets some properties when new plugin updates are available
		/// </summary>
		public void NotifyUpdatesAvailable ()
		{
			updates_available = true;
			Show ();
			trayIcon.FromPixbuf = IconProvider.PixbufFromIconName 
				("dialog-information", (int)IconSize.Menu);
			Notifications.SendNotification ("Plugin updates are available for download",
				"Updates Available", IconProvider.PixbufFromIconName 
				("software-update-available", (int)IconSize.Dialog));
		}
		
		public void GetLocationOnScreen (out Gdk.Screen screen, out int x, out int y)
		{
			Gtk.Orientation orientation;
			Gdk.Rectangle area;
			trayIcon.GetGeometry (out screen, out area, out orientation);
			x = area.Left;
			y = area.Bottom;
		}
		
		protected void OnActivateSummonDo (object sender, EventArgs args)
		{
			Controller c = Do.Controller;
			if (!c.IsSummoned)
				c.Summon ();
			else
				c.Vanish ();
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