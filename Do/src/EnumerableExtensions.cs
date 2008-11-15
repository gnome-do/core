// Enumerable.cs created with MonoDevelop
// User: david at 12:49 PM 10/21/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;

namespace Do
{
	public static class EnumerableExtensions
	{
		public static int Count<T> (this IEnumerable<T> self)
		{
			int count = 0;
			foreach (T x in self)
				count++;
			return count;
		}

		public static int FindIndex<T> (this IEnumerable<T> self, Func<T, bool> p)
		{
			int index = 0;
			foreach (T x in self) {
				if (p (x)) return index;
				else index++;
			}
			return -1;
		}
		
		public static IEnumerable<T> ForEach<T> (this IEnumerable<T> self, Action<T> f)
		{
			foreach (T x in self)
				f (x);
			return self;
		}
	}
}
