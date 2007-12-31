/* OpenWithCommand.cs
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
using System.Collections.Generic;

using Do.Addins;

namespace Do.Universe
{
	/// <summary>
	/// A command providing "open with..." semantics to file items.
	/// </summary>
	public class OpenWithCommand : ICommand
	{
		public OpenWithCommand ()
		{
		}
		
		public string Name
		{
			get { return "Open with..."; }
		}
		
		public string Description
		{
			get { return "Opens files in specified applications."; }
		}
		
		public string Icon
		{
			get { return "gtk-open"; }
		}
		
		public Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (IFileItem),
				};
			}
		}
		
		public Type[] SupportedModifierItemTypes
		{
			get {
				return new Type[] {
					typeof (ApplicationItem),
				};
			}
		}

		public bool SupportsItem (IItem item)
		{
			return true;
		}
		
		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			// This is too strict - many desktop files are incomplete,
			// so MimeTypes is not reliable.
			/* 
			return items[0] is FileItem &&
							(modItem as ApplicationItem).MimeTypes.Contains (
								(items[0] as FileItem).MimeType);
			*/
			return true;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			List<string> uris;

			if (modifierItems.Length == 0) return;

			uris = new List<string> ();
			foreach (IItem item in items) {
				uris.Add ((item as IFileItem).URI);
			}
			(modifierItems[0] as ApplicationItem).RunWithURIs (uris);
		}
	}
}
