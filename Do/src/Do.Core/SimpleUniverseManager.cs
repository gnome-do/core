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
using System.Collections.Generic;
using System.Threading;

using Do;
using Do.Addins;
using Do.Universe;

namespace Do.Core
{
	// Threading Heirarchy:
	// universeLock may be locked within quickResultsLock and actionLock
	// No other nested locks should be allowed
	
	public class SimpleUniverseManager : IUniverseManager
	{
		private Dictionary<string, IObject> universe;
		private Dictionary<char, Dictionary<string, IObject>> quickResults;
		private List<IObject> actions;
		private List<string> items_with_children;
		private Thread thread;
		private DateTime last_update = DateTime.Now;
		
		private object universeLock = new object ();
		private object quickResultsLock = new object ();
		private object actionLock = new object ();
		private object childrenLock = new object ();
		
		private float epsilon = 0.00001f;
		
		public SimpleUniverseManager()
		{
			universe = new Dictionary<string, IObject> ();
			quickResults = new Dictionary<char,Dictionary<string,IObject>> ();
			actions = new List<IObject> ();
			items_with_children = new List<string> ();
			
			for (char key = 'a'; key <= 'z'; key++) {
				quickResults [key] = new Dictionary<string,IObject> ();
			}
		}

		public IObject[] Search (string query, Type[] searchFilter)
		{
			if (query.Length == 1) {
				lock (quickResultsLock) {
					char key = Convert.ToChar (query.ToLower ());
					if (quickResults.ContainsKey (key)) {
						return Search (query, searchFilter, quickResults[key].Values, null);
					}
				}
			}
			
			if (searchFilter.Length == 1 && searchFilter[0] == typeof (IAction))
				lock (actionLock)
					return Search (query, searchFilter, actions, null);
			
			lock (universeLock) 
				return Search (query, searchFilter, universe.Values, null);
		}
		
		public IObject[] Search (string query, Type[] searchFilter, IObject otherObj)
		{
			if (searchFilter.Length == 1 && searchFilter[0] == typeof (IAction))
				lock (actionLock)
					return Search (query, searchFilter, actions, otherObj);
			
			if (query.Length == 1) {
				lock (quickResultsLock) {
					char key = Convert.ToChar (query.ToLower ());
					if (quickResults.ContainsKey (key))
						return Search (query, searchFilter, quickResults[key].Values, null);
				}
			}
			
			lock (universeLock) 
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
			
			// Ideally we would do stable sorts all the time, but quicksort is... quickest
			// so we do a stable sort only on small lists
			if (results.Count < 100)
				InsertionSort (results);
			else
				results.Sort ();
			
			return results.ToArray ();
		}
		
		public bool ObjectHasChildren (IObject o)
		{
			IItem item = o as IItem;
			if (item == null) return false;
			
			string uid = UIDForObject (item);
			bool known = (universe.ContainsKey (uid) || items_with_children.Contains (uid));
			if (known) {
				lock (childrenLock)
					return (items_with_children.Contains (uid));
			} else {
				//hack, fixme
				foreach (DoItemSource s in PluginManager.GetItemSources ()) {
					if (s.ChildrenOfItem (item).Count > 0) {
						lock (childrenLock)
							items_with_children.Add (uid);
						return true;
					}
				}
			}
			return false;
		}
		
		private void InsertionSort (List<IObject> list)
		{
			if (list == null)
				throw new ArgumentNullException( "list" );
			
			IObject key;
			for (int j = 1; j < list.Count; j++) {
				key = list[j];
				
				int i = j - 1;
				for (; i >= 0 && (list[i] as DoObject).CompareTo (key as DoObject) > 0; i--) {
					list[i + 1] = list[i];
				}
				list[i + 1] = key;
			}
		}
		
