// SearchFinishState.cs
// 
// Copyright (C) 2008 GNOME-Do
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

namespace Do.Interface
{
	
	
	public class SearchFinishState
	{
		private bool selection_changed, query_changed;
		private string query;
		private Item selection;
		
		public bool SelectionChanged { 
			get {
				return selection_changed;
			}
		}
		public bool QueryChanged { 
			get {
				return query_changed;
			}
		}
		public string Query {
			get {
				return query;
			}
		}
		
		public Item Selection {
			get {
				return selection;
			}
		}
		
		public SearchFinishState(bool selectionChanged, bool queryChanged, Item selection, string query)
		{
			selection_changed = selectionChanged;
			query_changed = queryChanged;
			this.selection = selection;
			this.query = query;
		}
	}
}
