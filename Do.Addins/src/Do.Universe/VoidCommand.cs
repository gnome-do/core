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
	public class VoidCommand : ICommand
	{
		public string Name
		{
			get { return "Do Nothing"; }
		}
		
		public string Description
		{
			get { return "Does absolutely nothing."; }
		}
		
		public string Icon
		{
			get { return "gtk-stop"; }
		}
		
		public Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (IItem)
				};
			}
		}
		
		public Type[] SupportedModifierItemTypes
		{
			get {
				return new Type[] {
					typeof (IItem)
				};
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
		}
	}
}
