/* NotificationHelper.cs
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

using Gtk;
using Gdk;
using Mono.Unix;
using LibNotify = Notifications;
	
using Do.Platform;
using Do.Interface;

namespace Do.Platform.Linux
{
	
	internal class NotificationHelper
	{
		const string DefaultIconName = "gnome-do";
		static readonly string ActionButtonLabel = Catalog.GetString ("Ok");

		const int IconSize = 24;
		const int LettersPerWord = 7;
		const int MillisecondsPerWord = 200;
		const int MinNotifyShow = 5000;
		const int MaxNotifyShow = 10000;

		Pixbuf DefaultIcon { get; set; }

		public event EventHandler<NotificationEventArgs> NotificationClosed;
	
		public NotificationHelper ()
		{
			DefaultIcon = IconProvider.PixbufFromIconName (DefaultIconName, IconSize);
		}

		static int ReadableDurationForMessage (string title, string message)
		{
			int t = (title.Length + message.Length) / LettersPerWord * MillisecondsPerWord;	
			return Math.Min (Math.Max (t, MinNotifyShow), MaxNotifyShow);
		}

		public void Notify (Notification note, Screen screen, int x, int y)
		{
			LibNotify.Notification notify = ToNotify (note);
			notify.SetGeometryHints (screen, x, y);
			notify.Show ();
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
			notify.Closed += (sender, e) => OnNotificationClosed (note);

			if (note is ActionableNotification) {
				ActionableNotification anote = note as ActionableNotification;
				notify.AddAction (GLib.Markup.EscapeText (anote.ActionLabel),
				    anote.ActionLabel, (sender, e) => anote.PerformAction ());
			}

			return notify;
		}

		void OnNotificationClosed (Notification note)
		{
			if (NotificationClosed == null) return;
			NotificationClosed (this, new NotificationEventArgs (note));
		}
		
	}

}
