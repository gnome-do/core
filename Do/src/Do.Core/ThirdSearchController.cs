// ThirdSearchController.cs
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
using System.Linq;
using System.Collections.Generic;

using Do.Universe;
using Do.Platform;
using Do.Interface;

namespace Do.Core
{
	
	
	public class ThirdSearchController : SimpleSearchController
	{
		ISearchController FirstController, SecondController;
		uint timer = 0;
		
		bool SearchNeeded {
			get {
				Act act = null;
				
				try {
					act = GetContextualAction ();
				} catch (Exception e) {
					Log<ThirdSearchController>.Error (e.Message);
				}
				
				if (act == null)
					return false;
				
				return act.SupportedModifierItemTypes.Any ();
			}
		}
		
		Act GetContextualAction ()
		{
			// fixme : This really should have a buffer to it that gets reset when an upstream selection changes
			if (FirstController.Selection == null || SecondController.Selection == null)
				return null;
			
			Item first, second;
			first = FirstController.Selection;
			second = SecondController.Selection;
			
			if (first.IsAction () && first.AsAction ().Safe.SupportsItem (second))
				return first.AsAction ();
			else if (second.IsAction () && second.AsAction ().Safe.SupportsItem (first))
				return second.AsAction ();
			// fixme
			throw new Exception ("Could not get contextual action");
		}
		
		Item GetContextualItem ()
		{
			// fixme : This really should have a buffer to it that gets reset when an upstream selection changes
			if (FirstController.Selection == null || SecondController.Selection == null)
				return null;
			
			Item first, second;
			first = FirstController.Selection;
			second = SecondController.Selection;
			
			if (first.IsAction () && first.AsAction ().Safe.SupportsItem (second))
				return second;
			else if (second.IsAction () && second.AsAction ().Safe.SupportsItem (first))
				return first;
			// fixme
			throw new Exception ("Could not get contextual item");
		}
		
		public ThirdSearchController(ISearchController FirstController, ISearchController SecondController) : base ()
		{
			this.FirstController  = FirstController;
			this.SecondController = SecondController;
			
			SecondController.SearchFinished += delegate (object o, SearchFinishState state) {
				if (state.SelectionChanged)
					OnUpstreamSelectionChanged ();
			};
		}
		
		public override IEnumerable<Type> SearchTypes {
			get { 
				if (TextMode)
					yield return typeof (ITextItem);
				else
					yield return typeof (Item);
			}
		}

		public override bool TextMode {
			get { 
				return textMode || ImplicitTextMode; 
			}
			set { 
				if (context.ParentContext != null) return;
				if (!value) { //if its false, no problems!  We can always leave text mode
					textMode = value;
					textModeFinalize = false;
				} else {
					Act action = null;
					
					try {
						action = GetContextualAction ();
					} catch (Exception e) {
						Log<ThirdSearchController>.Error (e.Message);
					}
					
					if (action == null)
						return; //you have done something weird, ignore it!
					
					foreach (Type t in action.SupportedModifierItemTypes) {
						if (t == typeof (ITextItem)) {
							textMode = value;
							textModeFinalize = false;
						}
					}
				}
				
				if (textMode == value)
					BuildNewContextFromQuery ();
			}
		}
		
		private void OnUpstreamSelectionChanged ()
		{
			if (!SearchNeeded) {
				if (context.LastContext != null || context.ParentContext != null || 
				    !string.IsNullOrEmpty (context.Query) || context.Results.Any () || context.FullSelection.Any ()) {
					context.Destroy ();
					context = new SimpleSearchContext ();
					base.OnSearchFinished (true, true, Selection, Query);
				}
				return;
			}
			
			textMode = false;
			if (timer > 0) {
				GLib.Source.Remove (timer);
			}
			base.OnSearchStarted (true);//trigger our search start now
			timer = GLib.Timeout.Add (200, delegate {
				context.Destroy ();
				context = new SimpleSearchContext ();
				UpdateResults (true);
				return false;
			});
		}
		
