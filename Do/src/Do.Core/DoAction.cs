/* DoAction.cs
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
using System.Threading;
using System.Collections.Generic;

using Do;
using Do.Universe;

namespace Do.Core {

	public class DoAction : DoObject, IAction {

		IEnumerable<Type> item_types, moditem_types;

		public DoAction (IAction action):
			base (action)
		{
		}
	
		public IEnumerable<Type> SupportedItemTypes
		{
			get {
				if (item_types != null) return item_types;

				try {
					item_types = (Inner as IAction).SupportedItemTypes;
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

		public IEnumerable<Type> SupportedModifierItemTypes
		{
			get {
				if (moditem_types != null) return moditem_types;
				
				try {
					moditem_types = (Inner as IAction).SupportedModifierItemTypes;
					// Call ToArray to strictly evaluate the IEnumerable before we leave
					// the try block.
					if (moditem_types != null) moditem_types = moditem_types.ToArray ();
				} catch (Exception e) {
					LogError ("SupportedModifierItemTypes", e);
				} finally {
					moditem_types = moditem_types ?? Enumerable.Empty<Type> ();
				}
				return moditem_types;
			}
		}
		
		public bool ModifierItemsOptional
		{
			get {
				bool optional = true;
				try {
					optional = (Inner as IAction).ModifierItemsOptional;
				} catch (Exception e) {
					LogError ("ModifierItemsOptional", e);
				}
				return optional;
			}
		}
		
		public IEnumerable<IItem> DynamicModifierItemsForItem (IItem item)
		{
			IEnumerable<IItem> modItems  = null;
			
			try {
				modItems = (Inner as IAction)
					.DynamicModifierItemsForItem (DoItem.EnsureIItem (item));
				// Call ToList to strictly evaluate the IEnumerable before we leave
				// the try block.
				if (modItems != null) modItems = modItems.ToList ();
			} catch (Exception e) {
				LogError ("DynamicModifierItemsForItem", e);
			} finally {
				modItems = modItems ?? Enumerable.Empty<IItem> ();
			}
			return modItems.Select (i => DoItem.EnsureDoItem (i));
		}

		public bool SupportsItem (IItem item)
		{
			bool supports = false;
			
			item = DoItem.EnsureIItem (item);
			if (!item.IsAssignableToAny (SupportedItemTypes))
				return false;

			try {
				supports = (Inner as IAction).SupportsItem (item);
			} catch (Exception e) {
				LogError ("SupportsItem", e);
			}
			return supports;
		}

		public bool SupportsModifierItemForItems (IEnumerable<IItem> items, IItem modItem)
		{
			bool supports = false;

			items = items.Select (i => DoItem.EnsureIItem (i));
			modItem = DoItem.EnsureIItem (modItem);
			if (!modItem.IsAssignableToAny (SupportedModifierItemTypes))
				return false;

			try {
				supports = (Inner as IAction).SupportsModifierItemForItems (items, modItem);
			} catch (Exception e) {
				LogError ("SupportsModifierItemForItems", e);
			}
			return supports;
		}

		public IEnumerable<IItem> Perform (IEnumerable<IItem> items, IEnumerable<IItem> modItems)
		{
			IEnumerable<IItem> results = null;
			
			try {
				results = (Inner as IAction).Perform (
					items.Select (i => DoItem.EnsureIItem (i)),
					modItems.Select (i => DoItem.EnsureIItem (i))
				);
				// Call ToList to strictly evaluate the IEnumerable before we leave
				// the try block.
				if (results != null) results = results.ToList ();
			} catch (Exception e) {
				LogError ("Perform", e);
			} finally {
				results = results ?? Enumerable.Empty<IItem> ();
			}
			results = results.Select (i => DoItem.EnsureDoItem (i));
			
			// If we have results to feed back into the window, do so in a new
			// iteration.
			if (results.Any ()) {
				GLib.Timeout.Add (10, delegate {
					Do.Controller.SummonWithObjects (results.Cast<IObject> ());
					return false;
				});
			}
			return results;
		}
		
	}	
}
