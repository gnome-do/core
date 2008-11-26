/* NotificationsImplementation.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *  
 * This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
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
using GLib;
using Notifications;

using Do.Platform;

namespace Do.Platform.Linux
{
	
	public class NotificationsImplementation : Notifications.Implementation
	{
		const int IconSize = 24, MinNotifyShow = 5000, MaxNotifyShow = 10000;
		Pixbuf default_icon = IconProvider.PixbufFromIconName ("gnome-do", IconSize);
		
		#region Notifications.Implementation
		
		/// <summary>
		/// Shows a libnotify style notification
		/// </summary>
		/// <param name="title">
		/// A <see cref="System.String"/> title of the notification
		/// </param>
		/// <param name="message">
		/// A <see cref="System.String"/> the body text of the notification
		/// </param>
		/// <param name="icon">
		/// A <see cref="System.String"/> icon name. Set this to null or empty to use default Do icon
		/// </param>
		/// <param name="actionLabel">
		/// A <see cref="System.String"/> label for the action's button
		/// </param>
		/// <param name="onClick">
		/// A <see cref="Action"/> action to excecute. Set this to null for no action
		/// </param>
		public void Notify (string title, string message, string icon, string actionLabel, Action onClick)
		{
			//Show the status icon so that we can associate our notification with it
			StatusIcon.Notify ();
			//we delay this so that the Icon has time to show and we can get its location
			GLib.Timeout.Add (500, delegate {
				Gtk.Application.Invoke (delegate {
					Notification msg;
					Screen screen;
					int x, y;
					
					(StatusIcon.Imp as StatusIconImplementation).GetLocationOnScreen (out screen, out x, out y);
					
					msg = new Notification (); 
					msg.Summary = title;
					msg.Body = GLib.Markup.EscapeText (message);
					msg.Icon = (!string.IsNullOrEmpty (icon) ? IconProvider.PixbufFromIconName (icon, IconSize) : default_icon);
					// message.Length / 10 * 1000 is an aprox time to read based on number of chars
					msg.Timeout = Math.Min (Math.Max (message.Length / 10 * 1000, MinNotifyShow), MaxNotifyShow);
					msg.SetGeometryHints (screen, x, y);
					msg.Closed += OnNotificationClosed;

					if (onClick != null)
						msg.AddAction (actionLabel, actionLabel + "_Action", (o, a) => onClick ());
					
					msg.Show ();
				});
				return false;
			});
		}
		
		protected void OnNotificationClosed (object sender, EventArgs args)
		{
			StatusIcon.Hide ();
		}
		
		#endregion		
	}
}
