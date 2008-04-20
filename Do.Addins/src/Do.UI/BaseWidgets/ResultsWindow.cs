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

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	public delegate void OnSelectionChanged (object sender, ResultsWindowSelectionEventArgs args);

	public class ResultsWindow : Gtk.Window
	{
		private int DefaultResultIconSize = 32;
		private int DefaultWindowWidth = 352;
		private int NumberResultsDisplayed = 6;
		
		public string ResultInfoFormat
		{
			set { resultInfoFormat = value; }
			get { return resultInfoFormat; }
		}
		
		private string resultInfoFormat = "<b>{0}</b>\n<small>{1}</small>";
		const string QueryLabelFormat = "<b>{0}</b>";

		public event OnSelectionChanged SelectionChanged;

		protected enum Column {
			ItemColumn = 0,
			NameColumn = 1,
			NumberColumns = 2
		}

		ScrolledWindow resultsScrolledWindow;
		TreeView resultsTreeview;
		IObject[] results;
		int selectedIndex;
		bool selectedIndexSet;
		bool quietSelectionChange;
		Label queryLabel;
		Frame frame;
		string query;
		Gdk.Color backgroundColor;
		VBox vbox;


		public ResultsWindow (Gdk.Color backgroundColor, int NumberResults) 
			: base (Gtk.WindowType.Toplevel)
		{
			this.backgroundColor = backgroundColor;
			this.NumberResultsDisplayed = NumberResults;
			
			Build ();
			results = null;
			selectedIndex = 0;
			selectedIndexSet = false;
			Shown += OnShown;
		}
		
		public ResultsWindow (Gdk.Color backgroundColor, int DefaultIconSize, 
		                      int WindowWidth, int NumberResults) 
			: base (Gtk.WindowType.Toplevel)
		{
			this.backgroundColor = backgroundColor;
			this.DefaultResultIconSize = DefaultIconSize;
			this.DefaultWindowWidth = WindowWidth;
			this.NumberResultsDisplayed = NumberResults;
			
			Build ();
			results = null;
			selectedIndex = 0;
			selectedIndexSet = false;
			Shown += OnShown;
		}

		protected virtual void OnShown (object sender, EventArgs args)
		{
			// setting Results calls Clear(), resets this.
			int savedSelectedIndex;    

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
			if (null != SelectionChanged) {
				SelectionChanged (this, args);
			}
		}

		protected void Build ()
		{
			Alignment align;
			TreeViewColumn column;
			CellRenderer   cell;

			KeepAbove = true;
			AppPaintable = true;
			AcceptFocus = false;
			// This typehint gets the window to raise all the way to top.
			TypeHint = WindowTypeHint.Splashscreen;
			
			SetColormap ();
			
			frame = new Frame ();
			frame.DrawFill = true;
			frame.DrawFrame = true;
			frame.FillColor = frame.FrameColor = backgroundColor;
			frame.FillAlpha = .55;
			frame.FrameAlpha = .7;
			frame.Radius = 0;
			frame.Show ();
			
			vbox = new VBox (false, 0);
			Add (frame);
			frame.Add (vbox);
			vbox.BorderWidth = 4;
			vbox.Show ();

			align = new Alignment (0.0F, 0.0F, 0, 0);
			align.SetPadding (1, 2, 1, 1);
			queryLabel = new Label ();
			queryLabel.UseMarkup = true;
			queryLabel.SingleLineMode = true;
			align.Add (queryLabel);
			vbox.PackStart (align, false, false, 0);
			// queryLabel.Show ();
			// align.Show ();
			
			resultsScrolledWindow = new ScrolledWindow ();
			resultsScrolledWindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			resultsScrolledWindow.ShadowType = ShadowType.In;
			vbox.PackStart (resultsScrolledWindow, true, true, 0);
			resultsScrolledWindow.Show ();

			resultsTreeview = new TreeView ();
			resultsTreeview.EnableSearch = false;
			resultsTreeview.HeadersVisible = false;
			// If this is not set the tree will call IconDataFunc for all rows to 
			// determine the total height of the tree
			resultsTreeview.FixedHeightMode = true;
			
			resultsScrolledWindow.Add (resultsTreeview);
			resultsTreeview.Show ();

			resultsTreeview.Model = new ListStore (new Type[] {
				typeof (IObject),				
				typeof (string)
			});

			column = new TreeViewColumn ();			
			column.Sizing = Gtk.TreeViewColumnSizing.Fixed; 
				// because resultsTreeview.FixedHeightMode = true:  
				
			cell = new CellRendererPixbuf ();				
			cell.SetFixedSize (-1, 4 + DefaultResultIconSize - (int) cell.Ypad);

			int width, height;
			cell.GetFixedSize (out width, out height);
				
			column.PackStart (cell, false);
			column.SetCellDataFunc (cell, new TreeCellDataFunc (IconDataFunc));
				
			vbox.SetSizeRequest (DefaultWindowWidth, 
				(height + 4) * NumberResultsDisplayed + 10);
			
			cell = new CellRendererText ();
			(cell as CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
			column.PackStart (cell, true);
			column.AddAttribute (cell, "markup", (int) Column.NameColumn);
			
			resultsTreeview.AppendColumn (column);

			resultsTreeview.Selection.Changed += OnResultRowSelected;
		}
			
		public void UpdateColors (Gdk.Color backgroundColor)
		{
			this.backgroundColor = backgroundColor;
			frame.FillColor = backgroundColor;
		}
						
		private void IconDataFunc (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{			
			CellRendererPixbuf renderer = cell as CellRendererPixbuf;
			IObject o = (resultsTreeview.Model as ListStore).GetValue (iter, 0) as IObject;
			renderer.Pixbuf = IconProvider.PixbufFromIconName (o.Icon, DefaultResultIconSize);
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
			// Query = "";
		}

		public SearchContext Context
		{
			set {
				if (value == null) return;

				if (Results.GetHashCode () != value.Results.GetHashCode ()) {
					Results = value.Results;
					Query = value.Query;
				}
				SelectedIndex = value.Cursor;
			}
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

				// Don't bother updating widgets if we're not visible -
				// instead, do these things on Show.
				if (!Visible) {
					selectedIndex = value;
					return;
				}

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
				try {
					return results[selectedIndex];
				} catch {
					return null;
				}
			}
		}

		public IObject[] Results
		{
			//Needed for hashing
			get { return results ?? results = new IObject[0]; }
			set {				
				ListStore store;
				TreeIter iter, first_iter;
				bool seen_first;				
				string info;

				Clear ();
				results = value ?? new IObject[0];
				store = resultsTreeview.Model as ListStore;
				first_iter = default (TreeIter);
				seen_first = false;

				// Don't bother updating widgets if we're not visible -
				// instead, do these things on Show.
				if (!Visible) return;

				foreach (IObject result in results) {					
					
					info = string.Format (ResultInfoFormat, result.Name, result.Description);
					info = Util.Appearance.MarkupSafeString (info);
					iter = store.AppendValues (new object[] {
						result,
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

		public string Query
		{
			set {
				query = value;
				queryLabel.Markup = string.Format (QueryLabelFormat, value ?? "");
			}
			get { return query; }
		}

		// Draw a border around the window.
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Cairo.Context cairo;
			
			using (cairo = Gdk.CairoHelper.Create (GdkWindow)) {
				cairo.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
				cairo.Color = new Cairo.Color (1.0, 1.0, 1.0, 0.0);
				cairo.Operator = Cairo.Operator.Source;
				cairo.Paint ();
			}

			return base.OnExposeEvent (evnt);
		}
		
		protected virtual void SetColormap ()
		{
			Gdk.Colormap  colormap;

			colormap = Screen.RgbaColormap;
			if (colormap == null) {
				colormap = Screen.RgbColormap;
				Console.Error.WriteLine ("No alpha support.");
			}
			Colormap = colormap;
		}
	}
}
