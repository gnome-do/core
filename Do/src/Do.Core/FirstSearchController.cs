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
using System.Linq;
using System.Collections.Generic;

using Do.UI;
using Do.Universe;
using Do.Platform;

namespace Do.Core
{
	
	public class FirstSearchController : SimpleSearchController
	{
		public FirstSearchController() : base ()
		{
		}
		
		public override bool ToggleSecondaryCursor (int cursorLocation)
		{
			bool update = (SecondaryCursors.Length == 0);
			bool result = base.ToggleSecondaryCursor (cursorLocation);
			
			if (update && result)
				UpdateResults ();
			return result;
		}
		
		public override void Reset ()
		{
			base.Reset ();
			textMode = false;
		}

		public override IEnumerable<Type> SearchTypes {
			get {
				if (TextMode) {
					yield return typeof (ITextItem);
				} else if (context.SecondaryCursors.Any ()) {
					// This is pretty bad.
					yield return Results [SecondaryCursors [0]].GetType ();
				} else {
					foreach (Type t in defaultFilter) yield return t;
				}
			}
		}

		public override bool TextMode {
			get { 
				return textMode || ImplicitTextMode; 
			}
			set { 
				if (context.ParentContext != null) return;
				textMode = value; 
				textModeFinalize = false;
				if (Query.Length > 0)
					BuildNewContextFromQuery ();
			}
		}
		
		protected override void UpdateResults ()
		{
			context.Results = InitialResults ();
			bool searchChanged = context.LastContext == null || context.LastContext.Selection != context.Selection;
			base.OnSearchFinished (searchChanged, true, Selection, Query);
		}
		
		public override void SetString (string str)
		{
			context.Query = str;
			BuildNewContextFromQuery ();
		}

		private void BuildNewContextFromQuery ()
		{
			string query = Query;
			
			context.Destroy ();
			context = new SimpleSearchContext ();
			
			foreach (char c in query.ToCharArray ()) {
				context.LastContext = context.Clone () as SimpleSearchContext;
				context.Query += c;
				context.Results = InitialResults ();
			}
			base.OnSearchFinished (true, true, Selection, Query);
		}
	}
}
