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
using Do.Addins;
using Do.Universe;
using Do.Platform;

namespace Do.Core
{
	// Threading Heirarchy:
	// universe_lock may be locked within and action_lock
	// No other nested locks should be allowed
	
	public class UniverseManager : IUniverseManager
	{

		Thread thread, update_thread;
		List<IObject> actions;
		Dictionary<string, IObject> universe;
		
		object action_lock = new object ();
		object universe_lock = new object ();
		
		float epsilon = 0.00001f;
		
		/// <value>
		/// The amount of time between updates.
		/// </value>
		TimeSpan UpdateTimeout {
			get {
				return new TimeSpan (0, 0, 1);
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
		
		public bool UpdatesEnabled { get; set; }
		
		public UniverseManager ()
		{
			actions = new List<IObject> ();
			universe = new Dictionary<string, IObject> ();
			UpdatesEnabled = true;
		}

		public IEnumerable<IObject> Search (string query, IEnumerable<Type> filter)
		{	
				return Search (query, filter, (IObject) null);
		}
		
		public IEnumerable<IObject> Search (string query, IEnumerable<Type> filter, IObject other)
		{
			if (filter.Count () == 1 && filter.First () == typeof (IAction))
				lock (action_lock)
					return Search (query, filter, actions, other);
			else
				lock (universe_lock) 
					return Search (query, filter, universe.Values, other);
		}
		
		public IEnumerable<IObject> Search (string query, IEnumerable<Type> filter, IEnumerable<IObject> objects)
		{
			return Search (query, filter, objects, null);
		}
		
		public IEnumerable<IObject> Search (string query, IEnumerable<Type> filter, IEnumerable<IObject> objects, IObject other)
		{
			return objects.Where (iobj => {
					DoObject o = iobj as DoObject;
					o.UpdateRelevance (query, other as DoObject);
					return epsilon < Math.Abs (o.Relevance) && 
						(!filter.Any () || DoObject.Unwrap (o).IsAssignableToAny (filter));
				})
				.OrderByDescending (o => (o as DoObject).Relevance)
				.ToArray ();
		}
		
		/// <summary>
		/// Returns if an object likely contains children.
		/// </summary>
		/// <param name="o">
		/// A <see cref="IObject"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool ObjectHasChildren (IObject o)
		{
			return o is DoItem && (o as DoItem).HasChildren;
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
				
				foreach (DoItemSource source in PluginManager.ItemSources) {
					#warning The Log is not threadsafe...
					Log.Debug ("Updating item source \"{0}\".", source.Name);
					UpdateSource (source);

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
			lock (action_lock) {
				lock (universe_lock) {
					foreach (DoAction action in actions) {
						universe.Remove (action.UID);
					}
					actions.Clear ();
					foreach (DoAction action in PluginManager.Actions) {
							actions.Add (action);
							universe [action.UID] = action;			
					}
				}
			}
		}
		
		/// <summary>
		/// Updates an item source and syncs it into the universe
		/// </summary>
		void UpdateSource (DoItemSource source)
		{
			lock (universe_lock) {
				foreach (DoItem item in source.Items) {
					if (universe.ContainsKey (item.UID))
						universe.Remove (item.UID);
				}
				try {
					source.UpdateItems ();
				} catch {
					Log.Error ("There was an error updated items for {0}", source.Name);
				}
				foreach (DoItem item in source.Items) {
					universe  [item.UID] = item;
				}
			}
		}
		
		/// <summary>
		/// Used to perform and intialization of Do's universe and related indexes
		/// </summary>
		void LoadUniverse ()
		{
			ReloadActions ();
			
			foreach (DoItemSource source in PluginManager.ItemSources)
				UpdateSource (source);
			
			Log.Info ("Universe contains {0} items.", universe.Count);
		}
		
		/// <summary>
		/// Add a list of IItems to the universe
		/// </summary>
		/// <param name="items">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		public void AddItems (IEnumerable<IItem> items)
		{
			foreach (IItem i in items) {
				DoItem tmp = i as DoItem;
				if (tmp == null)
					tmp = new DoItem (i);
				
				if (universe.ContainsKey (tmp.UID))
					continue;
				
				lock (universe_lock) {
					universe.Add (tmp.UID, i);
				}
			}
		}

		/// <summary>
		/// Remove a list of IItems from the universe.  This removal does not prevent these
		/// items from returning to the universe at a future date.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		public void DeleteItems (IEnumerable<IItem> items)
		{
			lock (universe_lock) {
				foreach (IItem i in items) {
					DoItem item = DoItem.Wrap (i) as DoItem;
					universe.Remove (item.UID);
				}
			}
		}

		/// <summary>
		/// Attempts to get an Object for a given UID.
		/// </summary>
		/// <param name="UID">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="item">
		/// A <see cref="IObject"/>
		/// </param>
		public bool TryGetObjectForUID (string uid, out IObject o)
		{
			if (universe.ContainsKey (uid)) {
				o = universe [uid];
			} else {
				o = null;
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
