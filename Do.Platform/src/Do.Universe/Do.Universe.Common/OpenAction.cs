// OpenAction.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this
// source distribution.
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
using System.IO;
using System.Collections.Generic;

using Mono.Unix;

using Do.Platform;

namespace Do.Universe.Common
{
	/// <summary>
	/// A command providing "open" semantics to many kinds of items.
	/// </summary>
	public class OpenAction : Act
	{

		public override string Name {
			get { return Catalog.GetString ("Open"); }
		}
		
		public override string Description {
			get { return Catalog.GetString ("Opens many kinds of items."); }
		}
		
		public override string Icon {
			get { return "gtk-open"; }
		}
		
		public override IEnumerable<Type> SupportedItemTypes {
			get {
				yield return typeof (IOpenableItem);
				yield return typeof (IUriItem);
				// Support opening manually-typed paths.
				yield return typeof (ITextItem);
			}
		}

		public override bool SupportsItem (Item item)
		{
			if (item is ITextItem) {
				// Check if typed text is a valid path.
				string path = Services.Environment.ExpandPath ((item as ITextItem).Text);
				if (256 < path.Length) return false;
				return Directory.Exists (path) || File.Exists (path);
			}
			return true;
		}

		public override IEnumerable<Item> Perform (IEnumerable<Item> items, IEnumerable<Item> modItems)
		{
			IEnvironmentService env = Services.Environment;

			foreach (Item item in items) {
				if (item is IOpenableItem)
					(item as IOpenableItem).Open ();
				else if (item is IUriItem)
					env.OpenPath ((item as IUriItem).Uri);
				else if (item is ITextItem)
					env.OpenPath ((item as ITextItem).Text);
			}
			return null;
		}

	}
}
