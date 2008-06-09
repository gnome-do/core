/* Notifications.cs
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
using Gdk;
using Notifications;
using Do.UI;

namespace Do
{
	public static class Notifications
	{
	
		private static NotificationIcon tray_icon;
		const int timeout = 5000;
		
		static Notifications ()
		{
			tray_icon = Do.NotificationIcon;
		}
		
		public static void SendNotification (string message)
		{
			SendNotification (message, "GNOME Do", null);
		}

		public static void SendNotification (string message, string title)
		{
			SendNotification (message, title, null);
		}
		
		public static void SendNotification (string message, string title, Pixbuf icon)
		{
			tray_icon.Show ();
			Notification msg;
			
			Screen screen;
			int x, y;
			
			if (icon == null)
				 msg = new Notification (title, message);
			else
				msg = new Notification (title, message, icon);
			msg.Timeout = timeout;
			msg.Closed += new EventHandler (OnNotificationClosed);
			
			//I put this as far as I could from show to give enough time for
			//x and y to initialize.
			tray_icon.GetLocationOnScreen (out screen, out x, out y);
			msg.SetGeometryHints (screen, x, y);
			
			msg.Show ();
		}
		
		private static void OnNotificationClosed (object sender, EventArgs args)
		{
			if (!Do.Preferences.StatusIconVisible)
				tray_icon.Hide ();
		}
	}
}
