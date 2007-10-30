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
using System.Collections.Generic;

namespace Do.Core
{
	
	public abstract class GCObjectManager
	{
		private SearchContext lastSearch, previousSearch;
		private Stack<SearchContext> previous_searches;
		
		public GCObjectManager()
		{
			previous_searches = new Stack<SearchContext> ();
		}
		
		protected void Search (SearchContext context)
		{
			bool repeat = false;
			
			if (previousSearch != null) {
				switch (GetContextRelation (context, previousSearch)) {
				case ContextRelation.Fresh:
					context.Results = null;
					lastSearch = previousSearch = null;
					previous_searches.Clear ();
					goto search;
				case ContextRelation.Repeat:
					repeat = true;
					context.Results = previousSearch.Results;
					previous_searches.Pop ();
					if (previous_searches.Count > 1) {
						lastSearch = previous_searches.Pop ();
						previousSearch = previous_searches.Peek ();
						previous_searches.Push (lastSearch);
					} else {
						lastSearch = previous_searches.Peek ();
						previousSearch = null;
					}
					goto search;
				case ContextRelation.Continuation:
					break;
				}
			}
			if (lastSearch != null) {
				switch (GetContextRelation (context, lastSearch)) {
				case ContextRelation.Fresh:
					context.Results = null;
					lastSearch = previousSearch = null;
					previous_searches.Clear ();
					break;
				case ContextRelation.Repeat:
					repeat = true;
					context.Results = lastSearch.Results;
					previous_searches.Pop ();
					if (previous_searches.Count > 0) {
						lastSearch = previous_searches.Peek ();
					} else {
						lastSearch = null;
					}
					break;
				case ContextRelation.Continuation:
					context.Results = lastSearch.Results;
					break;
				}
			} else {
				context.Results = null;
			}
			search:
			if (!repeat) {
				PerformSearch (context);
			}
			previousSearch = lastSearch;
			lastSearch = context;
			previous_searches.Push (lastSearch);
		}
		
		protected abstract ContextRelation GetContextRelation (SearchContext a, SearchContext b);
		protected abstract void PerformSearch (SearchContext context);
	}
}
