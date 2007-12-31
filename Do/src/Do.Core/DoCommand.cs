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

		protected ICommand command;

		public DoCommand (ICommand command):
			base (command)
		{
			this.command = command;
		}

		public override string Icon
		{
			get { return command.Icon ?? kDefaultCommandIcon; }
		}

		public Type[] SupportedItemTypes
		{
			get { return command.SupportedItemTypes ?? new Type[0]; }
		}

		public Type[] SupportedModifierItemTypes
		{
			get { return command.SupportedModifierItemTypes ?? new Type[0]; }
		}

		public bool SupportsItem (IItem item)
		{
			bool supports;
			
			item = EnsureIItem (item);
			if (!IObjectTypeCheck (item, SupportedItemTypes)) return false;

			supports = false;
			// Unless I call Gtk.Threads.Enter/Leave, this method freezes and does not return!
			// WTF!?!?@?@@#!@ *Adding* these calls makes the UI freeze in unrelated execution paths.
			// Why is this so fucking weird? The freeze has to do with Gtk.Clipboard interaction in DefineWordCommand.Text. 
			//Gdk.Threads.Enter ();
			try {
				supports = command.SupportsItem (item);
			} finally {
				//Gdk.Threads.Leave ();
			}
			return supports;
		}

		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			items = EnsureIItemArray (items);
			modItem = EnsureIItem (modItem);
			if (!IObjectTypeCheck (modItem, SupportedModifierItemTypes)) return false;

			try {
				return command.SupportsModifierItemForItems (items, modItem);
			} catch {
				return false;
			}
		}

		public void Perform (IItem[] items, IItem[] modItems)
		{
			items = EnsureIItemArray (items);
			modItems = EnsureIItemArray (modItems);

			InternalItemSource.LastItem.IItem = items[0]; // TODO: Create a command performed event and move this.
			try {
				command.Perform (items, modItems);
			} catch (Exception e) {
				Log.Error ("Command \"{0}\" encountered an error: {1}", command.Name, e.Message);
			}
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
				item = (item as DoItem).IItem;
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
					inner_items[i] = (items[i] as DoItem).IItem;
				}
			}
			return inner_items;
		}

		public bool AcceptsOnlyText
		{
			get {
				return SupportedItemTypes.Length == 1
					&& typeof (ITextItem).IsAssignableFrom (SupportedItemTypes[0]);
			}
		}
	}
}
