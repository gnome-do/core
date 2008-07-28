// SecondSearchController.cs
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
	/// <summary>
	/// A search controller most useful in the second pane of Gnome-Do
	/// </summary>
	public class SecondSearchController : SimpleSearchController
	{
		private ISearchController FirstController;
		private uint timer = 0, wait_timer = 0;
		private const int type_wait = 200;
		
		private bool IsSearching {
			get {
				return (timer > 0 || wait_timer > 0);
			}
		}
		
		public override IObject Selection {
			get { 
				if (IsSearching)
					FastSearch ();
				return context.Selection;
			}
		}

		public SecondSearchController(ISearchController FirstController) : base ()
		{
			this.FirstController = FirstController;
			FirstController.SelectionChanged += OnUpstreamSelectionChanged;
		}
		
		//Similar to running UpdateResults (), except we dont have any timeouts
		private void FastSearch ()
		{
			if (FirstController.Selection == null)
				return;
			
			//Clear our timers
			if (timer > 0) {
				GLib.Source.Remove (timer);
				timer = 0;
			}
			if (wait_timer > 0) {
				GLib.Source.Remove (wait_timer);
				wait_timer = 0;
			}
			context.Results = GetContextResults ();
			base.OnSelectionChanged ();
		}
		
		protected override List<IObject> InitialResults ()
		{
			if (textMode)
				return new List<IObject> ();
			//We continue off our previous results if possible
			if (context.LastContext != null && context.LastContext.Results.Length != 0) {
				return new List<IObject> (Do.UniverseManager.Search (context.Query, 
				                                                     SearchTypes, 
				                                                     context.LastContext.Results, 
				                                                     FirstController.Selection));
			} else if (context.ParentContext != null && context.Results.Length != 0) {
				return new List<IObject> (context.Results);
			} else { 
				//else we do things the slow way
				return new List<IObject> (Do.UniverseManager.Search (context.Query, 
				                                                     SearchTypes, 
				                                                     FirstController.Selection));
			}
		}

		private void OnUpstreamSelectionChanged ()
		{
			textMode = false;
			if (timer > 0) {
				GLib.Source.Remove (timer);
			}
			base.OnSearchStarted (true);//trigger our search start now
			timer = GLib.Timeout.Add (type_wait, delegate {
				Gdk.Threads.Enter ();
				try { 
					context.Destroy ();
					context = new SimpleSearchContext ();
					UpdateResults (true);
				} finally { 
					Gdk.Threads.Leave (); 
				}
				timer = 0;
				return false;
			});
		}

		/// <summary>
		/// Set up our results list.
		/// </summary>
		/// <returns>
		/// A <see cref="IObject"/>
		/// </returns>
		private IObject[] GetContextResults ()
		{
			List<IObject> initresults = InitialResults ();
			
			List<IObject> results = new List<IObject> ();
			if (FirstController.Selection is IItem) {
				//We need to find actions for this item
				//TODO -- Make this work for multiple items
				foreach (IAction action in initresults)
					if (action.SupportsItem (FirstController.Selection as IItem))
						results.Add (action);
				
			} else if (FirstController.Selection is IAction) {
				//We need to find items for this action
				IAction action = FirstController.Selection as IAction;
				if (!textMode) {
					foreach (IItem item in initresults) {
						if (action.SupportsItem (item))
							results.Add (item);
					}
				}
				IItem textItem = new DoTextItem (Query);
				if (action.SupportsItem (textItem))
					results.Add (textItem);
			}
			
			return results.ToArray ();
		}
		
		protected override void UpdateResults ()
		{
			base.OnSearchStarted (false);
			UpdateResults (false);
		}

		
		/// <summary>
		/// This method is pretty much a wrapper around GetContextResults () with a timer at the
		/// end.  This is very useful since we might not want this timer and adding a bool to turn
		/// this on is more stateful than i would like.
		/// </summary>
		private void UpdateResults (bool upstreamSearch)
		{
			DateTime time = DateTime.Now;
			
			// if we dont have a first controller selection, we can stop now.
			if (FirstController.Selection == null)
				return;
			
			//we only do this if its false because we already did it up before the timeout
			if (!upstreamSearch)
				base.OnSearchStarted (false);
			
			context.Results = GetContextResults ();
			
			// we now want to know how many ms have elapsed since we started this process
			uint ms = Convert.ToUInt32 (DateTime.Now.Subtract (time).TotalMilliseconds);
			ms += type_wait; // we also know we waited this long at the start
			if (ms > Timeout || !upstreamSearch) {
				//we were too slow, our engine has been defeated and we must return results as
				//quickly as possible
				
				//FIX ME!!!
				if (context.LastContext != null && context.Selection != context.LastContext.Selection) {
					base.OnSelectionChanged ();
					base.OnSearchFinished (true);
				} else {
					base.OnSearchFinished (false);
				}
			} else {
				//yay, we beat the user with a stick
				if (wait_timer > 0) {
					GLib.Source.Remove (wait_timer);
				}
				
				// So the idea here is that we will wait long enough that we still go the full
				// 250ms before showing results.
				wait_timer = GLib.Timeout.Add (Timeout - ms, delegate {
					Gdk.Threads.Enter ();
					try {
						//FIXME!!!
						if (context.LastContext != null && context.Selection != context.LastContext.Selection) {
							base.OnSelectionChanged ();
							base.OnSearchFinished (true);
						} else {
							base.OnSearchFinished (false);
						}
					} finally {
						Gdk.Threads.Leave ();
					}
					wait_timer = 0;
					return false;
				});
			}
		}

		public override Type[] SearchTypes {
			get { 
				
				if (FirstController.Selection is IAction) {
					// the basic idea here is that if the first controller selection is an action
					// we can move right to filtering on what it supports.  This is not strictly needed,
					// but speeds up searches since we get more specific results back.  Returning a
					// typeof (IItem) would have the same effect here and MUST be used to debug.
					// ----return new Type[] {typeof (IItem)};
					return (FirstController.Selection as IAction).SupportedItemTypes;
				} else {
					if (textMode)
						return new Type[] {typeof (ITextItem)};
					return new Type[] {typeof (IAction)};
				}
			}
		}
		
		public override void Reset ()
		{
			while (context.LastContext != null) {
				context = context.LastContext;
			}
			textMode = false;
			
			base.OnSelectionChanged ();
		}


		/// <value>
		/// Set text mode.
		/// </value>
		public override bool TextMode { //FIXME
			get { 
				return textMode;
			}
			set {
				if (context.ParentContext != null) return;
				if (!value) {
					textMode = value;
				} else if (FirstController.Selection is IAction) {
					IAction action = FirstController.Selection as IAction;
					foreach (Type t in action.SupportedItemTypes) {
						if (t == typeof (ITextItem) && action.SupportsItem (new DoTextItem (Query))) {
							textMode = value;
						}
					}
				}
				
				if (textMode == value)
					BuildNewContextFromQuery ();
			}
		}

		/// <summary>
		/// Builds up a new context from a query from scratch.  Useful after changing filters.
		/// </summary>
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
			base.OnSelectionChanged ();
		}	
	}
}
