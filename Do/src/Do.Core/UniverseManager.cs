/* UniverseManager.cs
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
using System.Reflection;
using System.Collections.Generic;

using Do;
using Do.Universe;

namespace Do.Core
{	
	
	public class UniverseManager
	{
		
		Dictionary<string, IObject[]> firstResults;
		Dictionary<int, IObject> universe;
		
		List<DoItemSource> doItemSources;
		List<DoCommand> doCommands;
		
		public UniverseManager()
		{
			universe = new Dictionary<int, IObject> ();
			doItemSources = new List<DoItemSource> ();
			doCommands = new List<DoCommand> ();
			firstResults = new Dictionary<string, IObject[]> ();
			
			LoadBuiltins ();
			LoadAddins ();
			BuildUniverse ();
			BuildFirstResults ();	
		}

		protected void LoadBuiltins ()
		{
			LoadAssembly (typeof (IItem).Assembly);
		}

		protected void LoadAddins ()
		{
			List<string> addin_dirs;
			
			addin_dirs = new List<string> ();
			addin_dirs.Add ("~/.do/addins".Replace ("~",
				   Environment.GetFolderPath (Environment.SpecialFolder.Personal)));
			
			foreach (string addin_dir in addin_dirs) {
				string[] files;
				
				files = null;
				try {
					files = System.IO.Directory.GetFiles (addin_dir);
				} catch (Exception e) {
					Log.Error ("Could not read addins directory {0}: {1}", addin_dir, e.Message);
					continue;
				}
				
				foreach (string file in files) {
					Assembly addin;
					
					if (!file.EndsWith (".dll")) continue;
					try {
						addin = Assembly.LoadFile (file);
						LoadAssembly (addin);
					} catch (Exception e) {
						Log.Error ("Do encountered and error while trying to load addin {0}: {1}", file, e.Message);
						continue;
					}
				}
			}
		}
		
		private void LoadAssembly (Assembly addin)
		{
			if (addin == null) return;
			
			foreach (Type type in addin.GetTypes ()) {
				if (type.IsAbstract) continue;
				if (type == typeof(VoidCommand)) continue;
				
				foreach (Type iface in type.GetInterfaces ()) {
					if (iface == typeof (IItemSource)) {
						IItemSource source;
						
						source = System.Activator.CreateInstance (type) as IItemSource;
						doItemSources.Add (new DoItemSource (source));
						Log.Info ("Successfully loaded \"{0}\" Item Source.", source.Name);
					}
					if (iface == typeof (ICommand)) {
						ICommand command;
						
						command = System.Activator.CreateInstance (type) as ICommand;
						doCommands.Add (new DoCommand (command));
						Log.Info ("Successfully loaded \"{0}\" Command.", command.Name);
					}
				}
			}
		}

		private void BuildFirstResults () 
		{			
			List<IObject> results;
			RelevanceSorter comparer;

			//For each starting character add every matching object from the universe to
			//the firstResults dictionary with the key of the character
			for (char keypress = 'a'; keypress < 'z'; keypress++) {
				results = new List<IObject> (universe.Values);
				comparer = new RelevanceSorter (keypress.ToString ());
				firstResults[keypress.ToString ()] = comparer.NarrowResults (results).ToArray ();
			}
		}
		
		private void BuildUniverse () {
			// Hash items.
			foreach (DoItemSource source in doItemSources) {
				foreach (DoItem item in source.Items) {
					universe[item.GetHashCode ()] = item;
				}
			}
			// Hash commands.
			foreach (DoCommand command in doCommands) {
				universe[command.GetHashCode ()] = command;
			}
		}
		
		public SearchContext Search (SearchContext newSearchContext)
		{
			string keypress = newSearchContext.SearchString;
			List<IObject> filtered_results;
			SearchContext lastContext = newSearchContext.LastContext;
		
			//Get the results based on the search string
			filtered_results = GenerateUnfilteredList (newSearchContext);
			//Filter results based on the required type
			filtered_results = filterResultsByType (filtered_results, newSearchContext.SearchTypes, keypress);
			//Filter results based on object dependencies
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
		
		private List<IObject> GenerateUnfilteredList (SearchContext context) 
		{
			string query;
			RelevanceSorter comparer;
			SearchContext lastContext;
			List<IObject> results;
			
			query = context.SearchString.ToLower ();
			lastContext = context.LastContext;
		
			//If this is the initial search for the all the corresponding items/commands for the first object
			/// we don't need to filter based on search string
			if (context.SearchString == "" && context.FirstObject != null) {
				results = new List<IObject> ();
				//If command, just grab the commands for the item
				if (ContainsType (context.SearchTypes, typeof (ICommand))) {
					foreach (DoCommand command in CommandsForItem (context.FirstObject as DoItem)) {
						results.Add (command);
					}
				}
				//If item, use the command to item map
				else if (ContainsType (context.SearchTypes, typeof (IItem))) {
					results.AddRange (universe.Values);
				}
			}
			else {
				// We can build on the last results.
				// example: searched for "f" then "fi"
				if (lastContext != null) {
					comparer = new RelevanceSorter (query);
					results = new List<IObject> (lastContext.Results);
					results = comparer.NarrowResults (results);
				}

				// If someone typed a single key, BOOM we're done.
				else if (firstResults.ContainsKey (query)) {
					results = new List<IObject> (firstResults[query]);
					
				}

				// Or we just have to do an expensive search...
				// This is the current behavior on first keypress.
				else {
					results = new List<IObject> ();
					results.AddRange (universe.Values);
					comparer = new RelevanceSorter (query);
					results.Sort (comparer);
				}
			}
			return results;
		}
			
		
		private List<IObject> filterResultsByType (List<IObject> results, Type[] acceptableTypes, string keypress) 
		{
			List<IObject> new_results;
			
			new_results = new List<IObject> ();
			//Add a text item based on the key entered
			if (keypress != "") 
				results.Add (new DoItem (new TextItem (keypress)));
			else
				results.Add (new DoItem (new TextItem ("Enter Word Definition")));
			
			//Now we look through the list and add an object when its type belongs in acceptableTypes
			foreach (IObject iobject in results) {
				List<Type> implementedTypes = DoObject.GetAllImplementedTypes (iobject);
				foreach (Type type in acceptableTypes) {
					if (implementedTypes.Contains (type)) {
						new_results.Add (iobject);
						break;
					}
				}
			}
			return new_results;
		}
		
		private List<IObject> filterResultsByDependency (List<IObject> results, IObject independentObject)
		{
			if (independentObject == null)
				return results;
			List <IObject> filtered_results = new List<IObject> ();
			
			
			if (independentObject is DoCommand) {
				DoCommand cmd;
				
				cmd = independentObject as DoCommand;
				foreach (DoItem item in results) {
					//If the independent object is a command, add the result if its item type is supported
					foreach (Type supported_type in cmd.SupportedItemTypes) {
						if (supported_type.IsAssignableFrom (item.IItem.GetType ()) && cmd.SupportsItem (item)) {
							filtered_results.Add (item);
						}
					}
				}
			}
			else if (independentObject is DoItem) {
				foreach (IObject iobject in results) {
					//If the ind. object is an item, run the function commands for items to see if the result is in it
					List<DoCommand> supportedCommands = CommandsForItem (independentObject as DoItem);
					if (supportedCommands.Contains (iobject as DoCommand)) {
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

		List<DoCommand> CommandsForItem (DoItem item)
		{
			List<DoCommand> item_commands;

			item_commands = new List<DoCommand> ();
			foreach (DoCommand command in doCommands) {
				if (command.SupportsItem (item)) {
					item_commands.Add (command);
				}
			}
			return item_commands;
		}
		
	}
}
