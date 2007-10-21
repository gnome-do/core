/* OpenCommand.cs
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
	
	/// <summary>
	/// A command providing "open" semantics to many kinds of items.
	/// </summary>
	public class OpenCommand : ICommand
	{
	
		public OpenCommand ()
		{
		}
		
		public string Name {
			get { return "Open"; }
		}
		
		public string Description {
			get { return "Opens many kinds of items."; }
		}
		
		public string Icon {
			get { return "gtk-open"; }
		}
		
		public Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof (IOpenableItem),
					typeof (IURIItem),
				};
			}
		}
		
		public Type[] SupportedModifierItemTypes {
			get {
				return null;
			}
		}

		public bool SupportsItem (IItem item)
		{
			return true;
		}
		
		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return false;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			string open_item;
			string error_message;
			
			open_item = null;
			foreach (IItem item in items) {
				if (item is IOpenableItem) {
					(item as IOpenableItem).Open ();
					continue;
				}
				else if (item is IURIItem) {
					open_item = (item as IURIItem).URI;
				}
				Util.Environment.Open (open_item, out error_message);
			}
		}
		
	}
}
