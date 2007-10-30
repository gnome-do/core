// UniverseManager.cs created with MonoDevelop
// User: dave at 4:01 PM 10/30/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;

using Do;
using Do.Universe;

namespace Do.Core
{
	
	
	public class UniverseManager
	{
		
		Dictionary<string, IObject[]> firstCharacterResultsCommands;
		Dictionary<string, IObject[]> firstCharacterResultsItems;
		Dictionary<string, IObject[]> firstCharacterResultsModifierItems;
	
		Dictionary<string, IObject> commandsUniverse;
		Dictionary<string, IObject> itemsUniverse;
		Dictionary<string, List<Type>> commandToItemMap;
		Dictionary<Type, List<Command>> itemToCommandMap;
		
		private List<ItemSource> itemSources;
		private SearchManager searchManager;
		
		public UniverseManager()
		{
			List<IObject> keypress_matches;
			RelevanceSorter comparer;
			SentencePositionLocator sentencePosition;
			
			itemSources = new List<ItemSource> ();
			universe = new Dictionary<IObject, bool> ();
			// commandsUniverse = new Dictionary<IObject, bool> ();
			commandToItemMap = new Dictionary<string, List<Type> > ();
			itemToCommandMap = new Dictionary<Type, List<Command> > ();
			
			foreach (ItemSource source in BuiltinItemSources) {
				itemSources.Add (source);
			}
			
			foreach (Command command in BuiltinCommands) {
				if (!(commandsUniverse.ContainsKey (command))) {
					commandsUniverse[command] = command;
					foreach (Type type in command.SupportedItemTypes) {
						List<Command> commands;
						
						itemToCommandMap.TryGetValue (type, out commands);
						if (commands == null) {
							 itemToCommandMap[type] = commands = new List<Command> ();
						}
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
					if (!(itemToCommandMap.ContainsKey (item.IItem.GetType ()))) {
						itemToCommandMap[item.IItem.GetType()] = commandListFormatted;
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
				comparer = new RelevanceSorter (keypress.ToString (),SentencePositionLocator.ModifierItem);
				firstCharacterResultsModifierItems[keypress.ToString ()] = comparer.NarrowResults (universeList).ToArray ();			
			}
		}
		
		public SearchContext ChangeSearchPosition (SearchContext searchContext, SentencePositionLocator newPosition) {
			SearchContext newSearchContext;
			newSearchContext = new SearchContext ();
			if (newPosition == SentencePositionLocator.Command) {
				if (searchContext.LastCommandContext != null) {
					newSearchContext = searchContext.LastCommandContext;
				}
				else {
					newSearchContext = searchContext.Clone ();
					newSearchContext.SearchPosition = SentencePositionLocator.Command;
					newSearchContext.LastContext = null;
					newSearchContext.Results = CommandsForItem (newSearchContext.Item).ToArray () as Command[];
				}
			}
			else if (newPosition == SentencePositionLocator.Item) {
				if (searchContext.LastItemContext != null) {
					newSearchContext = searchContext.LastItemContext;
				}
				else {
					newSearchContext = searchContext.Clone ();
					newSearchContext.SearchPosition = SentencePositionLocator.Command;
					newSearchContext.LastContext = null;
					newSearchContext.Results = null;
				}
			}
			if (searchContext.SearchPosition == SentencePositionLocator.Item) {
				newSearchContext.LastItemContext = searchContext;
			}
			else if (searchContext.SearchPosition == SentencePositionLocator.Command) {
				newSearchContext.LastCommandContext = searchContext;
			}
			else {
				newSearchContext.LastModifierItemContext = searchContext;
			}
			return newSearchContext;
		}
		
		public List<Command> CommandsForItem (Item item) {
			Type item_type;
			
			item_type = item.IItem.GetType();
			if (!itemToCommandMap.ContainsKey (item_type)) {
				itemToCommandMap[item_type] = new List<Command> ();
			}
			return itemToCommandMap[item_type];
		}
		
		public SearchContext Search (SearchContext newSearchContext)
		{
			string keypress;
			IObject[] results;
			List<Item> filtered_items;
			List<Command> filtered_commands;
			List<IObject> filtered_results;
			RelevanceSorter comparer;
			Dictionary<string, IObject[]> firstResults;
			SearchContext lastContext;

			lastContext = newSearchContext.LastContext;
			if (newSearchContext.SearchPosition == SentencePositionLocator.Command) {
				keypress = newSearchContext.CommandSearchString;
				firstResults = firstCharacterResultsCommands;
				if (newSearchContext.LastContext != null) {
					if (newSearchContext.SearchPosition != newSearchContext.LastContext.SearchPosition) {
						lastContext = newSearchContext.LastCommandContext;
					}
				}
			}
			else if (newSearchContext.SearchPosition == SentencePositionLocator.Item) {
				keypress = newSearchContext.ItemSearchString;
				firstResults = firstCharacterResultsItems;
				if (newSearchContext.LastContext != null) {
					if (newSearchContext.SearchPosition != newSearchContext.LastContext.SearchPosition) {
						lastContext = newSearchContext.LastItemContext;
					}
				}
			}
			else {
				keypress = newSearchContext.IndirectItemSearchString;
				firstResults = firstCharacterResultsModifierItems;
				if (newSearchContext.LastContext != null) {
					if (newSearchContext.SearchPosition != newSearchContext.LastContext.SearchPosition) {
						lastContext = newSearchContext.LastModifierItemContext;
					}
				}
			}
			
			// We can build on the last results.
			// example: searched for "f" then "fi"
			if (lastContext != null) {
				results = lastContext.Results;
				comparer = new RelevanceSorter (keypress, newSearchContext.SearchPosition);
				filtered_results = new List<IObject> (results);
				// Sort results based on new keypress string
				filtered_results = comparer.NarrowResults (filtered_results);
			}

			// If someone typed a single key, BOOM we're done.
			else if (firstResults.ContainsKey (keypress)) {
				results = firstResults[keypress];
				filtered_results = new List<IObject> (results);
			}

			// Or we just have to do an expensive search...
			// This is the current behavior on first keypress.
			else {
				filtered_results = new List<IObject> ();
				if (newSearchContext.SearchPosition == SentencePositionLocator.Command) {
					filtered_results.AddRange (commandsUniverse.Values);
				}
				else {
					filtered_results.AddRange (itemsUniverse.Values);
				}
				comparer = new RelevanceSorter (keypress, newSearchContext.SearchPosition);
				filtered_results.Sort (comparer);
				// Sort results based on keypress
			}

			if (newSearchContext.SearchPosition == SentencePositionLocator.Command) {
				IObject[] filteredResultsArray = filtered_results.ToArray ();
				for (int i = 0; i < filteredResultsArray.Length; i++) {
					IObject result = (filteredResultsArray[i]);
					List<System.Type> itemsForCommand;
					if (commandToItemMap.ContainsKey (((Command) result).Name + ((Command) result).Description)) {
						itemsForCommand = commandToItemMap[((Command) result).Name + ((Command) result).Description];
						if (!(itemsForCommand.Contains (newSearchContext.Item.IItem.GetType ()))) {
							filtered_results.Remove (result);
						}
					}
				}
			}

			// Now we filter the results to make sure they have
			// relevant types; if someone is searching for Items
			// we shouldn't return any Commands.			
//			foreach (IObject result in results) {
//				// Test if result is appropriate for
//				// the search context...
//				// Here we check: "if we're searcing for items.
//				// result better be an Item type."
//				if ((newSearchContext.TypeFlag & TypeFlag.Item) && !(result is IItem))
//					continue;  // Disregard the current results
//				
//				// If result passes all the tests for
//				// suitability to the current context:
//				filtered_results.Add (result); 
//			}
			if (newSearchContext.SearchPosition == SentencePositionLocator.Command) {
				filtered_commands = new List<Command> ();
				foreach (IObject iobject in filtered_results) {
					filtered_commands.Add ((Command) iobject);
				}
				newSearchContext.Results = filtered_commands.ToArray ();
			}
			else {
				filtered_items = new List<Item> ();
				foreach (IObject iobject in filtered_results) {
					filtered_items.Add ((Item) iobject);
				}
				newSearchContext.Results = filtered_items.ToArray ();
			}
			// This is a clever way to keep
			// a stack of incremental results.
			// NOTE: Clone should return a deep (enough) copy.
			// Also note - tricky pointer magic.
			SearchContext temp;
			temp = newSearchContext;
			newSearchContext = newSearchContext.Clone ();
			lastContext = temp;
			newSearchContext.LastContext = lastContext;
			
			if (newSearchContext.SearchPosition == SentencePositionLocator.Command) {
				newSearchContext.LastCommandContext = lastContext;
			}
			else if (newSearchContext.SearchPosition == SentencePositionLocator.Item) {
				newSearchContext.LastItemContext = lastContext;
				newSearchContext.LastCommandContext = null;
			}
			else {
				newSearchContext.LastModifierItemContext = lastContext;
			}
			
			return newSearchContext;
		}
		
		public static ItemSource [] BuiltinItemSources {
			get {
				return new ItemSource [] {
					new ItemSource (new ApplicationItemSource ()),
					new ItemSource (new FirefoxBookmarkItemSource ()),
					new ItemSource (new DirectoryFileItemSource ()),
					new ItemSource (new GNOMESpecialLocationsItemSource ()),	
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
				};
			}
		}
		
		private Command[] _CommandsForItem (Item item) {
			List<Command> commands;
			Console.WriteLine ("HELLLLLOOOO");
			
			commands = new List<Command> ();
			foreach (Type type in GCObject.GetAllImplementedTypes (item)) {
				Console.WriteLine ("{0} is a subtype of {1}", item.GetType(), type);
				if (itemToCommandMap.ContainsKey (type)) {
					foreach (Command command in itemToCommandMap[type] as IEnumerable<Command>) {
						if (command.SupportsItem (item.IItem)) {
							commands.Add (command);
						}
					}
				}
			}
			return commands.ToArray ();
		}
		
		public Dictionary<string, IObject> CommandsUniverse {
			get { return commandsUniverse; }
			set { commandsUniverse = value; }
		}
		
		public Dictionary<string, IObject> ItemsUniverse {
			get { return itemsUniverse; }
			set { itemsUniverse = value; }
		}
		
	}
}
