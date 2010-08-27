// UniverseManager.cs
// 
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Do;
using Do.Platform;
using Do.Universe;
using Do.Universe.Safe;

using UniverseCollection = System.Collections.Generic.Dictionary<string, Do.Universe.Item>;

namespace Do.Core
{
	
	public class UniverseManager
	{

		Thread update_thread;
		UniverseCollection universe;
		EventHandler initialized;
		object universe_lock;
		ManualResetEvent reload_requested;

		const float epsilon = 0.00001f;
		
		/// <value>
		/// The amount of time between updates.
		/// </value>
		TimeSpan UpdateTimeout {
			get {
				int minutes = Services.System.GetOnBatteryPower () ? 10 : 2;
				return new TimeSpan (0, minutes, 0);
			}
		}
		
		/// <value>
		/// The amount of time spent on each update.
		/// </value>
		TimeSpan UpdateRunTime {
			get {
				int milliseconds = Services.System.GetOnBatteryPower () ? 600 : 200;
				return new TimeSpan (0, 0, 0, 0, milliseconds);
			}
		}
		
		bool BuildCompleted { get; set; }

		public event EventHandler Initialized {
			add {
				if (BuildCompleted)
					value (this, EventArgs.Empty);
				initialized += value;
			}
			remove {
				initialized -= value;
			}
		}
		
		public UniverseManager ()
		{
			universe = new UniverseCollection ();
			universe_lock = new object ();
			reload_requested = new ManualResetEvent (false);

			update_thread = new Thread (new ThreadStart (UniverseUpdateLoop));
			update_thread.IsBackground = true;
			update_thread.Priority = ThreadPriority.Lowest;
			update_thread.Name = "Universe Update Dispatcher";
			
			Services.Network.StateChanged += OnNetworkStateChanged;
		}

		void OnNetworkStateChanged (object sender, NetworkStateChangedEventArgs e)
		{
			Reload ();
		}
			
		public void Initialize ()
		{
			Services.Application.RunOnThread (InitializeAsync);
		}

		public void InitializeAsync ()
		{
			// Do the initial load of the universe.
			ReloadUniverse ();

			// Notify subscribers that the universe has been loaded.
			Services.Application.RunOnMainThread (() => {
				BuildCompleted = true;
				if (initialized != null)
					initialized (this, EventArgs.Empty);
			});

			// Start the update thread.
			update_thread.Start ();
		}

		public IEnumerable<Item> Search (string query, IEnumerable<Type> filter)
		{
			return Search (query, filter, (Item) null);
		}
		
		public IEnumerable<Item> Search (string query, IEnumerable<Type> filter, Item other)
		{
			lock (universe_lock) 
				return Search (query, filter, universe.Values, other);
		}
		
		public IEnumerable<Item> Search (string query, IEnumerable<Type> filter, IEnumerable<Item> objects)
		{
			return Search (query, filter, objects, null);
		}
		
		public IEnumerable<Item> Search (string query, IEnumerable<Type> filter, IEnumerable<Item> elements, Item other)
		{
			Item text = new ImplicitTextItem (query);
			string lquery = query.ToLower ();

			return elements
				.Where (element => element.PassesTypeFilter (filter) && epsilon < Math.Abs (element.UpdateRelevance (lquery, other)))
				.OrderByDescending (element => element.Relevance)
				.Concat (text.PassesTypeFilter (filter) ? new [] { text } : Enumerable.Empty<Item> ())
				.ToArray ();
		}
		
		/// <summary>
		/// Returns if an object likely contains children.
		/// </summary>
		/// <param name="o">
		/// A <see cref="Item"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool ItemHasChildren (Item item)
		{
			return item.HasChildren ();
		}
		
		/// <summary>
		/// Continuously updates the universe on a worker thread.
		/// </summary>
		void UniverseUpdateLoop ()
		{
			Random rand = new Random ();
			DateTime startUpdate;

			while (true) {
				if (reload_requested.WaitOne (UpdateTimeout)) {
					// We've been asked to do a full reload.  Kick this off, then go back to waiting for the timeout.
					reload_requested.Reset ();
					ReloadUniverse ();
					continue;
				}

				if (Do.Controller.IsSummoned)
					continue;
				startUpdate = DateTime.UtcNow;

				if (rand.Next (10) == 0) {
					ReloadActions (universe);
				}

				foreach (ItemSource source in PluginManager.ItemSources) {
					ReloadSource (source, universe);

					if (UpdateRunTime < DateTime.UtcNow - startUpdate) {
						// We've exhausted our update timer.  Continue after UpdateTimeout.
						if (reload_requested.WaitOne (UpdateTimeout)) {
							// We've been asked to reload the universe; break back to the start of the loop
							break;
						}
						// Start the update timer again, and continue reloading sources…
						startUpdate = DateTime.UtcNow;
					}
				}
			}
		}
		
