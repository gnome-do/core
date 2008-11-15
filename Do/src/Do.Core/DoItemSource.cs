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

	public class DoItemSource : DoObject, IItemSource, IItem {
		
		public DoItemSource (IItemSource source):
			base (source)
		{
		}

		public IEnumerable<Type> SupportedItemTypes
		{
			get {
				IEnumerable<Type> types = null;
				try {
					types = (Inner as IItemSource).SupportedItemTypes;
				} catch (Exception e) {
					LogError ("SupportedItemTypes", e);
				} finally {
					types = types ?? Type.EmptyTypes;
				}
				return types;
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
				} catch (Exception e) {
					LogError ("Items", e);
				} finally {
					items = items ?? Enumerable.Empty<IItem> ();
				}
				
				return items.Select (i => DoItem.EnsureDoItem (i) as IItem);
			}
		}
		
		public IEnumerable<IItem> ChildrenOfItem (IItem item)
		{
			IEnumerable<IItem> children = null;
			item = DoItem.EnsureIItem (item);

			if (!IObjectTypeCheck (item, SupportedItemTypes))
				return Enumerable.Empty<IItem> ();

			try {
				children = (Inner as IItemSource).ChildrenOfItem (item);
			} catch (Exception e) {
				LogError ("ChildrenOfItem", e);
			} finally {
				children = children ?? Enumerable.Empty<IItem> ();
			}

			return children.Select (i => DoItem.EnsureDoItem (i) as IItem);
		}
	}
}
