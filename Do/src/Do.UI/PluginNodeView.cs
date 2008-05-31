// PluginNodeView.cs
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

using Gtk;
using Mono.Addins;
using Mono.Addins.Setup;

using Do.Core;
using Do.Universe;

namespace Do.UI
{
	public class PluginNodeView : NodeView
	{
		enum Column {
			Enabled = 0,
			Description,
			Id,
			NumColumns,
		}

		const int IconSize = 26;
		const int WrapWidth = 310;
		const string DescriptionFormat = "<b>{0}</b> <small>v{2}</small>\n<small>{1}</small>";

		Dictionary<string, string> addins;

		public PluginNodeView () :
			base ()
		{
			CellRenderer cell;

			addins = new Dictionary<string,string> ();

			RulesHint = true;
			HeadersVisible = false;
			Model = new ListStore (
				typeof (bool),
				typeof (string),
				typeof (string));

			cell = new CellRendererToggle ();
			cell.SetFixedSize (20, 20);
			(cell as CellRendererToggle).Activatable = true;
			(cell as CellRendererToggle).Toggled += OnPluginToggle;
			AppendColumn ("Enable", cell, "active", Column.Enabled);

			cell = new CellRendererPixbuf ();				
			cell.SetFixedSize (IconSize + 8, IconSize + 8);
			AppendColumn ("Icon", cell, new TreeCellDataFunc (IconDataFunc));

			cell = new Gtk.CellRendererText ();
			(cell as CellRendererText).WrapWidth = WrapWidth;
			(cell as CellRendererText).WrapMode = Pango.WrapMode.Word;
			AppendColumn ("Plugin", cell, "markup", Column.Description);

			(Model as ListStore).SetSortColumnId ((int)Column.Description,
					SortType.Ascending);
			Selection.Changed += OnSelectionChanged;

			Refresh ();
		}

		private void IconDataFunc (TreeViewColumn column,
								   CellRenderer cell,
								   TreeModel model,
								   TreeIter iter)
		{
			string id, icon;
			CellRendererPixbuf renderer;

			renderer = cell as CellRendererPixbuf;
			id = (Model as ListStore).GetValue (iter, (int)Column.Id) as string;
			icon = PluginManager.IconForAddin (id);
			renderer.Pixbuf = IconProvider.PixbufFromIconName (icon, IconSize);
		}

		public void Refresh () {
			ListStore store;

			store = Model as ListStore;
			store.Clear ();
			addins.Clear ();
			// Add other (non-online) addins.
			foreach (Addin a in AddinManager.Registry.GetAddins ()) {
				addins [Addin.GetIdName (a.Id)] = a.Id;
				store.AppendValues ( a.Enabled, Description (a), a.Id);
			}
			// Add online plugins asynchronously so UI doesn't block.
			RefreshOnlinePluginsAsync ();
		}

		void RefreshOnlinePluginsAsync ()
		{
			ListStore store;
			SetupService setup;

			store = Model as ListStore;
			setup = new SetupService (AddinManager.Registry);

			new Thread ((ThreadStart) delegate {
				setup.Repositories.UpdateAllRepositories (
					new ConsoleProgressStatus (true));
				// Add addins from online repositories.
				Application.Invoke (delegate {
					try {
					/// >>>>>>
					foreach (AddinRepositoryEntry e in
						setup.Repositories.GetAvailableAddins ()) {
					// If addin already made its way into the store,
					// skip.
					if (addins.ContainsKey (Addin.GetIdName (e.Addin.Id)))
						continue;
					addins [Addin.GetIdName (e.Addin.Id)] = e.Addin.Id;
					store.AppendValues (
						AddinManager.Registry.IsAddinEnabled (e.Addin.Id),
						Description (e),
						e.Addin.Id);
					}
					ScrollToCell (TreePath.NewFirst (), Columns[0], true, 0, 0);
					/// >>>>>>
					} catch {
						// A crash may result if window is closed before this
						// event occurs.
					}
				});
			}).Start ();
		}

		string Description (string name, string desc, string version)
		{
			return string.Format (DescriptionFormat, name, desc, version);
		}
		string Description (Addin a)
		{
			return Description (a.Name, a.Description.Description, a.Version);
		}

		string Description (AddinRepositoryEntry a)
		{
			return Description (a.Addin);
		}

		string Description (AddinHeader a)
		{
			return Description (a.Name, a.Description, a.Version);
		}

		public string[] GetSelectedAddins () {
			string id;
			TreeIter iter;
			ListStore store;

			if (Selection.CountSelectedRows () == 0)
				return new string [0];

			store = Model as ListStore;
			Selection.GetSelected (out iter);
			id = store.GetValue (iter, (int)Column.Id) as string;
			return new string[] { id };
		}


		protected void OnPluginToggle (object sender, ToggledArgs args)
		{
			string addinId;
			bool enabled;
			TreeIter iter;
			ListStore store;

			store = Model as ListStore;
			if (!store.GetIter (out iter, new TreePath (args.Path)))
				return;

			addinId = (string) store.GetValue (iter, (int)Column.Id);
			enabled = (bool) store.GetValue (iter, (int)Column.Enabled);

			if (null != PluginToggled) {
				PluginToggled (addinId, !enabled);
			}
			store.SetValue (iter, 0,
				AddinManager.Registry.IsAddinEnabled (addinId));
		}

		protected void OnSelectionChanged (object sender, EventArgs args)
		{
			string[] ids;

			ids = GetSelectedAddins ();
			if (ids.Length == 0) {
				return;
			}
			if (null != PluginSelected) {
				PluginSelected (ids [0]);
			}
		}

		public event PluginToggledDelegate PluginToggled;
		public event PluginSelectedDelegate PluginSelected;

		public delegate void PluginToggledDelegate (string id, bool enabled);
		public delegate void PluginSelectedDelegate (string id);
	}
}