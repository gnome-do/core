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
		// How long between update events (seconds).
		const int UpdateInterval = 60;
		// Maximum amount of time to spend updating (millseconds).
		const int MaxUpdateTime = 250;

		Dictionary<string, List<IObject>> firstResults;
		Dictionary<int, IObject> universe;

		List<DoItemSource> doItemSources;
		List<DoCommand> doCommands;

		// Keep track of next data structures to update.
		int itemSourceCursor;
		int firstResultsCursor;

		public UniverseManager()
		{
			universe = new Dictionary<int, IObject> ();
			doItemSources = new List<DoItemSource> ();
			doCommands = new List<DoCommand> ();
			firstResults = new Dictionary<string, List<IObject>> ();
			itemSourceCursor = firstResultsCursor = 0;
		}

		internal void Initialize ()
		{
			LoadBuiltins ();
			LoadAddins ();
			BuildUniverse ();
			BuildFirstResults ();

			GLib.Timeout.Add (UpdateInterval * 1000, new GLib.TimeoutHandler (OnTimeoutUpdate));
		}

		bool OnTimeoutUpdate ()
		{
			DateTime then;
			int t_update;
			
			if (Do.Controller.MainWindow.Visible) return true;

			// Keep track of the total time (in ms) we have spend updating.
			// We spend half of MaxUpdateTime updating item sources, then
			// another half of MaxUpdateTime updating first results lists.
			t_update = 0;
			while (t_update < MaxUpdateTime / 2) {
				DoItemSource itemSource;
				ICollection<IItem> oldItems;
				Dictionary<int, DoItem> newItems;

				then = DateTime.Now;
				itemSourceCursor = (itemSourceCursor + 1) % doItemSources.Count;
				itemSource = doItemSources[itemSourceCursor];
				newItems = new Dictionary<int, DoItem> ();
				// Remember old items.
				oldItems = itemSource.Items;	
				// Update the item source.
				itemSource.UpdateItems ();
				// Create a map of the new items.
				foreach (DoItem newItem in itemSource.Items) {
					newItems[newItem.GetHashCode ()] = newItem;
				}
				// Update the universe by either updating items, adding new items,
				// or removing items.
				foreach (DoItem newItem in itemSource.Items) {
					if (universe.ContainsKey (newItem.GetHashCode ()) &&
							universe[newItem.GetHashCode ()] is DoItem) {
						// We're updating an item. This updates the item across all
						// first results lists.
						(universe[newItem.GetHashCode ()] as DoItem).Inner = newItem.Inner;
					} else {
						// We're adding a new item. It might take a few minutes to show
						// up in all results lists.
						universe[newItem.GetHashCode ()] = newItem;
					}
				}
				// See if there are any old items that didn't make it into the
				// set of new items. These items need to be removed from the universe.
				foreach (DoItem oldItem in oldItems) {
					if (!newItems.ContainsKey (oldItem.GetHashCode ()) &&
							universe.ContainsKey (oldItem.GetHashCode ())) {
						universe.Remove (oldItem.GetHashCode ());
					}
				}
				Log.Info ("Updated \"{0}\" Item Source.", itemSource.Name);
				t_update += (DateTime.Now - then).Milliseconds;
			}

			// Updating a first results list takes about 50ms at most, so we can afford
			// to update a couple of them.
			t_update = 0;
			while (t_update < MaxUpdateTime / 2) {
				DoObjectRelevanceSorter sorter;
				string firstResultKey = null;
				int currentFirstResultsList = 0;

				then = DateTime.Now;
				firstResultsCursor = (firstResultsCursor + 1) % firstResults.Count;
				// Now pick a first results list to update.
				foreach (KeyValuePair<string, List<IObject>> keyval in firstResults) {
					if (currentFirstResultsList == firstResultsCursor) {
						firstResultKey = keyval.Key;
						break;
					}
					currentFirstResultsList++;
				}
				sorter = new DoObjectRelevanceSorter (firstResultKey);
				if (firstResults.ContainsKey (firstResultKey)) {
					firstResults.Remove (firstResultKey);
				}
				firstResults[firstResultKey] = sorter.SortAndNarrowResults (universe.Values);
				Log.Info ("Updated first results for '{0}'.", firstResultKey);
				t_update += (DateTime.Now - then).Milliseconds;
			}
			return true;
		}

		internal ICollection<DoItemSource> ItemSources
		{
			get { return doItemSources.AsReadOnly (); }
		}

		protected void LoadBuiltins ()
		{
			// Load from Do.Addins asembly.
			LoadAssembly (typeof (IItem).Assembly);
			// Load from main application assembly.
			LoadAssembly (typeof (DoItem).Assembly);
		}

		protected void LoadAddins ()
		{
			List<string> addin_dirs;

			addin_dirs = new List<string> ();
			addin_dirs.Add ("~/.do/plugins".Replace ("~",
			                Environment.GetFolderPath (Environment.SpecialFolder.Personal)));

			foreach (string addin_dir in addin_dirs) {
				string[] files;

				files = null;
				try {
					files = System.IO.Directory.GetFiles (addin_dir);
				} catch (Exception e) {
					Log.Error ("Could not read plugins directory {0}: {1}", addin_dir, e.Message);
					continue;
				}

				foreach (string file in files) {
					Assembly addin;

					if (!file.EndsWith (".dll")) continue;
					try {
						addin = Assembly.LoadFile (file);
						LoadAssembly (addin);
					} catch (Exception e) {
						Log.Error ("Encountered and error while trying to load plugin {0}: {1}", file, e.Message);
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
				if (type == typeof (VoidCommand)) continue;
				if (type == typeof (DoCommand)) continue;
				if (type == typeof (DoItem)) continue;

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
			DoObjectRelevanceSorter sorter;

			// For each starting character, add every matching object from the universe to
			// the firstResults list corresponding to that character.
			for (char keypress = 'a'; keypress < 'z'; keypress++) {
				sorter = new DoObjectRelevanceSorter (keypress.ToString ());
				firstResults[keypress.ToString ()] = sorter.SortAndNarrowResults (universe.Values);
			}
		}

		private void BuildUniverse ()
		{
			// Hash commands.
			foreach (DoCommand command in doCommands) {
				universe[command.UID.GetHashCode ()] = command;
			}

			// Hash items.
			foreach (DoItemSource source in doItemSources) {
				foreach (DoItem item in source.Items) {
					universe[item.UID.GetHashCode ()] = item;
				}
			}
		}

		public void Search (ref SearchContext context)
		{
			List<IObject> results = new List<IObject> ();

			// First check to see if the search context is equivalent to the
			// lastContext or parentContext. Just return that context if it is.
			SearchContext oldContext = context.EquivalentPreviousContextIfExists ();
			if (oldContext != null) {
				context = oldContext.GetContinuedContext ();
				return;
			}
			else if (context.FindingChildren) {
				context = ChildContext (context);
				return;
			}
			else if (context.FindingParent) {
				context = ParentContext (context);
				return;
			}
			else {
				if (context.Independent) {
					results = IndependentResults (context);
				} else {
					results = DependentResults (context);
				}
			}
			results.AddRange (AddNonUniverseItems (context));

			context.Results = results.ToArray ();
			// Keep a stack of incremental results.
			context = context.GetContinuedContext ();
		}

		private SearchContext ParentContext (SearchContext context)
		{
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

		private SearchContext ChildContext (SearchContext context)
		{
			List<IItem> children;
			SearchContext newContext;

			// Check to if the current object has children first
			if (context.Selection is IItem) {
				children = ChildrenOfItem (context.Selection as IItem);
			} else {
				children = new List<IItem> ();
			}

			// Don't do anything if there are no children
			if (children.Count == 0) {
				context.FindingChildren = false;
				return context;
			}

			newContext = context.Clone ();
			newContext.ParentContext = context;
			newContext.Query = "";
			newContext.Results = children.ToArray ();
			newContext.FindingChildren = false;
			newContext.LastContext = new SearchContext (false);

			context.FindingChildren = true;
			return newContext.GetContinuedContext ();
		}

		private List<IObject> DependentResults (SearchContext context)
		{
			List<IObject> results = null;
			string query = context.Query.ToLower ();

			if (context.CommandSearch && context.Query == "") {
				return InitialCommandResults (context);
			}
			else if (context.LastContext.LastContext != null) {
				return FilterPreviousSearchResultsWithContinuedContext (context);
			}

			// After this, we're only searching for items or mod items in a new
			// search.
			if (firstResults.ContainsKey (query))
				results = new List<IObject> (firstResults[query]);
			else
				results = new List<IObject> (universe.Values);

			// Filter the results appropriately.
			if (context.ModItemsSearch)
				results = GetModItemsFromList (context, results);
			else // These are items:
				results = GetItemsFromList (context, results);

			return results;
		}

		private List<IObject> AddNonUniverseItems (SearchContext context)
		{
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

		// This generates a list of modifier items supported by the context in a given initial list.
		public List<IObject> GetModItemsFromList (SearchContext context, List<IObject> initialList)
		{
			List<IObject> results = new List<IObject> ();
			IItem[] items = context.Items.ToArray ();

			foreach (IObject iobject in initialList) {
				if (iobject is IItem) {
					// If the item is supported add it
					if (context.Command.SupportsModifierItemForItems (items, iobject as IItem)) {
						results.Add (iobject);
					}
				}
			}
			return results;
		}

		// Same as GetModItemsFrom list but for items
		public List<IObject> GetItemsFromList (SearchContext context, List<IObject> initialList)
		{
			List<IObject> results = new List<IObject> ();
			foreach (IObject iobject in initialList) {
				if (iobject is IItem) {
					if (context.Command.SupportsItem (iobject as IItem)) {
						results.Add (iobject);
					}
				}
			}
			return results;
		}

		// This will filter out the results in the previous context that match the current query
		private List<IObject> FilterPreviousSearchResultsWithContinuedContext (SearchContext context)
		{
			DoObjectRelevanceSorter sorter;
		 
			sorter = new DoObjectRelevanceSorter (context.Query.ToLower ());
			return sorter.SortResults (context.LastContext.Results);
		}


		private List<IObject> IndependentResults (SearchContext context)
		{
			string query;
			List<IObject> results;

			query = context.Query.ToLower ();
			// We can build on the last results.
			// example: searched for "f" then "fi"
			if (context.LastContext.LastContext != null) {
				results = FilterPreviousSearchResultsWithContinuedContext (context);
			}

			// If someone typed a single key, BOOM we're done.
			else if (firstResults.ContainsKey (query)) {
				results = new List<IObject> (firstResults[query]);
			}

			// Or we just have to do an expensive search...
			else {
				DoObjectRelevanceSorter sorter;
				sorter = new DoObjectRelevanceSorter (query);
				results = sorter.SortAndNarrowResults (universe.Values);
			}
			return results;
		}

		// This method gives us all the commands that are supported by all the items in the list
		private List<IObject> InitialCommandResults (SearchContext context)
		{
			List<IObject> commands = new List<IObject> ();
			List<IObject> commands_to_remove = new List<IObject> ();
			bool initial = true;

			foreach (IItem item in context.Items) {
				List<IObject> item_commands = CommandsForItem (item);

				// If this is the first item in the list, add all of its supported commands.
				if (initial) {
					commands.AddRange (item_commands);
					initial = false;
				}
				//For every subsequent item, check every command in the pre-existing list
				//if its not supported by this item, remove it from the list
				else {
					foreach (ICommand command in commands) {
						if (!item_commands.Contains (command)) {
							commands_to_remove.Add (command);
						}
					}
				}
			}
			foreach (IObject rm in commands_to_remove)
				commands.Remove (rm);
			return commands;
		}

		//Function to determine whether a type array contains a type
		static public bool ContainsType (Type[] typeArray, Type checkType)
		{
			foreach (Type type in typeArray) {
				if (type.Equals (checkType))
					return true;
			}
			return false;
		}

		public List<IObject> CommandsForItem (IItem item)
		{
			List<IObject> item_commands;

			item_commands = new List<IObject> ();
			foreach (ICommand command in doCommands) {
				if (command.SupportsItem (item)) {
					item_commands.Add (command);
				}
			}
			return item_commands;
		}

		public List<IItem> ChildrenOfItem (IItem parent)
		{
			List<IItem> children;

			children = new List<IItem> ();
			foreach (DoItemSource source in doItemSources) {
				children.AddRange (source.ChildrenOfItem (parent));
			}
			return children;
		}
	}
}
