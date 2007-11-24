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
		const int MOD_ITEMS = 1;
		const int GET_CHILDREN = 2;
		const int GET_PARENT = 4;

		List<DoItem> items;
		DoCommand command;
		List<DoItem> modifierItems;
		string query;
		SearchContext lastContext;
		SearchContext parentContext;
		IObject[] results;
		Type[] searchTypes;
		int flag;
		IObject parentObject;
		int cursor;
				
		public SearchContext ():
			this (true)
		{
		}
		
		public SearchContext (bool bufferLastContext)
		{
			Build ();
			if (bufferLastContext) {
				lastContext = new SearchContext (false);
			}
		}
		
		public void Build () {
			SearchTypes = new Type[0];
			flag = 0;
			items = new List<DoItem> ();
			modifierItems = new List<DoItem> ();
			Query = "";
			lastContext = null;
			cursor = 0;
			searchTypes = new Type [] { typeof (IItem), typeof (ICommand) };
			query = "";
		}
		
		public SearchContext Clone () {
			SearchContext clonedContext = new SearchContext ();
			clonedContext.Command = command;
			clonedContext.Items = items;
			clonedContext.ModifierItems = modifierItems;
			clonedContext.Query = query;
			clonedContext.LastContext = lastContext;
			clonedContext.ParentContext = parentContext;
			if (results != null) {
				clonedContext.Results = (IObject[]) (results.Clone ());
			}
			clonedContext.flag = flag;
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
		
		public SearchContext ParentContext {
			get {
				return parentContext;
			}
			set {
				parentContext = value;
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
			
		public string Query {
			get {
				return query;
			}
			set {
				query = value;
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
		
		public bool Equivalent (SearchContext test) {
			//If its null, return false right away so a null exception isn't thrown
			if (test == null)
				return false;
			
			//Test to see if the search strings are the same
			if (query != test.Query)
				return false;
			
			//Test to see if the type filters are the same
			if (test.SearchTypes.Length != SearchTypes.Length)
				return false;
			foreach (Type type in SearchTypes) {
				if (!(UniverseManager.ContainsType (test.SearchTypes, type)))
					return false;
			}
			
			//Chech to see if items the same, but only if items are supposed to be fixed
			if (test.CommandSearch || test.ModItemsSearch)
				foreach (DoItem item in test.Items)
					if (!(Items.Contains (item)))
						return false;
			
			//Check to see if commands are the same, but only if commands are supposed to be fixed
			if (test.ItemsSearch || test.ModItemsSearch)
				if (!(test.Command.Equals (Command)))
					return false;
			
			if (test.flag != flag)
				return false;
			
			return true;
		}
		
		public bool ItemsSearch
		{
			get {
				if (command != null && searchTypes[0].Equals (typeof (IItem)) && searchTypes.Length == 1) {
					return true;
				}
				return false;
			}
		}
		
		public bool CommandSearch
		{
			get {
				if (items == null)
					return false;
				if (searchTypes.Length == 0)
					return false;

				if (items.Count != 0 && searchTypes[0].Equals (typeof (ICommand)) && searchTypes.Length == 1) {
					return true;
				}
				return false;
			}
		}
		
		public bool ModItemsSearch {
			get {
				return ((flag & MOD_ITEMS) == MOD_ITEMS);
			}
			set {
				if (value)
					flag = flag | MOD_ITEMS;
				else
					flag = flag & ~(MOD_ITEMS);
			}
		}
	
		public bool FindingChildren {
			get {
				return ((flag & GET_CHILDREN) == GET_CHILDREN);
			}
			set {
				if (value)
					flag = flag | GET_CHILDREN;
				else
					flag = flag & ~(GET_CHILDREN);
			}
		}
		
		public bool FindingParent {
			get {
				return ((flag & GET_PARENT) == GET_PARENT);
			}
			set {
				if (value)
					flag = flag | GET_PARENT;
				else
					flag = flag & ~(GET_PARENT);
			}
		}
		
		public bool Independent {
			get {
				return (!(CommandSearch || ItemsSearch || ModItemsSearch));
			}
		}
		
		public void ResetAllObjects ()
		{
			items = new List<DoItem> ();
			modifierItems = new List<DoItem> ();
			command = null;
		}
		
		public IObject GenericObject {
			set {
				if (value is DoItem) {
					Items = new List<DoItem> ();
					Items.Add (value as DoItem);
				}
				else {
					Command = value as DoCommand;
				}
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
		
		public IObject ParentObject {
			get {
				return parentObject;
			}
			set {
				parentObject = value;
			}
		}
		
		public int Cursor {
			get {
				return cursor;
			}
			set {
				cursor = value;
			}
		}
		
		public SearchContext EquivalentPreviousContextIfExists () {
			if (Equivalent (LastContext.LastContext))
				return LastContext.LastContext;
			return null;
		}
		
		public SearchContext GetContinuedContext () {
			SearchContext clone;
			clone = Clone ();
			clone.LastContext = this;
			return clone;
		}
	}
}
