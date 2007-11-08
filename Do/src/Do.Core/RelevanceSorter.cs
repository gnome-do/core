// /home/dave/trunk-md/Do/src/Do.Core/RelevanceSorter.cs created with MonoDevelop
// User: dave at 8:16 PM 10/19/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections;
using System.Collections.Generic;
using Do.Universe;

namespace Do.Core
{
	
	
	public class RelevanceSorter : IComparer<IObject>
	{
		const int kMaxSearchResults = 1000;
		string searchString;
		
		public RelevanceSorter(string searchString)
		{
			this.searchString = searchString;
		}
		
		public int Compare (IObject x, IObject y) {
			int xScore = (x as GCObject).Score = (x as GCObject).ScoreForAbbreviation (searchString);
			int yScore = (y as GCObject).Score = (y as GCObject).ScoreForAbbreviation (searchString);
			return (xScore - yScore);
		}
		
		public List<IObject> NarrowResults (List<IObject> broadResults) {
			List<IObject> results = new List<IObject> ();
			
			//First throw out the non-zero items, there's no point wasting sorting time on them
			foreach (GCObject obj in broadResults) {
				obj.Score = obj.ScoreForAbbreviation (searchString);
				if (obj.Score > 0) {
					results.Add (obj);
				}
			}
			
				    //Sort the remaining items
			results.Sort (new GCObjectScoreComparer ());
			//Return the results in List<IObject> System.FormatException
			return results.GetRange (0, Math.Min (kMaxSearchResults, results.Count));
		}
	}
}
