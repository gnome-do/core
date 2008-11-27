/* DoItemSource.cs
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
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core {

	public class DoItemSource : DoObject, IItemSource {

		public static IItemSource Wrap (IItemSource o)
		{
			return o is DoItemSource ? o : new DoItemSource (o);
		}

		public static IItemSource Unwrap (IItemSource o)
		{
			while (o is DoItemSource)
				// We do a traditional cast to throw a cast exception if the wrong
				// dynamic type was passed.
				o = (IItemSource) (o as DoItemSource).Inner;
			return o;
		}

		IEnumerable<Type> item_types;
		
		public DoItemSource (IItemSource source):
			base (source)
		{
		}

		public IEnumerable<Type> SupportedItemTypes
		{
			get {
				if (item_types != null) return item_types;

				try {
					item_types = (Inner as IItemSource).SupportedItemTypes;
					// Call ToArray to strictly evaluate the IEnumerable before we leave
					// the try block.
					if (item_types != null) item_types = item_types.ToArray ();
				} catch (Exception e) {
					LogError ("SupportedItemTypes", e);
				} finally {
					item_types = item_types ?? Enumerable.Empty<Type> ();
				}
				return item_types;
			}
		}

		public void UpdateItems ()
		{
			try {
				(Inner as IItemSource).UpdateItems ();
			} catch (Exception e) {
				LogError ("UpdateItems", e);
			}
		}
		
		public IEnumerable<IItem> Items
		{
			get {
				IEnumerable<IItem> items = null;
				
				try {
					items = (Inner as IItemSource).Items;
					// Call ToList to strictly evaluate the IEnumerable before we leave
					// the try block.
					if (items != null) items = items.ToList ();
				} catch (Exception e) {
					LogError ("Items", e);
				} finally {
					items = items ?? Enumerable.Empty<IItem> ();
				}
				
				return items.Select (i => DoItem.Wrap (i) as IItem);
			}
		}

		public bool SupportsItem (IItem item)
		{
			item = DoItem.Unwrap (item);;
			return SupportedItemTypes.Any (t => t.IsInstanceOfType (item));
		}

		public IEnumerable<IItem> ChildrenOfItem (IItem item)
		{
			IEnumerable<IItem> children = null;
			item = DoItem.Unwrap (item);

			if (!item.IsAssignableToAny (SupportedItemTypes))
				return Enumerable.Empty<IItem> ();

			try {
				children = (Inner as IItemSource).ChildrenOfItem (item);
				// Call ToList to strictly evaluate the IEnumerable before we leave
				// the try block.
				if (children != null) children = children.ToList ();
			} catch (Exception e) {
				LogError ("ChildrenOfItem", e);
			} finally {
				children = children ?? Enumerable.Empty<IItem> ();
			}

			return children.Select (i => DoItem.Wrap (i) as IItem);
		}
	}
}
