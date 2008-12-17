/* ActionableNotification.cs
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
	
	public class ActionableNotification : Notification
	{

		public virtual Action Action { get; protected set; }
		public virtual string ActionLabel { get; protected set; }

		protected ActionableNotification ()
			: this ("", "", DefaultIcon, "", DefaultAction)
		{
		}

		public ActionableNotification (string title, string body, string icon, string actionLabel, Action action)
			: base (title, body, icon)
		{
			if (actionLabel == null) throw new ArgumentNullException ("actionLabel");
			if (action == null) throw new ArgumentNullException ("action");
			
			ActionLabel = actionLabel;
			Action = action;
		}

		public override string ToString ()
		{
			return string.Format("[ActionableNotification: Title={0}, Body={1}, Icon={2}, ActionLabel={3}, Action={4}]", Title, Body, Icon, ActionLabel, Action);
		}

		static void DefaultAction ()
		{
		}

	}
}
