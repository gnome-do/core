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
		const int WrapWidth = 305;
		const string DescriptionFormat =
			"<b>{0}</b> <small>v{2}</small>\n<small>{1}</small>";

		protected string repository;
		protected string filter;
		protected Dictionary<string, string> addins;
		
		public string Filter {
			get { return filter; }
			set {
				filter = value;
				Refresh (false);
			}
		}
		
		public string ShowRepository {
			get { return repository; }
			set {
				repository = value;
				Refresh (false);
			}
		}

		public PluginNodeView () :
			base ()
		{
			ListStore store;
			CellRenderer cell;

			filter = "";
			repository = PluginManager.AllPluginsRepository;
			addins = new Dictionary<string,string> ();

			RulesHint = true;
			HeadersVisible = false;
			Model = store = new ListStore (
				typeof (bool),
				typeof (string),
				typeof (string));

			cell = new CellRendererToggle ();
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

			store.SetSortFunc ((int) Column.Id,
				new TreeIterCompareFunc (DefaultTreeIterCompareFunc));
			store.SetSortColumnId ((int) Column.Id, SortType.Descending);
			
			Selection.Changed += OnSelectionChanged;

			Refresh ();
		}
		
		public int DefaultTreeIterCompareFunc(TreeModel model, TreeIter a, 
            TreeIter b)
        {
        	string repA, repB;
        	int scoreA, scoreB;
			ListStore store = Model as ListStore;
        	
        	repA = store.GetValue (a, (int)Column.Description) as string;
        	repB = store.GetValue (b, (int)Column.Description) as string;
        	
			if (string.IsNullOrEmpty (repA) || string.IsNullOrEmpty (repB))
        		return 0;

			if (filter == "") {
				return string.Compare (repB, repA,
					StringComparison.CurrentCultureIgnoreCase);
			}
        	
        	scoreA = repA.IndexOf (filter,
				StringComparison.CurrentCultureIgnoreCase);
        	scoreB = repB.IndexOf (filter,
				StringComparison.CurrentCultureIgnoreCase);

            return scoreB - scoreA;
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
		
		bool AddinShouldShow (Addin a)
		{
			return a.Name.ToLower ().Contains (filter.ToLower ()) &&
				PluginManager.AddinIsFromRepository (a, ShowRepository);
		}
		
		bool AddinShouldShow (AddinRepositoryEntry e)
		{
			return e.Addin.Name.ToLower ().Contains (filter.ToLower ()) &&
				PluginManager.AddinIsFromRepository (e, ShowRepository);
		}

		public void Refresh () {
			Refresh (true);
		}
		
		public void Refresh (bool goOnline) {
			ListStore store;

			store = Model as ListStore;
			store.Clear ();
			addins.Clear ();
			// Add other (non-online) addins.
			foreach (Addin a in AddinManager.Registry.GetAddins ()) {
				if (!AddinShouldShow (a)) continue;
				addins [Addin.GetIdName (a.Id)] = a.Id;
				store.AppendValues (a.Enabled, Description (a), a.Id);
			}
			// Add online plugins asynchronously so UI doesn't block.
			RefreshOnlinePluginsAsync (goOnline);
		}

		void RefreshOnlinePluginsAsync (bool goOnline)
		{
			ListStore store;
			SetupService setup;

			store = Model as ListStore;
			setup = new SetupService (AddinManager.Registry);

			new Thread ((ThreadStart) delegate {
				if (goOnline) {
					setup.Repositories.UpdateAllRepositories (
						new ConsoleProgressStatus (true));
				}
				// Add addins from online repositories.
				Application.Invoke (delegate {
					try {
					/// >>>>>>
					foreach (AddinRepositoryEntry e in
						setup.Repositories.GetAvailableAddins ()) {
					if (!AddinShouldShow (e)) continue;
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
					if (addins.Count > 0) {
						ScrollToCell (TreePath.NewFirst (), Columns [0], true,
							0, 0);
					}
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
			if (null != PluginSelected) {
				PluginSelected (this,
					new PluginSelectionEventArgs (GetSelectedAddins ()));
			}
		}

		public event PluginToggledDelegate PluginToggled;
		public event PluginSelectedDelegate PluginSelected;

		public delegate void PluginToggledDelegate (string id, bool enabled);
		public delegate void PluginSelectedDelegate (object sender, PluginSelectionEventArgs args);
	}
}
