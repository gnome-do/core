/* DoItem.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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
using System.Collections;
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core {

	public class DoItem : DoObject, IItem {

		/// <summary>
		/// Returns the inner item if the static type of given item is an DoItem
		/// subtype. Returns the argument otherwise.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem"/> that may or may not be an DoItem subtype.
		/// </param>
		/// <returns>
		/// A <see cref="IItem"/> that is NOT an DoItem subtype (the inner IItem
		/// of an DoItem).
		/// </returns>
		public static IItem EnsureIItem (IItem item)
		{
			while (item is DoItem)
				item = (item as DoItem).Inner as IItem;
			return item;
		}

		public static DoItem EnsureDoItem (IItem item)
		{
			if (item is DoItem)
				return item as DoItem;
			else
				return new DoItem (item);;
		}
		
		public DoItem (IItem item):
			base (item)
		{
		}

	}
}
