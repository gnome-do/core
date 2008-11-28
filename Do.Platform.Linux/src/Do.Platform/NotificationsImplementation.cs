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
		const int NotifyDelay = 500;
		const string DefaultIconName = "gnome-do";
		const int IconSize = 24, MinNotifyShow = 5000, MaxNotifyShow = 10000;

		static readonly Pixbuf default_icon;
		
		static NotificationsImplementation ()
		{
			default_icon = Icons.PixbufFromIconName (DefaultIconName, IconSize);
		}

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
			Screen screen;
			Notification msg;
			int x, y, readableTimeout;
			
			x = y = -1;
			screen = null;
			
			// Show the status icon so that we can associate our notification with it
			StatusIcon.Notify ();
			
			if (StatusIcon.Imp is StatusIconImplementation) {
				(StatusIcon.Imp as StatusIconImplementation).GetLocationOnScreen (out screen, out x, out y);
			}
			
			msg = new Notification ();
			msg.Icon = string.IsNullOrEmpty (icon) ? default_icon : Icons.PixbufFromIconName (icon, IconSize);
			msg.Body = GLib.Markup.EscapeText (message);
			msg.Closed += (o, a) => StatusIcon.Hide ();
			msg.Summary = GLib.Markup.EscapeText (title);
			
			// if our status icon check doesn't fail, then we associate the notification with it.
			if (screen != null)
				msg.SetGeometryHints (screen, x, y);
			
			// this is an aprox time to read based on number of chars
			readableTimeout = message.Length / 10 * 1000;	
			msg.Timeout = Math.Min (Math.Max (readableTimeout, MinNotifyShow), MaxNotifyShow);
			
			if (onClick != null) {
				string label = GLib.Markup.EscapeText (actionLabel);
				msg.AddAction (label, actionLabel + "_Action", (o, a) => onClick ());
			}
			
			// we delay this so that the icon has time to show and we can get its location
			GLib.Timeout.Add (NotifyDelay, () => {
			    Gtk.Application.Invoke ((o, a) => msg.Show ()); 
			    return false;
			});
		}

		#endregion
		
		protected void OnNotificationClosed (object sender, EventArgs args)
		{
			StatusIcon.Hide ();
		}
	}
}
