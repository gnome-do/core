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
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core {

	public class DoItem : DoObject, IItem {

		protected static IDictionary<IItem, bool> has_children;

		static DoItem ()
		{
			has_children = new Dictionary<IItem, bool> ();
		}

		public static IItem Wrap (IItem i)
		{
			return i is DoItem ? i : new DoItem (i);
		}

		public static IItem Unwrap (IItem o)
		{
			while (o is DoItem)
				// We do a traditional cast to throw a cast exception if the wrong
				// dynamic type was passed.
				o = (IItem) (o as DoItem).Inner;
			return o;
		}
		
		public DoItem (IItem item):
			base (item)
		{
		}

		public bool HasChildren {
			get {
				if (!has_children.ContainsKey (this)) {
					has_children [this] = PluginManager.GetItemSources ()
						.Any (s => s.SupportsItem (this) && s.ChildrenOfItem (this).Any ());
				}
				return has_children [this];
			}
		}

	}
}
