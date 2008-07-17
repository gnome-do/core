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
	
	
	public class SimpleUniverseManager : IUniverseManager
	{
		private Dictionary<string, IObject> universe;
		private Dictionary<char, List<IObject>> quickResults;
		
		private object universeLock = new object ();
		private object quickResultsLock = new object ();
		
		const int maxResults = 1000;
		
		public SimpleUniverseManager()
		{
			universe = new Dictionary<string, IObject> ();
			quickResults = new Dictionary<char,List<IObject>> ();
			
			for (char key = 'a'; key <= 'z'; key++) {
				quickResults [key] = new List<IObject> ();
			}
		}

		/// <summary>
		/// Provides search functionality for Universe.  Pass a null for searchFilter for default filtering
		/// </summary>
		/// <param name="query">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="searchFilter">
		/// A <see cref="Type"/>
		/// </param>
		/// <returns>
		/// A <see cref="IObject"/>
		/// </returns>
		public IObject[] Search (string query, Type[] searchFilter)
		{
			if (query.Length == 1) {
				lock (quickResultsLock) {
					char key = Convert.ToChar (query.ToLower ());
					if (quickResults.ContainsKey (key))
						return Search (query, searchFilter, quickResults[key]);
				}
			}
			
			lock (universeLock) 
				return Search (query, searchFilter, universe.Values);
		}
		
		/// <summary>
		/// Returns a basic search based on the base array and relevance scores
		/// </summary>
		/// <param name="query">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="searchFilter">
		/// A <see cref="Type"/>
		/// </param>
		/// <param name="baseArray">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		/// <returns>
		/// A <see cref="IObject"/>
		/// </returns>
		public IObject[] Search (string query, Type[] searchFilter, IEnumerable<IObject> baseArray)
		{
			List<IObject> results = new List<IObject> ();
			query = query.ToLower ();
			
			float epsilon = 0.00001f;
		
			foreach (DoObject obj in baseArray) {
				obj.UpdateRelevance (query, null);
				if (Math.Abs (obj.Relevance) > epsilon) {
					if (searchFilter.Length == 0)
						results.Add (obj);
					else
						foreach (Type t in searchFilter)
							if (t.IsInstanceOfType (obj.Inner))
								results.Add (obj);
				}
			}
			results.Sort ();
			
			if (results.Count > maxResults)
				return results.GetRange (0, maxResults).ToArray ();
			return results.ToArray ();
		}
		
		/// <summary>
		/// Threaded universe building
		/// </summary>
		private void BuildUniverse ()
		{
			//Originally i had threaded the loading of each plugin, but they dont seem to like this...
			Thread thread;
			thread = new Thread (new ThreadStart (LoadUniverse));
			thread.IsBackground = true;
			thread.Start ();
		}
		
		private void LoadUniverse ()
		{
			Dictionary<string, IObject> loc_universe;
			Dictionary<char, List<IObject>> loc_quick;
			if (universe.Values.Count > 0) {
				loc_universe = new Dictionary<string,IObject> ();
				loc_quick = new Dictionary<char,List<IObject>> ();
			} else {
				loc_universe = universe;
				loc_quick = quickResults;
			}
			
			foreach (DoAction action in PluginManager.GetActions ()) {
				lock (universeLock)
					loc_universe[action.UID] = action;
				RegisterQuickResults (loc_quick, action);
			}
			
			foreach (DoItemSource source in PluginManager.GetItemSources ()) {
				source.UpdateItems ();
				foreach (DoItem item in source.Items) {
					lock (universeLock)
						loc_universe[item.UID] = item;
					RegisterQuickResults (loc_quick, item);
				}
			}
			
			lock (universeLock)
				universe = loc_universe;
			lock (quickResultsLock)
				quickResults = loc_quick;
			
			loc_universe = null;
			loc_quick = null;
		}
		
		private void RegisterQuickResults (Dictionary<char, List<IObject>> quickResults, IObject result)
		{
			if (quickResults == null) return;
			
			lock (quickResultsLock) {
				foreach (char key in quickResults.Keys) {
					if ((result.Name + result.Description).ToLower ().Contains (key.ToString ()))
						quickResults[key].Add (result);
				}
			}
		}
		
		private void DeleteQuickResult (IObject result)
		{
			lock (quickResultsLock) 
				foreach (List<IObject> list in quickResults.Values)
					list.Remove (result);
		}
		
		/// <summary>
		/// Add a list of IItems to the universe
		/// </summary>
		/// <param name="items">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		public void AddItems (IEnumerable<IItem> items)
		{
			lock (universeLock) {
				foreach (IItem i in items) {
					if (i is DoItem && !universe.ContainsKey ((i as DoItem).UID)) {
						universe.Add ((i as DoItem).UID, i);
						RegisterQuickResults (quickResults, i);
					} else {
						DoItem di = new DoItem (i);
						if (!universe.ContainsKey (di.UID)) {
							universe.Add (di.UID, di);
							RegisterQuickResults (quickResults, di);
						}
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
			lock (universeLock) {
				foreach (IItem i in items) {
					if (i is DoItem) {
						universe.Remove ((i as DoItem).UID);
					} else {
						universe.Remove (new DoItem (i).UID);
					}
					DeleteQuickResult (i);
				}
			}
		}

		public string UIDForObject (IObject o)
		{
			if (o is DoObject)
				return (o as DoObject).UID;
			return new DoObject (o).UID;
		}
		
		public void TryGetObjectForUID (string UID, out IObject item)
		{
			lock (universeLock) {
				if (universe.ContainsKey (UID))
					item = universe[UID];
				else
					item = null;
			}
		}
		
		public void Reload ()
		{
			BuildUniverse ();
		}
		
		public void Initialize ()
		{
			BuildUniverse ();
			GLib.Timeout.Add (2 * 10 * 1000, delegate {
				BuildUniverse ();
				return true;
			});
		}
	}
}
