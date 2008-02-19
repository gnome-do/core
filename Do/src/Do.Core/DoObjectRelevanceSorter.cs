/* DoObjectRelevanceSorter.cs
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
	public class DoObjectRelevanceSorter
	{
		const int kMaxSearchResults = 1000;
		
		string query;
		
		public DoObjectRelevanceSorter (string query)
		{
			this.query = query;
		}
	
		public List<IObject> SortResults (ICollection<IObject> broadResults, bool strict)
		{
			List<IObject> results;
		 
			results	= new List<IObject> ();
			foreach (DoObject obj in broadResults) {
				if (strict && query.Length > 0 &&
					!LetterOccursAfterDelimiter (char.ToLower (query[0]), obj.Name.ToLower ()))
					continue;

				obj.Score = obj.ScoreForAbbreviation (query);
				if (obj.Score > 0) {
					results.Add (obj);
				}
			}
			results.Sort (new DoObjectScoreComparer ());
			return results;
		}


		public List<IObject> SortAndNarrowResults (ICollection<IObject> broadResults, bool strict)
		{
			List<IObject> results;

			results = SortResults (broadResults, strict);
			// Shorten the list if neccessary.
			if (results.Count > kMaxSearchResults)
				results = results.GetRange (0, kMaxSearchResults);
			return results;
		}

		bool LetterOccursAfterDelimiter (char a, string s)
		{
			int idx;

			idx = 0;
			while (idx < s.Length && (idx = s.IndexOf (a, idx)) > -1) {
				if (idx == 0 ||
					(idx > 0 && s[idx-1] == ' ')) {
					return true;
				}
				idx++;
			}
			return false;
		}
		


		class DoObjectScoreComparer : IComparer<IObject>
		{
			public int Compare (IObject x, IObject y) {
				DoObject a, b;
				int compareRelevance, compareScore, compareAction;
				
				a = x as DoObject;
				b = y as DoObject;
				
				// Actions are penalized when compared against non-actions.
				compareAction = 100 * ((b is DoAction?-1:0) - (a is DoAction?-1:0));

				compareRelevance = b.Relevance - a.Relevance;
				compareScore = b.Score - a.Score;

				return (int) (
					compareAction	 * .10 +
					compareRelevance * .20 +
					compareScore     * .70
				);
			}
		}

	}
}
