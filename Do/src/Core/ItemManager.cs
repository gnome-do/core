// GCCatalog.cs created with MonoDevelop
// User: dave at 12:59 AM 8/17/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

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
			int numScoreNonZero;
			Item [] items;
			
			System.Console.WriteLine("Searching for items matching {0} with {1}intermediate results", context.ItemSearchString, context.Results == null ? "no ":"");
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
			
			// Chop the array where the scores become zero
			for (numScoreNonZero = 0; numScoreNonZero < items.Length; ++numScoreNonZero) {
				if (items [numScoreNonZero].Score == 0) break;
			}
			Array.Resize<Item> (ref items, numScoreNonZero);
			
			context.Results = items;
		}



		
	}
}
