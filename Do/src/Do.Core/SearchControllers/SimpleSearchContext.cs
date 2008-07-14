/* SimpleSearchContext.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core
{
	public class SimpleSearchContext : ICloneable
	{
		string query;
		int cursor;
		IObject[] secondaryCursors;
		IObject[] results;
		
		SimpleSearchContext lastContext;
		SimpleSearchContext parentContext;
		
		public SimpleSearchContext ()
		{
			secondaryCursors = new IObject[0];
			
			query = "";
			results = new IObject[0];
			cursor = 0;
		}
		
		public SimpleSearchContext LastContext
		{
			get { return lastContext; }
			set { lastContext = value; }
		}
		
		public SimpleSearchContext ParentContext
		{
			get { return parentContext; }
			set { parentContext = value; }
		}
		
		public string Query
		{
			get { return query ?? query = ""; }
			set { query = value.ToLower (); }
		}

		public IObject[] Results
		{
			get { return results ?? results = new IObject[0]; }
			set {
				results = value ?? new IObject[0];
				cursor = 0;
				
				if (SecondaryCursors.Length == 0) return;
				
				List<IObject> secondary = new List<IObject> ();
				foreach (IObject obj in SecondaryCursors) {
					foreach (IObject robj in Results) {
						if (obj == robj) {
							secondary.Add (obj);
						}
					}
				}
				
				SecondaryCursors = secondary.ToArray ();
			}
		}
		
		public IObject Selection
		{
			get {
				try {
					return results[cursor];
				} catch {
					return null;
				}
			}
		}
		
		public IObject[] FullSelection
		{
			get {
				List<IObject> outList = new List<IObject> ();
				outList.AddRange (SecondaryCursors);
				
				//Juggle our selection to front to give best possible legacy plugin support.  Ideally this
				//wont matter
				outList.Remove (Selection);
				outList.Insert (0, Selection);
				
				return outList.ToArray ();
			}
		}
		
		public int Cursor
		{
			get { return cursor; }
			set { 
				if (value > Results.Length - 1)
					cursor = Results.Length - 1;
				else if ( value <= 0 )
					cursor = 0;
				else
					cursor = value;
			}
		}
		
		public IObject[] SecondaryCursors
		{
			get {
				return secondaryCursors;
			}
			set {
				secondaryCursors = value;
			}
		}
		
		public object Clone ()
		{
			SimpleSearchContext clone;
			
			clone = new SimpleSearchContext ();
			clone.Query = query;
			clone.LastContext = lastContext;
			clone.ParentContext = parentContext;
			clone.Cursor = Cursor;
			clone.SecondaryCursors = SecondaryCursors; //Cloning these makes no sense
			clone.Results = results.Clone () as IObject[];
			return clone;
		}
		
		public void Destroy (bool preserveParent)
		{
			if (preserveParent) {
				if (LastContext != null)
					LastContext.Destroy ();
			} else {
				Destroy ();
			}
				
		}
		
		public void Destroy ()
		{
			if (LastContext != null)
				LastContext.Destroy ();
			
			if (ParentContext != null)
				ParentContext.Destroy ();
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		public override string ToString ()
		{
			return "SearchContext " + base.ToString () + 
			"\n\tQuery: \"" + query + "\"" +
			"\n\tHas last context: " + (lastContext != null) +
			"\n\tHas parent context: " + (parentContext != null) +
			"\n\tCursor: " + cursor +
			"\n\tResults: " + results;
		}
	}
}
