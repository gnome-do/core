// Enumerable.cs created with MonoDevelop
// User: david at 12:49 PM 10/21/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace Do
{
	public static class EnumerableExtensions
	{
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

		// TODO: move this to ObjectExtensions.cs (?)
		public static IEnumerable<T> Cons<T> (this T t, IEnumerable<T> ts)
		{
			yield return t;

			if (ts != null)
				foreach (T x in ts)
					yield return x;
		}

		public static IEnumerable<T> ToSafeEnumerable<T> (this IEnumerable<T> self)
			where T : class
		{
			IEnumerator enumerator = self.GetEnumerator ();
			while (true) {
				T current;
				try {
					if (enumerator.MoveNext ())
						current = enumerator.Current as T;
					else break;
				} catch (InvalidOperationException e) {
					throw e;
				} catch (Exception) {
					continue;
				}
				yield return current;
			}
		}

	}
}
