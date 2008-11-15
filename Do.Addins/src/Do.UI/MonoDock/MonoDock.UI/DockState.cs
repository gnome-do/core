// DockState.cs
// 
// Copyright (C) 2008 GNOME Do
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

using Do.Universe;
using Do.Addins;
using Do.UI;

namespace MonoDock.UI
{
	
	
	public class DockState
	{
		IObject[] current_items = new IObject[3];
		IObject[] old_items = new IObject[3];
		string [] queries = new string[3];
		DateTime[] timestamps = new DateTime[3];
		
		#region First Pane
		public IObject First {
			get {
				return current_items[0];
			}
		}
		
		public IObject FirstOld {
			get {
				return old_items[0];
			}
		}
		
		public string FirstQuery {
			get { return queries[0]; }
		}
		#endregion
		
		#region Second Pane
		public IObject Second {
			get {
				return current_items[1];
			}
		}
		
		public IObject SecondOld {
			get {
				return old_items[1];
			}
		}
		
		public string SecondQuery {
			get { return queries[1]; }
		}
		#endregion
		
		#region Third Pane
		public IObject Third {
			get {
				return current_items[2];
			}
		}
		
		public IObject ThirdOld {
			get {
				return old_items[2];
			}
		}
		
		public string ThirdQuery {
			get { return queries[2]; }
		}
		#endregion
		
		#region Timestamps
		public DateTime ThirdChangeTime {
			get {
				return timestamps[2];
			}
		}

		public DateTime SecondChangeTime {
			get {
				return timestamps[1];
			}
		}

		public DateTime FirstChangeTime {
			get {
				return timestamps[0];
			}
		}
		#endregion
		
		public IObject this [Pane pane] {
			get {
				return GetPaneItem (pane);
			}
			set {
				SetPaneItem (value, pane);
			}
		}

		public void SetPaneItem (IObject item, Pane pane)
		{
			if (current_items[(int) pane] == item)
				return;
			
			old_items[(int) pane] = current_items[(int) pane];
			current_items[(int) pane] = item;
			timestamps[(int) pane] = DateTime.Now;
		}
		
		public void SetPaneQuery (string query, Pane pane)
		{
			queries[(int) pane] = query;
		}
		
		public IObject GetPaneItem (Pane pane)
		{
			return current_items[(int) pane];
		}
		
		public IObject GetOldPaneItem (Pane pane)
		{
			return old_items[(int) pane];
		}
		
		public void Clear ()
		{
			current_items = new IObject[3];
			old_items = new IObject[3];
			timestamps = new DateTime[3];
		}
	}
}