		/// <summary>
		/// Threaded universe building
		/// </summary>
		private void BuildUniverse ()
		{
			//Originally i had threaded the loading of each plugin, but they dont seem to like this...
			if (thread != null && thread.IsAlive) return;
			
			thread = new Thread (new ThreadStart (LoadUniverse));
			thread.IsBackground = true;
			thread.Start ();
		}
		
		
		/// <summary>
		/// Do not call inside main thread unless you really like a locked Do.
		/// </summary>
		private void LoadUniverse ()
		{
			Dictionary<string, IObject> loc_universe;
			Dictionary<char, Dictionary<string, IObject>> loc_quick;
			List<IObject> loc_actions;
			List<string> loc_children;
			if (universe.Values.Count > 0) {
				loc_universe = new Dictionary<string,IObject> ();
				loc_quick    = new Dictionary<char,Dictionary<string,IObject>> ();
				for (char key = 'a'; key <= 'z'; key++) {
					loc_quick [key] = new Dictionary<string,IObject> ();
				}
				loc_actions  = new List<IObject> ();
				loc_children = new List<string> ();
			} else {
				loc_universe = universe;
				loc_quick    = quickResults;
				loc_actions  = actions;
				loc_children = items_with_children;
			}
			
			foreach (DoAction action in PluginManager.GetActions ()) {
				lock (universeLock)
					loc_universe[action.UID] = action;
				RegisterQuickResults (loc_quick, action);
				lock (actionLock)
					loc_actions.Add (action);
			}
			
			foreach (DoItemSource source in PluginManager.GetItemSources ()) {
				source.UpdateItems ();
				foreach (DoItem item in source.Items) {
					lock (universeLock)
						loc_universe[item.UID] = item;
					RegisterQuickResults (loc_quick, item);
					
					foreach (DoItemSource s in PluginManager.GetItemSources ()) {
						if (s.ChildrenOfItem (item).Count > 0) {
							lock (childrenLock)
								loc_children.Add (item.UID);
							break;
						}
					}
				}
			}
			
			lock (universeLock)
				universe = loc_universe;
			lock (quickResultsLock)
				quickResults = loc_quick;
			lock (actionLock)
				actions = loc_actions;
			lock (childrenLock)
				items_with_children = loc_children;
			
			loc_universe = null;
			loc_quick    = null;
			loc_actions  = null;
			loc_children = null;
			
			//maxResults = (int)universe.Count/7;
			last_update = DateTime.Now;
			Log.Info ("Universe contains {0} items.", universe.Count);
		}
		
		/// <summary>
		/// Registers quickResults into the passed dictionary of the result passed
		/// </summary>
		/// <param name="quickResults">
		/// A <see cref="Dictionary`2"/>
		/// </param>
		/// <param name="result">
		/// A <see cref="IObject"/>
		/// </param>
		private void RegisterQuickResults (Dictionary<char, Dictionary<string, IObject>> quickResults, IObject result)
		{
			if (quickResults == null) return;
			
			DoObject do_result;
			if (result is DoObject)
				do_result = result as DoObject;
			else
				do_result = new DoObject (result);
			
			lock (quickResultsLock) {
				foreach (char key in quickResults.Keys) {
					do_result.UpdateRelevance (key.ToString (), null);
					if (do_result.Relevance > epsilon)
						quickResults[key][do_result.UID] = do_result;
				}
			}
		}
		
		/// <summary>
		/// Deletes a result from the global quickresults dictionary
		/// </summary>
		/// <param name="result">
		/// A <see cref="IObject"/>
		/// </param>
		private void DeleteQuickResult (IObject result)
		{
			string UID = new DoObject (result).UID;
			lock (quickResultsLock) {
				foreach (Dictionary<string, IObject> list in quickResults.Values)
					list.Remove (UID);
			}
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
				if (i is DoItem && !universe.ContainsKey ((i as DoItem).UID)) {
					lock (universeLock) {
						universe.Add ((i as DoItem).UID, i);
					}
					RegisterQuickResults (quickResults, i);
				} else {
					DoItem di = new DoItem (i);
					if (!universe.ContainsKey (di.UID)) {
						lock (universeLock) {
							universe.Add (di.UID, di);
						}
						RegisterQuickResults (quickResults, di);
					}
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
			foreach (IItem i in items) {
				if (i is DoItem) {
					lock (universeLock) {
						universe.Remove ((i as DoItem).UID);
					}
				} else {
					lock (universeLock) {
						universe.Remove (new DoItem (i).UID);
					}
				}
				DeleteQuickResult (i);
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
			if (o is DoObject)
				return (o as DoObject).UID;
			return new DoObject (o).UID;
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
			Console.WriteLine ("Reload");
			BuildUniverse ();
		}
		
		public void Initialize ()
		{
			BuildUniverse ();
			GLib.Timeout.Add (5 * 60 * 1000, delegate {
				if (DBus.PowerState.OnBattery () && 
				    DateTime.Now.Subtract (last_update).TotalMinutes < 15) 
					return true;
				BuildUniverse ();
				return true;
			});
		}
	}
}
