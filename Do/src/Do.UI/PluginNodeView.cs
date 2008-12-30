/* PluginNodeView.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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
using System.Threading;
using System.Collections.Generic;

using Gtk;
using Mono.Unix;
using Mono.Addins;
using Mono.Addins.Setup;

using Do.Core;
using Do.Platform;
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
		const int WrapWidth = 324;
		const string DescriptionFormat = "<b>{0}</b> <small>v{2}</small>\n<small>{1}</small>";

		protected string filter;
		protected string category;

		public string Filter {
			get { return filter; }
			set {
				filter = value ?? "";
				Refresh ();
			}
		}

		public string ShowCategory {
			get { return category; }
			set {
				category = value;
				Refresh ();
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
			AppendColumn ("Icon", cell, new TreeCellDataFunc (IconDataFunc));

			cell = new Gtk.CellRendererText ();
			(cell as CellRendererText).WrapWidth = WrapWidth;
			(cell as CellRendererText).WrapMode = Pango.WrapMode.Word;
			AppendColumn ("Plugin", cell, "markup", Column.Description);

			store.SetSortFunc ((int) Column.Id, new TreeIterCompareFunc (DefaultTreeIterCompareFunc));
			store.SetSortColumnId ((int) Column.Id, SortType.Descending);

			Selection.Changed += OnSelectionChanged;

			Refresh ();
		}

		public int DefaultTreeIterCompareFunc(TreeModel model, TreeIter a, TreeIter b)
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

		protected virtual void IconDataFunc (TreeViewColumn column,
			CellRenderer cell, TreeModel model, TreeIter iter)
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
			
			// Don't show addins that do not match the filter.
			if (!entry.Addin.Name.ToLower ().Contains (filter.ToLower ()))
			    return false;
			// Make sure addin is allowed by current classifier.
			if (!PluginManager.PluginClassifiesAs (entry, category))
				return false;

			return true;
		}

		public virtual void Refresh ()
		{
			ListStore store;
			SetupService setup;

			store = Model as ListStore;
			setup = new SetupService (AddinManager.Registry);
			
			store.Clear ();
			setup.Repositories.UpdateAllRepositories (new ConsoleProgressStatus (true));
			foreach (AddinRepositoryEntry e in setup.Repositories.GetAvailableAddins ()) {
				if (!AddinShouldShow (e)) continue;
				store.AppendValues (
					AddinManager.Registry.IsAddinEnabled (e.Addin.Id),
					Description (e),
					e.Addin.Id);
			}
			ScrollFirst (false);
		}

		void ScrollFirst (bool select)
		{
			if ((Model as ListStore).Data.Count == 0) return;
			ScrollToCell (TreePath.NewFirst (), Columns [0], true, 0, 0);
			if (select) Selection.SelectPath (TreePath.NewFirst ());
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

		public string [] GetSelectedAddins () {
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
			store.SetValue (iter, (int)Column.Enabled, !enabled);
			
			if (null != PluginToggled) {
				PluginToggled (addinId, !enabled);
			}
			store.SetValue (iter, (int)Column.Enabled,
					AddinManager.Registry.IsAddinEnabled (addinId));
		}

		protected void OnSelectionChanged (object sender, EventArgs args)
		{
			if (null != PluginSelected) {
				PluginSelected (this, new PluginSelectionEventArgs (GetSelectedAddins ()));
			}
		}
		
		public event PluginToggledDelegate PluginToggled;
		public event PluginSelectedDelegate PluginSelected;

		public delegate void PluginToggledDelegate (string id, bool enabled);
		public delegate void PluginSelectedDelegate (object sender, PluginSelectionEventArgs args);
	}
}
