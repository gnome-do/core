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
		private ISearchController FirstController, SecondController;
		private uint timer = 0;
		
		private bool SearchNeeded {
			get {
				if (FirstController.Selection == null || SecondController.Selection == null)
					return false;
				
				Act action;
				if (FirstController.Selection is Act) {
					action = FirstController.Selection as Act;
				} else if (SecondController.Selection is Act) {
					action = SecondController.Selection as Act;
				} else {
					return false;
				}
				
				return action.SupportedModifierItemTypes.Any ();
			}
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
					Act action;
					if (FirstController.Selection is Act)
						action = FirstController.Selection as Act;
					else if (SecondController.Selection is Act)
						action = SecondController.Selection as Act;
					else
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
				context.Destroy ();
				context = new SimpleSearchContext ();
				
				base.OnSearchFinished (true, true, Selection, Query);
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
		
		protected override List<Element> InitialResults ()
		{
			//We continue off our previous results if possible
			if (context.LastContext != null && context.LastContext.Results.Any ()) {
				return new List<Element> (Do.UniverseManager.Search (context.Query, 
					SearchTypes, context.LastContext.Results, FirstController.Selection));
			} else if (context.ParentContext != null && context.Results.Any ()) {
				return new List<Element> (context.Results);
			} else { 
				//else we do things the slow way
				return new List<Element> (
					Do.UniverseManager.Search (context.Query, SearchTypes, FirstController.Selection));
			}
		}

		private IList<Element> GetContextResults ()
		{
			Item item = null;
			Act action = null;
			IEnumerable<Item> items = null;
			List<Item> modItems = new List<Item> ();

			if (FirstController.Selection is Act) {
				action = FirstController.Selection as Act;
				item = SecondController.Selection as Item;
				items = SecondController.FullSelection.OfType<Item> ();
			} else if (SecondController.Selection is Act) {
				action = SecondController.Selection as Act;
				item = FirstController.Selection as Item;
				items = FirstController.FullSelection.OfType<Item> ();
			} else {
				Log.Warn ("No action found. The interface is out of sync.");
				return new List<Element> ();
			}

			// If we don't support modifier items, don't search.
			if (!action.Safe.SupportedModifierItemTypes.Any ())
				return new List<Element> ();
			
			if (!textMode) {
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
			}
			return modItems.OfType<Element> ().ToList<Element> ();
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
	}
}
