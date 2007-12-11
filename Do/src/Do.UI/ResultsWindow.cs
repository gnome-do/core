/* ResultsWindow.cs
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
using System.Runtime.InteropServices;

using Gtk;
using Gdk;

using Do.Universe;
using Do.Addins;

namespace Do.UI
{
	public delegate void OnSelectionChanged (object sender, ResultsWindowSelectionEventArgs args);

	public class ResultsWindowSelectionEventArgs : EventArgs
	{
		int index;
		IObject selection;

		public ResultsWindowSelectionEventArgs (int index, IObject selection)
		{
			this.index = index;
			this.selection = selection;
		}

		public int SelectedIndex
		{
			get { return index; }
		}

		public IObject SelectedObject
		{
			get {
				return selection;
			}
		}
	}

	public class ResultsWindow : Gtk.Window
	{
		const int ResultIconSize = 32;
		const int NumberResultsDisplayed = 6;
		const string ResultInfoFormat = "<b>{0}</b>\n<small>{1}</small>";

		public event OnSelectionChanged SelectionChanged;

		protected enum Column {
			ItemColumn = 0,
			PixbufColumn = 1,
			NameColumn = 2,
			NumberColumns = 3
		}

		ScrolledWindow resultsScrolledWindow;
		TreeView resultsTreeview;
		IObject[] results;
		int selectedIndex;
		bool selectedIndexSet;
		bool quietSelectionChange;

		public ResultsWindow () : base (Gtk.WindowType.Toplevel)
		{
			Build ();
			results = null;
			selectedIndex = 0;
			selectedIndexSet = false;
			SelectionChanged = OnSelectionChangedEvent;
			Shown += OnShown;
		}

		protected virtual void OnSelectionChangedEvent (object sender, ResultsWindowSelectionEventArgs args)
		{
		}

		protected virtual void OnShown (object sender, EventArgs args)
		{
			int savedSelectedIndex;    // setting Results calls Clear(), resets this.

			// Do this to load the icons.
			savedSelectedIndex = selectedIndex;
			quietSelectionChange = true;
			Results = results;
			selectedIndexSet = false;
			SelectedIndex = savedSelectedIndex;
			quietSelectionChange = false;
		}

		private void NotifySelectionChanged ()
		{
			ResultsWindowSelectionEventArgs args;

			args = new ResultsWindowSelectionEventArgs (SelectedIndex, SelectedObject);
			SelectionChanged (this, args);
		}

		protected void Build ()
		{
			VBox           vbox;
			TreeViewColumn column;
			CellRenderer   cell;

			KeepAbove = true;
			AppPaintable = true;
			AcceptFocus = false;
			// This typehint gets the window to raise all the way to top.
			TypeHint = WindowTypeHint.Splashscreen;

			vbox = new VBox (false, 0);
			Add (vbox);
			vbox.BorderWidth = 4;
			vbox.SetSizeRequest (360, (ResultIconSize + 9) * NumberResultsDisplayed);
			vbox.Show ();

			resultsScrolledWindow = new ScrolledWindow ();
			resultsScrolledWindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			resultsScrolledWindow.ShadowType = ShadowType.In;
			vbox.PackStart (resultsScrolledWindow, true, true, 0);
			resultsScrolledWindow.Show ();

			resultsTreeview = new TreeView ();
			resultsTreeview.EnableSearch = false;
			resultsTreeview.HeadersVisible = false;
			resultsScrolledWindow.Add (resultsTreeview);
			resultsTreeview.Show ();

			resultsTreeview.Model = new ListStore (new Type[] {
				typeof (IObject),
				typeof (Pixbuf),
				typeof (string)
			});

			column = new TreeViewColumn ();

			cell = new CellRendererPixbuf ();
			cell.SetFixedSize (-1, 4 + ResultIconSize - (int) cell.Ypad);
			column.PackStart (cell, false);
			column.AddAttribute (cell, "pixbuf", (int) Column.PixbufColumn);

			cell = new CellRendererText ();
			(cell as CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
			column.PackStart (cell, true);
			column.AddAttribute (cell, "markup", (int) Column.NameColumn);

			resultsTreeview.AppendColumn (column);

			resultsTreeview.Selection.Changed += OnResultRowSelected;
		}

		public virtual void SelectNext ()
		{
			if (SelectedIndex < results.Length - 1) {
				SelectedIndex++;
			}
		}

		public virtual void SelectPrev ()
		{
			if (0 < SelectedIndex) {
			  SelectedIndex--;
			}
		}

		private void OnResultRowSelected (object sender, EventArgs args)
		{
			if (!selectedIndexSet || quietSelectionChange) return;
			if (resultsTreeview.Selection.GetSelectedRows().Length > 0) {
				selectedIndex = resultsTreeview.Selection.GetSelectedRows()[0].Indices[0];
				NotifySelectionChanged ();
			}
		}

		public virtual void Clear ()
		{
			(resultsTreeview.Model as ListStore).Clear ();
			selectedIndex = 0;
			selectedIndexSet = false;
			results = null;
		}

		public int SelectedIndex
		{
			get { return selectedIndex; }
			set {
				TreeModel model;
				TreeIter iter;
				TreePath path;
				int new_selection;

				if (selectedIndexSet &&
				    value == selectedIndex) return;

				selectedIndexSet = true;

				if (results.Length == 0)
					return;
				else if (value >= results.Length)
					new_selection = results.Length - 1;
				else if (value < 0)
					new_selection = 0;
				else
					new_selection = value;

				resultsTreeview.Selection.GetSelected (out model, out iter);
				path = model.GetPath (iter);
				// TODO: Just jump to new index instead of iterating like this.
				while (new_selection > selectedIndex) {
					selectedIndex++;
					path.Next ();
				}
				while (new_selection < selectedIndex) {
					selectedIndex--;
					path.Prev ();
				}
				resultsTreeview.Selection.SelectPath (path);
				resultsTreeview.ScrollToCell (path, null, false, 0.0F, 0.0F);
			}
		}

		public IObject SelectedObject
		{
			get {
				if (results != null && 0 <= selectedIndex && selectedIndex < results.Length) {
					return results[selectedIndex];
				} else {
					return null;
				}
			}
		}

		public IObject[] Results
		{
			get { return results; }
			set {
				ListStore store;
				TreeIter iter, first_iter;
				bool seen_first;
				Pixbuf icon;
				string info;

				Clear ();
				results = (value == null ? new IObject[0] : value);
				store = resultsTreeview.Model as ListStore;
				first_iter = default (TreeIter);
				seen_first = false;

				foreach (IObject result in results) {
					if (Visible)
						icon = Util.Appearance.PixbufFromIconName (result.Icon, ResultIconSize);
					else
						icon = Util.Appearance.PixbufFromIconName ("empty", ResultIconSize);

					info = string.Format (ResultInfoFormat, result.Name, result.Description);
					info = Util.Appearance.MarkupSafeString (info);
					iter = store.AppendValues (new object[] {
						result,
						icon,
						info
					});
					if (!seen_first) {
						first_iter = iter;
						seen_first = true;
					}
				}
				if (seen_first) {
					resultsTreeview.ScrollToCell (resultsTreeview.Model.GetPath (first_iter),
					                              null, false, 0.0F, 0.0F);
					resultsTreeview.Selection.SelectIter (first_iter);
				}
			}
		}

		// Draw a border around the window.
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Cairo.Context cairo;
			Gdk.Color border_color;

			using (cairo = Gdk.CairoHelper.Create (GdkWindow)) {
				cairo.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
				border_color = Style.Dark (StateType.Normal);
				cairo.Color = new Cairo.Color ((double) border_color.Red / ushort.MaxValue,
				                               (double) border_color.Green / ushort.MaxValue,
				                               (double) border_color.Blue / ushort.MaxValue,
				                               1.0);
				cairo.Operator = Cairo.Operator.Source;
				cairo.Stroke ();
			}

			return base.OnExposeEvent (evnt);
		}

	}


}
