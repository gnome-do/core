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

using Mono.Addins;

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

		private Dictionary<DynamicItemSource, UniverseCollection> dynamicUniverses;
		private IEnumerable<Item> DynamicItems {
			get {
				return dynamicUniverses.Values.SelectMany (collection => collection.Values);
			}
		}

		private void OnItemsAvailable (object sender, ItemsAvailableEventArgs args)
		{
			DynamicItemSource source = sender as DynamicItemSource;
			if (source == null) {
				Log<UniverseManager>.Error ("OnItemsAvailable called from a non-DynamicItemSource.");
				return;
			}
			lock (universe_lock) {
				foreach (Item item in args.newItems) {
					try {
						dynamicUniverses[source].Add (item.UniqueId, item);
					} catch (ArgumentException) {
						Log<UniverseManager>.Error ("DynamicItemSource {0} attmpted to add duplicate Item {1}", source.Name, item.UniqueId);
					}
				}
			}
		}

		private void OnItemsUnavailable (object sender, ItemsUnavailableEventArgs args)
		{
			DynamicItemSource source = sender as DynamicItemSource;
			if (source == null) {
				Log<UniverseManager>.Error ("OnItemsUnavailable called from a non-DynamicItemSource.");
				return;
			}
			lock (universe_lock) {
				foreach (Item item in args.unavailableItems) {
					dynamicUniverses[source].Remove (item.UniqueId);
				}
			}
		}

		private void OnPluginChanged (object sender, ExtensionNodeEventArgs args)
		{
			DynamicItemSource source = args.ExtensionObject as DynamicItemSource;
			switch (args.Change) {
			case ExtensionChange.Add:
				lock (universe_lock) {
					dynamicUniverses[source] = new UniverseCollection ();
				}
				source.ItemsAvailable += OnItemsAvailable;
				source.ItemsUnavailable += OnItemsUnavailable;
				Log<UniverseManager>.Debug ("Added new ItemSource: {0}", source.Name);
				break;
			case ExtensionChange.Remove:
				source.ItemsAvailable -= OnItemsAvailable;
				source.ItemsUnavailable -= OnItemsUnavailable;
				lock (universe_lock) {
					dynamicUniverses.Remove (source);
				}
				Log<UniverseManager>.Debug ("Removed ItemSource: {0}", source.Name);
				break;
			}
		}

		public UniverseManager ()
		{
			universe = new UniverseCollection ();
			dynamicUniverses = new Dictionary<DynamicItemSource, Dictionary<string, Item>> ();
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
			// Populate the initial actions.
			ReloadActions(universe);

			// Hook up ItemSource notifications
			AddinManager.AddExtensionNodeHandler ("/Do/ItemSource", OnPluginChanged);
			AddinManager.AddExtensionNodeHandler ("/Do/DynamicItemSource", OnPluginChanged);

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
				return Search (query, filter, universe.Values.Concat (DynamicItems), other);
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

		private bool UpdateItemSources (TimeSpan timeout)
		{
			var startUpdate = DateTime.UtcNow;
			foreach (ItemSource source in PluginManager.ItemSources) {
				Log<UniverseManager>.Debug ("Reloading item source \"{0}\"...", source.Name);
				try {
					source.UpdateAndEmit ();
				} catch (Exception e) {
					Log<UniverseManager>.Error ("Error while updating item source \"{0}\": {1}",
					                            source.Name, e.Message);
				}

				if (timeout < DateTime.UtcNow - startUpdate)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Continuously updates the universe on a worker thread.
		/// </summary>
		void UniverseUpdateLoop ()
		{
			Random rand = new Random ();

			while (true) {
				if (reload_requested.WaitOne (UpdateTimeout)) {
					reload_requested.Reset ();
					ReloadActions (universe);
					if (!UpdateItemSources(new TimeSpan(0, 1, 0)))
						Log<UniverseManager>.Warn("Updating item sources timed out. Universe will be incomplete!");
					continue;
				}

				if (Do.Controller.IsSummoned)
					continue;

				if (rand.Next (10) == 0) {
					ReloadActions (universe);
				}

				UpdateItemSources(UpdateTimeout);
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
		/// Resolves the ItemSource for an Item.
		/// </summary>
		/// <returns>The sources for item.</returns>
		/// <param name="element">Element.</param>
		/// <remarks>Debug only</remarks>
		internal IEnumerable<DynamicItemSource> ResolveSourcesForItem (Item element)
		{
			lock (universe_lock) {
				foreach (KeyValuePair<DynamicItemSource, UniverseCollection> entry in dynamicUniverses) {
					if (entry.Value.ContainsKey (element.UniqueId)) {
						yield return entry.Key;
					}
				}
			}
			yield break;
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
