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
using System.Threading;

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
		
		Thread indexThread;
		Mutex universeMutex;
		Mutex firstResultsMutex;
		
		const int kUpdateWaitTime = 300000;
		
		public UniverseManager()
		{
			universe = new Dictionary<int, IObject> ();
			doItemSources = new List<DoItemSource> ();
			doCommands = new List<DoCommand> ();
			firstResults = new Dictionary<string, IObject[]> ();
			
			universeMutex = new Mutex ();
			firstResultsMutex = new Mutex ();
			LoadBuiltins ();
			LoadAddins ();
			universeMutex.WaitOne ();
			BuildUniverse (universe);
			firstResultsMutex.WaitOne ();
			BuildFirstResults (universe, firstResults);
			universeMutex.ReleaseMutex ();
			firstResultsMutex.ReleaseMutex ();

			ThreadStart updateJob = new ThreadStart (UpdateUniverse);
			indexThread = new Thread (updateJob);
			indexThread.Start ();
		}
		
		public void KillIndexThread () {
			indexThread.Abort ();
		}
		
		public void AwakeIndexThread () {
			Monitor.Enter (indexThread);
			Monitor.Pulse (indexThread);
			Monitor.Exit (indexThread);
		}
		
		public void UpdateUniverse ()
		{
			Dictionary<string, IObject[]> updateFirstResults;
			Dictionary<int, IObject> updateUniverse;
			while (true) {
				Monitor.Enter (indexThread);
				Monitor.Wait (indexThread, kUpdateWaitTime);
				Monitor.Exit (indexThread);
				updateUniverse = new Dictionary<int, IObject> ();
				updateFirstResults = new Dictionary<string,IObject[]> ();
				
				LoadBuiltins ();
				LoadAddins ();
				BuildUniverse (updateUniverse);
				//Possible idea to implement: when updating the universe check to see if
				//any objects that have non-zero relevance with each character string were added.
				//Then pass the array of non-valid characters to the BuildFirstResults method, to avoid
				//unnecessary work. For example if the only new object was called Art, there is no need
				//to re-index the cache for anything else besides characters 'A', 'R' and 'T'
				BuildFirstResults (updateUniverse, updateFirstResults);

				universeMutex.WaitOne ();
				universe = updateUniverse;
				universeMutex.ReleaseMutex ();
				
				firstResultsMutex.WaitOne ();
				firstResults = updateFirstResults;
				firstResultsMutex.ReleaseMutex ();
			}
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

		private void BuildFirstResults (Dictionary<int, IObject> builtUniverse,
		                                Dictionary<string, IObject[]> resultsToIndex) 
		{			
			List<IObject> results;
			RelevanceSorter comparer;

			//For each starting character add every matching object from the universe to
			//the firstResults dictionary with the key of the character
			for (char keypress = 'a'; keypress < 'z'; keypress++) {
				results = new List<IObject> (builtUniverse.Values);
				comparer = new RelevanceSorter (keypress.ToString ());
				resultsToIndex[keypress.ToString ()] = comparer.NarrowResults (results).ToArray ();
			}
		}
		
		private void BuildUniverse (Dictionary<int, IObject> universeToBuild) {
			// Hash items.
			foreach (DoItemSource source in doItemSources) {
				foreach (DoItem item in source.Items) {
					universeToBuild[item.GetHashCode ()] = item;
				}
			}
			// Hash commands.
			foreach (DoCommand command in doCommands) {
				universeToBuild[command.GetHashCode ()] = command;
			}
		}
		
		public SearchContext Search (SearchContext context)
		{
			universeMutex.WaitOne ();
			firstResultsMutex.WaitOne ();
			
			List<IObject> results;
		
			// Get the results based on the search string
			results = GenerateUnfilteredList (context);
			// Filter results based on the required type
			results = FilterResultsByType (results, context.SearchTypes);
			// Filter results based on object dependencies
			results = FilterResultsByCommandDependency (results, context.Command);
			context.Results = results.ToArray ();
			
			universeMutex.ReleaseMutex ();
			firstResultsMutex.ReleaseMutex ();

			// Keep a stack of incremental results.
			SearchContext clone;
			clone = context.Clone ();
			clone.LastContext = context;
			return clone;
		}
		
		private List<IObject> GenerateUnfilteredList (SearchContext context) 
		{
			string query;
			RelevanceSorter comparer;
			List<IObject> results;
			
			query = context.SearchString.ToLower ();
		
			// Special handling for commands that take only text as an item:
			if (context.ContainsCommand () && !(context.ContainsSecondObject ())
			    && (context.Command as DoCommand).AcceptsOnlyText) {
				results = new List<IObject> ();
				results.Add (new DoItem (new TextItem (query == "" ? "Enter Text" : context.SearchString)));
				return results;
			}
			//If this is the initial search for the all the corresponding items/commands for the first object
			/// we don't need to filter based on search string
			else if (context.SearchString == "" && context.ContainsFirstObject ()) {
				results = new List<IObject> ();
				
				//If command, we have to grab the set of commands that are valid for all items in our list
				if (ContainsType (context.SearchTypes, typeof (ICommand))) {
					bool firstList = true;
					//Iterate through the item list in SearchContext
					foreach (DoItem item in context.Items) {
						List<IObject> newResults = new List<IObject> ();						
						//Insert all the commands for that item into a list
						foreach (DoCommand command in CommandsForItem (item)) {
							newResults.Add (command);
						}
						//If this is the first item in the list set the command results to the commands for that item
						if (firstList) {
							results.AddRange (newResults);
							firstList = false;
						}
						//If this is not the first item, iterate through all the commands for the items and if its
						//not in the results list for the previous items, remove it from the new results list
						//We are trying to get the intersection of the set of all of these commands.
						else {
							foreach (DoCommand command in newResults) {
								if (!(results.Contains (command))) {
									newResults.Remove (command);
								}
							}
							results = newResults;
						}
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
				if (context.LastContext != null) {
					comparer = new RelevanceSorter (query);
					results = new List<IObject> (context.LastContext.Results);
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
			results.Add (new DoItem (new TextItem (context.SearchString)));
			
			return results;
		}
			
		private List<IObject> FilterResultsByType (List<IObject> results, Type[] acceptableTypes) 
		{
			List<IObject> new_results;
			
			new_results = new List<IObject> ();			
			//Now we look through the list and add an object when its type belongs in acceptableTypes
			foreach (IObject result in results) {
				List<Type> implementedTypes = DoObject.GetAllImplementedTypes (result);
				foreach (Type type in acceptableTypes) {
					if (implementedTypes.Contains (type)) {
						new_results.Add (result);
						break;
					}
				}
			}
			return new_results;
		}
		
		private List<IObject> FilterResultsByCommandDependency (List<IObject> results, DoCommand constraint)
		{
			List <IObject> filtered_results;
			
			if (constraint == null) return results;
			filtered_results = new List<IObject> ();
			
			foreach (DoItem item in results) {
				// If the constraint is a DoCommand, add the result if it's supported.
				if ((constraint as DoCommand).SupportsItem (item)) {
					filtered_results.Add (item);
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