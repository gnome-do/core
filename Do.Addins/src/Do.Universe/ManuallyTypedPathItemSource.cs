/* ManuallyTypedPathItemSource.cs
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

	public class ManuallyTypedPathItemSource : AbstractItemSource
	{
		public ManuallyTypedPathItemSource ()
		{
		}

		public override string Name
		{
			get {
				return Catalog.GetString ("Manually-typed path items");
			}
		}

		public override string Description
		{
			get {
				return Catalog.GetString ("Provides children files to manually-typed paths.");
			}
		}

		public override Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (ITextItem),
				};
			}
		}

		public override ICollection<IItem> ChildrenOfItem (IItem item)
		{
			List<IItem> children;
			string path;
		 
			children = new List<IItem> ();
			path = (item as ITextItem).Text;
			path = path.Replace ("~",
					Environment.GetFolderPath (Environment.SpecialFolder.Personal));

			// For directories, return their contents.
			if (Directory.Exists (path)) {
				foreach (string child_path in Directory.GetFileSystemEntries (path)) {
					children.Add (new FileItem (child_path));
				}
			// For files, return their FileItem representations.
			} else if (File.Exists (path)) {
				children.Add (new FileItem (path));
			}
			return children;
		}
	}
}
