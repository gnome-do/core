// /home/dave/trunk-md/Do/src/Do.Core/SearchManager.cs created with MonoDevelop
// User: dave at 7:22 PM 10/19/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.Collections;
using Do.Universe;

namespace Do.Core
{	
	public enum SentencePositionLocator {
		Command = 0,
		Item = 1,
		ModifierItem = 2
	}
	
	public enum SearchAction {
		Append = 0,
		Delete = 1,
		Reset = 2
	}
	
	public class SearchManager
	{
		Dictionary<string, IObject[]> firstCharacterResultsCommands;
		Dictionary<string, IObject[]> firstCharacterResultsItems;
		Dictionary<string, IObject[]> firstCharacterResultsModifierItems;
	
		Dictionary<string, IObject> commandsUniverse;
		Dictionary<string, IObject> itemsUniverse;
		Dictionary<string, System.Collections.Generic.List<System.Type>> commandToItemMap;
		Dictionary<string, System.Collections.Generic.List<Command>> itemToCommandMap;
		
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
		
		public SearchManager (Dictionary<string, IObject> newCommandsUniverse,
		                      Dictionary<string, IObject> newItemUniverse,
		                      Dictionary<string, List<System.Type>> newCommandToItemMap,
		                      Dictionary<string, List<Command>> newItemToCommandMap)
		{
			commandsUniverse = newCommandsUniverse;
			itemsUniverse = newItemUniverse;
			commandToItemMap = newCommandToItemMap;
			itemToCommandMap = newItemToCommandMap;
			List<IObject> keypress_matches;
			RelevanceSorter comparer;
			SentencePositionLocator sentencePosition;
			
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
					newSearchContext.Results = CommandsForItem (newSearchContext.Item);
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
		
		public Command[] CommandsForItem (Item item) {
			Console.WriteLine (item.IItem.GetType ());
			if (itemToCommandMap.ContainsKey (item.IItem.GetType ().ToString ())) {
				return (itemToCommandMap[item.IItem.GetType ().ToString ()].ToArray ());
			}
			else { 
				Command[] commandList = new Command[0];
				return commandList;
			}
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
	}
}
