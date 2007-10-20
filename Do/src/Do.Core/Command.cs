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

using Do.Universe;

namespace Do.Core
{

	public class Command : GCObject, ICommand
	{
		public static readonly string DefaultCommandIcon = "gnome-run";
	
		protected ICommand command;
		
		public Command (ICommand command) {
			if (command == null) {
				throw new ArgumentNullException ();
			}
			this.command = command;
		}
		
		public override string Name {
			get { return command.Name; }
		}
		
		public override string Description {
			get { return command.Description; }
		}
		
		public override string Icon {
			get { return (command.Icon == null ? DefaultCommandIcon : command.Icon); }
		}
		
		public Type[] SupportedTypes {
			get { return (command.SupportedTypes == null ? new Type[0] : command.SupportedTypes); }
		}
		
		public Type[] SupportedModifierTypes {
			get { return (command.SupportedModifierTypes == null ? new Type[0] : command.SupportedModifierTypes); }
		}

		public bool SupportsItem (IItem item)
		{
			return command.SupportsItem (item);
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			IItem[] inner_items;
			IItem[] inner_modifierItems;
			
			inner_items = items.Clone () as IItem[];
			inner_modifierItems = modifierItems.Clone () as IItem[];
			
			for (int i = 0; i < items.Length; ++i) {
				if (items[i] is Item) {
					inner_items[i] = (items[i] as Item).IItem;
				}
			}
			for (int i = 0; i < modifierItems.Length; ++i) {
				if (modifierItems[i] is Item) {
					inner_modifierItems[i] = (modifierItems[i] as Item).IItem;
				}
			}
			command.Perform (inner_items, inner_modifierItems);
		}
		
	}
}
