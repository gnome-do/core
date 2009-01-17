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
using System.Linq;

using Do.Interface;
using Do.Universe;

namespace Do.Core
{
	public abstract class SimpleSearchController : ISearchController
	{	
		protected bool textMode = false;
		protected bool textModeFinalize = false;
		
		protected SimpleSearchContext context;
		protected Type[] searchFilter;
		protected Type[] defaultFilter = Type.EmptyTypes;
		protected const int Timeout = 300;
		
		public IUIContext UIContext {
			get {
				return context.GetIUIContext (TextMode, TextType);
			}
		}
		
		protected bool ImplicitTextMode {
			get {
				return !textMode && Results.Count == 1 && Results [0] is ImplicitTextItem;
			}
		}
		
		public TextModeType TextType {
			get {
				if (textMode) {
					if (textModeFinalize)
						return TextModeType.ExplicitFinalized;
					return TextModeType.Explicit;
				}
				if (ImplicitTextMode)
					return TextModeType.Implicit;
				return TextModeType.None;
			}
		}
		
		public IList<Element> Results {
			get {
				return context.Results;
			}
			set {
				context.Results = value;
				OnSearchFinished (true, false, Selection, Query);
			}
		}

		public IList<Element> FullSelection {
			get {
				return context.FullSelection;
			}
		}

		public virtual Element Selection {
			get {
				return context.Selection;
			}
		}

		public int Cursor {
			get {
				return context.Cursor;
			}
			set {
				Element tmp = Selection;
				int ctmp = context.Cursor;
				context.Cursor = value;
				if (tmp != Selection || context.Cursor != ctmp) {
					OnSearchFinished (true, false, Selection, Query);
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
			set { }
		}

		public bool DefaultFilter {
			get {
				return SearchTypes == defaultFilter;
			}
		}

		public abstract IEnumerable<Type> SearchTypes { get; }
		
		public string Query {
			get { return context.Query; }
		}

		protected SimpleSearchController ()
		{
			context = new SimpleSearchContext ();
			searchFilter = defaultFilter;
		}
		
		public void AddChar (char character)
		{
			context.LastContext = (SimpleSearchContext) context.Clone ();
			context.Query += character;
			UpdateResults ();
		}
		
		public void FinalizeTextMode ()
		{
			if (TextType == TextModeType.Explicit)
				textModeFinalize = true;
		}
		
		protected abstract void UpdateResults ();
		
		protected virtual List<Element> InitialResults ()
		{
			return new List<Element> (Do.UniverseManager.Search (context.Query, SearchTypes));
		}
		
		public virtual void DeleteChar ()
		{
			if (context.LastContext == null) {
				return;
			}
			
			Element tmp = context.Selection;
			context = context.LastContext;
			OnSearchFinished (tmp != context.Selection, true, Selection, Query);
		}

		public virtual bool ToggleSecondaryCursor (int cursorLocation)
		{
			if (Results.Count - 1 < cursorLocation) return false;
			
			List<Element> secondary;
			secondary = new List<Element> (context.SecondaryCursors);
			
			Element newObject = Results[cursorLocation];
			
			if (secondary.Contains (newObject))
				secondary.Remove (newObject);
			else if (newObject is Item)
				secondary.Add (newObject);
			else
				return false;
			
			context.SecondaryCursors = secondary.ToArray ();
			
			return true;
		}
		
		public bool ItemChildSearch ()
		{
			if (context.Selection is Act)
				return false;
			
			Item item = context.Selection as Item;
			List<Element> children = new List<Element> ();

			foreach (ItemSource source in PluginManager.ItemSources) {
				foreach (Element child in source.Safe.ChildrenOfItem (item))
					children.Add (child);
			}
			
			if (!children.Any ())
				return false;
			
			SimpleSearchContext newContext = new SimpleSearchContext ();
			newContext.ParentContext = context;
			context = newContext;
			
			context.Results =
				Do.UniverseManager.Search (Query, defaultFilter, children)
				.ToList ();

			OnSearchFinished (true, context.ParentContext.Query != context.Query, Selection, Query);
			return true;
		}
		
		public bool ItemParentSearch ()
		{
			if (context.ParentContext == null) return false;
			
			string old_query = Query;
			SimpleSearchContext parent = context.ParentContext;
			context.Destroy (true);
			context = parent;
			OnSearchFinished (true, old_query != Query, Selection, Query);
			return true;
		}
		
		public virtual void Reset ()
		{
			searchFilter = defaultFilter;
			context.Destroy ();
			context = new SimpleSearchContext ();
			textModeFinalize = false;
			textMode = false;
		}
		
		protected void OnSearchStarted (bool upstream_search)
		{
			SearchStarted (upstream_search);
		}	
		
		protected void OnSearchFinished (bool selection_changed, bool query_changed, Element selection, string query)
		{
			SearchFinished (this, new SearchFinishState (selection_changed, query_changed, selection, query));
		}
		
		public abstract void SetString (string str);
		
		public event SearchStartedEventHandler SearchStarted;
		public event SearchFinishedEventHandler SearchFinished;
	}
}
