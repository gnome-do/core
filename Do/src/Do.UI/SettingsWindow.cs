/* SettingsWindow.cs
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

using Gtk;

using Do;

namespace Do.UI {
	
	public partial class SettingsWindow : Gtk.Window {
		
		protected enum Column {
			CheckButtonColumn = 0,
			InfoColumn = 1,
			NumberColumns = 2
		}
		
		public SettingsWindow () : 
				base (Gtk.WindowType.Toplevel)
		{
			TreeViewColumn column;
			CellRenderer   cell;
			
			Build ();
			
			all_plugins_treeview.Model  = new ListStore (new Type [] {		
				typeof (string),
			});
			
			column = new TreeViewColumn ();			
			column.Sizing = Gtk.TreeViewColumnSizing.Fixed; 
				
			cell = new Gtk.CellRendererToggle ();
			column.PackStart (cell, false);
			all_plugins_treeview.AppendColumn (column);			

			// Column 2
			column = new TreeViewColumn ();			
			column.Sizing = Gtk.TreeViewColumnSizing.Fixed; 
			
			cell = new CellRendererPixbuf ();				
			cell.SetFixedSize (-1, 30 - (int) cell.Ypad);
			column.PackStart (cell, false);
			column.SetCellDataFunc (cell, new TreeCellDataFunc (IconDataFunc));
			
			cell = new CellRendererText ();
			(cell as CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
			column.PackStart (cell, true);	
			column.AddAttribute (cell, "markup", (int) Column.InfoColumn);		

			all_plugins_treeview.Selection.Changed += OnPluginSelected;
			
			ListStore store = all_plugins_treeview.Model as ListStore;
			store.AppendValues (new object [] {
				"Hello, world!",
			});
		}
		
		private void OnPluginSelected (object sender, EventArgs args)
		{
			Console.WriteLine ("Hey!");
		}

		protected virtual void plugins_btnClicked (object sender, System.EventArgs e)
		{
		}

		protected virtual void ok_btnClicked (object sender, System.EventArgs e)
		{
			Hide ();
		}

		protected virtual void help_btnClicked (object sender, System.EventArgs e)
		{
			Util.Environment.Open ("https://wiki.ubuntu.com/GnomeDo/Use");
		}

		protected virtual void preferences_btnClicked (object sender, System.EventArgs e)
		{
		}
		
		private void IconDataFunc (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{			
			CellRendererPixbuf rend;
			
			rend = cell as CellRendererPixbuf;
			//IObject o = (resultsTreeview.Model as ListStore).GetValue (iter, 0) as IObject;
			rend.Pixbuf = IconProvider.PixbufFromIconName ("firefox", 14);
		}		
	}
}
