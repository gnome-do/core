/* ${FileName}
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
using System.Collections;
using System.Collections.Generic;

namespace Do.Core
{
	
	public class ItemManager : GCObjectManager
	{
		
		private List<ItemSource> _sources;
			
		public ItemManager ()
		{
			_sources = new List<ItemSource> ();
		}
		
		public void AddItemSource (ItemSource source) {
			if (!_sources.Contains (source)) {
				_sources.Add (source);
			}
		}
		
		public override string ToString ()
		{
			string description = GetType () + " {\n"; 
			foreach (ItemSource source in _sources) {
				description += "\t" + source.ToString ();
			}
			description += "\n}";
			return description;
		}
		
		private Item [] AllItems () {
			List<Item> items;

			items = new List<Item> ();
			foreach (ItemSource source in _sources) {
				items.AddRange (source.Items);
			}
			return items.ToArray ();
		}
		
		public void UpdateItemSources ()
		{
			foreach (ItemSource source in _sources) {
				source.UpdateItems ();
			}
		}
		
		public Item [] ItemsForAbbreviation (string ab)
		{
			SearchContext context;
			
			context = new SearchContext ();
			context.ItemSearchString = ab;
			Search (context);
			return context.Results as Item [];
		}
		
		protected override ContextRelation GetContextRelation (SearchContext a, SearchContext b)
		{
			if (a.ItemSearchString == b.ItemSearchString)
				return ContextRelation.Repeat;
			else if (a.ItemSearchString.StartsWith (b.ItemSearchString))
				return ContextRelation.Continuation;
			else
				return ContextRelation.Fresh;
		}
		
		protected override void PerformSearch (SearchContext context)
		{
			int numScoreAboveCutoff, cutoff;
			Item [] items;
			
			cutoff = 30;
			// System.Console.WriteLine("Searching for items matching {0} with {1}intermediate results", context.ItemSearchString, context.Results == null ? "no ":"");
			// Use intermediate search results if available.
			if (context.Results == null) {
				items = AllItems ();
			} else {
				items = context.Results as Item [];
			}
			
			// Score the commands based on the search string and sort them.
			foreach (GCObject item in items) {
				item.Score = item.ScoreForAbbreviation (context.ItemSearchString);
			}
			Array.Sort<GCObject> (items, new GCObjectScoreComparer ());
			
			// Chop the array where the scores become less than cutoff
			for (numScoreAboveCutoff = 0; numScoreAboveCutoff < items.Length; ++numScoreAboveCutoff) {
				if (items [numScoreAboveCutoff].Score < cutoff) break;
			}
			Array.Resize<Item> (ref items, numScoreAboveCutoff);
			
			context.Results = items;
		}



		
	}
}
