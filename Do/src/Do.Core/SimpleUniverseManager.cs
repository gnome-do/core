// SimpleUniverseManager.cs
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

namespace Do.Core
{
	// Threading Heirarchy:
	// universe_lock may be locked within quick_results_lock and action_lock
	// No other nested locks should be allowed
	
	public class SimpleUniverseManager : IUniverseManager
	{

		Thread thread, update_thread;
		List<IObject> actions;
		List<string> items_with_children;
		Dictionary<string, IObject> universe;
//		Dictionary<char, Dictionary<string, IObject>> quick_results;
		
		object action_lock = new object ();
		object children_lock = new object ();
		object universe_lock = new object ();
//		object quick_results_lock = new object ();
		
		float epsilon = 0.00001f;
		
		int UpdateTimeout {
			get {
				return (DBus.PowerState.OnBattery ()) ? 120*1000 : 30*1000;
			}
		}
		
		int UpdateRunTime {
			get {
				return (DBus.PowerState.OnBattery ()) ? 600 : 200;
			}
		}
		
		internal bool UpdatesEnabled { get; set; }
		
		public SimpleUniverseManager ()
		{
			actions = new List<IObject> ();
			items_with_children = new List<string> ();
			universe = new Dictionary<string, IObject> ();
//			quick_results = new Dictionary<char,Dictionary<string,IObject>> ();
			
//			for (char key = 'a'; key <= 'z'; key++)
//				quick_results [key] = new Dictionary<string,IObject> ();
		}

		public IObject[] Search (string query, Type[] searchFilter)
		{
//			if (query.Length == 1) {
//				lock (quick_results_lock) {
//					char key = Convert.ToChar (query.ToLower ());
//					if (quick_results.ContainsKey (key)) {
//						return Search (query, searchFilter, quick_results[key].Values, null);
//					}
//				}
//			}
			
			if (searchFilter.Length == 1 && searchFilter[0] == typeof (IAction))
				lock (action_lock)
					return Search (query, searchFilter, actions, null);
			
			lock (universe_lock) 
				return Search (query, searchFilter, universe.Values, null);
		}
		
		public IObject[] Search (string query, Type[] searchFilter, IObject otherObj)
		{
			if (searchFilter.Length == 1 && searchFilter[0] == typeof (IAction))
				lock (action_lock)
					return Search (query, searchFilter, actions, otherObj);
			
//			if (query.Length == 1) {
//				lock (quick_results_lock) {
//					char key = Convert.ToChar (query.ToLower ());
//					if (quick_results.ContainsKey (key))
//						return Search (query, searchFilter, quick_results[key].Values, null);
//				}
//			}
			
			lock (universe_lock) 
				return Search (query, searchFilter, universe.Values, otherObj);
		}
		
		public IObject[] Search (string query, Type[] searchFilter, IEnumerable<IObject> baseArray)
		{
			return Search (query, searchFilter, baseArray, null);
		}
		
		public IObject[] Search (string query, Type[] searchFilter, IEnumerable<IObject> baseArray, IObject compareObj)
		{
			List<IObject> results = new List<IObject> ();
			query = query.ToLower ();
			
			foreach (DoObject obj in baseArray) {
				obj.UpdateRelevance (query, compareObj as DoObject);
				if (Math.Abs (obj.Relevance) > epsilon) {
					if (searchFilter.Length == 0) {
						results.Add (obj);
					} else {
						foreach (Type t in searchFilter) {
							if (t.IsInstanceOfType (obj.Inner)) {
								results.Add (obj);
								break;
							}
						}
					}
				}
			}
			results.Sort ();
			return results.ToArray ();
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
			IItem item = o as IItem;
			if (item == null) return false;
			
			string uid = UIDForObject (item);
			
			// First we need to check and see if we already know this item has children
			lock (children_lock)
				if (items_with_children.Contains (uid))
					return true;
			
			// It did not, lets check and see if we even know about this object in universe
			bool known; 
			lock (universe_lock)
				known = universe.ContainsKey (uid);
			
			// If we know the item in universe, but its not in the item list, we can
			// assume with relative safety that the item has no children items
			if (known) 
				return false;
			
			bool supported = PluginManager.GetItemSources ()
				.Where (source => SourceSupportsItem (source, item) && source.ChildrenOfItem (item).Any ())
				.Any ();
			
			if (supported)
				lock (children_lock)
					items_with_children.Add (uid);
			return supported;
		}
		
