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

using Gdk;
using LibNotify = Notifications;
	
using Do.Platform;

namespace Do.Platform.Linux
{	
	public enum NotificationCapability {
       		actions,
       		append,
		body,
      		body_hyperlinks,
       		body_images,
       		body_markup,
       		icon_multi,
       		icon_static,
       		image_svg,
		max,
		positioning, // not an official capability
		scaling, // not an official capability
		sound
	}
	
	internal class NotificationHelper
	{
		const string DefaultIconName = "gnome-do";
		
		const int IconSize = 48;
		const int LettersPerWord = 7;
		const int MillisecondsPerWord = 350;
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

		public void Notify (Notification note)
		{
			Notify (note, Screen.Default, 0, 0);
		}
		
		public void Notify (Notification note, Screen screen, int x, int y)
		{
			LibNotify.Notification notify = ToNotify (note);
			notify.SetGeometryHints (screen, x, y);
			notify.Show ();
		}
		
		public bool SupportsCapability (NotificationCapability capability)
		{
			// positioning and scaling are not actual capabilities, i just know for a fact most other servers
			// support geo. hints, and notify-osd is the only that auto scales images
			if (capability == NotificationCapability.positioning)
				return LibNotify.Global.ServerInformation.Name != "notify-osd";
			else if (capability == NotificationCapability.scaling)
				return LibNotify.Global.ServerInformation.Name == "notify-osd";
			
			return Array.IndexOf (LibNotify.Global.Capabilities, Enum.GetName (typeof (NotificationCapability), capability)) > -1;
		}

		LibNotify.Notification ToNotify (Notification note)
		{
			LibNotify.Notification notify = new LibNotify.Notification ();
			notify.Body = GLib.Markup.EscapeText (note.Body);
			notify.Summary = GLib.Markup.EscapeText (note.Title);
			notify.Closed += (sender, e) => OnNotificationClosed (note);
			notify.Timeout = ReadableDurationForMessage (note.Title, note.Body);
			
			if (SupportsCapability (NotificationCapability.scaling) && !note.Icon.Contains ("@")) {
				notify.IconName = string.IsNullOrEmpty (note.Icon)
					? DefaultIconName
					: note.Icon;
			} else {
				notify.Icon = string.IsNullOrEmpty (note.Icon)
					? DefaultIcon
					: IconProvider.PixbufFromIconName (note.Icon, IconSize);
			}

			if (note is ActionableNotification) {
				ActionableNotification anote = note as ActionableNotification;
				notify.AddAction (GLib.Markup.EscapeText (anote.ActionLabel),
				    anote.ActionLabel, (sender, e) => anote.PerformAction ());
			}

			return notify;
		}

		void OnNotificationClosed (Notification note)
		{
			if (NotificationClosed != null)
				NotificationClosed (this, new NotificationEventArgs (note));
		}
	}
}
	
