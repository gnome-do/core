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
		/// <summary>
		/// Finds the index of the first element satsifying the given predicate.
		/// TODO: remove this
		/// </summary>
		/// <param name="self">
		/// A <see cref="IEnumerable"/> to search.
		/// </param>
		/// <param name="p">
		/// A <see cref="Func"/> predicate.
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> index of the first element satisfying p, or -1
		/// is no element satisfies p.
		/// </returns>
		public static int FindIndex<T> (this IEnumerable<T> self, Func<T, bool> p)
		{
			int index = 0;
			foreach (T x in self) {
				if (p (x)) return index;
				else index++;
			}
			return -1;
		}

		/// <summary>
		/// Performs the specified action on each member of an enumerable.
		/// </summary>
		/// <param name="self">
		/// A <see cref="IEnumerable"/> whose members will have an action performed on them.
		/// </param>
		/// <param name="f">
		/// A <see cref="Action"/> to perform on each member.
		/// </param>
		/// <returns>
		/// The original <see cref="IEnumerable"/> for chaining.
		/// </returns>
		public static IEnumerable<T> ForEach<T> (this IEnumerable<T> self, Action<T> f)
		{
			foreach (T x in self)
				f (x);
			return self;
		}

		/// <summary>
		/// Prepends ("cons") an element to an enumerable.
		/// </summary>
		/// <remarks>
		/// If maybeTs is null, a singleton enumerable is returned.
		/// </remarks>
		/// <param name="t">
		/// A <see cref="T"/> to prepend.
		/// </param>
		/// <param name="ts">
		/// A <see cref="IEnumerable"/> that may be null.
		/// </param>
		/// <returns>
		/// A <see cref="IEnumerable"/>
		/// </returns>
		public static IEnumerable<T> Cons<T> (this T t, IEnumerable<T> maybeTs)
		{
			if (t == null) throw new ArgumentNullException ("t");
			
			yield return t;
			if (maybeTs == null) yield break;
			foreach (T x in maybeTs) yield return x;
		}

	}
}
