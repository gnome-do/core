// PluginNodeView.cs
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
using System.Threading;
using System.Collections.Generic;

using Gtk;
using Mono.Unix;
using Mono.Addins;
using Mono.Addins.Setup;

using Do.Core;
using Do.Platform;
using Do.Platform.Linux;
using Do.Universe;
using Do.Interface;

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
		const int IconPadding = 8;
		const int WrapWidth = 320;
		const string DescriptionFormat = "<b>{0}</b> <small>v{2}</small>\n<small>{1}</small>";

		string filter, category;

		public delegate void PluginToggledDelegate (string id, bool enabled);

		public event PluginToggledDelegate PluginToggled;
		public event EventHandler<PluginSelectionEventArgs> PluginSelected;

		public string Filter {
			get { return filter; }
			set {
				filter = value ?? "";
				Refresh (false);
			}
		}

		public string ShowCategory {
			get { return category; }
			set {
				category = value;
				Refresh (false);
			}
		}

		public PluginNodeView () :
			base ()
		{
			ListStore store;
			CellRenderer cell;

			filter = "";
			category = "";

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
			cell.SetFixedSize (IconSize + IconPadding, IconSize + IconPadding);
			AppendColumn ("Icon", cell, (TreeCellDataFunc)IconDataFunc);

			cell = new Gtk.CellRendererText ();
			(cell as CellRendererText).WrapWidth = WrapWidth;
			(cell as CellRendererText).WrapMode = Pango.WrapMode.Word;
			AppendColumn ("Plugin", cell, "markup", Column.Description);

			store.SetSortFunc ((int) Column.Id, SortAlphabeticallyWithFilter);
			store.SetSortColumnId ((int) Column.Id, SortType.Descending);

			Selection.Changed += OnSelectionChanged;

			Refresh (true);
		}

		int SortAlphabeticallyWithFilter (TreeModel model, TreeIter a, TreeIter b)
		{
			string repA, repB;
			int scoreA, scoreB;
			ListStore store = Model as ListStore;

			repA = store.GetValue (a, (int)Column.Description) as string;
			repB = store.GetValue (b, (int)Column.Description) as string;

			if (string.IsNullOrEmpty (repA) || string.IsNullOrEmpty (repB))
				return 0;

			if (filter == "") {
				return string.Compare (repB, repA, StringComparison.CurrentCultureIgnoreCase);
			}

			scoreA = repA.IndexOf (filter, StringComparison.CurrentCultureIgnoreCase);
			scoreB = repB.IndexOf (filter, StringComparison.CurrentCultureIgnoreCase);

			return scoreB - scoreA;
		}

		void IconDataFunc (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			string id, icon;
			CellRendererPixbuf renderer;

			renderer = cell as CellRendererPixbuf;
			id = (Model as ListStore).GetValue (iter, (int)Column.Id) as string;
			icon = PluginManager.IconForAddin (id);
			renderer.Pixbuf = IconProvider.PixbufFromIconName (icon, IconSize);
		}

		bool AddinShouldShow (AddinRepositoryEntry entry)
		{
			if (entry == null) throw new ArgumentNullException ("entry");
			return
				// Don't show addins that do not match the filter.
				entry.Addin.Name.ToLower ().Contains (filter.ToLower ()) &&
				// Make sure addin is allowed by current classifier.
				PluginManager.PluginClassifiesAs (entry, category);
		}

		bool AddinShouldShow (Addin addin)
		{
			if (addin == null) throw new ArgumentNullException ("addin");
			return
				// Don't show addins that do not match the filter.
				addin.Name.ToLower ().Contains (filter.ToLower ()) &&
				// Make sure addin is allowed by current classifier.
				PluginManager.PluginClassifiesAs (addin, category);
		}

		public void Refresh (bool checkRepositories)
		{
			ListStore store = Model as ListStore;
			// We use seen to deduplicate plugins, preferring non-repository
			// plugins to repository plugins (user-installed plugins override
			// the repository).
			ICollection<string> seen = new HashSet<string> ();
			SetupService setup = new SetupService (AddinManager.Registry);

			if (checkRepositories)
				setup.Repositories.UpdateAllRepositories (new ConsoleProgressStatus (true));
			store.Clear ();
			// Add non-repository plugins.
			foreach (Addin a in AddinManager.Registry.GetAddins ()) {
				if (seen.Contains (a.Id) || !AddinShouldShow (a)) continue;
				store.AppendValues (a.Enabled, Description (a), a.Id);
				seen.Add (a.Id);
			}
			// Add repository plugins.
			foreach (AddinRepositoryEntry e in setup.Repositories.GetAvailableAddins ()) {
				if (seen.Contains (e.Addin.Id) || !AddinShouldShow (e)) continue;
				store.AppendValues (AddinManager.Registry.IsAddinEnabled (e.Addin.Id),
					Description (e), e.Addin.Id);
				seen.Add (e.Addin.Id);
			}
			ScrollFirst (false);
		}

		public void ScrollFirst (bool selectFirst)
		{
			if (!IsRealized) return;

			ScrollToPoint (0, 0);
			if (selectFirst)
				Selection.SelectPath (TreePath.NewFirst ());
		}

		string Description (string name, string desc, string version)
		{
			return string.Format (DescriptionFormat, name, desc, version);
		}

		protected string Description (Addin a)
		{
			return Description (a.Name, a.Description.Description, a.Version);
		}

		protected string Description (AddinRepositoryEntry a)
		{
			return Description (a.Addin);
		}

		protected string Description (AddinHeader a)
		{
			return Description (a.Name, a.Description, a.Version);
		}

		public string [] GetSelectedAddins ()
		{
			string id;
			TreeIter iter;
			ListStore store;

			if (Selection.CountSelectedRows () == 0)
				return new string [0];

			store = Model as ListStore;
			Selection.GetSelected (out iter);
			id = store.GetValue (iter, (int)Column.Id) as string;
			return new [] { id };
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
			// Set the check state.
			store.SetValue (iter, (int)Column.Enabled, !enabled);
			// Flush the gui thread so the checkbox state changes right away.
			Services.Application.FlushMainThreadQueue ();

			// Notify subscribers.
			if (null != PluginToggled)
				PluginToggled (addinId, !enabled);
			// Set checked state again (don't assume enable/disable worked).
			store.SetValue (iter, (int)Column.Enabled,
				AddinManager.Registry.IsAddinEnabled (addinId));
		}

		protected void OnSelectionChanged (object sender, EventArgs args)
		{
			if (PluginSelected == null) return;
			
			PluginSelected (this, new PluginSelectionEventArgs (GetSelectedAddins ()));
		}
	}
}
