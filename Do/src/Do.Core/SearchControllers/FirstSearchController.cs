// FirstSearchController.cs
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
	
	
	public class FirstSearchController : SimpleSearchController
	{
		
		public FirstSearchController() : base ()
		{
		}
		
		protected override void UpdateResults ()
		{
			base.UpdateResults ();
			//Do.PrintPerf ("FirstControlerUpdate Start");
			List<IObject> results;
			if (!textMode)
				results = InitialResults ();
			else
				results = new List<IObject> ();
				
			
			if (DefaultFilter) {
				results.Add (new DoTextItem (Query));
			} else {
				foreach (Type t in SearchTypes) {
					if (t == typeof (IItem) || t == typeof (ITextItem)) {
						results.Add (new DoTextItem (Query));
					}
				}
			}
			
			context.Results = results.ToArray ();
			//Do.PrintPerf ("FirstControllerResultsAssigned");
			
			//TODO -- Clean this up.  Too fried to think through proper logic now.
			try {
				if (((context.LastContext == null || context.LastContext.Selection == null) && context.Selection != null) ||
					context.LastContext.Selection != context.Selection)
					base.OnSelectionChanged ();
			} catch {
			}
			
			//Do.PrintPerf ("FirstControlerUpdate Stop");
		}

		public override Type[] SearchTypes {
			get {
				if (textMode) {
					return new Type[] {typeof (ITextItem)};
				} else if (context.SecondaryCursors.Length > 0) {
					return new Type[] {(Results[SecondaryCursors[0]] as DoItem).Inner.GetType ()};
				} else {
					return defaultFilter;
				}
			}
		}

		public override bool TextMode {
			get { return textMode; }
			set { 
				textMode = value; 
				if (Query.Length > 0)
					BuildNewContextFromQuery ();
				else
					base.OnSelectionChanged ();
			}
		}
		
		public override void Reset ()
		{
			base.Reset ();
			textMode = false;
		}
		
		public override bool AddSecondaryCursor (int cursorLocation)
		{
			bool update = (SecondaryCursors.Length == 0);
			bool result = base.AddSecondaryCursor (cursorLocation);
			
			if (update && result) {
				UpdateResults ();
			}
			
			return result;
		}

		
		private void BuildNewContextFromQuery ()
		{
			string query = Query;
			
			context = new SimpleSearchContext ();
			List<IObject> results;
			foreach (char c in query.ToCharArray ()) {
				context.LastContext = context.Clone () as SimpleSearchContext;
				context.Query += c;
				
				results = InitialResults ();
				
				results.Add (new DoTextItem (Query));
				context.Results = results.ToArray ();
			}
			base.OnSelectionChanged ();
		}

	}
}
