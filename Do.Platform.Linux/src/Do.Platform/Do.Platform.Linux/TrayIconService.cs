/* TrayIconService.cs
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
using Mono.Unix;

using Gdk;
using Gtk;

using Do.Platform;
using Do.Platform.ServiceStack;
using Do.Interface;

namespace Do.Platform.Linux
{

	public class TrayIconService : IInitializedService
	{
		const int IconSize = 24;
		const int NotifyDelay = 250;
		const string IconName = "gnome-do";

		// Must remain static until IServices have their own preferences.
		static TrayIconPreferences Preferences { get; set; }

		// Need until IServices have their own preferences.
		public static bool Visible {
			get { return Preferences.IconVisible; }
			set { Preferences.IconVisible = value; }
		}

		StatusIcon status_icon;
		NotificationHelper notifier;

		public TrayIconService ()
		{
			notifier = new NotificationHelper ();
			notifier.NotificationClosed += OnNotificationClosed;
			
			status_icon = new StatusIcon ();
			status_icon.FromPixbuf = IconProvider.PixbufFromIconName (IconName, IconSize);
			status_icon.PopupMenu += OnPopupMenu;
		}

#region IService
		
#region IInitializedService
		
		public void Initialize ()
		{
			Preferences = new TrayIconPreferences ();
			Preferences.IconVisibleChanged += OnIconVisibleChanged;
			if (Preferences.IconVisible) Show ();

			// Listen for notifications so we can show a libnotify bubble.
			Services.Notifications.Notified += OnNotified;
		}

#endregion
#endregion

		void OnIconVisibleChanged (object sender, EventArgs e)
		{
			if (Preferences.IconVisible)
				Show ();
			else
				Hide ();
		}

		void OnNotified (object sender, NotificationEventArgs e)
		{
			ShowNotification (e.Notification);
		}

		void OnNotificationClosed (object sender, NotificationEventArgs e)
		{
			if (!Preferences.IconVisible) Hide ();
		}

		void ShowNotification (Notification note)
		{
			int x, y;
			Screen screen;

			Show ();
			GetLocationOnScreen (out screen, out x, out y);

			// We delay this so that the status icon has time to show.
			GLib.Timeout.Add (NotifyDelay, () => {
			    Gtk.Application.Invoke ((sender, e) => notifier.Notify (note, screen, x, y));
			    return false;
			});
		}

		void Show ()
		{
			status_icon.Visible = true;
		}

		void Hide ()
		{
			status_icon.Visible = false;
		}

		void GetLocationOnScreen (out Screen screen, out int x, out int y)
		{
			Rectangle area;
			Orientation orientation;

			status_icon.GetGeometry (out screen, out area, out orientation);
			x = area.X + area.Width / 2;
			y = area.Y + area.Height - 5;
		}

		void OnPopupMenu (object sender, EventArgs e) 
		{
			Services.Windowing.ShowMainMenu (PositionMainMenu);
		}

		void PositionMainMenu (int menuHeight, int menuWidth, out int x, out int y)
		{
			Screen screen;
			Rectangle area;
			Orientation orientation;

			status_icon.GetGeometry (out screen, out area, out orientation);

			x = area.X;
			y = area.Y;

			if (y + menuHeight >= screen.Height)
				y -= menuHeight;
			else
				y += area.Height;
		}

	}
}
