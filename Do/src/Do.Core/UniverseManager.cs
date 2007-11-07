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
		//Add GTK dialog box, while indexing
		
		Dictionary<string, IObject[]> firstCharacterResults;
	
		// Change to univariate hash
		Dictionary<int, IObject> universe;
		
		Dictionary<Command, List<IObject>> commandToItemMap;
		
		private List<ItemSource> itemSources;
		private List<Command> all_commands;
		
		public UniverseManager()
		{
			List<IObject> keypress_matches;
			RelevanceSorter comparer;
			
			itemSources = new List<ItemSource> ();
			universe = new Dictionary<int, IObject> ();
			all_commands = new List<Command> ();
			commandToItemMap = new Dictionary<Command, List<IObject>> ();
			// commandsUniverse = new Dictionary<IObject, bool> ();

			foreach (ItemSource source in BuiltinItemSources) {
				itemSources.Add (source);
			}
			
			foreach (Command command in BuiltinCommands) {
				if (!(universe.ContainsKey (command.GetHashCode ()))) {
					universe[command.GetHashCode ()] = command;
				}
				all_commands.Add (command);
				commandToItemMap.Add (command, new List<IObject> ());
			}	

			foreach (ItemSource source in itemSources) {
				foreach (Item item in source.Items) {
					foreach (Command command in all_commands) {
						List<IObject> commandResults = commandToItemMap[command];
						List<Type> supportedItemTypes = new List<Type>
							(command.SupportedItemTypes);
						List<Type> implementedItemTypes = new List<Type>
							(GCObject.GetAllImplementedTypes (item.IItem));
						foreach (Type type in supportedItemTypes) {
							if (implementedItemTypes.Contains (type)) {
								commandResults.Add (item);
								break;
							}
						}
					}
					if (universe.ContainsKey (item.GetHashCode ())) {
					}
					else {
						universe.Add (item.GetHashCode (), item);
					}
				}
			}
			
			//Do this Load/save results to XML
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
			string keypress;
			List<IObject> filtered_results;
			RelevanceSorter comparer;
			Dictionary<string, IObject[]> firstResults;
			SearchContext lastContext;

			keypress = newSearchContext.SearchString.ToLower ();
			lastContext = newSearchContext.LastContext;
			
			int i = 0;
			if (newSearchContext.SearchString == "" && newSearchContext.FirstObject != null) {
				filtered_results = new List<IObject> ();
				if (ContainsType (newSearchContext.SearchTypes, typeof (Command))) {
					foreach (Command command in CommandsForItem (newSearchContext.FirstObject as Item)) {
						filtered_results.Add (command);
					}
				}
				else if (ContainsType (newSearchContext.SearchTypes, typeof (Item))) {
					commandToItemMap.TryGetValue (newSearchContext.FirstObject as Command, out filtered_results);
				}
			}
			else {
				// We can build on the last results.
				// example: searched for "f" then "fi"
				if (lastContext != null) {
					comparer = new RelevanceSorter (keypress);
					filtered_results = new List<IObject> (lastContext.Results);
					// Sort results based on new keypress string
					filtered_results = comparer.NarrowResults (filtered_results);
				}

				// If someone typed a single key, BOOM we're done.
				else if (firstCharacterResults.ContainsKey (keypress)) {
					filtered_results = new List<IObject> 
						(firstCharacterResults[keypress]);
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
			
			filtered_results = filterResultsByType (filtered_results, newSearchContext.SearchTypes, keypress);
			filtered_results = filterResultsByDependency(filtered_results, newSearchContext.FirstObject);

			newSearchContext.Results = filtered_results.ToArray ();
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
		
		private List<IObject> filterResultsByType (List<IObject> results, Type[] acceptableTypes, string keypress) 
		{
			List<IObject> filtered_results = new List<IObject> ();
			//Add a text item based on the key entered
			if (keypress != "") 
				results.Add (new Item (new TextItem (keypress)));
			else
				results.Add (new Item (new TextItem ("Enter Word Definition")));
			
			//Now we look through the list and add an object when it's type belongs in acceptableTypes
			foreach (IObject iobject in results) {
				List<Type> implementedTypes = GCObject.GetAllImplementedTypes (iobject);
				foreach (Type type in acceptableTypes) {
					if (implementedTypes.Contains (type)) {
						filtered_results.Add (iobject);
						break;
					}
				}
			}
			
			return filtered_results;
		}
		
		private List<IObject> filterResultsByDependency (List<IObject> results, IObject independentObject)
		{
			if (independentObject == null)
				return results;
			List <IObject> filtered_results = new List<IObject> ();
			
			
			if (independentObject is Command) {
				foreach (IObject iobject in results) {
					//If the independent object is a command, add the result if its item type is supported
					List<Type> supportedItemTypes = new List<Type>
						(((Command) independentObject).SupportedItemTypes);
					List<Type> implementedItemTypes = new List<Type>
						(GCObject.GetAllImplementedTypes ((iobject as Item).IItem));
					foreach (Type type in supportedItemTypes) {
						if (implementedItemTypes.Contains (type)) {
							filtered_results.Add (iobject);
							break;
						}
					}
				}
			}
			else if (independentObject is Item) {
				foreach (IObject iobject in results) {
					//If the ind. object is an item, run the function commands for items to see if the result is in it
					List<Command> supportedCommands = CommandsForItem (independentObject as Item);
					if (supportedCommands.Contains (iobject as Command)) {
						filtered_results.Add (iobject);
					}
				}
			}
			return filtered_results;
		}
				
		
		//Function to determine whether a type array contains a type
		private bool ContainsType (Type[] typeArray, Type checkType) {
			foreach (Type type in typeArray) {
				if (type.Equals (checkType))
					return true;
			}
			return false;
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
		
		List<Command> CommandsForItem (Item item)
		{
			List<Command> commands;

			commands = new List<Command> ();
			foreach (Command command in all_commands) {
				foreach (Type item_type in command.SupportedItemTypes) {
					if (item_type.IsAssignableFrom (item.IItem.GetType ()) 
					    && command.SupportsItem (item.IItem)) {
						commands.Add (command);
					}
				}
			}
			return commands;
		}
		
	}
}
