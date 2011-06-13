/* KeybindingsPreferencesWidget.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

using Gtk;

using Mono.Unix;

using Do;
using Do.Interface;
using Do.Platform.Linux;

namespace Do.UI
{
	[System.ComponentModel.Category("Do")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class KeybindingsPreferencesWidget : Bin, IConfigurable
	{
		private KeybindingTreeView kbview;

		// This must be an explicit interface method to disambiguate between
		// Widget.Name and IConfigurable.Name
		string IConfigurable.Name {
			get { return Catalog.GetString ("Keyboard"); }
		}
		
		public string Description {
			get { return ""; }
		}

		public string Icon {
			get { return ""; }
		}
		
		public KeybindingsPreferencesWidget ()
		{
			Build ();
			
			kbview = new KeybindingTreeView ();
			kbview.ColumnsAutosize ();
            action_scroll.Add (kbview);
            action_scroll.ShowAll ();
		}
		
		public Bin GetConfiguration ()
        {
        	return this;
        }
	}
}
