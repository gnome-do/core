// UniverseManager.cs created with MonoDevelop
// User: dave at 4:01 PM 10/30/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Do
{
	
	
	public class UniverseManager
	{
		Dictionary<string, IObject[]> firstCharacterResultsCommands;
		Dictionary<string, IObject[]> firstCharacterResultsItems;
		Dictionary<string, IObject[]> firstCharacterResultsModifierItems;
	
		Dictionary<string, IObject> commandsUniverse;
		Dictionary<string, IObject> itemsUniverse;
		Dictionary<string, System.Collections.Generic.List<System.Type>> commandToItemMap;
		Dictionary<string, System.Collections.Generic.List<Command>> itemToCommandMap;
		
		public UniverseManager()
		{
			itemSources = new List<ItemSource> ();
			itemsUniverse = new Dictionary<string,IObject> ();
			commandsUniverse = new Dictionary<string,IObject> ();
			commandToItemMap = new Dictionary<string,System.Collections.Generic.List<System.Type>> ();
			itemToCommandMap = new Dictionary<string,System.Collections.Generic.List<Command>> ();
			commandLists = new Hashtable ();
			List<IObject> keypress_matches;
			RelevanceSorter comparer;
			SentencePositionLocator sentencePosition;
			
			foreach (ItemSource source in BuiltinItemSources) {
				itemSources.Add (source);
			}
			
			foreach (Command command in BuiltinCommands) {
				if (!(commandsUniverse.ContainsKey (command.Name+command.Description))) {
					commandsUniverse.Add (command.Name+command.Description, command);
					foreach (Type type in command.SupportedItemTypes) {
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
					if (itemsUniverse.ContainsKey (item.Name+item.Description)) {
					}
					else {
						itemsUniverse.Add (item.Name+item.Description, item);
					}
					Command[] commandList = _CommandsForItem (item);
					List<Command> commandListFormatted = new List<Command> ();
					commandListFormatted.AddRange (commandList);
					if (!(itemToCommandMap.ContainsKey (item.IItem.GetType ().ToString ()))) {
						itemToCommandMap.Add (item.IItem.GetType ().ToString (), commandListFormatted);
					}
					foreach (Command command in commandList) {
						if (commandToItemMap.ContainsKey (command.Name+command.Description)) {
							List<System.Type> itemList = commandToItemMap[command.Name+command.Description];
							itemList.Add (item.IItem.GetType ());
						}
						else {
							List<System.Type> itemList = new List<System.Type> ();
							itemList.Add(item.IItem.GetType ());
							commandToItemMap[command.Name+command.Description] = itemList;
						}
					}
				}
			}
			
			firstCharacterResultsCommands = new Dictionary<string, IObject[]> ();
			sentencePosition = SentencePositionLocator.Command;
			for (char keypress = 'a'; keypress < 'z'; keypress++) {
				List <IObject> universeList = new List<IObject> ();
				universeList.AddRange (commandsUniverse.Values);
				comparer = new RelevanceSorter (keypress.ToString (), SentencePositionLocator.Command);
				firstCharacterResultsCommands[keypress.ToString ()] = comparer.NarrowResults (universeList).ToArray ();
			}
			
			firstCharacterResultsItems = new Dictionary<string, IObject[]> ();
			sentencePosition = SentencePositionLocator.Item;
			for (char keypress = 'a'; keypress < 'z'; keypress++) {
				List <IObject> itemUniverseList = new List<IObject> ();
				itemUniverseList.AddRange (itemsUniverse.Values);
				comparer = new RelevanceSorter (keypress.ToString (), SentencePositionLocator.Item);
				firstCharacterResultsItems[keypress.ToString ()] = comparer.NarrowResults (itemUniverseList).ToArray ();
			}
			
			firstCharacterResultsModifierItems = new Dictionary<string, IObject[]> ();
			sentencePosition = SentencePositionLocator.ModifierItem;
			for (char keypress = 'a'; keypress < 'z'; keypress++) {
				List <IObject> universeList = new List<IObject> ();
				universeList.AddRange (itemsUniverse.Values);
				comparer = new RelevanceSorter (keypress.ToString (), SentencePositionLocator.ModifierItem);
				firstCharacterResultsModifierItems[keypress.ToString ()] = comparer.NarrowResults (universeList).ToArray ();			
			}
		}
		
		public Dictionary<string, IObject> CommandsUniverse {
			get { return commandsUniverse; }
			set { commandsUniverse = value; }
		}
		
		public Dictionary<string, IObject> ItemsUniverse {
			get { return itemsUniverse; }
			set { itemsUniverse = value; }
		}
		
		public Dictionary<string, List<System.Type>> CommandToItemMap {
			get { return commandToItemMap; }
			set { commandToItemMap = value; }
		}
		
		public Dictionary<string, List<Command>> ItemToCommandMap {
			get { return itemToCommandMap; }
			set { itemToCommandMap = value; }
		}
	}
}
