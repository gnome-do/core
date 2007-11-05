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
	public enum SentencePositionLocator {
		Ambigious = 0,
		Command = 1,
		Item = 2,
		ModifierItem = 3
	}
	
	
	public class UniverseManager
	{
		
		Dictionary<string, IObject[]> firstCharacterResults;
	
		Dictionary<int, IObject> universe;
		
		Dictionary<int, List<Command>> itemToCommandMap;
		
		private List<ItemSource> itemSources;
		
		public UniverseManager()
		{
			List<IObject> keypress_matches;
			RelevanceSorter comparer;
			SentencePositionLocator sentencePosition;
			
			itemSources = new List<ItemSource> ();
			universe = new Dictionary<int, IObject> ();
			itemToCommandMap = new Dictionary<int,List<Command>> ();
			// commandsUniverse = new Dictionary<IObject, bool> ();

			foreach (ItemSource source in BuiltinItemSources) {
				itemSources.Add (source);
			}
			
			foreach (Command command in BuiltinCommands) {
				if (!(universe.ContainsKey (command.GetHashCode ()))) {
					universe[command.GetHashCode ()] = command;
				}
			}
					

			foreach (ItemSource source in itemSources) {
				foreach (Item item in source.Items) {
					if (!(itemToCommandMap.ContainsKey (item.GetHashCode ()))) {
						Command[] itemCommands = _CommandsForItem (item);
						List<Command> commandList = new List<Command> ();
						commandList.AddRange (itemCommands);
						itemToCommandMap.Add (item.GetHashCode (), commandList);
					}
					if (universe.ContainsKey (item.GetHashCode ())) {
					}
					else {
						universe.Add (item.GetHashCode (), item);
					}
				}
			}
			
			firstCharacterResults = new Dictionary<string, IObject[]> ();
			for (char keypress = 'a'; keypress < 'z'; keypress++) {
				List <IObject> universeList = new List<IObject> ();
				universeList.AddRange (universe.Values);
				comparer = new RelevanceSorter (keypress.ToString ());
				firstCharacterResults[keypress.ToString ()] = comparer.NarrowResults (universeList).ToArray ();
			}
		}
		
		
		public SearchContext Search (SearchContext newSearchContext)
		{
			Console.WriteLine (newSearchContext.SearchString);
			string keypress;
			IObject[] results;
			List<Item> filtered_items;
			List<Command> filtered_commands;
			List<IObject> filtered_results;
			RelevanceSorter comparer;
			Dictionary<string, IObject[]> firstResults;
			SearchContext lastContext;

			keypress = newSearchContext.SearchString.ToLower ();
			lastContext = newSearchContext.LastContext;
			
			int i = 0;
			if (newSearchContext.SearchString == "") {
				results = new IObject[universe.Values.Count];
				foreach (IObject result in universe.Values) {
					results[i] = result;
					i++;
				}
				filtered_results = new List<IObject> (results);
			}
			else {
				// We can build on the last results.
				// example: searched for "f" then "fi"
				if (lastContext != null && lastContext.SearchPosition == newSearchContext.SearchPosition) {
					results = lastContext.Results;
					comparer = new RelevanceSorter (keypress);
					filtered_results = new List<IObject> (results);
					// Sort results based on new keypress string
					filtered_results = comparer.NarrowResults (filtered_results);
				}

				// If someone typed a single key, BOOM we're done.
				else if (firstCharacterResults.ContainsKey (keypress)) {
					results = firstCharacterResults[keypress];
					filtered_results = new List<IObject> (results);
				}

				// Or we just have to do an expensive search...
				// This is the current behavior on first keypress.
				else {
					filtered_results = new List<IObject> ();
					filtered_results.AddRange (universe.Values);
					comparer = new RelevanceSorter (keypress);
					filtered_results.Sort (comparer);
					// Sort results based on keypress
				}
			}

			if (newSearchContext.SearchPosition == SentencePositionLocator.Ambigious) { 
			}
			else if (newSearchContext.SearchPosition == SentencePositionLocator.Command) {
				List<Command> commandsForItem = new List<Command> ();
				commandsForItem.AddRange (itemToCommandMap[newSearchContext.FirstObject.GetHashCode ()]);
				Console.WriteLine (newSearchContext.FirstObject.Name);
				IObject[] filtered_results_array = filtered_results.ToArray ();
				for (i = 0; i < filtered_results_array.Length; i++) {
					GCObject result = (GCObject) filtered_results_array[i];
					if (result.GetType () != typeof(Command)) {
						filtered_results.Remove (result);
					}
					else {
						Console.WriteLine (result.Name + " "+commandsForItem.Contains ((Command) result));
						if (!(commandsForItem.Contains ((Command) result))) {
							filtered_results.Remove (result);
						}
					}
				}
			}
			else {
				IObject[] filtered_results_array = filtered_results.ToArray ();
				for (i = 0; i < filtered_results_array.Length; i++) {
					GCObject result = (GCObject) filtered_results_array[i];
					if (result.GetType () != typeof(Item)) {
						filtered_results.Remove (result);
					}					
					else {
						if (!(((Command) (newSearchContext.FirstObject)).SupportsItem ((Item) result))) {
							filtered_results.Remove (result);
						}
					}
				}
			}

			newSearchContext.Results = new GCObject[filtered_results.ToArray ().Length];
			i = 0;
			foreach (GCObject gcobject in filtered_results) {
				newSearchContext.Results[i] = gcobject;
				i++;
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
			
			
			commands = new List<Command> ();
			Console.WriteLine ("Name: "+item.Name);
			foreach (IObject result in universe.Values) {
				if (result.GetType ().Equals (typeof (Command))) {
					if (((Command) result).SupportsItem (item.IItem)) {
						Console.WriteLine ("Adding "+result.Name);
						commands.Add ((Command) result);
					}
				}
			}
			return commands.ToArray ();
		}
		
	}
}