		protected override List<Item> InitialResults ()
		{
			Item other = null;
			try {
				other = GetContextualItem ();
			} catch {
				return new List<Item> ();
			}
			
			// We continue off our previous results if possible
			if (context.LastContext != null && context.LastContext.Results.Any ()) {
				return new List<Item> (Do.UniverseManager.Search (context.Query, SearchTypes, context.LastContext.Results, other));
			} else if (context.ParentContext != null && context.Results.Any ()) {
				return new List<Item> (context.Results);
			} else { 
				// else we do things the slow way
				return new List<Item> (Do.UniverseManager.Search (context.Query, SearchTypes, other));
			}
		}

		private IList<Item> GetContextResults ()
		{
			Item item = null;
			Act action = null;
			IEnumerable<Item> items = null;
			List<Item> modItems = new List<Item> ();
			
			try {
				action = GetContextualAction ();
			} catch (Exception e) {
				Log<ThirdSearchController>.Error (e.Message);
				return modItems;
			}
			
			if (action == null)
				return modItems;
			
			if (FirstController.Selection == action) {
				item = SecondController.Selection;
				items = SecondController.FullSelection;
			} else if (SecondController.Selection == action) {
				item = FirstController.Selection;
				items = FirstController.FullSelection;
			} else {
				Log<ThirdSearchController>.Debug ("No action found. The interface is out of sync.");
				return modItems;
			}
			
			// If we don't support modifier items, don't search.
			if (!action.Safe.SupportedModifierItemTypes.Any ())
				return modItems;
		
			// Add appropriate modifier items from universe.
			foreach (Item modItem in InitialResults ()) {
				if (action.Safe.SupportsModifierItemForItems (items, modItem))
					modItems.Add (modItem);
			}
			// Add any dynamic modifier items on the first search.
			if (Query.Length == 0) {
				foreach (Item modItem in action.Safe.DynamicModifierItemsForItem (item)) {
					modItem.UpdateRelevance ("", item);
					modItems.Add (modItem);
				}
			}
			// Sort modifier items before we potentially add a text item.
			modItems.Sort ();
			return modItems;
		}
		
		public override void Reset ()
		{
			if (context.LastContext == null) {
				context.Destroy ();
				context = new SimpleSearchContext ();
				return;
			}
			
			while (context.LastContext != null) {
				context = context.LastContext;
			}
			textMode = false;
			
			base.OnSearchFinished (true, true, Selection, Query);
		}
		
		protected override void UpdateResults ()
		{
			UpdateResults (false);
		}
		
		private void UpdateResults (bool upstream_search)
		{
			if (!upstream_search)
				base.OnSearchStarted (false);
			
			context.Results = GetContextResults ();
			if (context.Results == null)
				return;
			
			
			bool selection_changed = (context.LastContext == null || 
			                          context.LastContext.Selection != context.Selection);
			base.OnSearchFinished (selection_changed, true, Selection, Query);
		}
		
		public override void SetString (string str)
		{
			context.Query = str;
			BuildNewContextFromQuery ();
		}

		private void BuildNewContextFromQuery ()
		{
			string query = Query;
			
			context = new SimpleSearchContext ();

			context.Results = GetContextResults ();
			foreach (char c in query.ToCharArray ()) {
				context.LastContext = context.Clone () as SimpleSearchContext;
				context.Query += c;

				context.Results = GetContextResults ();
			}
			base.OnSearchFinished (true, true, Selection, Query);
		}	
		
		protected override bool AcceptChildItem (Item item)
		{
			//fixme
			if (FirstController.Selection.IsAction () && FirstController.Selection.AsAction ().SupportsItem (SecondController.Selection)) {
				Act action = FirstController.Selection.AsAction ();
				return action.Safe.SupportsModifierItemForItems (SecondController.FullSelection, item);
			} else if (SecondController.Selection.IsAction ()) {
				Act action = SecondController.Selection.AsAction ();
				return action.Safe.SupportsModifierItemForItems (FirstController.FullSelection, item);
			}
			return true;
		}
	}
}
