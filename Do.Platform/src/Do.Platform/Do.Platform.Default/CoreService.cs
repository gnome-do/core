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

using Do.Universe;
using Do.Platform;

namespace Do.Platform.Default
{
	
	public class CoreService : ICoreService
	{

		#region ICoreService
		
		public event EventHandler UniverseInitialized {
			add {
				Log.Debug ("Default ICoreService cannot add to UniverseInitialized.");
				value (this, EventArgs.Empty);
			}
			remove {
				Log.Debug ("Default ICoreService cannot remove from UniverseInitialized.");
			}
		}
		
		public Item GetItem (string uid)
		{
			Log.Debug ("Default ICoreService cannot get Items.");
			return new EmptyItem ();
		}
		
		public IEnumerable<Item> GetItemsOrderedByRelevance ()
		{
			Log.Debug ("Default ICoreService cannot get Items.");
			yield break;
		}
		
		public void PerformDefaultAction (Item item, IEnumerable<Type> filter)
		{
			Log.Debug ("Default ICoreService cannot perform default actions.");
		}
		
		public void PerformActionOnItem (Act action, Item item)
		{
			Log.Debug ("Default ICoreService cannot perform actions.");
		}
		
		public IEnumerable<Act> GetActionsForItemOrderedByRelevance (Item item, bool allowThirdPaneRequiredActions)
		{
			Log.Debug ("Default ICoreService cannot get Actions.");
			yield break;
		}
		
		#endregion
		
	}

}
