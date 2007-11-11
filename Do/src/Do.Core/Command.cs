/* Command.cs
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

	public class Command : GCObject, ICommand
	{
		public const string kDefaultCommandIcon = "gnome-run";
	
		protected ICommand command;
		
		public Command (ICommand command):
			base (command)
		{
			this.command = command;
		}
		
		public override string Icon {
			get { return command.Icon ?? kDefaultCommandIcon; }
		}
		
		public Type[] SupportedItemTypes {
			get { return (command.SupportedItemTypes == null ? new Type[0] : command.SupportedItemTypes); }
		}
		
		public Type[] SupportedModifierItemTypes {
			get { return (command.SupportedModifierItemTypes == null ? new Type[0] : command.SupportedModifierItemTypes); }
		}

		public bool SupportsItem (IItem item)
		{
			if (item is Item)
				item = (item as Item).IItem;
			return command.SupportsItem (item);
		}
		
		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			
			items = EnsureIItemArray (items);
			return command.SupportsModifierItemForItems (items, EnsureIItem (modItem));
		}
		
		public void Perform (IItem[] items, IItem[] modItems)
		{
			items = EnsureIItemArray (items);
			modItems = EnsureIItemArray (modItems);

			
			new Thread ((ThreadStart) delegate {
				Gdk.Threads.Enter ();
				
				try {
					command.Perform (items, modItems);
				} catch (Exception e) {
					Log.Error ("Command \"{0}\" encountered an error: {1}", command.Name, e.Message);
				}
				
				Gdk.Threads.Leave ();
			}).Start ();
		}
		
		/// <summary>
		/// Returns the inner item if the static type of given
		/// item is an Item subtype. Returns the argument otherwise.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem"/> that may or may not be an Item subtype.
		/// </param>
		/// <returns>
		/// A <see cref="IItem"/> that is NOT an Item subtype (the inner IItem of an Item).
		/// </returns>
		IItem EnsureIItem (IItem item)
		{
			if (item is Item)
				item = (item as Item).IItem;
			return item;
		}
		
		/// <summary>
		/// Like EnsureItem but for arrays of IItems.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem[]"/> that may contain
		/// Item subtypes.
		/// </param>
		/// <returns>
		/// A <see cref="IItem[]"/> of inner IItems.
		/// </returns>
		IItem[] EnsureIItemArray (IItem[] items)
		{
			IItem[] inner_items;
			
			inner_items = items.Clone () as IItem[];
			for (int i = 0; i < items.Length; ++i) {
				if (items[i] is Item) {
					inner_items[i] = (items[i] as Item).IItem;
				}
			}
			return inner_items;
		}
		
	}
}
