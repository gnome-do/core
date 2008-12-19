/* Action.cs
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

namespace Do.Universe
{	

	public abstract class Act : Element
	{
		
		/// <value>
		/// An array of Item sub-interfaces that this action supports.
		/// null is ok--it signifies that NO types are supported.
		/// </value>
		protected abstract IEnumerable<Type> SupportedItemTypes { get; }
		
		/// <value>
		/// An array of Item sub-interfaces that this action supports as modifier
		/// items. If your action uses modifier items, either optionally or necessarily,
		/// either from the item universe or provided dynamically with DynamicModifierItemsForItem,
		/// this must return a non-empty array; if you return null or an empty array
		/// here, your action will not work with modifier items.
		/// null is ok--it signifies that NO types are supported.
		/// </value>
		protected virtual IEnumerable<Type> SupportedModifierItemTypes {
			get { yield break; }
		}
		
		/// <value>
		/// Whether modifier items are optional (if you indicate that modifier
		/// items are /not/ optional, this means that they are required).
		/// </value>
		protected virtual bool ModifierItemsOptional {
			get { return false; }
		}
		
		/// <summary>
		/// Indicate whether the action can operate on the given item.  The item is
		/// guaranteed to be a subtype of some supported type.
		/// </summary>
		/// <param name="item">
		/// A <see cref="Item"/> to test for supportability.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether or not to accept the
		/// item.
		/// </returns>
		protected virtual bool SupportsItem (Item item)
		{
			return true;
		}
		
		/// <summary>
		/// Similar to SupportsItem, but uses an array of Items as a context for
		/// determining whether or not a modifier item is supported. Items will
		/// have already been accepted by SupportsItem. The modifier Item is
		/// guaranteed to be a subtype of a supported type. If the action class
		/// implementing this interface does not support any modifier item types,
		/// this method will never be called.
		/// </summary>
		/// <param name="items">
		/// A <see cref="Item[]"/> of Items that the user is attempting to perform
		/// this action on.
		/// </param>
		/// <param name="modItem">
		/// A <see cref="Item"/> that will be used to modify the behavior of this
		/// action when performed on the items in the Item array.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether this action supports
		/// the particular modifier item for the items in the Item array.
		/// </returns>
		protected virtual bool SupportsModifierItemForItems (IEnumerable<Item> items, Item modItem)
		{
			return true;
		}
		
		/// <summary>
		/// If you would like to supply modifier items dynamically, do
		/// so here.
		/// </summary>
		/// <param name="item">
		/// An <see cref="Item[]"/> for which you will or will not provide
		/// dynamic modifier items.
		/// </param>
		/// <returns>
		/// An <see cref="Item[]"/> containing items to use as modifier items.
		/// null is ok--it signifies that no modifier items are provided.
		/// </returns>
		protected virtual IEnumerable<Item> DynamicModifierItemsForItem (Item item)
		{
			yield break;
		}

		/// <summary>
		/// Perform this action on the given items with the given modifier items.
		/// </summary>
		/// <param name="items">
		/// A <see cref="Item[]"/> of primary items.
		/// </param>
		/// <param name="modItems">
		/// A <see cref="Item[]"/> of modifier items.
		/// </param>
		/// <returns>
		/// A <see cref="Item[]"/> of result items to present to the user.
		/// You may return null--this is the same as returning an empty array.
		/// </returns>
		protected abstract IEnumerable<Item> Perform (IEnumerable<Item> items, IEnumerable<Item> modItems);


		#region Safe alternatives

		public IEnumerable<Type> SupportedItemTypesSafe {
			get {
				return SupportedItemTypes ?? Type.EmptyTypes;
			}
		}
		
		public IEnumerable<Type> SupportedModifierItemTypesSafe {
			get {
				return SupportedModifierItemTypes ?? Type.EmptyTypes;
			}
		}

		public bool ModifierItemsOptionalSafe {
			get {
				return ModifierItemsOptional;
			}
		}

		public IEnumerable<Item> DynamicModifierItemsForItemSafe (Item item)
		{
			IEnumerable<Item> modItems = null;

			item = ProxyItem.Unwrap (item);

			// This is a duplicate check, so we may want to remove it.
			if (!SupportedItemTypes.Any (type => type.IsInstanceOfType (item)))
				return Enumerable.Empty<Item> ();
			
			try {
				// Strictly evaluate the IEnumerable before we leave the try block.
				modItems = DynamicModifierItemsForItem (item).ToArray ();
			} catch (Exception e) {
				modItems = null;
				LogSafeError ("DynamicModifierItemsForItem", e);
				// Log.Debug (e.StackTrace);
			} finally {
				modItems = modItems ?? Enumerable.Empty<Item> ();
			}
			return modItems;
		}

		public bool SupportsItemSafe (Item item)
		{
			item = ProxyItem.Unwrap (item);
			
			if (!SupportedItemTypes.Any (type => type.IsInstanceOfType (item)))
				return false;

			try {
				return SupportsItem (item);
			} catch (Exception e) {
				LogSafeError ("SupportsItem", e);
			}
			return false;
		}

		public bool SupportsModifierItemForItemsSafe (IEnumerable<Item> items, Item modItem)
		{
			items = ProxyItem.Unwrap (items);
			modItem = ProxyItem.Unwrap (modItem);
			
			if (!SupportedModifierItemTypes.Any (type => type.IsInstanceOfType (modItem)))
				return false;

			try {
				return SupportsModifierItemForItems (items, modItem);
			} catch (Exception e) {
			 	LogSafeError ("SupportsModifierItemForItems", e);
			}
			return false;
		}

		public IEnumerable<Item> PerformSafe (IEnumerable<Item> items, IEnumerable<Item> modItems)
		{
			IEnumerable<Item> results = null;
			
			items = ProxyItem.Unwrap (items);
			modItems = ProxyItem.Unwrap (modItems);
			
			try {
				// Strictly evaluate the IEnumerable before we leave the try block.
				results = Perform (items, modItems);
				if (results != null) results = results.ToArray ();
			} catch (Exception e) {
				results = null;
				LogSafeError ("Perform", e);
			} finally {
				results = results ?? Enumerable.Empty<Item> ();
			}
			return results;
		}

		#endregion
	}
}
