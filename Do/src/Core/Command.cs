// Command.cs created with MonoDevelop
// User: dave at 7:09 PM 8/17/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using Do.PluginLib;

namespace Do.Core
{

	public class Command : GCObject
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
		
		public Type[] SupportedIndirectTypes {
			get { return (command.SupportedIndirectTypes == null ? new Type[0] : command.SupportedIndirectTypes); }
		}

		public bool SupportsItem (IItem item)
		{
			return command.SupportsItem (item);
		}
		
		public void PerformOnItem (Item item)
		{
			command.PerformOnItem (item.IItem);
		}
		
		public void PerformOnItemWithIndirectItem (Item item, Item iitem) {
			command.PerformOnItemWithIndirectItem (item.IItem, iitem.IItem);
		}
		
	}
}
