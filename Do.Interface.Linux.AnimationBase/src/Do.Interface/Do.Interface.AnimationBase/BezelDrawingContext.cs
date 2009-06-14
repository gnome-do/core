// BezelDrawingContext.cs
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
using System.Collections.Generic;

using Do.Interface;
using Do.Universe;

namespace Do.Interface.AnimationBase
{
	public class BezelDrawingContext
	{
		private Item [] objects = new Item [3];
		private string [] queries = new string [3];
		private bool [] text_mode = new bool [3];
		
		public bool GetPaneTextMode (Pane pane) {
			return text_mode [(int) pane];
		}
		
		public void SetPaneTextMode (Pane pane, bool textMode) {
			text_mode [(int) pane] = textMode;
		}
		
		public Item GetPaneObject (Pane pane) {
			return objects [(int) pane];
		}
		
		public void SetPaneObject (Pane pane, Item obj) {
			objects [(int) pane] = obj;
		}
		
		public string GetPaneQuery (Pane pane) {
			return queries [(int) pane];
		}
		
		public void SetPaneQuery (Pane pane, string query) {
			queries [(int) pane] = query;
		}
	}
}
