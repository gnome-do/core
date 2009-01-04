/* SafeAct.cs
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

namespace Do.Universe.Safe
{	

	public class SafeAct : Act
	{

		public Act Act { protected get; set; }

		public SafeAct () : this (null)
		{
		}

		public SafeAct (Act act)
		{
			Act = act;
		}

		public override string Name {
			get { return (Act as Element).Safe.Name; }
		}

		public override string Description {
			get { return (Act as Element).Safe.Description; }
		}

		public override string Icon {
			get { return (Act as Element).Safe.Icon; }
		}

		public override IEnumerable<Type> SupportedItemTypes {
			get {
				IEnumerable<Type> types = null;
				try {
					// We don't strictly evalute here because Linq is unlikely and we
					// want this to be fast.
					types = Act.SupportedItemTypes;
				} catch (Exception e) {
					types = null;
					SafeElement.LogSafeError (Act, e, "SupportedItemTypes");
				} finally {
					types = types ?? Type.EmptyTypes;
				}
				return types;
			}
		}
		
		public override IEnumerable<Type> SupportedModifierItemTypes {
			get {
				IEnumerable<Type> types = null;
				try {
					// We don't strictly evalute here because Linq is unlikely and we
					// want this to be fast.
					types = Act.SupportedModifierItemTypes;
				} catch (Exception e) {
					types = null;
					SafeElement.LogSafeError (Act, e, "SupportedModifierItemTypes");
				} finally {
					types = types ?? Type.EmptyTypes;
				}
				return types;
			}
		}
		
		public override bool ModifierItemsOptional {
			get {
				if (!SupportedModifierItemTypes.Any ())
					return true;
				try {
					return Act.ModifierItemsOptional;
				} catch (Exception e) {
					SafeElement.LogSafeError (Act, e, "ModifierItemsOptional");
				}
				return false;
			}
		}
		
		public override IEnumerable<Item> DynamicModifierItemsForItem (Item item)
		{
			IEnumerable<Item> modItems = null;

			item = ProxyItem.Unwrap (item);

			// This is a duplicate check, so we may want to remove it.
			if (!SupportedItemTypes.Any (type => type.IsInstanceOfType (item)))
				return Enumerable.Empty<Item> ();
			
			try {
				// Strictly evaluate the IEnumerable before we leave the try block.
				modItems = Act.DynamicModifierItemsForItem (item).ToArray ();
			} catch (Exception e) {
				modItems = null;
				SafeElement.LogSafeError (Act, e, "DynamicModifierItemsForItem");
			} finally {
				modItems = modItems ?? Enumerable.Empty<Item> ();
			}
			return modItems;
		}

		public override bool SupportsItem (Item item)
		{
			item = ProxyItem.Unwrap (item);
			
			if (!SupportedItemTypes.Any (type => type.IsInstanceOfType (item)))
				return false;

			try {
				return Act.SupportsItem (item);
			} catch (Exception e) {
				SafeElement.LogSafeError (Act, e, "SupportsItem");
			}
			return false;
		}

		public override bool SupportsModifierItemForItems (IEnumerable<Item> items, Item modItem)
		{
			items = ProxyItem.Unwrap (items);
			modItem = ProxyItem.Unwrap (modItem);
			
			if (!SupportedModifierItemTypes.Any (type => type.IsInstanceOfType (modItem)))
				return false;

			try {
				return Act.SupportsModifierItemForItems (items, modItem);
			} catch (Exception e) {
			 	SafeElement.LogSafeError (Act, e, "SupportsModifierItemForItems");
			}
			return false;
		}

		public override IEnumerable<Item> Perform (IEnumerable<Item> items, IEnumerable<Item> modItems)
		{
			IEnumerable<Item> results = null;
			
			items = ProxyItem.Unwrap (items);
			modItems = ProxyItem.Unwrap (modItems);
			
			try {
				// Strictly evaluate the IEnumerable before we leave the try block.
				results = Act.Perform (items, modItems);
				if (results != null) results = results.ToArray ();
			} catch (Exception e) {
				results = null;
				SafeElement.LogSafeError (Act, e, "Perform");
			} finally {
				results = results ?? Enumerable.Empty<Item> ();
			}
			return results;
		}

	}
}
