// PluginSeletionEventArgs.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//

using System;
using System.Threading;
using System.Collections.Generic;

namespace Do.UI
{
	public class PluginSelectionEventArgs : EventArgs
	{
		string[] selected_addins;
		
		public PluginSelectionEventArgs (IEnumerable<string> selected)
			: base ()
		{
			selected_addins = new List<string> (selected).ToArray ();
		}
	
		public string[] SelectedAddins {
			get { return selected_addins; }
		}
	}
}