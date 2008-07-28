// SearchController.cs
// 
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;

using Do.Addins;
using Do.Universe;
using Do.UI;

namespace Do.Core
{
	public abstract class SimpleSearchController : ISearchController
	{	
		protected bool textMode = false;
		
		protected SimpleSearchContext context;
		protected Type[] searchFilter;
		protected Type[] defaultFilter = new Type[0];
		protected const int Timeout = 300;
		
		public IUIContext UIContext {
			get {
				return context.GetIUIContext (TextMode);
			}
		}
		
		public IObject[] Results {
			get {
				return context.Results;
			}
			set {
				context.Results = value;
				OnSelectionChanged ();
			}
		}

		public IObject[] FullSelection {
			get {
				return context.FullSelection;
			}
		}

		public virtual IObject Selection {
			get {
				return context.Selection;
			}
		}

		public int Cursor {
			get {
				return context.Cursor;
			}
			set {
				IObject tmp = Selection;
				context.Cursor = value;
				if (tmp != Selection) {
					try {
						OnSelectionChanged ();
					} catch {}
				}
			}
		}

		public int[] SecondaryCursors { 
			get { 
				return context.SecondaryCursorsToIntArray (); 
			}
		}

		public virtual bool TextMode {
			get {
				return textMode;
			}
			set { //fixme
				return;
			}
		}

		public bool DefaultFilter {
			get {
				return (SearchTypes == defaultFilter);
			}
		}

		public abstract Type[] SearchTypes {
			get;
		}
		
		public string Query {
			get {
				return context.Query;
			}
		}

		public SimpleSearchController ()
		{
			context = new SimpleSearchContext ();
			searchFilter = defaultFilter;
		}
		
		public void AddChar (char character)
		{
			context.LastContext = (SimpleSearchContext) context.Clone ();
			context.Query += character;
			
			QueryChanged ();
			UpdateResults ();
			
		}
		
		protected abstract void UpdateResults ();
		//protected a void OnUpstreamSelectionChanged ()
		/*{
			context.Destroy ();
			context = new SimpleSearchContext ();
			UpdateResults ();
		}*/
		
		protected virtual List<IObject> InitialResults ()
		{
			//We continue off our previous results if possible
			if (context.LastContext != null && 
			    context.LastContext.Results.Length != 0) {
				return new List<IObject> (Do.UniverseManager.
				                          Search (context.Query, SearchTypes, 
				                                  context.LastContext.Results));
			} else if (context.ParentContext != null) {
				//If we have a parent context we NEVER do a full search.  Just return
				//the results as they are.
				return new List<IObject> (context.Results);
			} else { 
				//else we do things the slow way
				return new List<IObject> (Do.UniverseManager.
				                          Search (context.Query, SearchTypes));
			}
		}
		
		public virtual void DeleteChar ()
		{
			if (context.LastContext == null) {
				return;
			}
			
			IObject tmp = context.Selection;
			context = context.LastContext;
			
			
			if (tmp != context.Selection)
				SelectionChanged ();
			QueryChanged ();
		}

		public virtual bool AddSecondaryCursor (int cursorLocation)
		{
			if (Results.Length - 1 < cursorLocation) return false;
			
			List<IObject> secondary;
			secondary = new List<IObject> (context.SecondaryCursors);
			
			IObject newObject = Results[cursorLocation];
			if (secondary.Contains (newObject))
				secondary.Remove (newObject);
			else
				secondary.Add (newObject);
			
			context.SecondaryCursors = secondary.ToArray ();
			
			return true;
		}
		
		public bool ItemChildSearch ()
		{
			if (context.Selection is IAction)
				return false;
			
			IItem item = context.Selection as IItem;
			List<IObject> children = new List<IObject> ();

			foreach (DoItemSource source in PluginManager.GetItemSources ()) {
				foreach (IObject child in source.ChildrenOfItem (item))
					children.Add (child);
			}
			
			if (children.Count == 0)
				return false;
			
			SimpleSearchContext newContext = new SimpleSearchContext ();
			newContext.ParentContext = context;
			context = newContext;
			
			context.Results = children.ToArray ();
			OnSelectionChanged ();
			return true;
		}
		
		public bool ItemParentSearch ()
		{
			if (context.ParentContext == null) return false;
			
			SimpleSearchContext parent = context.ParentContext;
			context.Destroy (true);
			context = parent;
			OnSelectionChanged ();
			return true;
		}
		
		public virtual void Reset ()
		{
			searchFilter = defaultFilter;
			context.Destroy ();
			GC.Collect ();
			context = new SimpleSearchContext ();
		}
		
		protected void OnSelectionChanged ()
		{
			SelectionChanged ();
		}
		
		protected void OnSearchStarted (bool upstream_search)
		{
			SearchStarted (upstream_search);
		}	
		
		protected void OnSearchFinished (bool selection_changed)
		{
			SearchFinished (selection_changed);
		}
		
		public event NullEventHandler SelectionChanged;
		public event NullEventHandler QueryChanged;
		public event SearchStartedEventHandler SearchStarted;
		public event SearchFinishedEventHandler SearchFinished;
	}
}
