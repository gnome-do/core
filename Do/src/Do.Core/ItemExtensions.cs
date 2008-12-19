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

		static IDictionary<Item, bool> has_children;

		static ItemExtensions ()
		{
			has_children = new Dictionary<Item, bool> ();
		}

		public static bool HasChildren (this Item self) {
			if (!has_children.ContainsKey (self)) {
				has_children [self] = PluginManager.ItemSources
					.Any (source => source.ChildrenOfItemSafe (self).Any ());
			}
			return has_children [self];
		}

	}
}
