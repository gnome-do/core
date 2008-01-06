/* OpenManuallyTypedPathCommand.cs
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
	public class OpenManuallyTypedPathCommand : AbstractCommand
	{
		public OpenManuallyTypedPathCommand ()
		{
		}

		public override string Name
		{
			get {
				return Catalog.GetString ("Open Path");
			}
		}

		public override string Description
		{
			get {
				return Catalog.GetString ("Opens manually-typed file and folder paths.");
			}
		}

		public override string Icon
		{
			get { return "gtk-open"; }
		}

		public override Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (ITextItem),
				};
			}
		}

		public override bool SupportsItem (IItem item)
		{
			string path;
		 
			path = (item as ITextItem).Text;
			path = path.Replace ("~",
					Environment.GetFolderPath (Environment.SpecialFolder.Personal));
			return Directory.Exists (path) || File.Exists (path);
		}

		public override IItem[] Perform (IItem[] items, IItem[] modItems)
		{
			string path;

			foreach (IItem item in items) {
				path = (item as ITextItem).Text;
				path = path.Replace (" ", "\\ ");
				Util.Environment.Open (path);
			}
			return null;
		}
	}
}
