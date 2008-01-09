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
			get {
				return (Inner as ICommand).Icon ?? kDefaultCommandIcon;
			}
		}

		public Type[] SupportedItemTypes
		{
			get {
				return (Inner as ICommand).SupportedItemTypes ?? new Type[0];
			}
		}

		public Type[] SupportedModifierItemTypes
		{
			get {
				return (Inner as ICommand).SupportedModifierItemTypes ?? new Type[0];
			}
		}
		
		public bool ModifierItemsOptional
		{
			get {
				return (Inner as ICommand).ModifierItemsOptional;
			}
		}
		
		public IItem[] DynamicModifierItemsForItem (IItem item)
		{
			ICommand command = Inner as ICommand;
			IItem[] modItems;
			
			modItems = null;
			item = EnsureIItem (item);
			try {
				modItems = command.DynamicModifierItemsForItem (item);
			} catch {
				modItems = null;
			} finally {
				modItems = modItems ?? new IItem[0];
			}
			return EnsureDoItemArray (modItems);
		}

		public bool SupportsItem (IItem item)
		{
			ICommand command = Inner as ICommand;
			bool supports;
			
			item = EnsureIItem (item);
			if (!IObjectTypeCheck (item, SupportedItemTypes))
				return false;

			// Unless I call Gtk.Threads.Enter/Leave, this method freezes and does not return!
			// WTF!?!?@?@@#!@ *Adding* these calls makes the UI freeze in unrelated execution paths.
			// Why is this so fucking weird? The freeze has to do with Gtk.Clipboard interaction in DefineWordCommand.Text. 
			//Gdk.Threads.Enter ();
			try {
				supports = command.SupportsItem (item);
			} catch {
				supports = false;
			} finally {
				//Gdk.Threads.Leave ();
			}
			return supports;
		}

		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			ICommand command = Inner as ICommand;
			bool supports;

			items = EnsureIItemArray (items);
			modItem = EnsureIItem (modItem);
			if (!IObjectTypeCheck (modItem, SupportedModifierItemTypes))
				return false;

			try {
				supports = command.SupportsModifierItemForItems (items, modItem);
			} catch {
				supports = false;
			}
			return supports;
		}

		public IItem[] Perform (IItem[] items, IItem[] modItems)
		{
			ICommand command = Inner as ICommand;
			IItem[] resultItems;
			
			items = EnsureIItemArray (items);
			modItems = EnsureIItemArray (modItems);

			// TODO: Create a command performed event and move this.
			if (items.Length > 0 )
				InternalItemSource.LastItem.Inner = items[0];
			
			resultItems = null;
			try {
				resultItems = command.Perform (items, modItems);
			} catch (Exception e) {
				resultItems = null;
				Log.Error ("Command \"{0}\" encountered an error: {1}",
						Inner.Name, e.Message);
			} finally {
				resultItems = resultItems ?? new IItem[0];
			}
			resultItems = EnsureDoItemArray (resultItems);
			
			// If we have results to feed back into the window, do so in a new
			// iteration.
			if (resultItems.Length > 0) {
				GLib.Timeout.Add (50, delegate {
					Do.Controller.SummonWithObjects (resultItems);
					return false;
				});
			}
			return resultItems;
		}

	}	
}