		internal static bool SourceSupportsItem (IItemSource source, IItem item)
		{
			// While is used to get to the innermost wrapped IItem.
			while (item is DoItem)
				item = (item as DoItem).Inner as IItem;
			
			return source.SupportedItemTypes
				.Where (t => t.IsInstanceOfType (item))
				.Any ();
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
			DateTime time;
			DateTime last_action_update = DateTime.Now;
			char source_char = 'a';
			while (true) {
				if (thread.IsAlive)
					thread.Join ();
				
				if (!UpdatesEnabled) {
					Thread.Sleep (UpdateTimeout*3);
					continue;
				}
				
				time = DateTime.Now;
				
				if (DateTime.Now.Subtract (last_action_update).TotalMinutes > 4) {
					Log.Info ("Updating Actions");
					ReloadActions ();
					last_action_update = DateTime.Now;
					Thread.Sleep (UpdateTimeout);
					continue;
				}
				
				while (DateTime.Now.Subtract (time).TotalMilliseconds < UpdateRunTime) {
					var sources = PluginManager.GetItemSources ()
						.Where ((DoItemSource s) => s.Name.ToLower ().StartsWith (source_char.ToString ()));
					
					foreach (DoItemSource item_source in sources) {
						Log.Info ("Updating Item Source: {0}", item_source.Name);
						UpdateSource (item_source);
					}
					
					if (source_char == 'z')
						source_char = 'a';
					else
						source_char++;
				}
				
				Thread.Sleep (UpdateTimeout);
			}
		}
		
		void ReloadActions ()
		{
			lock (action_lock)
				actions.Clear ();
			
			// Since we are updating all of our actions right now, we need to clear them all out
			// of the quick results so we can re-register them.
//			lock (quick_results_lock) {
//				foreach (Dictionary<string, IObject> dict in quick_results.Values) {
//					var tmpactions = dict.Values.Where (iobj => iobj is IAction);
//					foreach (IObject action in tmpactions)
//						DeleteQuickResult (action);
//				}
//			}
			
			foreach (DoAction action in PluginManager.GetActions ()) {
				lock (action_lock)
					actions.Add (action);
//				RegisterQuickResults (quick_results, action);
				lock (universe_lock)
					universe[action.UID] = action;			
			}
		}
		
		void UpdateSource (DoItemSource item_source)
		{
			lock (universe_lock) {
				foreach (DoItem item in item_source.Items) {
					if (universe.ContainsKey (item.UID))
						universe.Remove (item.UID);
//					DeleteQuickResult (item);
				}
				try {
					item_source.UpdateItems ();
				} catch {
					Log.Error ("There was an error updated items for {0}", item_source.Name);
				}
				foreach (DoItem item in item_source.Items) {
					universe[item.UID] = item;
//					RegisterQuickResults (quick_results, item);
					
					bool supported = PluginManager.GetItemSources ()
						.Where (s => SourceSupportsItem (s, item) && s.ChildrenOfItem (item).Any ())
						.Any ();
					
					if (supported)
						lock (children_lock)
							items_with_children.Add (item.UID);
				}
			}
		}
		
		/// <summary>
		/// Used to perform and intialization of Do's universe and related indexes
		/// </summary>
		void LoadUniverse ()
		{
			ReloadActions ();
			
			foreach (DoItemSource source in PluginManager.GetItemSources ())
				UpdateSource (source);
			
			Log.Info ("Universe contains {0} items.", universe.Count);
		}
		
		/// <summary>
		/// Registers quick_results into the passed dictionary of the result passed
		/// </summary>
		/// <param name="quick_results">
		/// A <see cref="Dictionary`2"/>
		/// </param>
		/// <param name="result">
		/// A <see cref="IObject"/>
		/// </param>
//		void RegisterQuickResults (Dictionary<char, Dictionary<string, IObject>> quick_results, IObject result)
//		{
//			if (quick_results == null) return;
//			
//			DoObject do_result = (result as DoObject) ?? new DoObject (result);
//			
//			lock (quick_results_lock) {
//				foreach (char key in quick_results.Keys) {
//					do_result.UpdateRelevance (key.ToString (), null);
//					if (do_result.Relevance > epsilon)
//						quick_results[key][do_result.UID] = do_result;
//				}
//			}
//		}
		
		/// <summary>
		/// Deletes a result from the global quickresults dictionary
		/// </summary>
		/// <param name="result">
		/// A <see cref="IObject"/>
		/// </param>
//		void DeleteQuickResult (IObject result)
//		{
//			string UID = new DoObject (result).UID;
//			lock (quick_results_lock) {
//				foreach (Dictionary<string, IObject> list in quick_results.Values)
//					list.Remove (UID);
//			}
//		}
		
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
//				RegisterQuickResults (quick_results, i);
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
			foreach (IItem i in items) {
				DoItem item = (i as DoItem) ?? new DoItem (i);
				lock (universe_lock)
				      universe.Remove (item.UID);
//				DeleteQuickResult (i);
			}
		}

		/// <summary>
		/// Returns the UID for an object
		/// </summary>
		/// <param name="o">
		/// A <see cref="IObject"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string UIDForObject (IObject o)
		{
			DoObject ob  = (o as DoObject) ?? new DoObject (o);
			return ob.UID;
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
		public void TryGetObjectForUID (string UID, out IObject item)
		{
			if (universe.ContainsKey (UID)) {
				item = (universe[UID] as DoObject).Inner;
			} else {
				item = null;
			}
			
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
