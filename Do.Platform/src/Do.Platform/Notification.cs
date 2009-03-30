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

		protected const string DefaultIcon = "";

		public virtual string Body { get; protected set; }
		public virtual string Icon { get; protected set; }
		public virtual string Title { get; protected set; }

		protected Notification ()
			: this ("", "", DefaultIcon)
		{
		}

		public Notification (string title, string body, string icon)
		{
			if (title == null) throw new ArgumentNullException ("title");
			if (body == null) throw new ArgumentNullException ("body");
			if (icon == null) throw new ArgumentNullException ("icon");
			
			Title = title;
			Body = body;
			Icon = icon;
		}
	}
}
