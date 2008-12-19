// Element_RelevanceProvider.cs
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

using Do.Universe;

namespace Do.Core
{

	/// <summary>
	/// Relevance related extension methods on Element class.
	/// </summary>
	public static class Element_RelevanceProvider
	{

		static readonly IRelevanceProvider provider = RelevanceProvider.DefaultProvider;

		/// <summary>
		/// Increase the relevance of receiver for string match and other Element.
		/// </summary>
		/// <param name="self">
		/// A <see cref="Element"/> whose relevance is to be increased.
		/// </param>
		/// <param name="match">
		/// A <see cref="System.String"/> of user input for which the receiver should become more relevant.
		/// </param>
		/// <param name="other">
		/// A <see cref="Element"/> (maybe null) context.
		/// </param>
		public static void IncreaseRelevance (this Element self, string match, Element other)
		{
			provider.IncreaseRelevance (self, match, other);
		}

		/// <summary>
		/// Decrease the relevance of receiver for string match and other Element.
		/// </summary>
		/// <param name="self">
		/// A <see cref="Element"/> whose relevance is to be increased.
		/// </param>
		/// <param name="match">
		/// A <see cref="System.String"/> of user input for which the receiver should become less relevant.
		/// </param>
		/// <param name="other">
		/// A <see cref="Element"/> (maybe null) context.
		/// </param>
		public static void DecreaseRelevance (this Element self, string match, Element other)
		{
			provider.DecreaseRelevance (self, match, other);
		}

		/// <summary>
		/// Simply retrieves the receivers relevance and updates the receivers state
		/// (Element.Relevance is set).
		/// </summary>
		/// <param name="self">
		/// A <see cref="Element"/> whose relevance should be updated to reflect
		/// the state of the world.
		/// </param>
		/// <param name="match">
		/// A <see cref="System.String"/> to retrieve relevance info for.
		/// </param>
		/// <param name="other">
		/// A <see cref="Element"/> (maybe null) to retrieve relevance info for.
		/// </param>
		public static void UpdateRelevance (this Element self, string match, Element other)
		{
			self.Relevance = provider.GetRelevance (self, match, other);
		}
	}
}
