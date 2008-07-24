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

		protected override void OnUpstreamSelectionChanged ()
		{
			if (timer > 0) {
				GLib.Source.Remove (timer);
			}
			base.UpdateResults ();//trigger our search start now
			timer = GLib.Timeout.Add (type_wait, delegate {
				Gdk.Threads.Enter ();
				try { 
					base.OnUpstreamSelectionChanged (); 
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
			base.UpdateResults ();
			
			//Do.PrintPerf ("SecondUpdate Start");
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
				foreach (IItem item in initresults) {
					if (action.SupportsItem (item))
						results.Add (item);
				}
				
				IItem textItem = new DoTextItem (Query);
				if (action.SupportsItem (textItem))
					results.Add (textItem);
			}
			
			return results.ToArray ();
		}
		
		protected override void UpdateResults ()
		{
			DateTime time = DateTime.Now;
			if (FirstController.Selection == null)
				return;
			
			context.Results = GetContextResults ();
			
			uint ms = Convert.ToUInt32 (DateTime.Now.Subtract (time).TotalMilliseconds);
			if (ms + type_wait > Timeout) {
				base.OnSelectionChanged ();
			} else {
				if (wait_timer > 0) {
					GLib.Source.Remove (wait_timer);
				}
				
				Console.WriteLine (Timeout - ms - type_wait);
				wait_timer = GLib.Timeout.Add (Timeout - ms - type_wait, delegate {
					Gdk.Threads.Enter ();
					try {
						base.OnSelectionChanged ();
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
					// ok so the basic idea here is that if the first controller selection is an action
					// we can move right to filtering on what it supports.  This is not strictly needed,
					// but speeds up searches since we get more specific results back.  Returning a
					// typeof (IItem) would have the same effect here and MUST be used to debug.
					//return new Type[] {typeof (IItem)};
					return (FirstController.Selection as IAction).SupportedItemTypes;
				} else {
					return new Type[] {typeof (IAction)};
				}
			}
		}

		public override bool TextMode {
			get { 
				return false;
			}
			set {  }
		}

	}
}
