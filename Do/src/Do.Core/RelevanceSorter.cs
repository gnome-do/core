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
			return 0;
		}
		
		public List<IObject> NarrowResults (List<IObject> broadResults) {
			List<IObject> narrowResults;
			int numScoreNonZero;

			IObject[] newArray = broadResults.ToArray ();
			GCObject[] results = new GCObject[newArray.Length];
			for (int i = 0; i < newArray.Length; i++) {
				results[i] = (GCObject) (newArray[i]);
			}
			
			foreach (GCObject result in results) {
				result.Score = result.ScoreForAbbreviation (searchString);
			}
			Array.Sort<GCObject> (results, new GCObjectScoreComparer ());
			
			// Chop the array where the scores become zero
			for (numScoreNonZero = 0; numScoreNonZero < results.Length && numScoreNonZero < kMaxSearchResults; ++numScoreNonZero) {
				if (results[numScoreNonZero].Score == 0) break;
			}
			Array.Resize<GCObject> (ref results, numScoreNonZero);
			
			narrowResults = new List<IObject> (results);
			return narrowResults;
		}
	}
}
