// ItemExtensions.cs
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core
{

	static class ItemExtensions
	{
		static readonly IRelevanceProvider provider = RelevanceProvider.DefaultProvider;
		static IDictionary<Item, bool> has_children;

		static ItemExtensions ()
		{
			has_children = new Dictionary<Item, bool> ();
		}

		public static bool HasChildren (this Item self)
		{
			if (!has_children.ContainsKey (self)) {
				has_children[self] = self.GetChildren ().Any ()
					||
					PluginManager.ItemSources
					.Any (source => source.Safe.ChildrenOfItem (self).Any ())
					||
					PluginManager.DynamicItemSources
					.Any ((source) => source.ChildrenOfItem (self).Any());
			}
			return has_children [self];
		}

		public static bool IsAction (this Item self) 
		{
			return ProxyItem.Unwrap (self) is Act;
		}
		
		public static Act AsAction (this Item self)
		{
			return ProxyItem.Unwrap (self) as Act;
		}
		
		/// <summary>
		/// Increase the relevance of receiver for string match and other Item.
		/// </summary>
		/// <param name="self">
		/// A <see cref="Item"/> whose relevance is to be increased.
		/// </param>
		/// <param name="match">
		/// A <see cref="System.String"/> of user input for which the receiver should become more relevant.
		/// </param>
		/// <param name="other">
		/// A <see cref="Item"/> (maybe null) context.
		/// </param>
		public static void IncreaseRelevance (this Item self, string match, Item other)
		{
			provider.IncreaseRelevance (self, match, other);
		}

		/// <summary>
		/// Decrease the relevance of receiver for string match and other Item.
		/// </summary>
		/// <param name="self">
		/// A <see cref="Item"/> whose relevance is to be increased.
		/// </param>
		/// <param name="match">
		/// A <see cref="System.String"/> of user input for which the receiver should become less relevant.
		/// </param>
		/// <param name="other">
		/// A <see cref="Item"/> (maybe null) context.
		/// </param>
		public static void DecreaseRelevance (this Item self, string match, Item other)
		{
			provider.DecreaseRelevance (self, match, other);
		}

		/// <summary>
		/// Simply retrieves the receivers relevance and updates the receivers state
		/// (Item.Relevance is set).
		/// </summary>
		/// <param name="self">
		/// A <see cref="Item"/> whose relevance should be updated to reflect
		/// the state of the world.
		/// </param>
		/// <param name="match">
		/// A <see cref="System.String"/> to retrieve relevance info for.
		/// </param>
		/// <param name="other">
		/// A <see cref="Item"/> (maybe null) to retrieve relevance info for.
		/// </param>
		public static float UpdateRelevance (this Item self, string match, Item other)
		{
			return self.Relevance = provider.GetRelevance (self, match, other);
		}
	}
}
