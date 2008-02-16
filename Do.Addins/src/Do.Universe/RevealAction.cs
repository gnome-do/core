/* RevealAction.cs
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

using Do.Addins;

namespace Do.Universe
{
	public class RevealAction : AbstractAction
	{
		public RevealAction ()
		{
		}
		
		public override string Name {
			get { return "Reveal"; }
		}
		
		public override string Description {
			get { return "Reveals a file in the file manager."; }
		}
		
		public override string Icon {
			get { return "file-manager"; }
		}
		
		public override Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof (IFileItem),
				};
			}
		}
		
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{
			foreach (IFileItem file in items) {
				String escapedPath;
				// Nautilus does not have a "reveal file" option, so we just open the
				// parent directory for now.
				escapedPath = System.IO.Path.GetDirectoryName (file.Path)
					.Replace (" ", "\\ ")
					.Replace ("'", "\\'");
				Util.Environment.Open (escapedPath);
			}
			return null;
		}
	}
}
