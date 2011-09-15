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
using System.Linq;
using System.Collections.Generic;

using Do.Universe.Safe;

namespace Do.Universe
{
	
 	public abstract class Item : Element, IItem, IComparable<Item>
	{
		static SafeItem safe_item = new SafeItem ();

		/// <value>
		/// Quick access to a safe equivalent of the reciever.
		/// </value>
		/// <remarks>
		/// The caller DOES NOT have exclusive access to the value
		/// returned; DO NOT put the value in a collection, linq statement,
		/// or otherwise retain the value returned. The following is the
		/// sole legitimate use:
		/// <code>
		/// string name = item.Safe.Name;
		/// </code>
		/// In words: access the property, but do not retain it.
		/// </value>
		/// </remarks>
		public new SafeItem Safe {
			get {
				safe_item.Item = this;
				return safe_item;
			}
		}

		/// <summary>
		/// Returns a safe equivalent of the reciever. Unlike Safe,
		/// this returns a new safe wrapper instance that the caller has
		/// exclusive access to. You may want to call this in a multi-threaded
		/// context, or if you need a collection of safe instances.
		/// </summary>
		/// <returns>
		/// A <see cref="SafeAct"/>
		/// </returns>
		public new SafeItem RetainSafe ()
		{
			return new SafeItem (this);
		}

		public int CompareTo (Item item)
		{
			return CompareTo (item as Element);
		}

		/// <summary>
		/// Get the children of this <paramref name="Item"/>.
		/// </summary>
		/// <remarks>
		/// This may (and for many Items will be) an empty Enumerable, signifying no children.
		/// </remarks>
		/// <returns>
		/// A <see cref="IEnumerable<Item>"/>
		/// </returns>
		public virtual IEnumerable<Item> GetChildren ()
		{
			return Enumerable.Empty<Item> ();
		}
	}
}
