/* Notifications.cs
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

namespace Do.Platform
{
	
	public static class Notifications
	{

		const string DefaultIcon = "gnome-do";

		public interface Implementation
		{
			void Notify (string title, string message, string icon, string actionLabel, Action onClick);
		}
		
		public static Implementation Imp { get; private set; }
		
		public static void Initialize (Implementation imp)
		{
			if (Imp != null)
				throw new Exception ("Already has Implementation");
			if (imp == null)
				throw new ArgumentNullException ("Implementation may not be null");
			
			Imp = imp;
		}
		
		public static void Notify (string title, string message)
		{
		  Notify (title, message, DefaultIcon, null, null);
		}
		
		public static void Notify (string title, string message, string icon)
		{
		  Notify (title, message, icon, null, null);
		}
		
		#region Implementation
		
		/// <summary>
		/// Shows a notification, generally some sort of pop-up bubble
		/// </summary>
		/// <param name="message">
		/// A <see cref="System.String"/> body of the notification
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/> title of the notification
		/// </param>
		/// <param name="icon">
		/// A <see cref="System.String"/> name of the icon to show with the notification
		/// </param>
		/// <param name="actionLabel">
		/// A <see cref="System.String"/> label for the action's button
		/// </param>
		/// <param name="onClick">
		/// An <see cref="Action"/> to perform when notification is clicked
		/// </param>
		public static void Notify (string title, string message, string icon, string actionLabel, Action onClick)
		{
			Imp.Notify (title, message, icon, actionLabel, onClick);
		}
		
		#endregion
	}
}
