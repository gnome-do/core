//
//  DebugItemAction.cs
//
//  Author:
//       Christohper James Halse Rogers <raof@ubuntu.com>
//
//  Copyright (c) 2013 Christopher James Halse Rogers <raof@ubuntu.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;

using System.Linq;

using Do.Universe;
using Do.Platform;

namespace Do.Universe
{
	public class DebugItemAction : Act
	{
		public override string Name {
			get {
				return "Debug Item";
			}
		}

		public override string Description {
			get {
				return "Dumps debugging information about the current item to Do's log";
			}
		}

		public override string Icon {
			get {
				return "preferences-system";
			}
		}

		public override IEnumerable<Item> Perform (IEnumerable<Item> items, IEnumerable<Item> modItems)
		{
			foreach (var item in items) {
				Log<DebugItemAction>.Debug (
					"Debugging info for item\n" +
					"\tName: “{0}”\n" +
					"\tDescription: “{1}”\n" +
					"\tItem Type: “{2}”\n" +
					"\tItem Source(s): “{3}”",
					item.Name, item.Description, item.GetType ().FullName,
					Do.UniverseManager.ResolveSourcesForItem (item).Aggregate("",
						(value, source) => {
							if (String.IsNullOrEmpty(value)) {
								return source.GetType ().FullName;
							}
							return value + ", " + source.GetType ().FullName;
						})
				);
			}
			return null;
		}

		public override IEnumerable<Type> SupportedItemTypes {
			get {
				yield return typeof(Item);
				yield break;
			}
		}
	}
}

