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
using System.Collections.Generic;

using Do.Addins;
using Do.Universe;
using Do.UI;

namespace Do.Core
{
	
	
	public class ThirdSearchController : SimpleSearchController
	{
		private ISearchController FirstController, SecondController;
		private uint timer = 0;
		
		public ThirdSearchController(ISearchController FirstController, ISearchController SecondController) : base ()
		{
			this.FirstController  = FirstController;
			this.SecondController = SecondController;
			
			SecondController.SelectionChanged += OnUpstreamSelectionChanged;
		}
		
		public override Type[] SearchTypes {
			get { 
				if (textMode)
					return new Type[] {typeof (ITextItem)};
				return new Type[] {typeof (IItem)}; 
			}
		}

		public override bool TextMode {
			get { return textMode; }
			set { 
				if (context.ParentContext != null) return;
				if (!value) { //if its false, no problems!  We can always leave text mode
					textMode = value;
				} else {
					IAction action;
					if (FirstController.Selection is IAction)
						action = FirstController.Selection as IAction;
					else if (SecondController.Selection is IAction)
						action = SecondController.Selection as IAction;
					else
						return;
					
					foreach (Type t in action.SupportedModifierItemTypes) {
						if (t == typeof (ITextItem))
							textMode = value;
					}
				}
				
				if (textMode == value)
					BuildNewContextFromQuery ();
			}
		}
		
		protected override void OnUpstreamSelectionChanged ()
		{
			if (timer > 0) {
				GLib.Source.Remove (timer);
			}
			base.UpdateResults ();//trigger our search start now
			timer = GLib.Timeout.Add (60, delegate {
				Gdk.Threads.Enter ();
				try { 
					base.OnUpstreamSelectionChanged (); 
				} finally { 
					Gdk.Threads.Leave (); 
				}
				return false;
			});
		}

		private IObject[] GetContextResults ()
		{
			base.UpdateResults ();
			
			IAction action;
			IItem item;
			List<IItem> items = new List<IItem> ();
			if (FirstController.Selection is IAction) {
				
				action = FirstController.Selection as IAction;
				item   = SecondController.Selection as IItem;
				foreach (IObject obj in SecondController.FullSelection) {
					if (obj is IItem)
						items.Add (obj as IItem);
				}
				
			} else if (SecondController.Selection is IAction) {
				
				action = SecondController.Selection as IAction;
				item   = FirstController.Selection as IItem;
				foreach (IObject obj in FirstController.FullSelection) {
					if (obj is IItem)
						items.Add (obj as IItem);
				}
				
			} else {
				//Log.Error ("Something Very Strange Has Happened");
				return null;
			}

			//If we support nothing, dont search.
			if (action.SupportedModifierItemTypes.Length == 0)  return null;
			List<IObject> initresults = InitialResults ();
			
			List<IObject> results = new List<IObject> ();
			
			foreach (IItem moditem in initresults) {
				if (action.SupportsModifierItemForItems (items.ToArray (), moditem))
					results.Add (moditem);
			}
			
			results.AddRange (action.DynamicModifierItemsForItem (item));
			results.Sort ();
			
			IItem textItem = new DoTextItem (Query);
			if (action.SupportsModifierItemForItems (items.ToArray (), textItem))
				results.Add (textItem);
			
			return results.ToArray ();
			
		}
		
		protected override void UpdateResults ()
		{
			context.Results = GetContextResults ();
			if (context.Results == null)
				return;
			
			try {
				if (((context.LastContext == null || context.LastContext.Selection == null) && context.Selection != null) ||
					context.LastContext.Selection != context.Selection) {
					base.OnSelectionChanged ();
				}
			} catch { }
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
			base.OnSelectionChanged ();
		}	
	}
}
