/* SafeItemSource.cs
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

using Do.Universe;

namespace Do.Universe.Safe
{

	public class SafeItemSource : ItemSource
	{
		public ItemSource ItemSource { protected get; set; }

		public SafeItemSource () : this (null)
		{
		}

		public SafeItemSource (ItemSource itemSource)
		{
			ItemSource = itemSource;
		}

		public override string Name {
			get { return (ItemSource as Element).Safe.Name; }
		}

		public override string Description {
			get { return (ItemSource as Element).Safe.Description; }
		}

		public override string Icon {
			get { return (ItemSource as Element).Safe.Icon; }
		}

		public override IEnumerable<Type> SupportedItemTypes {
			get {
				IEnumerable<Type> types = null;
				try {
					// We don't strictly evalute here because Linq is unlikely and we
					// want this to be fast.
					types = ItemSource.SupportedItemTypes;
				} catch (Exception e) {
					types = null;
					SafeElement.LogSafeError (ItemSource, e, "SupportedItemTypes");
				} finally {
					types = types ?? Type.EmptyTypes;
				}
				return types;
			}
		}
		
		public override void UpdateItems ()
		{
			try {
				ItemSource.UpdateItems ();
			} catch (Exception e) {
				SafeElement.LogSafeError (ItemSource, e, "UpdateItems");
			}
		}
		
		public override IEnumerable<Item> Items
		{
			get {
				IEnumerable<Item> items = null;
				
				try {
					// Strictly evaluate the IEnumerable before we leave the try block.
					items = ItemSource.Items.ToArray ();
				} catch (Exception e) {
					items = null;
					SafeElement.LogSafeError (ItemSource, e, "Items");
				} finally {
					items = items ?? Enumerable.Empty<Item> ();
				}

				return items;
			}
		}

		public override IEnumerable<Item> ChildrenOfItem (Item item)
		{
			IEnumerable<Item> children = null;

			item = ProxyItem.Unwrap (item);

			if (!SupportedItemTypes.Any (type => type.IsInstanceOfType (item)))
			    return Enumerable.Empty<Item> ();

			try {
				// Strictly evaluate the IEnumerable before we leave the try block.
				children = ItemSource.ChildrenOfItem (item).ToArray ();
			} catch (Exception e) {
				children = null;
				SafeElement.LogSafeError (ItemSource, e, "ChildrenOfItem");
			} finally {
				children = children ?? Enumerable.Empty<Item> ();
			}
			return children;
		}

	}

}
