/* ${FileName}
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
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core
{
	
	public class SearchContext
	{
		List<DoItem> items;
		DoCommand command;
		List<DoItem> modifierItems;
		string searchString;
		int index;
		SearchContext lastContext;
		Type[] searchTypes;
		
		IObject[] results;
				
		public SearchContext ()
		{
			searchTypes = new Type [] { typeof (IItem), typeof (ICommand) };
			items = new List<DoItem> ();
			modifierItems = new List<DoItem> ();
		}
		
		public SearchContext Clone () {
			SearchContext clonedContext = new SearchContext ();
			clonedContext.Command = command;
			clonedContext.Items = items;
			clonedContext.ModifierItems = modifierItems;
			clonedContext.SearchString = searchString;
			clonedContext.LastContext = lastContext;
			if (results != null) {
				clonedContext.Results = (IObject[]) (results.Clone ());
			}
			clonedContext.SearchTypes = searchTypes;
			return clonedContext;
		}
		
		public SearchContext LastContext {
			get {
				return lastContext;
			}
			set {
				lastContext = value;
			}
		}
		
		public List<DoItem> Items {
			get {
				return items;
			}
			set {
				items = value;
			}
		}
		
		public List<DoItem> ModifierItems {
			get {
				return modifierItems;
			}
			set {
				modifierItems = value;
			}
		}
		
		public DoCommand Command {
			get {
				return command;
			}
			set {
				command = value;
			}
		}
			
		public string SearchString {
			get {
				return searchString;
			}
			set {
				searchString = value;
			}
		}

		public IObject[] Results {
			get {
				return results;
			}
			set {
				// NOTE Do something special here later; if
				// a client class sets this field, it must
				// be ensured that array contains IObjects.
				results = value;
			}
		}
		
		public int ObjectIndex {
			get {
				return index;
			}
			set {
				index = value;
			}
		}
		
		public Type[] SearchTypes {
			get {
				return searchTypes;
			}
			set {
				searchTypes = value;
			}
		}
		
		//This returns an array of the inner items, based on the list of DoItems
		//This is necessary because SearchContext stores its items as DoItems, but sometimes
		//methods like ActivateCommand want the IItems associated with DoItems
		public IItem[] IItems {
			get {
				IItem[] returnItems = new IItem [items.Count];
				int i = 0;
				foreach (DoItem item in items) {
					returnItems[i] = item.IItem;
					i++;
				}
				return returnItems;
			}
		}
		
		public IItem[] ModIItems {
			get {
				IItem[] returnItems = new IItem [modifierItems.Count];
				int i = 0;
				foreach (DoItem item in modifierItems) {
					returnItems[i] = item.IItem;
					i++;
				}
				return returnItems;
			}
		}
		
		public bool ContainsFirstObject ()
		{
			return (items.Count != 0 || command != null);
		}
		
		public bool ContainsSecondObject ()
		{
			return (items.Count != 0 && command != null);
		}
		
		public bool ContainsCommand ()
		{
			return (command != null);
		}
		
		public bool ContainsItems ()
		{
			return (items.Count != 0);
		}
		
		public bool ContainsModifierItems ()
		{
			return (modifierItems.Count != 0);
		}
		
		public void ResetAllObjects ()
		{
			items = new List<DoItem> ();
			modifierItems = new List<DoItem> ();
			command = null;
		}
		
		//When setting the "first object" on a search context, set the proper item/command
		/// value, then erase the other ones ensuring that the object is actually the "first"
		public IObject FirstObject {
			set {
				if (value is IItem) {
					command = null;
					items = new List<DoItem> ();
					modifierItems = new List<DoItem> ();
					items.Add (value as DoItem);
				}
				else if (value is ICommand) {
					items = new List<DoItem> ();
					modifierItems = new List<DoItem> ();
					command = value as DoCommand;
				}
			}
		}
		
		//Same as the first object, but don't reset the opposite item/command corresponding to the "first"
		/// only erase modifierItems
		public IObject SecondObject {
			set {
				if (value is IItem) {
					items = new List<DoItem> ();
					items.Add (value as DoItem);
					modifierItems = new List<DoItem> ();
				}
				else if (value is ICommand) {
					command = value as DoCommand;
					modifierItems = new List<DoItem> ();
				}
			}
		}
	}
}
