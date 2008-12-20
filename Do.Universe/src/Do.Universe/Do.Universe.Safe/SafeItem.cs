/* SafeItem.cs
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

using Do.Universe;

namespace Do.Universe.Safe
{
	
 	public class SafeItem : Item
	{
		public Item Item { protected get; set; }

		public SafeItem () : this (null)
		{
		}

		public SafeItem (Item item)
		{
			Item = item;
		}

		public override string Name {
			get { return (Item as Element).Safe.Name; }
		}

		public override string Description {
			get { return (Item as Element).Safe.Description; }
		}

		public override string Icon {
			get { return (Item as Element).Safe.Icon; }
		}
	}
}
