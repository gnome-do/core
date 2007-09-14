// GCObjectManager.cs created with MonoDevelop
// User: dave at 11:09 PM 8/30/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

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
