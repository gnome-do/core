// EnumerableExtensions.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
	public static class EnumerableExtensions
	{

		/// <summary>
		/// Performs the specified action on each member of an enumerable.
		/// </summary>
		/// <param name="self">
		/// A <see cref="IEnumerable"/> whose members will have an action performed on them.
		/// </param>
		/// <param name="action">
		/// A <see cref="Action"/> to perform on each member.
		/// </param>
		/// <returns>
		/// The original <see cref="IEnumerable"/> for chaining.
		/// </returns>
		public static IEnumerable<T> ForEach<T> (this IEnumerable<T> self, Action<T> action)
		{
			if (self == null) throw new ArgumentNullException ("self");
			if (action == null) throw new ArgumentNullException ("action");
			
			foreach (T x in self) action (x);
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
