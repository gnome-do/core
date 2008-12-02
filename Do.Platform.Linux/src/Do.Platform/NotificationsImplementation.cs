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
using Do.Interface;

namespace Do.Platform.Linux
{
	
	public class NotificationsImplementation : Platform.Notifications.Implementation
	{
		const string DefaultIconName = "gnome-do";

		const int LettersPerWord = 7;
		const int MillisecondsPerWord = 200;
		const int IconSize = 24;
		const int NotifyDelay = 250;
		const int MinNotifyShow = 5000;
		const int MaxNotifyShow = 10000;

		static readonly Pixbuf default_icon;
		
		static NotificationsImplementation ()
		{
			default_icon = IconProvider.PixbufFromIconName (DefaultIconName, IconSize);
		}

		static int ReadableDurationForMessage (string title, string message)
		{
			int t = (title.Length + message.Length) / LettersPerWord * MillisecondsPerWord;	
			return Math.Min (Math.Max (t, MinNotifyShow), MaxNotifyShow);
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
			Notification msg;
			
			// Show the status icon so that we can associate our notification with it
			StatusIcon.Notify ();
			
			msg = new Notification ();
			msg.Icon = string.IsNullOrEmpty (icon)
				? default_icon
				: IconProvider.PixbufFromIconName (icon, IconSize);
			msg.Body = GLib.Markup.EscapeText (message);
			msg.Summary = GLib.Markup.EscapeText (title);
			msg.Timeout = ReadableDurationForMessage (title, message);
			msg.Closed += (o, a) => StatusIcon.Hide ();

			if (onClick != null)
				msg.AddAction (GLib.Markup.EscapeText (actionLabel),
					actionLabel, (o, a) => onClick ());
			
			// If we can successfully get the location, then we associate the
			// notification with it.
			if (StatusIcon.Imp is StatusIconImplementation) {
				int x, y;
				Screen screen;

				(StatusIcon.Imp as StatusIconImplementation).GetLocationOnScreen (
					out screen, out x, out y);
				msg.SetGeometryHints (screen, x, y);
			}
			
			// We delay this so that the status icon has time to show.
			GLib.Timeout.Add (NotifyDelay, () => {
			    Gtk.Application.Invoke ((o, a) => msg.Show ()); 
			    return false;
			});
		}

		#endregion
		
	}
}
