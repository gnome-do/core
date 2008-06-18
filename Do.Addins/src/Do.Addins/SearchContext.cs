/* SearchContext.cs
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

namespace Do.Addins
{
	public class SearchContext : ICloneable, IEquatable<SearchContext>
	{
		List<IItem> items;
		List<IItem> modifierItems;
		
		IAction action;
		
		string query;
		Type[] searchTypes;
		Type[] previousSearchTypes;
		int cursor;
		IObject[] results;
		
		SearchContext lastContext;
		SearchContext parentContext;
		
		bool parentSearch, childrenSearch;
				
		public SearchContext ():
			this (true)
		{
		}
		
		public SearchContext (bool bufferLastContext)
		{
			items = new List<IItem> ();
			modifierItems = new List<IItem> ();
			action = null;
			
			query = "";
			searchTypes = new Type [] { typeof (IItem), typeof (IAction) };
			results = new IObject[0];
			cursor = 0;
			
			lastContext = parentContext = null;
			parentSearch = childrenSearch = false;
			
			if (bufferLastContext) {
				lastContext = new SearchContext (false);
			}
		}
		
		public SearchContext LastContext
		{
			get { return lastContext; }
			set { lastContext = value; }
		}
		
		public SearchContext ParentContext
		{
			get { return parentContext; }
			set { parentContext = value; }
		}
		
		public List<IItem> Items
		{
			get { return items ?? items = new List<IItem> (); }
			set { items = value; }
		}
		
		public List<IItem> ModifierItems
		{
			get { return modifierItems; }
			set { modifierItems = value; }
		}
		
		public IAction Action
		{
			get { return action; }
			set { action = value; }
		}
			
		public string Query
		{
			get { return query ?? query = ""; }
			set { query = value; }
		}

		public IObject[] Results
		{
			get { return results ?? results = new IObject[0]; }
			set {
				results = value ?? new IObject[0];
				cursor = 0;
			}
		}
		
		public bool ItemsSearch
		{
			get {
				return searchTypes.Length == 1 &&
					searchTypes[0] == typeof (IItem);
			}
		}
		
		public bool ActionSearch
		{
			get {
				return  searchTypes.Length == 1 &&
					searchTypes[0] == typeof (IAction);
			}
		}
		
		public bool ModifierItemsSearch
		{
			get {
				return searchTypes.Length == 1 &&
					searchTypes[0] == typeof (IItem) &&
					items.Count > 0 && action != null;
			}
		}
	
		public bool ChildrenSearch
		{
			get { return childrenSearch;}
			set { childrenSearch = value; }
		}
		
		public bool ParentSearch
		{
			get { return parentSearch; }
			set { parentSearch = value; }
		}
		
		public bool Independent
		{
			get {
				return !(ActionSearch || ItemsSearch || ModifierItemsSearch);
			}
		}
	
		public bool DefaultFilter
		{
			get {
				return (SearchTypes.Length == 2 && (
				          SearchTypes[0] == typeof (IItem) && SearchTypes[1] == typeof (IAction) ||
				          SearchTypes[0] == typeof (IAction) && SearchTypes[1] == typeof (IItem)));
			}
		}
		
		public bool TextMode
		{
			get {
				if (SearchTypes.Length == 1 && SearchTypes[0] == typeof (ITextItem)) {
				    return true;
				}
				return false;
			}
			set {
				if (value == true && SupportsTextMode) {
					PreviousSearchTypes = SearchTypes;
					SearchTypes = new Type[] {typeof (ITextItem)};
				} else if (value == false) {
					if (PreviousSearchTypes != null)
						SearchTypes = PreviousSearchTypes;
					else
						SearchTypes = new Type[] {typeof (IItem), typeof (IAction)};
				}
			}
		}
		
		private bool SupportsTextMode
		{
			get {
				if (results.Length == 0) return true;
				foreach (IObject i in results) {
					if (i is ITextItem) {
						return true;
					}
				}
				return false;
			}
		}
		
		public IObject Selection
		{
			get {
				try {
					return results[cursor];
				} catch {
					return null;
				}
			}
		}
		
		public Type[] SearchTypes
		{
			get { return searchTypes ??
					searchTypes = new Type[] {typeof (IItem), typeof (IAction)}; }
			set {
				PreviousSearchTypes = searchTypes;
				searchTypes = value;
			}
		}
		
		private Type[] PreviousSearchTypes
		{
			get { return previousSearchTypes ??
					previousSearchTypes = new Type[] {typeof (IItem), typeof (IAction)}; }
			set {
				previousSearchTypes = value;
			}
		}
		
		public int Cursor
		{
			get { return cursor; }
			set { 
				if (value > Results.Length - 1)
					cursor = Results.Length - 1;
				else if ( value <= 0 )
					cursor = 0;
				else
					cursor = value;
			}
		}
		
		public object Clone ()
		{
			SearchContext clone;
			
			clone = new SearchContext ();
			clone.Action = action;
			clone.Items = new List<IItem> (items);
			clone.ModifierItems = new List<IItem> (modifierItems);
			clone.Query = query;
			clone.LastContext = lastContext;
			clone.ParentContext = parentContext;
			clone.Cursor = Cursor;
			clone.Results = results.Clone () as IObject[];
			clone.ChildrenSearch = childrenSearch;
			clone.ParentSearch = parentSearch;
			clone.SearchTypes = searchTypes;
			return clone;
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			return Equals (o as SearchContext);
		}

		public bool Equals (SearchContext test)
		{			
			if (test == null) return false;
			if (query != test.Query) return false;
			
			// Test to see if the type filters are the same.
			// TODO: Use Array.Equals once mono supports it.
			if (test.SearchTypes.Length != SearchTypes.Length) return false; 	
			for (int i = 0; i < SearchTypes.Length; ++i) { 	
				if (test.SearchTypes[i] != SearchTypes[i])
					return false; 	
			}
			
			// Check to see if items the same, but only if items are supposed to be fixed.
			if (test.ActionSearch || test.ModifierItemsSearch) {
				// TODO: Use List.Equals once mono supports it.
				if (test.Items.Count != Items.Count) return false; 	
				for (int i = 0; i < Items.Count; ++i) { 	
					if (test.Items[i] != Items[i])
						return false; 	
				}
			}
			
			// Check to see if actions are the same, but only if actions are supposed to be fixed
			if (test.ItemsSearch || test.ModifierItemsSearch)
				if (test.Action != Action) return false;
			
			if (test.ModifierItemsSearch != ModifierItemsSearch ||
				test.ChildrenSearch != childrenSearch ||
				test.ParentSearch != parentSearch)
				return false;

			return true;
		}
		
		public void Clear ()
		{
			items = new List<IItem> ();
			modifierItems = new List<IItem> ();
			action = null;
		}
		
		public SearchContext EquivalentPreviousContextIfExists ()
		{
			if (Equals (LastContext.LastContext)) {
				return LastContext.LastContext;
			}
			return null;
		}
		
		public SearchContext GetContinuedContext ()
		{
			SearchContext clone;
			
			clone = Clone () as SearchContext;
			clone.LastContext = this;
			return clone;
		}
		
		public override string ToString ()
		{
			return "SearchContext " + base.ToString () + 
			"\n\tQuery: \"" + query + "\"" +
			"\n\tAction: " + action +
			"\n\tNumber of items: " + items.Count +
			"\n\tNumber of modifier items: " + modifierItems.Count +
			"\n\tHas last context: " + (lastContext != null) +
			"\n\tHas parent context: " + (parentContext != null) +
			"\n\tCursor: " + cursor +
			"\n\tResults: " + results +
			"\n\tSearching children: " + childrenSearch +
			"\n\tSearching parent: " + parentSearch;
		}
	}
}
