// CoreService.cs
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
using System.Linq;

using Mono.Unix;

using Do.Core;
using Do.Universe;
using Do.Platform;

namespace Do.Platform
{
	
	public class CoreService : ICoreService
	{
		#region ICoreService
		
		public event EventHandler UniverseInitialized {
			add    { Do.UniverseManager.Initialized += value; }
			remove { Do.UniverseManager.Initialized -= value; }
		}

		public Item GetItem (string uniqueId)
		{
			Item element;
			Do.UniverseManager.TryGetItemForUniqueId (uniqueId, out element);
			return element;
		}
		
		public IEnumerable<Item> GetItemsOrderedByRelevance ()
		{
			return Do.UniverseManager.Search ("", typeof (Item).Cons (null)).Where (i => !i.IsAction ());
		}

		public void PerformDefaultAction (Item item, IEnumerable<Type> filter)
		{
			Do.Controller.PerformDefaultAction (item, filter);
		}
		
		public void PerformActionOnItem (Act action, Item item)
		{
			Do.Controller.PerformActionOnItem (action, item);
		}
		
		public IEnumerable<Act> GetActionsForItemOrderedByRelevance (Item item, bool allowThirdPaneRequiredActions)
		{
			IEnumerable<Act> actions = Do.UniverseManager
				.Search ("", typeof (Act).Cons (null), item)
				.Where (i => !(i is ProxyItem)) //no proxy items
				.Cast<Act> ()
				.Where (a => a.Safe.SupportsItem (item));
			
			if (allowThirdPaneRequiredActions)
				return actions;
			return actions.Where (a => a.Safe.ModifierItemsOptional);
		}
		
		#endregion
	}
}
