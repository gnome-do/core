// PluginNodeView.cs
//
//GNOME Do is the legal property of its developers. Please refer to the
//COPYRIGHT file distributed with this
//source distribution.
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
//

using System;

using Gtk;
using Mono.Addins;

namespace Do
{
	public class PluginNodeView : NodeView
	{
		public PluginNodeView() : base ()
		{
			TreeViewColumn column;
			CellRenderer cell;
			
			this.Model  = new ListStore (typeof (bool), typeof (string),
			                                             typeof (Addin));
			
			column = new TreeViewColumn ();
			cell = new CellRendererToggle ();
			(cell as CellRendererToggle).Activatable = true;
			(cell as CellRendererToggle).Toggled += OnPluginToggle;
			this.AppendColumn ("Enable", cell, "active", 0);
			
			cell = new Gtk.CellRendererText ();
			(cell as CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
			this.AppendColumn ("Plugin Name", cell, "text", 1);
			
			ListStore store = this.Model as ListStore;
			
			foreach (Addin a in AddinManager.Registry.GetAddins ()) {
				store.AppendValues (a.Enabled, a.Name, a);
			}
		}
		
		protected void OnPluginToggle (object sender, ToggledArgs args)
		{
			Addin addin;
			bool enabled;
			TreeIter iter;
			ListStore store;
			
			store = Model as ListStore;
			if (!store.GetIter (out iter, new TreePath (args.Path)))
				return;
			
			addin = (Addin) store.GetValue (iter, 2);
			enabled = (bool) store.GetValue (iter, 0);
			
			if (null != PluginToggled) {
				PluginToggled (addin, !enabled);
			}
			store.SetValue (iter, 0, !enabled);
		}
		
		public event PluginToggledDelegate PluginToggled;
		public delegate void PluginToggledDelegate (Addin addin, bool enabled);
	}
}
