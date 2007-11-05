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
using Do.Universe;

namespace Do.Core
{
	public enum ContextRelation {
		Fresh,
		Repeat,
		Continuation
	}
	
	public class SearchContext
	{
		GCObject firstObject;
		GCObject secondObject;
		string searchString;
		int index;
		SentencePositionLocator searchPosition;
		SearchContext lastContext;
		
		GCObject[] results;
				
		public SearchContext ()
		{
			searchPosition = SentencePositionLocator.Ambigious;
		}
		
		public SearchContext Clone () {
			SearchContext clonedContext = new SearchContext ();
			clonedContext.FirstObject = firstObject;
			clonedContext.SecondObject = secondObject;
			clonedContext.SearchString = searchString;
			clonedContext.LastContext = lastContext;
			if (results != null) {
				clonedContext.Results = (GCObject[]) (results.Clone ());
			}
			clonedContext.SearchPosition = searchPosition;
			return clonedContext;
		}
		
		public SearchContext LastContext {
			get {
				return lastContext;
			}
			set {
				lastContext = value;
			}
		}
		
		public SentencePositionLocator SearchPosition {
			get {
				return searchPosition;
			}
			set {
				searchPosition = value;
			}
		}
		
		public GCObject FirstObject {
			get {
				return firstObject;
			}
			set {
				firstObject = value;
			}
		}
		
		public GCObject SecondObject {
			get {
				return secondObject;
			}
			set {
				secondObject = value;
			}
		}
			

		public string SearchString {
			get {
				return searchString;
			}
			set {
				searchString = value;
			}
		}

		public GCObject[] Results {
			get {
				return results;
			}
			set {
				// NOTE Do something special here later; if
				// a client class sets this field, it must
				// be ensured that array contains GCObjects.
				results = value;
			}
		}
		
		public int ObjectIndex {
			get {
				return index;
			}
			set {
				index = value;
			}
		}
						
	}
}
