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
using System.Collections.Generic;

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
			
			// Add notebook pages.
			foreach (KeyValuePair<string, Widget> page in Pages) {
				notebook.Add (page.Value);
			}
			
			// Select a default preference view.
			TreePath sel = TreePath.NewFirst ();
			sel.Next (); sel.Next ();
			ndview.Selection.SelectPath (sel);
		}
		
		KeyValuePair<string, Widget>[] pages;
		KeyValuePair<string, Widget>[] Pages {
			get {
				if (null == pages) {
					pages = new KeyValuePair<string, Widget> [] {
						new KeyValuePair<string, Widget> (
						    "General Preferences", new GeneralPreferencesWidget ()),
						new KeyValuePair<string, Widget> (
						    "Shortcut Keys", new KeybindingsPreferencesWidget ()),
						new KeyValuePair<string, Widget> (
						    "Manage Plugins", new ManagePluginsPreferencesWidget ()),
					};
				}
				return pages;
			}
		}
		
		NodeStore store;
        NodeStore Store {
            get {
	            if (store == null) {
                    store = new NodeStore (typeof (PreferencesTreeNode));
					foreach (KeyValuePair<string, Widget> page in Pages) {
						store.AddNode (new PreferencesTreeNode (page.Key));
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
			for (int i = 0; i < Pages.Length; ++i) {
				if (Pages [i].Key == node.Label) {
					notebook.CurrentPage = i;
					break;
				}
			}
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
