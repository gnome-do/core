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
		private bool no_wait = false;
		
		public override IObject Selection {
			get {
				if (context.Selection == null) {
					GLib.Source.Remove (timer);
					GLib.Source.Remove (wait_timer);
					
					no_wait = true;
					base.OnUpstreamSelectionChanged ();
					no_wait = false;
				}
				
				return context.Selection;
			}
		}

		public SecondSearchController(ISearchController FirstController) : base ()
		{
			this.FirstController = FirstController;
			FirstController.SelectionChanged += OnUpstreamSelectionChanged;
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
				if (wait_timer > 0) //also remove anything simply in update wait
					GLib.Source.Remove (wait_timer);
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
				return false;
			});
		}

		
		protected override void UpdateResults ()
		{
			if (FirstController.Selection == null)
				return;
			
			base.UpdateResults ();
			
			DateTime time = DateTime.Now;
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
			
			context.Results = results.ToArray ();
			
			uint ms = Convert.ToUInt32 (DateTime.Now.Subtract (time).TotalMilliseconds);
			if (ms > Timeout || no_wait) {
				base.OnSelectionChanged ();
			} else {
				if (wait_timer > 0) {
					GLib.Source.Remove (wait_timer);
				}
				wait_timer = GLib.Timeout.Add (Timeout - ms - type_wait, delegate {
					base.OnSelectionChanged ();
					return false;
				});
			}
			base.OnSelectionChanged ();
			
			//Do.PrintPerf ("SecondUpdate Stop");
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
