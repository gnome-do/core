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

namespace Do.Core
{
	
	public class UniverseManager
	{

		Thread update_thread;
		Dictionary<string, Element> universe;
		EventHandler initialized;
		
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
			universe = new Dictionary<string, Element> ();

			update_thread = new Thread (new ThreadStart (UniverseUpdateLoop));
			update_thread.IsBackground = true;
			update_thread.Priority = ThreadPriority.Lowest;
			
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
			Gtk.Application.Invoke ((sender, e) => {
				BuildCompleted = true;
				if (initialized != null)
					initialized (this, EventArgs.Empty);
			});

			// Start the update thread.
			update_thread.Start ();
		}

		public IEnumerable<Element> Search (string query, IEnumerable<Type> filter)
		{	
			return Search (query, filter, (Element) null);
		}
		
		public IEnumerable<Element> Search (string query, IEnumerable<Type> filter, Element other)
		{
			if (filter.Count () == 1 && filter.First () == typeof (Act))
				return Search (query, filter, PluginManager.Actions.OfType<Element> (), other);
			else
				lock (universe) 
					return Search (query, filter, universe.Values, other);
		}
		
		public IEnumerable<Element> Search (string query, IEnumerable<Type> filter, IEnumerable<Element> objects)
		{
			return Search (query, filter, objects, null);
		}
		
		public IEnumerable<Element> Search (string query, IEnumerable<Type> filter, IEnumerable<Element> elements, Element other)
		{
			Element text = new ImplicitTextItem (query);

			string lquery = query.ToLower ();

			foreach (Element element in elements)
				element.UpdateRelevance (lquery, other);

			return elements
				.Where (element => epsilon < Math.Abs (element.Relevance) && element.PassesTypeFilter (filter))
				.OrderByDescending (element => element.Relevance)
				.Concat (text.PassesTypeFilter (filter)
						? new [] { text }
						: Enumerable.Empty<Element> ()
				)
				.ToArray ();
		}
		
		/// <summary>
		/// Returns if an object likely contains children.
		/// </summary>
		/// <param name="o">
		/// A <see cref="Element"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool ElementHasChildren (Element element)
		{
			return element is Item && (element as Item).HasChildren ();
		}
		
		/// <summary>
		/// Continuously updates the universe on a worker thread.
		/// </summary>
		void UniverseUpdateLoop ()
		{
			Random rand = new Random ();
			DateTime startUpdate = DateTime.Now;

			while (true) {
				Thread.Sleep (UpdateTimeout);
				if (Do.Controller.IsSummoned) continue;
				startUpdate = DateTime.Now;
				
				if (rand.Next (10) == 0) {
					ReloadActions ();
				}
				
				foreach (ItemSource source in PluginManager.ItemSources) {
					ReloadSource (source);
					if (UpdateRunTime < DateTime.Now - startUpdate) {
						Thread.Sleep (UpdateTimeout);
						startUpdate = DateTime.Now;
					}
				}
			}
		}
		
		/// <summary>
		/// Reloads all actions in the universe.
		/// </summary>
		void ReloadActions ()
		{
			Log<UniverseManager>.Debug ("Reloading actions...");
			lock (universe) {
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
		void ReloadSource (ItemSource source)
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
			
			lock (universe) {
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
			ReloadActions ();
			PluginManager.ItemSources.ForEach (ReloadSource);
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
			lock (universe) {
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
			lock (universe) {
				foreach (Item item in items) {
					universe.Remove (item.UniqueId);
				}
			}
		}

		/// <summary>
		/// Attempts to get an Element for a given UniqueId.
		/// </summary>
		/// <param name="UniqueId">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="item">
		/// A <see cref="Element"/>
		/// </param>
		public bool TryGetElementForUniqueId (string uid, out Element element)
		{
			lock (universe) {
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
			Services.Application.RunOnThread (ReloadUniverse);
		}
	}
}
