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
		private object universeLock = new object ();
		
		/// <summary>
		/// Maximum amount of time spent doing an updated (milliseconds)
		/// </summary>
		const int updateTimeout = 250;
		
		public SimpleUniverseManager()
		{
			universe = new Dictionary<string, IObject> ();
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
			lock (universeLock) 
				return Search (query, searchFilter, universe.Values);
		}
		
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
			
			return results.ToArray ();
		}
		
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
			if (universe.Values.Count > 0) {
				loc_universe = new Dictionary<string,IObject> ();
			} else {
				loc_universe = universe;
			}
			
			foreach (DoAction action in PluginManager.GetActions ()) {
				lock (universeLock)
					loc_universe[action.UID] = action;
			}
			
			foreach (DoItemSource source in PluginManager.GetItemSources ()) {
				source.UpdateItems ();
				foreach (DoItem item in source.Items) {
					lock (universeLock)
						loc_universe[item.UID] = item;
				}
			}
			
			lock (universeLock) {
				universe = loc_universe;
			}
			loc_universe = null;
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
					} else {
						DoItem di = new DoItem (i);
						if (!universe.ContainsKey (di.UID))
							universe.Add (di.UID, di);
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
			GLib.Timeout.Add (2 * 60 * 1000, delegate {
				BuildUniverse ();
				return true;
			});
		}
	}
}
