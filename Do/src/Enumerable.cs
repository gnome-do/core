// Enumerable.cs created with MonoDevelop
// User: david at 12:49 PM 10/21/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;

namespace Do
{
	public static class Enumerable
	{

		public static T Aggregate<T> (IEnumerable<T> self, Func<T, T, T> f)
		{
			T acc;
			bool first = true;
			foreach (T x in self) {
				acc = first ? x : f (acc, x);
				first = false;
			}
			return acc;
		}

		public static T2 Aggregate<T1, T2> (IEnumerable<T1> self, T2 acc, Func<T2, T1, T2> f)
		{
			foreach (T1 x in self)
				acc = f (acc, x);
			return acc;
		}

		public static T3 Aggregate<T1, T2, T3> (IEnumerable<T1> self, T2 acc, Func<T2, T1, T2> f, Func<T2, T3> f2)
		{
			return f2 (Aggregate (self, acc, f));
		}
			
		public static bool All<T> (this IEnumerable<T> self, Func<T, bool> p)
		{
			foreach (T x in self)
				if (!p (x)) return false;
			return true;
		}

		public static bool Any<T> (this IEnumerable<T> self)
		{
			foreach (T x in self)
				return true;
			return false;
		}
		
		public static bool Any<T> (this IEnumerable<T> self, Func<T, bool> p)
		{
			foreach (T x in self)
				if (p (x)) return true;
			return false;
		}

		public static IEnumerable<T2> Select<T1, T2> (this IEnumerable<T1> self, Func<T1, T2> f)
		{
			List<T2> xs = new List<T2> ();
			foreach (T1 x in self)
				xs.Add (f (x));
			return xs;
		}

		public static IEnumerable<T> Where<T> (this IEnumerable<T> self, Func<T, bool> p)
		{
			List<T> xs = new List<T> ();
			foreach (T x in self)
				if (p (x)) xs.Add (x);
			return xs;
		}
	}
}
