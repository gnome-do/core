/* OpenAction.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Collections.Generic;

using Mono.Unix;

using Do.Addins;

namespace Do.Universe
{
	/// <summary>
	/// A command providing "open" semantics to many kinds of items.
	/// </summary>
	public class OpenAction : AbstractAction
	{
		public OpenAction ()
		{
		}
		
		public override string Name
		{
			get { return Catalog.GetString ("Open"); }
		}
		
		public override string Description
		{
			get { return Catalog.GetString ("Opens many kinds of items."); }
		}
		
		public override string Icon
		{
			get { return "gtk-open"; }
		}
		
		public override Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (IOpenableItem),
					typeof (IURIItem),
					// Support opening manually-typed paths.
					typeof (ITextItem),
				};
			}
		}

		public override bool SupportsItem (IItem item)
		{
			if (item is ITextItem) {	
				// Check if typed text is a valid path.
				string path = (item as ITextItem).Text
					.Replace ("~", Paths.UserHome);
				return Directory.Exists (path) || File.Exists (path);
			}
			return true;
		}

		
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{
			string toOpen = null;
			foreach (IItem item in items) {
				if (item is IOpenableItem) {
					(item as IOpenableItem).Open ();
					continue;
				}

				if (item is IURIItem) {
					toOpen = (item as IURIItem).URI;
				} else if (item is ITextItem) {
					// item is a valid file or folder path.
					toOpen = (item as ITextItem).Text
						.Replace (" ", "\\ ");
				}
				Util.Environment.Open (toOpen);
			}
			return null;
		}
	}
}
