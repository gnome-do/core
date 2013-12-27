// TrayIconService.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using Mono.Unix;

using Gdk;
using Gtk;

using Do.Platform;
using Do.Platform.ServiceStack;

namespace Do.Platform.Linux
{

	public class TrayIconService : IInitializedService
	{
		const int IconSize = 24;
		const int NotifyDelay = 2000;
		const string IconName = "gnome-do";
		const string MonochromeIconName = "gnome-do-symbolic";

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
			
			Gtk.IconInfo info = Gtk.IconTheme.Default.ChooseIcon (new [] {MonochromeIconName, IconName}, IconSize, IconLookupFlags.ForceSvg);
			
			status_icon = new StatusIcon ();
			status_icon.Pixbuf = info.LoadIcon ();
			status_icon.PopupMenu += OnPopupMenu;
			status_icon.Activate += OnActivate;
		}

		~TrayIconService ()
		{
			status_icon.Activate -= OnActivate;
			status_icon.PopupMenu -= OnPopupMenu;
		}

		public void Initialize ()
		{
			Preferences = new TrayIconPreferences ();
			Preferences.IconVisibleChanged += OnIconVisibleChanged;
			if (!Preferences.IconVisible) 
				Hide ();

			// Listen for notifications so we can show a libnotify bubble.
			Services.Notifications.Notified += OnNotified;
		}

		void OnIconVisibleChanged (object sender, EventArgs e)
		{
			if (Preferences.IconVisible)
				Show ();
			else
				Hide ();
		}

		void OnNotified (object sender, NotificationEventArgs e)
		{
			Services.Application.RunOnMainThread (() => {
				ShowNotification (e.Notification);
			});
		}

		void OnNotificationClosed (object sender, NotificationEventArgs e)
		{
			Services.Application.RunOnMainThread (() => {
				if (!Preferences.IconVisible) Hide ();
			});
		}

		// Only run on the main thread.
		void ShowNotification (Notification note)
		{
			int x, y;
			Screen screen;
			
			if (notifier.SupportsCapability (NotificationCapability.positioning)) {
				Show ();
				GetLocationOnScreen (out screen, out x, out y);
				// We delay this so that the status icon has time to show.
				Services.Application.RunOnMainThread (() => {
			    	notifier.Notify (note, screen, x, y);
				}, NotifyDelay);
			} else {
				Services.Application.RunOnMainThread (() => {
					notifier.Notify (note);
				});
			}
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

		void OnActivate (object sender, EventArgs e) 
		{
			Services.Windowing.SummonMainWindow ();
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
