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
using Do.Interface;

namespace Do.Core
{
	public class SimpleSearchContext : ICloneable
	{
		string query;
		int cursor;
		IList<Item> results;
		
		public SimpleSearchContext ()
		{
			SecondaryCursors = new Item[0];
			query = "";
			results = new Item[0];
		}
		
		public SimpleSearchContext LastContext   { get; set; }
		public SimpleSearchContext ParentContext { get; set; }
		public Item[] SecondaryCursors        { get; set; }
		
		public string Query
		{
			get {
				if (query == null)
					query = "";
				return query;
			}
			set { query = value; }
		}

		public IList<Item> Results
		{
			get {
				if (results == null)
					results = new Item[0];
				return results;
			}
			set {
				results = value ?? new List<Item> (0);
				
				cursor = 0;
				
				if (SecondaryCursors.Length == 0) return;
				
				List<Item> secondary = new List<Item> ();
				foreach (Item obj in SecondaryCursors) {
					foreach (Item robj in Results) {
						if (obj == robj) {
							secondary.Add (obj);
						}
					}
				}
				
				SecondaryCursors = secondary.ToArray ();
			}
		}
		
		public Item Selection
		{
			get {
				try {
					return results[cursor];
				} catch {
					return null;
				}
			}
		}
		
		public Item[] FullSelection
		{
			get {
				List<Item> outList = new List<Item> ();
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
				if (value > Results.Count - 1)
					cursor = Results.Count - 1;
				else if ( value <= 0 )
					cursor = 0;
				else
					cursor = value;
			}
		}
		
		/// <summary>
		/// Creates a new IUIContext from the current context.  Contexts however are not away if
		/// the controller wishes for them to be displayed in Large Text Mode, so this value must
		/// be passed in.  Parent contexts are always assumed to not use large text mode.
		/// </summary>
		/// <param name="textMode">
		/// A <see cref="System.Boolean"/>
		/// </param>
		/// <returns>
		/// A <see cref="IUIContext"/>
		/// </returns>
		public IUIContext GetIUIContext (bool textMode, TextModeType type)
		{
			if (ParentContext == null)
				return new UIContext (Selection, Results, Cursor, SecondaryCursorsToIntArray (), 
				                      Query, textMode, type, null);
			else
				return new UIContext (Selection, Results, Cursor, SecondaryCursorsToIntArray (), 
				                      Query, textMode, type, ParentContext.GetIUIContext (false, TextModeType.None));
		}
		
		/// <summary>
		/// It is very convinient to convert to an int array.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int[] SecondaryCursorsToIntArray () 
		{
			if (SecondaryCursors.Length == 0)
				return new int[0];
			List<int> cursors = new List<int> ();
				
			foreach (Item obj in SecondaryCursors) {
				for (int i = 0; i < Results.Count; i++) {
					if (Results[i] == obj)
						cursors.Add (i);
				}
			}
			
			return cursors.ToArray ();
		}
		
		public object Clone ()
		{
			SimpleSearchContext clone;
			
			clone = new SimpleSearchContext ();
			clone.Query = query;
			clone.LastContext = LastContext;
			clone.ParentContext = ParentContext;
			clone.Cursor = Cursor;
			clone.SecondaryCursors = SecondaryCursors; //Cloning these makes no sense
//			clone.Results = results.Clone () as Item[];
			clone.Results = new List<Item> (results);
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
			"\n\tHas last context: " + (LastContext != null) +
			"\n\tHas parent context: " + (ParentContext != null) +
			"\n\tCursor: " + cursor +
			"\n\tResults: " + results;
		}
	}
}
