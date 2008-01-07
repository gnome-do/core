/* DoCommand.cs
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
using System.Threading;

using Do.Universe;

namespace Do.Core
{
	public class DoCommand : DoObject, ICommand
	{
		public const string kDefaultCommandIcon = "gnome-run";

		public DoCommand (ICommand command):
			base (command)
		{
		}

		public override string Icon
		{
			get { return (Inner as ICommand).Icon ?? kDefaultCommandIcon; }
		}

		public Type[] SupportedItemTypes
		{
			get { return (Inner as ICommand).SupportedItemTypes ?? new Type[0]; }
		}

		public Type[] SupportedModifierItemTypes
		{
			get { return (Inner as ICommand).SupportedModifierItemTypes ?? new Type[0]; }
		}
		
		public bool ModifierItemsOptional
		{
			get { return (Inner as ICommand).ModifierItemsOptional; }
		}
		
		public IItem[] DynamicModifierItemsForItem (IItem item)
		{
			IItem[] modItems;
			
			modItems = null;
			try {
				modItems = DynamicModifierItemsForItem (EnsureIItem (item));
			} catch {
				modItems = null;
			} finally {
				modItems = modItems ?? new IItem[0];
			}
			return EnsureDoItemArray (modItems);
		}

		public bool SupportsItem (IItem item)
		{
			bool supports;
			
			item = EnsureIItem (item);
			if (!IObjectTypeCheck (item, SupportedItemTypes)) return false;

			// Unless I call Gtk.Threads.Enter/Leave, this method freezes and does not return!
			// WTF!?!?@?@@#!@ *Adding* these calls makes the UI freeze in unrelated execution paths.
			// Why is this so fucking weird? The freeze has to do with Gtk.Clipboard interaction in DefineWordCommand.Text. 
			//Gdk.Threads.Enter ();
			try {
				supports = (Inner as ICommand).SupportsItem (item);
			} catch {
				supports = false;
			} finally {
				//Gdk.Threads.Leave ();
			}
			return supports;
		}

		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			bool supports;

			items = EnsureIItemArray (items);
			modItem = EnsureIItem (modItem);
			if (!IObjectTypeCheck (modItem, SupportedModifierItemTypes)) return false;

			try {
				supports = (Inner as ICommand).SupportsModifierItemForItems (items, modItem);
			} catch {
				supports = false;
			}
			return supports;
		}

		public IItem[] Perform (IItem[] items, IItem[] modItems)
		{
			IItem[] resultItems;
			
			items = EnsureIItemArray (items);
			modItems = EnsureIItemArray (modItems);

			InternalItemSource.LastItem.Inner = items[0]; // TODO: Create a command performed event and move this.
			
			resultItems = null;
			try {
				resultItems = (Inner as ICommand).Perform (items, modItems);
			} catch (Exception e) {
				resultItems = null;
				Log.Error ("Command \"{0}\" encountered an error: {1}", Inner.Name, e.Message);
			} finally {
				resultItems = resultItems ?? new IItem[0];
			}
			resultItems = EnsureDoItemArray (resultItems);
			
			// If we have results to feed back into the window, do so after a delay
			// so Perform has time to return in the window.
			if (resultItems.Length > 0) {
				GLib.Timeout.Add (50, delegate {
					Do.Controller.SummonWithObjects (resultItems);
					return false;
				});
			}
			return resultItems;
		}

		/// <summary>
		/// Returns the inner item if the static type of given
		/// item is an DoItem subtype. Returns the argument otherwise.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem"/> that may or may not be an DoItem subtype.
		/// </param>
		/// <returns>
		/// A <see cref="IItem"/> that is NOT an DoItem subtype (the inner IItem of an DoItem).
		/// </returns>
		IItem EnsureIItem (IItem item)
		{
			if (item is DoItem)
				item = (item as DoItem).Inner as IItem;
			return item;
		}

		/// <summary>
		/// Like EnsureItem but for arrays of IItems.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem[]"/> that may contain
		/// DoItem subtypes.
		/// </param>
		/// <returns>
		/// A <see cref="IItem[]"/> of inner IItems.
		/// </returns>
		IItem[] EnsureIItemArray (IItem[] items)
		{
			IItem[] inner_items;

			inner_items = items.Clone () as IItem[];
			for (int i = 0; i < items.Length; ++i) {
				if (items[i] is DoItem) {
					inner_items[i] = (items[i] as DoItem).Inner as IItem;
				}
			}
			return inner_items;
		}

		IItem[] EnsureDoItemArray (IItem[] items)
		{
			IItem[] do_items;

			do_items = items.Clone () as IItem[];
			for (int i = 0; i < items.Length; ++i) {
				if (!(items[i] is DoItem)) {
					do_items[i] = new DoItem (items[i]);
				}
			}
			return do_items;
		}
	}
}
