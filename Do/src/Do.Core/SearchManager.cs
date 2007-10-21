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
		Dictionary<string, System.Collections.Generic.List<Item>> commandToItemMap;
		
		SearchContext lastCommandContext;
		SearchContext lastItemContext;
		SearchContext lastModifierItemContext;
		
		public Dictionary<string, IObject> CommandsUniverse {
			get { return commandsUniverse; }
			set { commandsUniverse = value; }
		}
		
		public Dictionary<string, IObject> ItemsUniverse {
			get { return itemsUniverse; }
			set { itemsUniverse = value; }
		}
		
		public Dictionary<string, List<Item>> CommandToItemMap {
			get { return commandToItemMap; }
			set { commandToItemMap = value; }
		}
		
		public SearchManager (Dictionary<string, IObject> newCommandsUniverse,
		                      Dictionary<string, IObject> newItemUniverse,
		                      Dictionary<string, List<Item>> newCommandToItemMap)
		{
			commandsUniverse = newCommandsUniverse;
			itemsUniverse = newItemUniverse;
			commandToItemMap = newCommandToItemMap;
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
		
		
		
		public SearchContext DeleteLastSearchCharacter (SearchContext newContext)
		{
			SearchContext deletedContext;
			if (newContext.SearchPosition == SentencePositionLocator.Command) {
				deletedContext = lastCommandContext;
				if (lastCommandContext != null) {
					lastCommandContext = lastCommandContext.LastContext;
				}
			}
			else if (newContext.SearchPosition == SentencePositionLocator.Item) {
				deletedContext = lastItemContext;
				if (lastItemContext != null) {
					lastItemContext = lastItemContext.LastContext;
				}
			}
			else {
				deletedContext = lastModifierItemContext;
				if (lastModifierItemContext != null) {
					lastModifierItemContext = lastItemContext.LastContext;
				}
			}
			return deletedContext;
		}
		
		public SearchContext Search (SearchContext newSearchContext)
		{
			System.Console.WriteLine ("Flag A");
			string keypress;
			IObject[] results;
			List<Item> filtered_items;
			List<Command> filtered_commands;
			List<IObject> filtered_results;
			RelevanceSorter comparer;
			Dictionary<string, IObject[]> firstResults;
			Console.WriteLine ("Keypress: " + newSearchContext.ItemSearchString);
			SearchContext lastContext;

			if (newSearchContext.SearchPosition == SentencePositionLocator.Command) {
				Console.WriteLine ("FlagA1");
				keypress = newSearchContext.CommandSearchString;
				firstResults = firstCharacterResultsCommands;
				lastContext = lastCommandContext;
			}
			else if (newSearchContext.SearchPosition == SentencePositionLocator.Item) {
				Console.WriteLine ("FlagA2");
				keypress = newSearchContext.ItemSearchString;
				firstResults = firstCharacterResultsItems;
				lastContext = lastItemContext;
			}
			else {
				Console.WriteLine ("FlagA3");
				keypress = newSearchContext.IndirectItemSearchString;
				firstResults = firstCharacterResultsModifierItems;
				lastContext = lastModifierItemContext;
			}
			
			System.Console.WriteLine ("Flag B "+keypress);
			
			// We can build on the last results.
			// example: searched for "f" then "fi"
			if (lastContext != null) {
				System.Console.WriteLine ("Flag C1");
				results = lastContext.Results;
				comparer = new RelevanceSorter (keypress, newSearchContext.SearchPosition);
				filtered_results = new List<IObject> (results);
				// Sort results based on new keypress string
				filtered_results = comparer.NarrowResults (filtered_results);
			}

			// If someone typed a single key, BOOM we're done.
			else if (firstResults.ContainsKey (keypress)) {
				System.Console.WriteLine ("Flag C2");
				results = firstResults[keypress];
				filtered_results = new List<IObject> (results);
			}

			// Or we just have to do an expensive search...
			// This is the current behavior on first keypress.
			else {
				System.Console.WriteLine ("Flag C3");
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

			Console.WriteLine ("Flag D");
			if (newSearchContext.SearchPosition == SentencePositionLocator.Command) {
				IObject[] filteredResultsArray = filtered_results.ToArray ();
				for (int i = 0; i < filteredResultsArray.Length; i++) {
					System.Console.WriteLine (filteredResultsArray[i].Name);
					IObject result = (filteredResultsArray[i]);
					List<Item> itemsForCommand;
					if (commandToItemMap.ContainsKey (((Command) result).Name)) {
						itemsForCommand = commandToItemMap[((Command) result).Name];
						if (!(itemsForCommand.Contains (newSearchContext.Item))) {
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
			Console.WriteLine ("Flag E");
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
			Console.WriteLine ("Flag F");
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
				lastCommandContext = lastContext;
			}
			else if (newSearchContext.SearchPosition == SentencePositionLocator.Item) {
				lastItemContext = lastContext;
			}
			else {
				lastModifierItemContext = lastContext;
			}
			
			Console.WriteLine ("Flag G");
			return newSearchContext;
		}
	}
}
