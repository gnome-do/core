// UIContext.cs
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

namespace Do.Interface
{
	
	
	public class UIContext : IUIContext
	{
		private Item selection;
		private IList<Item> results;
		
		private int cursor;
		private int[] secondary;
		private string query;
		
		private bool largeText;
		private TextModeType textType;
		
		private IUIContext parentContext;
			
		public Item Selection {
			get {
				return selection;
			}
		}

		public IList<Item> Results {
			get {
				return results;
			}
		}

		public int Cursor {
			get {
				return cursor;
			}
		}

		public int[] SecondaryCursors {
			get {
				return secondary;
			}
		}

		public string Query {
			get {
				return query;
			}
		}
		
		public bool LargeTextDisplay {
			get {
				return largeText;
			}
		}
		
		public TextModeType LargeTextModeType {
			get {
				return textType;
			}
		}
		
		public IUIContext ParentContext {
			get {
				return parentContext;
			}
		}
		
		public UIContext(Item selection, IList<Item> results, int cursor, int[] secondaryCursors, 
		                 string query, bool largeTextDisplay, TextModeType type, IUIContext parentContext)
		{
			this.selection  = selection;
			this.results    = results;
			this.cursor     = cursor;
			this.secondary  = secondaryCursors;
			this.query      = query;
			this.largeText  = largeTextDisplay;
			this.parentContext = parentContext;
			this.textType   = type;
		}
	}
}
