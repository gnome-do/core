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
using System.Threading;

using Do.Universe;

namespace Do.Core
{

	public class DoAction : DoObject, IAction
	{
		public const string kDefaultActionIcon = "gnome-run";

		public DoAction (IAction action):
			base (action)
		{
		}
		
		// Legacy Command support.
		public DoAction (ICommand command):
			this (new ICommandWrapperAction (command))
		{
		}

		public override string Icon
		{
			get {
				return (Inner as IAction).Icon ?? kDefaultActionIcon;
			}
		}

		public Type[] SupportedItemTypes
		{
			get {
				return (Inner as IAction).SupportedItemTypes ?? new Type[0];
			}
		}

		public Type[] SupportedModifierItemTypes
		{
			get {
				return (Inner as IAction).SupportedModifierItemTypes ?? new Type[0];
			}
		}
		
		public bool ModifierItemsOptional
		{
			get {
				return (Inner as IAction).ModifierItemsOptional;
			}
		}
		
		public IItem[] DynamicModifierItemsForItem (IItem item)
		{
			IAction action = Inner as IAction;
			IItem[] modItems;
			
			modItems = null;
			item = EnsureIItem (item);
			try {
				modItems = action.DynamicModifierItemsForItem (item);
			} catch {
				modItems = null;
			} finally {
				modItems = modItems ?? new IItem[0];
			}
			return EnsureDoItemArray (modItems);
		}

		public bool SupportsItem (IItem item)
		{
			IAction action = Inner as IAction;
			bool supports;
			
			item = EnsureIItem (item);
			if (!IObjectTypeCheck (item, SupportedItemTypes))
				return false;

			try {
				supports = action.SupportsItem (item);
			} catch {
				supports = false;
			}
			return supports;
		}

		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			IAction action = Inner as IAction;
			bool supports;

			items = EnsureIItemArray (items);
			modItem = EnsureIItem (modItem);
			if (!IObjectTypeCheck (modItem, SupportedModifierItemTypes))
				return false;

			try {
				supports = action.SupportsModifierItemForItems (items, modItem);
			} catch {
				supports = false;
			}
			return supports;
		}

		public IItem[] Perform (IItem[] items, IItem[] modItems)
		{
			IAction action = Inner as IAction;
			IItem[] resultItems;
			
			items = EnsureIItemArray (items);
			modItems = EnsureIItemArray (modItems);

			// TODO: Create a action performed event and move this.
			if (items.Length > 0 )
				InternalItemSource.LastItem.Inner = items[0];
			
			resultItems = null;
			try {
				resultItems = action.Perform (items, modItems);
			} catch (Exception e) {
				resultItems = null;
				Log.Error ("Action \"{0}\" encountered an error: {1}",
						Inner.Name, e.Message);
			} finally {
				resultItems = resultItems ?? new IItem[0];
			}
			resultItems = EnsureDoItemArray (resultItems);
			
			// If we have results to feed back into the window, do so in a new
			// iteration.
			if (resultItems.Length > 0) {
				GLib.Timeout.Add (10, delegate {
					Do.Controller.SummonWithObjects (resultItems);
					return false;
				});
			}
			return resultItems;
		}
	}	

	// Wraps an ICommand as an IAction for legacy Command support.
	class ICommandWrapperAction : IAction
	{
		private ICommand c;

		public ICommandWrapperAction (ICommand command)
		{
			c = command;
		}

		public string Name
		{
			get { return c.Name; }
		}

		public string Description
		{
			get { return c.Description; }
	 	}

		public string Icon
		{
			get { return c.Icon; }
		}
		
		public Type[] SupportedItemTypes
		{
			get { return c.SupportedItemTypes; }
		}
		
		public Type[] SupportedModifierItemTypes
		{
			get { return c.SupportedModifierItemTypes; }
		}
		
		public bool ModifierItemsOptional
		{
			get { return c.ModifierItemsOptional; }
		}

		public bool SupportsItem (IItem item)
		{
			return c.SupportsItem (item);
		}

		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return c.SupportsModifierItemForItems (items, modItem);
		}
		
		public IItem[] DynamicModifierItemsForItem (IItem item)
		{
			return c.DynamicModifierItemsForItem (item);
		}
		
		public IItem[] Perform (IItem[] items, IItem[] modItems)
		{
			return c.Perform (items, modItems);
		}
	}

}
