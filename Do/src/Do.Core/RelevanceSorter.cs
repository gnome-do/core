/* RelevanceSorter.cs
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
	
	
	public class RelevanceSorter : IComparer<IObject>
	{
		const int kMaxSearchResults = 1000;
		
		string searchString;
		
		public RelevanceSorter(string searchString)
		{
			this.searchString = searchString;
		}
		
		public int Compare (IObject x, IObject y) {
			int xScore = (x as DoObject).Score = (x as DoObject).ScoreForAbbreviation (searchString);
			int yScore = (y as DoObject).Score = (y as DoObject).ScoreForAbbreviation (searchString);
			return (xScore - yScore);
		}
		
		public List<IObject> NarrowResults (List<IObject> broadResults) {
			List<IObject> results = new List<IObject> ();
			
			//First throw out the non-zero items, there's no point wasting sorting time on them
			foreach (DoObject obj in broadResults) {
				obj.Score = obj.ScoreForAbbreviation (searchString);
				if (obj.Score > 0) {
					results.Add (obj);
				}
			}
			
			//Sort the remaining items
			results.Sort (new DoObjectScoreComparer ());
			//Return the results in List<IObject> System.FormatException
			return results.GetRange (0, Math.Min (kMaxSearchResults, results.Count));
		}
	}
}
