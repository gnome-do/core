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
		
		Dictionary<string, List<IObject>> firstResults;
		Dictionary<int, IObject> universe;
		
		List<DoItemSource> doItemSources;
		List<DoCommand> doCommands;
		
		Thread indexThread;
		Mutex universeMutex;
		Mutex firstResultsMutex;
		
		const int kUpdateWaitTime = 300000;
		const int kMaxSearchResults = 1000;
		
		public UniverseManager()
		{
			universe = new Dictionary<int, IObject> ();
			doItemSources = new List<DoItemSource> ();
			doCommands = new List<DoCommand> ();
			firstResults = new Dictionary<string, List<IObject>> ();		
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

			//ThreadStart updateJob = new ThreadStart (UpdateUniverse);
			//indexThread = new Thread (updateJob);
			//indexThread.Start ();
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
			Dictionary<string, List<IObject>> updateFirstResults;
			Dictionary<int, IObject> updateUniverse;
			while (true) {
				Monitor.Enter (indexThread);
				Monitor.Wait (indexThread, kUpdateWaitTime);
				Monitor.Exit (indexThread);
				updateUniverse = new Dictionary<int, IObject> ();
				updateFirstResults = new Dictionary<string,List<IObject>> ();
				
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

		private void BuildFirstResults (Dictionary<int, IObject> startingUniverse,
		                                Dictionary<string, List<IObject>> newResults) 
		{			
			List<IObject> results;
			RelevanceSorter comparer;

			//For each starting character add every matching object from the universe to
			//the firstResults dictionary with the key of the character
			for (char keypress = 'a'; keypress < 'z'; keypress++) {
				results = new List<IObject> (startingUniverse.Values);
				comparer = new RelevanceSorter (keypress.ToString ());
				newResults[keypress.ToString ()] = comparer.NarrowResults (results);
			}
		}
		
		private void BuildUniverse (Dictionary<int, IObject> newUniverse) {
			// Hash commands.
			foreach (DoCommand command in doCommands) {
				newUniverse[command.UID.GetHashCode ()] = command;
			}
			
			// Hash items.
			foreach (DoItemSource source in doItemSources) {
				foreach (DoItem item in source.Items) {
					newUniverse[item.UID.GetHashCode ()] = item;
				}
			}
		}
		
		public SearchContext Search (SearchContext context) {
			universeMutex.WaitOne ();
			firstResultsMutex.WaitOne ();
			
			string query = context.Query.ToLower ();
			List<IObject> results = new List<IObject> ();
			SearchContext clone;
			
			//First check to see if the search context is equivalent to a lastContext or parentContext
			//Just return that context if it is
			SearchContext oldContext = context.EquivalentPreviousContextIfExists ();

			if (oldContext != null) {
				universeMutex.WaitOne ();
				firstResultsMutex.WaitOne ();
				return oldContext.GetContinuedContext ();
			}
			else if (context.FindingChildren)
			{
				universeMutex.WaitOne ();
				firstResultsMutex.WaitOne ();
				return ChildContext (context);
			}
			else if (context.FindingParent)
			{
				universeMutex.WaitOne ();
				firstResultsMutex.WaitOne ();
				return ParentContext (context);
			}
			else {
				if (context.Independent) {
					results = IndependentResults (context);
				}
				else {
					results = DependentResults (context);
				}
			}
			
			results.AddRange (AddNonUniverseItems (context));
			universeMutex.ReleaseMutex ();
			firstResultsMutex.ReleaseMutex ();

			context.Results = results.ToArray ();
			// Keep a stack of incremental results.
			return context.GetContinuedContext ();
		}
		
		private SearchContext ParentContext (SearchContext context) {
			//Since we are dealing with the parent, turn off the finding parent
			//flag
			context.FindingParent = false;
			//Check to see if parent context exists first
			if (context.ParentContext == null)
				return context;
			
			context = context.ParentContext;
			context.FindingChildren = false;
			return context;
		}
		
		private SearchContext ChildContext (SearchContext context) {
			IObject[] childObjects = new IObject[0];
			SearchContext newContext;
			context.FindingChildren = false;
			//Check to if the current object has children first			
			if (context.Results[context.Cursor] is DoItem) {
				childObjects = ChildrenOfObject (context.Results[context.Cursor]);
			}
			
			//Don't do anything if there are no children
			if (childObjects.Length == 0) {
				return context;
			}
			
			newContext = context.Clone ();
			newContext.ParentContext = context;
			newContext.Query = "";
			newContext.Results = this.AddNonUniverseItems (newContext).ToArray ();
			Array.Copy (childObjects, newContext.Results, newContext.Results.Length);
			newContext.Results = childObjects;
			newContext.FindingChildren = false;
			newContext.LastContext = new SearchContext (false);
			
			context.FindingChildren = true;
			
			return newContext.GetContinuedContext ();
		}
		
		private List<IObject> DependentResults (SearchContext context) {
			List<IObject> results;
			string query = context.Query.ToLower ();
			
			if (context.Query == "") {
				results = InitialDependentResults (context);
			}
			else {
				//If we have already have the results cached from this string, use the cache and filter out
				//what we need. However if this a child context the items might not be in the universe, and
				//if we're looking for commands it's quicker to do a item->command type lookup
				if (context.ItemsSearch && firstResults.ContainsKey (query) && context.ParentContext == null) {
					if (context.CommandSearch)
						results = GetModItemsFromList (context,
						                               new List<IObject> (firstResults[query]));
					else
					{
						results = GetItemsFromList (context,
						                            new List<IObject> (firstResults[query]));
					}
				}
				else {
					results = FilterPreviousList (context);
				}
			}
			return results;
		}
				
		private List<IObject> InitialDependentResults (SearchContext context) {
			List<IObject> results;	
			
			if (context.ModItemsSearch)
			results = GetModItemsFromList (context, 
				                               new List<IObject> (universe.Values));
			else if (context.CommandSearch)
				results = InitialCommandResults (context);
			else
			{
				results = GetItemsFromList (context,
				                            new List<IObject> (universe.Values));
			}	
			return results;
		}
			
		private List<IObject> AddNonUniverseItems (SearchContext context) {
			List<IObject> results = new List<IObject> ();
			
			//If we're on modifier items, add a text item if its supported
			if (context.ModItemsSearch) {
				if (ContainsType (context.Command.SupportedModifierItemTypes,
				                       typeof (TextItem))) {
					results.Add (new DoTextItem (context.Query));
				}
			}
			//Same if we're on items
			else if (context.ItemsSearch) {
				if (ContainsType (context.Command.SupportedItemTypes,
				                       typeof (ITextItem))) {
					results.Add (new DoTextItem (context.Query));
				}
			}
			//If independent always add a text item
			else if (context.Independent) {
				results.Add (new DoTextItem (context.Query));
			}
				
			return results;	
		}
		
		//This generates a list of modifier items supported by the context in a given initial list
		public List<IObject> GetModItemsFromList (SearchContext context, List<IObject> initialList) {
			int itemCount = 0;
			List<IObject> results = new List<IObject> ();
			
			foreach (IObject iobject in initialList) {
				if (iobject is DoItem) {
					//If the item is supported add it
					if (context.Command.SupportsModifierItemForItems (context.IItems,
				                                                  iobject as DoItem)) 
					{
						results.Add (iobject);
						itemCount++;
					}
				}
				//Make sure we don't go over the max search results.
				//Initial list should be pre-sorted, so this should only cut off least relevant
				//results
				if (itemCount == kMaxSearchResults)
					break;
			}
			return results;
		}
		
		//Same as GetModItemsFrom list but for items
		public List<IObject> GetItemsFromList (SearchContext context, List<IObject> initialList) {
			int itemCount = 0;
			List<IObject> results = new List<IObject> ();
			foreach (IObject iobject in initialList) {
				if (iobject is DoItem) {
					if (context.Command.SupportsItem (iobject as DoItem)) 
					{
						results.Add (iobject);
						itemCount++;
					}
				}
				if (itemCount == kMaxSearchResults)
					break;
			}
			return results;
		}
		
		//This will filter out the results in the previous context that match the current query
		private List<IObject> FilterPreviousList (SearchContext context) {
			string query = context.Query.ToLower ();
			RelevanceSorter comparer;
			List<IObject> results;
			
			comparer = new RelevanceSorter (query);
			results = new List<IObject> (context.LastContext.Results);
			return comparer.NarrowResults (results);
		}
		
		
		private List<IObject> IndependentResults (SearchContext context) {
			string query;
			RelevanceSorter comparer;
			List<IObject> results;
			
			query = context.Query.ToLower ();
			// We can build on the last results.
			// example: searched for "f" then "fi"
			if (context.LastContext.LastContext != null) {
				Console.WriteLine ("A");
				results = FilterPreviousList (context);
			}

			// If someone typed a single key, BOOM we're done.
			else if (firstResults.ContainsKey (query)) {
				Console.WriteLine ("B");
				results = new List<IObject> (firstResults[query]);
			}

			// Or we just have to do an expensive search...
			// This is the current behavior on first keypress.
			else {
				Console.WriteLine ("C");
				results = new List<IObject> ();
				results.AddRange (universe.Values);
				comparer = new RelevanceSorter (query);
				results.Sort (comparer);
			}
			return results;
		}
		
		//This method gives us all the commands that are supported by all the items in the list
		private List<IObject> InitialCommandResults (SearchContext context) {
			List<IObject> results = new List<IObject> ();
			bool initial = true;
			
			foreach (DoItem item in context.Items) {
				List<IObject> itemResults = new List<IObject> ();
				itemResults.AddRange (CommandsForItem (item));
				
				//If this is the first item in the list, add all of its supported commands
				if (initial) {
					results.AddRange (itemResults);
					initial = false;
				}
				
				//For every subsequent item, check every command in the pre-existing list
				//if its not supported by this item, remove it from the list
				else {
					List<IObject> filteredResults = new List<IObject> ();
					foreach (DoItem includedItem in results) {
						if (itemResults.Contains (includedItem))
							filteredResults.Add (includedItem);
					}
					results = filteredResults;
				}
			}
			return results;
		}
		
		
		//Function to determine whether a type array contains a type
		static public bool ContainsType (Type[] typeArray, Type checkType) {
			foreach (Type type in typeArray) {
				if (type.Equals (checkType))
					return true;
			}
			return false;
		}

		DoCommand[] CommandsForItem (DoItem item)
		{
			List<DoCommand> item_commands;

			item_commands = new List<DoCommand> ();
			foreach (DoCommand command in doCommands) {
				if (command.SupportsItem (item)) {
					item_commands.Add (command);
				}
			}
			return item_commands.ToArray ();
		}
		
		public IObject[] ChildrenOfObject (IObject parent)
		{
			List<IObject> children;
			
			children = new List<IObject> ();
			if (parent is DoItem) {
				foreach (DoItemSource source in doItemSources) {
					IItem iparent = (parent as DoItem).IItem;
					foreach (IItem child in source.ChildrenOfItem (iparent)) {
						children.Add (child);
					}
				}
			}
			
			return children.ToArray ();
		}
		
	}
}
