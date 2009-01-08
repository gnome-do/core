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
using Do.Universe;
using Do.Universe.Safe;
using Do.Platform;

namespace Do.Core
{
	
	public class UniverseManager
	{

		Thread thread, update_thread;
		Dictionary<string, Element> universe;
		
		object universe_lock = new object ();
		
		float epsilon = 0.00001f;
		
		/// <value>
		/// The amount of time between updates.
		/// </value>
		TimeSpan UpdateTimeout {
			get {
				int minutes = DBus.PowerState.OnBattery () ? 10 : 2;
				return new TimeSpan (0, minutes, 0);
			}
		}
		
		/// <value>
		/// The amount of time spent on each update.
		/// </value>
		TimeSpan UpdateRunTime {
			get {
				int milliseconds = DBus.PowerState.OnBattery () ? 600 : 200;
				return new TimeSpan (0, 0, 0, 0, milliseconds);
			}
		}
		
		public bool BuildCompleted { get; set; }
		
		public bool UpdatesEnabled { get; set; }
		
		public UniverseManager ()
		{
			universe = new Dictionary<string, Element> ();
			UpdatesEnabled = true;
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
				lock (universe_lock) 
					return Search (query, filter, universe.Values, other);
		}
		
		public IEnumerable<Element> Search (string query, IEnumerable<Type> filter, IEnumerable<Element> objects)
		{
			return Search (query, filter, objects, null);
		}
		
		public IEnumerable<Element> Search (string query, IEnumerable<Type> filter, IEnumerable<Element> elements, Element other)
		{
			Element text = new ImplicitTextItem (query);

			foreach (Element element in elements)
				element.UpdateRelevance (query, other);

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
		/// Threaded universe building
		/// </summary>
		void BuildUniverse ()
		{
			//Originally i had threaded the loading of each plugin, but they dont seem to like this...
			if (thread != null && thread.IsAlive) return;
			
			thread = new Thread (new ThreadStart (LoadUniverse));
			thread.IsBackground = true;
			thread.Start ();
			
			if (update_thread != null && update_thread.IsAlive)
				return;
			
			update_thread = new Thread (new ThreadStart (UniverseUpdateStart));
			update_thread.IsBackground = true;
			update_thread.Priority = ThreadPriority.Lowest;
			update_thread.Start ();
		}
		
		/// <summary>
		/// A continuous method that will update universe until a flag is set for it to stop.
		/// </summary>
		void UniverseUpdateStart ()
		{
			Random rand = new Random ();
			DateTime startUpdate = DateTime.Now;

			while (true) {
				Thread.Sleep (UpdateTimeout);
				if (!UpdatesEnabled) continue;
				
				if (thread.IsAlive) thread.Join ();
				
				if (rand.Next (10) == 0) {
					#warning The Log is not threadsafe...
					Log.Debug ("Updating Actions");
					ReloadActions ();
				}
				
				foreach (ItemSource source in PluginManager.ItemSources) {
					SafeItemSource safeSource = source.Safe;
					Log.Debug ("Updating item source \"{0}\".", safeSource.Name);
					UpdateSource (safeSource);

					if (UpdateRunTime < DateTime.Now - startUpdate) {
						Thread.Sleep (UpdateTimeout);
						startUpdate = DateTime.Now;
					}
				}
			}
		}
		
		/// <summary>
		/// Reloads all actions into the universe.  This is a straight reload, no intelligent 
		/// reloading is done.
		/// </summary>
		void ReloadActions ()
		{
			lock (universe_lock) {
				foreach (Act action in PluginManager.Actions) {
					universe.Remove (action.UniqueId);
				}
				foreach (Act action in PluginManager.Actions) {
						universe [action.UniqueId] = action;			
				}
			}
		}
		
		/// <summary>
		/// Updates an item source and syncs it into the universe
		/// </summary>
		void UpdateSource (SafeItemSource source)
		{
			lock (universe_lock) {
				foreach (Item item in source.Items) {
					if (universe.ContainsKey (item.UniqueId))
						universe.Remove (item.UniqueId);
				}
				source.UpdateItems ();
				foreach (Item item in source.Items) {
					universe  [item.UniqueId] = item;
				}
			}
		}
		
		/// <summary>
		/// Used to perform and intialization of Do's universe and related indexes
		/// </summary>
		void LoadUniverse ()
		{
			ReloadActions ();
			
			foreach (ItemSource source in PluginManager.ItemSources)
				UpdateSource (source.Safe);
			
			Log.Info ("Universe contains {0} items.", universe.Count);
			BuildCompleted = true;
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
		/// Attempts to get an Element for a given UniqueId.
		/// </summary>
		/// <param name="UniqueId">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="item">
		/// A <see cref="Element"/>
		/// </param>
		public bool TryGetElementForUniqueId (string uid, out Element o)
		{
			lock (universe_lock) {
				if (universe.ContainsKey (uid)) {
					o = universe [uid];
				} else {
					o = null;
				}
			}
			return o == null;
		}
		
		/// <summary>
		/// Causes the universe to be rebuilt in the background
		/// </summary>
		public void Reload ()
		{
			Log.Info ("Reloading Universe");
			BuildUniverse ();
		}
		
		public void Initialize ()
		{
			BuildUniverse ();
		}
	}
}
