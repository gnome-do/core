// IUniverseManager.cs
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
using System.Collections.Generic;

using Do.Addins;
using Do.Universe;

namespace Do.Core
{
	
	
	public interface IUniverseManager
	{
		/// <summary>
		/// Returns search results based on the query and the search Filter
		/// </summary>
		/// <param name="query">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="searchFilter">
		/// A <see cref="Type"/>
		/// </param>
		/// <returns>
		/// A <see cref="Element"/>
		/// </returns>
		IEnumerable<Element> Search (string query, IEnumerable<Type> searchFilter);
		
		/// <summary>
		/// Returns search results based on they query, the search filter, and a comparison object.
		/// </summary>
		/// <param name="query">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="searchFilter">
		/// A <see cref="Type"/>
		/// </param>
		/// <param name="compareObj">
		/// A <see cref="Element"/>
		/// </param>
		/// <returns>
		/// A <see cref="Element"/>
		/// </returns>
		IEnumerable<Element> Search (string query, IEnumerable<Type> searchFilter, Element compareObj);
		
		/// <summary>
		/// Returns search results based on a query and the search filter form a specified array.
		/// </summary>
		/// <param name="query">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="searchFilter">
		/// A <see cref="Type"/>
		/// </param>
		/// <param name="baseArray">
		/// A <see cref="Element"/>
		/// </param>
		/// <returns>
		/// A <see cref="Element"/>
		/// </returns>
		IEnumerable<Element> Search (string query, IEnumerable<Type> searchFilter, IEnumerable<Element> baseArray);
		
		/// <summary>
		/// Returns search results based on a query, search filter, a defined array, and has relevancy adjusticated
		/// for a secondary object.
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
		/// <param name="compareObj">
		/// A <see cref="Element"/>
		/// </param>
		/// <returns>
		/// A <see cref="Element"/>
		/// </returns>
		IEnumerable<Element> Search (string query, IEnumerable<Type> searchFilter, IEnumerable<Element> baseArray, Element compareObj);
		
		/// <summary>
		/// Directly adds items to the universe repository
		/// </summary>
		/// <param name="items">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		void AddItems (IEnumerable<IItem> items);
		
		/// <summary>
		/// Directly delete items from the universe repository
		/// </summary>
		/// <param name="items">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		void DeleteItems (IEnumerable<IItem> items);
		
		void Initialize ();
		
		/// <summary>
		/// Tells the universe manager to completely reload its database.  This may be slow.
		/// </summary>
		void Reload ();
		
		/// <summary>
		/// Checks to see if an Element LIKELY has child items.  This list is not authoritive.
		/// </summary>
		/// <param name="o">
		/// A <see cref="Element"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool ObjectHasChildren (Element o);
		
		/// <value>
		/// Enables/Disables background updates for universe.  This should be used primarily for power saving
		/// or cpu saving purposes.  This is does not clear memory associated with the background update
		/// thread.
		/// </value>
		bool UpdatesEnabled { get; set; }
	}
}
