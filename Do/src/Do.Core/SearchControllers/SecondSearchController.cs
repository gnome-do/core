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
		
		public SecondSearchController(ISearchController FirstController) : base ()
		{
			this.FirstController = FirstController;
			FirstController.SelectionChanged += OnUpstreamSelectionChanged;
		}
		
		protected override void UpdateResults ()
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
				foreach (IItem item in initresults)
					if (action.SupportsItem (item))
						results.Add (item);
				
				IItem textItem = new DoTextItem (Query);
				if (action.SupportsItem (textItem))
					results.Add (textItem);
			}
			
			context.Results = results.ToArray ();
			
			
			//TODO -- Clean this up.  Too fried to think through proper logic now.
			try {
				if (((context.LastContext == null || context.LastContext.Selection == null) && context.Selection != null) ||
					context.LastContext.Selection != context.Selection)
					base.OnSelectionChanged ();
			} catch { }
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
