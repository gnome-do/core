// IItemsService.cs
// 
// Copyright (C) 2009 GNOME Do
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
using System.Collections.ObjectModel;

using Do;
using Do.Interface;
using Do.Universe;
using Do.Platform;

using Docky.Interface;
using Docky.Utilities;

namespace Docky.Core
{
	
	
	public interface IItemsService : IDockService
	{
		event DockItemsChangedHandler DockItemsChanged;
		
		event UpdateRequestHandler ItemNeedsUpdate;
		
		/// <summary>
		/// Enable and disable updates to the items in the list.  
		/// Useful when universe is still being set up.
		/// </summary>
		bool UpdatesEnabled { get; }
		
		/// <summary>
		/// gets a read only collection of the dock items
		/// </summary>
		ReadOnlyCollection<AbstractDockItem> DockItems { get; }
		
		#region AddItem Overloads
		void AddItemToDock (Item item);
		
		void AddItemToDock (string identifier);
		
		void AddItemToDock (Item item, int position);
		
		void AddItemToDock (string identifier, int position);
		#endregion
		
		bool ItemCanBeMoved (int item);
		
		bool ItemCanBeRemoved (int item);
		
		void DropItemOnPosition (AbstractDockItem item, int position);

		void MoveItemToPosition (AbstractDockItem item, int position);
		
		void MoveItemToPosition (int item, int position);
		
		void ForceUpdate ();
		
		IconSource GetIconSource (AbstractDockItem item);

		bool RemoveItem (AbstractDockItem item);
		
		bool RemoveItem (int item);
		
		bool HotSeatItem (AbstractDockItem item, List<AbstractDockItem> seatedItems);
		
		bool ResetHotSeat (AbstractDockItem item);
	}
}
