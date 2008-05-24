/* PreferencesWindow.cs
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
using Mono.Addins.Gui;

namespace Do.UI
{	
	public partial class PreferencesWindow : Gtk.Window
	{
		NodeView ndview;
		
		public PreferencesWindow () : 
				base (Gtk.WindowType.Toplevel)
		{
			Build ();
			
			ndview = new NodeView (Store);
			ndview.AppendColumn ("", new CellRendererText (), "text", 0);
			ndview.HeadersVisible = false;
			ndview.NodeSelection.Changed += OnNodeViewSelectionChanged;

			scroll_window.Add (ndview);
			ndview.ShowAll ();
			ndview.Selection.SelectPath (TreePath.NewFirst ());
		}
		
		PreferencesTreeNode[] nodes;
		PreferencesTreeNode[] Nodes {
			get {
				if (null == nodes) {
					nodes = new PreferencesTreeNode [] {
						new PreferencesTreeNode ("General Preferences"),
						new PreferencesTreeNode ("Manage Plugins"),
					};
				}
				return nodes;
			}
		}
		
		NodeStore store;
        NodeStore Store {
            get {
	            if (store == null) {
                    store = new NodeStore (typeof (PreferencesTreeNode));
					foreach (ITreeNode node in Nodes) {
						store.AddNode (node);
					}
	            }
	            return store;
            }
        }
		
		void OnNodeViewSelectionChanged (object o, System.EventArgs args)
        {
            NodeSelection selection;
            PreferencesTreeNode node;
			
			selection = o as NodeSelection;
			node = selection.SelectedNode as PreferencesTreeNode;
			
			// Unsafe for now.
			notebook.CurrentPage = Array.IndexOf (Nodes, node);
        }

		protected virtual void OnBtnManagePluginsClicked (object sender, System.EventArgs e)
		{
			Window addins = AddinManagerWindow.Show (this);
			addins.DeleteEvent += delegate {
				Log.Info ("Completely refreshing universe...");
				Do.UniverseManager.Reload ();
				Log.Info ("Universe completely refreshed!");
			};
		}

		protected virtual void OnBtnCloseClicked (object sender, System.EventArgs e)
		{
			Destroy ();
		}

		protected virtual void OnBtnHelpClicked (object sender, System.EventArgs e)
		{
			Util.Environment.Open ("https://wiki.ubuntu.com/GnomeDo/Use");
		}

	}
}
