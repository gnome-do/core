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
using Mono.Addins;

using Gtk;

using Do;

namespace Do.UI {
	
	public partial class SettingsWindow : Gtk.Window {
		
		PluginNodeView nodeView;
		
		protected enum Column {
			CheckButtonColumn = 0,
			InfoColumn = 1,
			Addin = 2,
		}
		
		public SettingsWindow () : 
				base (Gtk.WindowType.Toplevel)
		{			
			Build ();
			
			mainNotebook.Page = 0;
			ok_btn.GrabFocus ();
			
			nodeView = new PluginNodeView ();
			nodeScroll.Add (nodeView);
			nodeView.Show ();
			
			nodeView.PluginToggled += OnPluginToggle;
			nodeView.Selection.Changed += OnPluginSelected;
		}
		
		private void OnPluginSelected (object sender, EventArgs args)
		{
			TreeIter iter;
			ListStore store = nodeView.Model as ListStore;
			
			if (!nodeView.Selection.GetSelected (out iter)) return;
			
			Addin addin = (Addin) store.GetValue (iter, (int) Column.Addin);
			
			name_label.Text = addin.Name;
			author_label.Text = addin.Description.Author;
			description_label.Text = addin.Description.Description;
		}
		
		private void OnPluginToggle (object sender, ToggledArgs args)
		{
			TreeIter iter;
			ListStore store = nodeView.Model as ListStore;
			
			if (!store.GetIter (out iter, new TreePath (args.Path))) return;
			
			Addin addin = (Addin) store.GetValue (iter, (int) Column.Addin);
			bool enabled = (bool) store.GetValue (iter, (int) Column.CheckButtonColumn);
			
			addin.Enabled = !enabled;
			store.SetValue (iter, (int) Column.CheckButtonColumn, !enabled);
		}

		protected virtual void plugins_btnClicked (object sender, System.EventArgs e)
		{
			mainNotebook.Page = 0;
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
			mainNotebook.Page = 1;
		}
	}
}