		/// <summary>
		/// Reloads all actions in the universe.
		/// </summary>
		void ReloadActions (UniverseCollection universe)
		{
			Log<UniverseManager>.Debug ("Reloading actions...");
			lock (universe_lock) {
				foreach (Act action in PluginManager.Actions) {
					if (universe.ContainsKey (action.UniqueId))
						universe.Remove (action.UniqueId);
				}
				foreach (Act action in PluginManager.Actions)
					universe [action.UniqueId] = action;
			}
		}
		
		/// <summary>
		/// Updates an item source and syncs it into the universe. This should
		/// not be called on the main thread to avoid blocking the UI if the
		/// item source takes a long time to update.
		/// </summary>
		void ReloadSource (ItemSource source, UniverseCollection universe)
		{
			SafeItemSource safeSource;
			IEnumerable<Item> oldItems, newItems;

			if (source == null) throw new ArgumentNullException ("source");
			
			safeSource = source.RetainSafe ();
			oldItems = safeSource.Items;
			// We call UpdateItems outside of the lock so as not to block other
			// threads in contention for the lock if UpdateItems blocks.
			Log<UniverseManager>.Debug ("Reloading item source \"{0}\"...", safeSource.Name);
			safeSource.UpdateItems ();
			newItems = safeSource.Items;
			
			lock (universe_lock) {
				foreach (Item item in oldItems) {
					if (universe.ContainsKey (item.UniqueId))
						universe.Remove (item.UniqueId);
				}
				foreach (Item item in newItems) {
					universe  [item.UniqueId] = item;
				}
			}
		}
		
		void ReloadUniverse ()
		{
			Log<UniverseManager>.Info ("Reloading universe...");
			
			// A new temporary universe is created so that searches made during the reload (as threaded 
			// searches are allowed will not see an interuption in available items). Additionally this 
			// serves to clear out unused items that are orphaned from their item service.
			UniverseCollection tmpUniverse = new UniverseCollection ();
			ReloadActions (tmpUniverse);
			PluginManager.ItemSources.ForEach (source => ReloadSource (source, tmpUniverse));
			
			// Clearing the old universe is not needed and considered harmful as enumerables in existence
			// already will be based off the old universe. Clearing it may cause an exception to be thrown.
			// Once those enumerables are destroyed, so too will the old universe.
			universe = tmpUniverse;
			Log<UniverseManager>.Info ("Universe contains {0} items.", universe.Count);
		}
		
		/// <summary>
		/// Add a list of Items to the universe
		/// </summary>
		/// <param name="items">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		public void AddItems (IEnumerable<Item> items)
		{
			lock (universe_lock) {
				foreach (Item item in items) {
					if (universe.ContainsKey (item.UniqueId)) continue;
					universe [item.UniqueId] = item;
				}
			}
		}

		/// <summary>
		/// Remove a list of Items from the universe.  This removal does not prevent these
		/// items from returning to the universe at a future date.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		public void DeleteItems (IEnumerable<Item> items)
		{
			lock (universe_lock) {
				foreach (Item item in items) {
					universe.Remove (item.UniqueId);
				}
			}
		}

		/// <summary>
		/// Attempts to get an Item for a given UniqueId.
		/// </summary>
		/// <param name="UniqueId">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="item">
		/// A <see cref="Item"/>
		/// </param>
		public bool TryGetItemForUniqueId (string uid, out Item element)
		{
			lock (universe_lock) {
				if (universe.ContainsKey (uid)) {
					element = universe [uid];
				} else {
					element = null;
				}
			}
			return element == null;
		}
		
		/// <summary>
		/// Causes the universe to be rebuilt in the background.
		/// </summary>
		public void Reload ()
		{
			Log<UniverseManager>.Info ("Requesting universe reload");
			reload_requested.Set ();
		}
	}
}
