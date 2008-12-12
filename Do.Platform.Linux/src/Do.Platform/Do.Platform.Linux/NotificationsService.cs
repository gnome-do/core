/* NotificationsService.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
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


using LibNotify = Notifications;
	
using Do.Platform;
using Do.Interface;

namespace Do.Platform.Linux
{
	
	public class NotificationsService : INotificationsService
	{
		const string DefaultIconName = "gnome-do";
		readonly string ActionButtonLabel = Catalog.GetString ("Ok");

		const int LettersPerWord = 7;
		const int MillisecondsPerWord = 200;
		const int IconSize = 24;
		const int NotifyDelay = 250;
		const int MinNotifyShow = 5000;
		const int MaxNotifyShow = 10000;

		Gdk.Pixbuf DefaultIcon { get; set; }

		public event EventHandler<NotificationEventArgs> Notified;
		
		public NotificationsService ()
		{
			DefaultIcon = IconProvider.PixbufFromIconName (DefaultIconName, IconSize);
		}

		static int ReadableDurationForMessage (string title, string message)
		{
			int t = (title.Length + message.Length) / LettersPerWord * MillisecondsPerWord;	
			return Math.Min (Math.Max (t, MinNotifyShow), MaxNotifyShow);
		}

		#region INotificationsService
		
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
		/// <param name="onClick">
		/// A <see cref="Action"/> action to excecute. Set this to null for no action
		/// </param>
		public void Notify (Notification note)
		{
			LibNotify.Notification notify = ToNotify (note);
			
			/*
			// Show the status icon so that we can associate our notification with it
			StatusIcon.Notify ();
		
			// If we can successfully get the location, then we associate the
			// notification with it.
			if (StatusIcon.Imp is StatusIconImplementation) {
				int x, y;
				Screen screen;

				(StatusIcon.Imp as StatusIconImplementation).GetLocationOnScreen (
					out screen, out x, out y);
				notify.SetGeometryHints (screen, x, y);
			}
			*/
			
			// We delay this so that the status icon has time to show.
			GLib.Timeout.Add (NotifyDelay, () => {
			    Gtk.Application.Invoke ((o, a) => notify.Show ()); 
			    return false;
			});

			OnNotified (note);
		}

		LibNotify.Notification ToNotify (Notification note)
		{
			LibNotify.Notification notify = new LibNotify.Notification ();
			
			notify.Icon = string.IsNullOrEmpty (note.Icon)
				? DefaultIcon
				: IconProvider.PixbufFromIconName (note.Icon, IconSize);
			notify.Body = GLib.Markup.EscapeText (note.Body);
			notify.Summary = GLib.Markup.EscapeText (note.Title);
			notify.Timeout = ReadableDurationForMessage (note.Title, note.Body);
			notify.Closed += (o, a) => StatusIcon.Hide ();

			notify.AddAction (GLib.Markup.EscapeText (note.ActionLabel),
			    note.ActionLabel, (sender, e) => note.Action ());

			return notify;
		}

		void OnNotified (Notification note)
		{
			note.Notify ();
			
			if (Notified != null)
				Notified (this, new NotificationEventArgs (note));
		}

		#endregion
		
	}

}
