// IUIContext.cs
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
	public enum TextModeType {
		None,
		Explicit,
		ExplicitFinalized,
		Implicit,
	}
	
	public interface IUIContext
	{
		/// <value>
		/// The current selection
		/// </value>
		Item Selection {get;}
		
		/// <value>
		/// The results list
		/// </value>
		IList<Item> Results {get;}
		
		/// <value>
		/// Integer index of the selection in the results list
		/// </value>
		int Cursor {get;}
		
		/// <value>
		/// Secondary selections indicies
		/// </value>
		int[] SecondaryCursors {get;}
		
		/// <value>
		/// The query that got us here
		/// </value>
		string Query {get;}
		
		/// <value>
		/// Tells the controller to present text in a larger box
		/// </value>
		bool LargeTextDisplay {get;}
		
		
		TextModeType LargeTextModeType {get;}
		
		/// <value>
		/// The parent context of the current UI context if it exists.
		/// </value>
		IUIContext ParentContext {get;}
	}
}
