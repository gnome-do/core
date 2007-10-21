// /home/dave/trunk-md/Do/src/Do.Core/UpdateManager.cs created with MonoDevelop
// User: dave at 1:24 PM 10/20/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.Collections;
using Do.Universe;

namespace Do.Core
{
	
	public class UpdateManager
	{
		private List<ItemSource> itemSources;
		private Dictionary<string, IObject> commandsUniverse;
		private Dictionary<string, IObject> itemsUniverse;
		private Dictionary<string, System.Collections.Generic.List<Item>> commandToItemMap;
		private Hashtable commandLists;
		private SearchManager searchManager;
		
		public UpdateManager ()
		{
			itemSources = new List<ItemSource> ();
			itemsUniverse = new Dictionary<string,IObject> ();
			commandsUniverse = new Dictionary<string,IObject> ();
			commandToItemMap = new Dictionary<string,System.Collections.Generic.List<Item>> ();
			commandLists = new Hashtable ();
			foreach (ItemSource source in BuiltinItemSources) {
				itemSources.Add (source);
			}
			
			foreach (Command command in BuiltinCommands) {
				if (!(commandsUniverse.ContainsKey (command.Name))) {
					commandsUniverse.Add (command.Name, command);
					foreach (Type type in command.SupportedTypes) {
						List<Command> commands;
						if (!commandLists.ContainsKey (type)) {
							commandLists[type] = new List<Command> ();
						}
						commands = commandLists[type] as List<Command>;
						if (!commands.Contains (command)) {
							commands.Add (command);
						}
					}
				}
			}
					
			foreach (ItemSource source in itemSources) {
				foreach (Item item in source.Items) {
					if (itemsUniverse.ContainsKey (item.Name)) {
						System.Console.WriteLine (item.Name);
					}
					else {
						itemsUniverse.Add (item.Name, item);
					}
					Command[] commandList = _CommandsForItem (item);
					foreach (Command command in commandList) {
						if (commandToItemMap.ContainsKey (command.Name)) {
							List<Item> itemList = commandToItemMap[command.Name];
							itemList.Add (item);
						}
						else {
							List<Item> itemList = new List<Item> ();
							itemList.Add(item);
							commandToItemMap[command.Name] = itemList;
						}
					}
				}
			}
			foreach (string CommandName in commandToItemMap.Keys) { Console.WriteLine ("Test3: "+CommandName); }
			searchManager = new SearchManager (commandsUniverse, itemsUniverse, commandToItemMap);
		}
		
		private Command[] _CommandsForItem (Item item) {
			List<Command> commands;
			List<Type> types;
			Type baseType;
			
			commands = new List<Command> ();
			types = new List<Type> ();
			
			// Climb up the inheritance tree adding types.
			baseType = item.IItem.GetType ();
			while (baseType != typeof (object)) {
				types.Add (baseType);
				baseType = baseType.BaseType;    
			}
			// Add all implemented interfaces
			types.AddRange (item.IItem.GetType ().GetInterfaces ());
			foreach (Type type in types) {
				if (commandLists.ContainsKey (type)) {
					foreach (Command command in commandLists[type] as IEnumerable<Command>) {
						if (command.SupportsItem (item.IItem)) {
							commands.Add (command);
						}
					}
				}
			}
			return commands.ToArray ();
		}
		
		public static ItemSource [] BuiltinItemSources {
			get {
				return new ItemSource [] {
					new ItemSource (new ApplicationItemSource ()),
					new ItemSource (new FirefoxBookmarkItemSource ()),
					// Index contents of Home (~) directory to 1 level
					new ItemSource (new DirectoryFileItemSource ("~", 1)),
					// Index contents of ~/Documents to 3 levels
					new ItemSource (new DirectoryFileItemSource ("~/Documents", 3)),
					// Index contents of ~/Desktop to 1 levels
					new ItemSource (new DirectoryFileItemSource ("~/Desktop", 2)),
				};
			}
		}
		
		public static Command [] BuiltinCommands {
			get {
				return new Command [] {
					new Command (new RunCommand ()),
					new Command (new OpenCommand ()),
					new Command (new OpenURLCommand ()),
					new Command (new RunInShellCommand ()),
					new Command (new DefineWordCommand ()),
					// new Command (new VoidCommand ()),
				};
			}
		}
		
		public SearchManager SearchManager {
			get { return searchManager; }
			set { searchManager = value; }
		}
	}
}
