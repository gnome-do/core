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
using System.Linq;

using Do.Universe;
using Do.Interface;
using Do.Interface.Widgets;

namespace Docky.Interface
{
	
	
	public class DockState
	{
		IObject[] current_items = new IObject[3];
		IObject[] old_items = new IObject[3];
		
		string [] queries = new string[3];
		
		int[] cursors = new int[3];
		int[] previous_cursors = new int[3];
		
		bool[] text_mode = new bool[3];
		
		DateTime[] timestamps = new DateTime[3];
		DateTime[] result_timestamps = new DateTime[3];
		DateTime[] cursor_timestamps = new DateTime[3];
		DateTime[] text_mode_timestamps = new DateTime[3];
		
		IList<IObject>[] results = new IList<IObject>[3];
		IList<IObject>[] results_prev = new IList<IObject>[3];
		
		Pane currentPane, previousPane = Pane.Second;
		
		DateTime current_pane_change; 
		DateTime last_cusor_change;
		DateTime third_pane_visibility_change;
		
		bool third_pane_visible;
		
		IObject IntroObject { get; set; }
		
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
		
		
		public bool ThirdPaneVisible { 
			get { return third_pane_visible; }
			set { 
				if (third_pane_visible == value)
					return;
				third_pane_visible = value;
				third_pane_visibility_change = DateTime.UtcNow;
			}
		}
		
		public DateTime ThirdPaneVisiblityTime {
			get { return third_pane_visibility_change; }
		}
		
		public IObject First {
			get {
				return GetPaneItem (Pane.First);
			}
		}
		
		public IObject Second {
			get {
				return GetPaneItem (Pane.Second);
			}
		}
		
		public IObject Third {
			get {
				return GetPaneItem (Pane.Third);
			}
		}
		
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
				SetItem (value, pane);
			}
		}
		
		public DockState ()
		{
			third_pane_visible = false;
			IntroObject = new DefaultLabelBoxObject ();
		}

		void SetItem (IObject item, Pane pane)
		{
			if (current_items[(int) pane] == item)
				return;
			
			old_items[(int) pane] = current_items[(int) pane];
			current_items[(int) pane] = item;
			timestamps[(int) pane] = DateTime.UtcNow;
		}
		
		void SetQuery (string query, Pane pane)
		{
			queries[(int) pane] = query;
		}
		
		void SetResults (IList<IObject> resultList, Pane pane)
		{
			if (results[(int) pane] != null && resultList.Count == results[(int) pane].Count) {
				bool same = true;
				for (int i=0; i<resultList.Count; i++) {
					if (results[(int) pane][i] != resultList[i]) {
						same = false;
						break;
					}
				}
				if (same)
					return;
			}
			
			results_prev[(int) pane] = results[(int) pane];
			results[(int) pane] = resultList;
			
			result_timestamps[(int) pane] = DateTime.UtcNow;
		}
		
		void SetCursor (int cursor, Pane pane)
		{
			if (cursor == cursors[(int) pane])
				return;
			
			previous_cursors[(int) pane] = cursors[(int) pane];
			cursors[(int) pane] = cursor;
			cursor_timestamps[(int) pane] = last_cusor_change = DateTime.UtcNow;
		}
		
		void SetTextMode (bool textMode, Pane pane)
		{
			if (text_mode[(int) pane] == textMode)
				return;
			
			text_mode[(int) pane] = textMode;
			text_mode_timestamps[(int) pane] = DateTime.UtcNow;
		}
		
		public IObject GetPaneItem (Pane pane)
		{
			if (pane == Pane.First && current_items[(int) pane] == null)
				return IntroObject;
			return current_items[(int) pane];
		}
		
		public IObject GetOldPaneItem (Pane pane)
		{
			return old_items[(int) pane];
		}
		
		public string GetPaneQuery (Pane pane)
		{
			return queries[(int) pane] ?? "";
		}
		
		public IList<IObject> GetPaneResults (Pane pane)
		{
			return results[(int) pane];
		}
		
		public IList<IObject> GetPanePreviousResults (Pane pane)
		{
			return results_prev[(int) pane];
		}
		
		public DateTime GetPaneResultsTime (Pane pane)
		{
			return result_timestamps[(int) pane];
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
		
		public bool GetTextMode (Pane pane)
		{
			return text_mode[(int) pane];
		}
		
		public void Clear ()
		{
			current_items = new IObject[3];
			results = new IList<IObject>[3];
			result_timestamps = new DateTime[3];
			queries = new string[3];
			old_items = new IObject[3];
			timestamps = new DateTime[3];
			
			text_mode = new bool[3];
			text_mode_timestamps = new DateTime[3];
		}
		
		public void ClearPane (Pane pane)
		{
			int i = (int) pane;
			
			current_items[i] = null;
			results[i] = null;
			text_mode_timestamps[i] = result_timestamps[i] = timestamps[i] = DateTime.UtcNow;
			queries[i] = null;
			text_mode[i] = false;
		}
		
		public void SetContext (IUIContext context, Pane pane)
		{
			this[pane] = context.Selection;
			SetQuery (context.Query, pane);
			SetResults (context.Results, pane);
			SetCursor (context.Cursor, pane);
			SetTextMode (context.LargeTextDisplay, pane);
		}
	}
}
