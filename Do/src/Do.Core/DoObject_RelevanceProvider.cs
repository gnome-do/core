// DoObject_RelevanceProvider.cs
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

namespace Do.Core
{

	/// <summary>
	/// Relevance related extension methods on DoObject class.
	/// </summary>
	public static class DoObject_RelevanceProvider
	{

		static readonly IRelevanceProvider provider = RelevanceProvider.DefaultProvider;

		/// <summary>
		/// Increase the relevance of receiver for string match and other DoObject.
		/// </summary>
		/// <param name="self">
		/// A <see cref="DoObject"/> whose relevance is to be increased.
		/// </param>
		/// <param name="match">
		/// A <see cref="System.String"/> of user input for which the receiver should become more relevant.
		/// </param>
		/// <param name="other">
		/// A <see cref="DoObject"/> (maybe null) context.
		/// </param>
		public static void IncreaseRelevance (this DoObject self, string match, DoObject other)
		{
			provider.IncreaseRelevance (self, match, other);
		}

		/// <summary>
		/// Decrease the relevance of receiver for string match and other DoObject.
		/// </summary>
		/// <param name="self">
		/// A <see cref="DoObject"/> whose relevance is to be increased.
		/// </param>
		/// <param name="match">
		/// A <see cref="System.String"/> of user input for which the receiver should become less relevant.
		/// </param>
		/// <param name="other">
		/// A <see cref="DoObject"/> (maybe null) context.
		/// </param>
		public static void DecreaseRelevance (this DoObject self, string match, DoObject other)
		{
			provider.DecreaseRelevance (self, match, other);
		}

		/// <summary>
		/// Simply retrieves the receivers relevance and updates the receivers state
		/// (DoObject.Relevance is set).
		/// </summary>
		/// <param name="self">
		/// A <see cref="DoObject"/> whose relevance should be updated to reflect
		/// the state of the world.
		/// </param>
		/// <param name="match">
		/// A <see cref="System.String"/> to retrieve relevance info for.
		/// </param>
		/// <param name="other">
		/// A <see cref="DoObject"/> (maybe null) to retrieve relevance info for.
		/// </param>
		public static void UpdateRelevance (this DoObject self, string match, DoObject other)
		{
			self.Relevance = provider.GetRelevance (self, match, other);
		}
	}
}
