/* Notification.cs
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

namespace Do.Platform
{
	
	public class Notification
	{
		public virtual string Title { get; protected set; }
		public virtual string Body { get; protected set; }
		public virtual string Icon { get; protected set; }
		public virtual string ActionLabel { get; protected set; }
		public virtual Action Action { get; protected set; }

		public event EventHandler Notified;
		
		public Notification (string title, string body, string icon, string actionLabel, Action action)
		{
			Title = title;
			Body = body;
			Icon = icon;
			ActionLabel = actionLabel;
			Action = action;
		}

		public void Notify ()
		{
			if (Notified != null) Notified (this, EventArgs.Empty);
		}

		public override string ToString ()
		{
			return string.Format("[Notification: Title={0}, Body={1}, Icon={2}, ActionLabel={3}, Action={4}]", Title, Body, Icon, ActionLabel, Action);
		}

	}
}
