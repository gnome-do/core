// Command.cs created with MonoDevelop
// User: dave at 7:09 PM 8/17/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using Do.PluginLib;

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
