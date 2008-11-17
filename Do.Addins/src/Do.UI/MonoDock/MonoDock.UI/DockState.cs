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
using System.Collections.Generic;

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
		
		int[] cursors = new int[3];
		int[] previous_cursors = new int[3];
		
		DateTime[] timestamps = new DateTime[3];
		DateTime[] result_timestamps = new DateTime[3];
		DateTime[] cursor_timestamps = new DateTime[3];
		
		IList<IObject>[] results = new IList<IObject>[3];
		
		Pane currentPane, previousPane = Pane.Second;
		DateTime current_pane_change, last_cusor_change;
		
		public Pane CurrentPane {
			get {
				return currentPane;
			}
			set {
				if (currentPane == value)
					return;
				previousPane = currentPane;
				currentPane = value;
				current_pane_change = DateTime.UtcNow;
			}
		}
		
		public Pane PreviousPane {
			get {
				return previousPane;
			}
		}
		
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
		
		public IList<IObject> FirstResults {
			get { return results[0]; }
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
		
		public IList<IObject> SecondResults {
			get { return results[1]; }
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
		
		public IList<IObject> ThirdResults {
			get { return results[2]; }
		}
		#endregion
		
		#region Timestamps
		public DateTime FirstChangeTime {
			get {
				return timestamps[0];
			}
		}
		
		public DateTime SecondChangeTime {
			get {
				return timestamps[1];
			}
		}
		
		public DateTime ThirdChangeTime {
			get {
				return timestamps[2];
			}
		}
		
		public DateTime CurrentPaneTime {
			get {
				return current_pane_change;
			}
		}
		
		public DateTime LastCursorChange {
			get {
				return last_cusor_change;
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
			timestamps[(int) pane] = DateTime.UtcNow;
		}
		
		public void SetPaneQuery (string query, Pane pane)
		{
			queries[(int) pane] = query;
		}
		
		public void SetPaneResults (IList<IObject> resultList, Pane pane)
		{
			results[(int) pane] = resultList;
			result_timestamps[(int) pane] = DateTime.UtcNow;
		}
		
		public void SetPaneCursor (int cursor, Pane pane)
		{
			previous_cursors[(int) pane] = cursors[(int) pane];
			cursors[(int) pane] = cursor;
			cursor_timestamps[(int) pane] = last_cusor_change = DateTime.UtcNow;
		}
		
		public IObject GetPaneItem (Pane pane)
		{
			return current_items[(int) pane];
		}
		
		public IObject GetOldPaneItem (Pane pane)
		{
			return old_items[(int) pane];
		}
		
		public string GetPaneQuery (Pane pane)
		{
			return queries[(int) pane];
		}
		
		public IList<IObject> GetPaneResults (Pane pane)
		{
			return results[(int) pane];
		}
		
		public int GetPaneCursor (Pane pane)
		{
			return cursors[(int) pane];
		}
		
		public int GetPanePreviousCursor (Pane pane)
		{
			return previous_cursors[(int) pane];
		}
		
		public DateTime GetPaneCursorTime (Pane pane)
		{
			return cursor_timestamps[(int) pane];
		}
		
		public void Clear ()
		{
			current_items = new IObject[3];
			results = new IList<IObject>[3];
			result_timestamps = new DateTime[3];
			queries = new string[3];
			old_items = new IObject[3];
			timestamps = new DateTime[3];
		}
		
		public void ClearPane (Pane pane)
		{
			int i = (int) pane;
			
			current_items[i] = null;
			results[i] = null;
			result_timestamps[i] = timestamps[i] = DateTime.UtcNow;
			queries[i] = null;
		}
	}
}
