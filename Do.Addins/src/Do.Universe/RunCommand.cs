/* ${FileName}
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

namespace Do.Universe
{
	
	public class RunCommand : ICommand
	{
	
		public string Name {
			get { return "Run"; }
		}
		
		public string Description {
			get { return "Run an application, script, or other executable."; }
		}
		
		public string Icon {
			get { return "gnome-run"; }
		}
		
		public Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof (IRunnableItem),
				};
			}
		}
		
		public Type[] SupportedModifierItemTypes {
			get { return null; }
		}

		public bool SupportsItem (IItem item)
		{
			if (item is IRunnableItem) {
				return true;
			}
			else {
				return false;
			}
		}
		
		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return false;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			foreach (IItem item in items) {
				if (item is IRunnableItem) {
					(item as IRunnableItem).Run ();
				}
			}
		}
	
		
	}
}
