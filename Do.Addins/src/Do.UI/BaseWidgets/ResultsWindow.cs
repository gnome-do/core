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
		Label queryLabel;
		Frame frame;
		string query;
		Gdk.Color backgroundColor;
		VBox vbox;
		
		int cursor;
		int[] secondary = new int[0];


		public ResultsWindow (Gdk.Color backgroundColor, int NumberResults) 
			: base (Gtk.WindowType.Toplevel)
		{
			this.backgroundColor = backgroundColor;
			this.NumberResultsDisplayed = NumberResults;
			
			Build ();
			results = null;
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
			//resultsTreeview.Selection.Mode = SelectionMode.Multiple;
			
			resultsScrolledWindow.Add (resultsTreeview);
			resultsTreeview.Show ();

			resultsTreeview.Model = new ListStore (new Type[] {
				typeof (IObject),				
				typeof (string), 
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
				(4 + height) * NumberResultsDisplayed + 10);
			
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
			bool isSecondary = false;
			foreach (int i in secondary)
				if (model.GetStringFromIter (iter) == i.ToString ())
					isSecondary = true;
			
			
			if (isSecondary) {
				Gdk.Pixbuf source = IconProvider.PixbufFromIconName (o.Icon, DefaultResultIconSize);
				Gdk.Pixbuf emblem = IconProvider.PixbufFromIconName ("gtk-add", DefaultResultIconSize / 2);
				Gdk.Pixbuf dest = new Pixbuf (Colorspace.Rgb, true, 8, DefaultResultIconSize, DefaultResultIconSize);
				
				source.Composite (dest, 
				                  0, 
				                  0, 
				                  DefaultResultIconSize, 
				                  DefaultResultIconSize, 
				                  0, 
				                  0, 
				                  1,
				                  1, 
				                  InterpType.Bilinear, 
				                  255);
				
				emblem.Composite (dest, 
				                  0, 
				                  0, 
				                  DefaultResultIconSize, 
				                  DefaultResultIconSize, 
				                  0, 
				                  0, 
				                  1,
				                  1, 
				                  InterpType.Bilinear, 
				                  255);
				
				
				renderer.Pixbuf = dest;
			} else {
				renderer.Pixbuf = IconProvider.PixbufFromIconName (o.Icon, DefaultResultIconSize);
			}
		}

		private void OnResultRowSelected (object sender, EventArgs args)
		{
			int temp;
			//cleanup needed
			try {
				temp = resultsTreeview.Selection.GetSelectedRows()[0].Indices[0];
			} catch {
				return;
			}
			
			if (temp == cursor)	return;		
			
			if (resultsTreeview.Selection.GetSelectedRows().Length == 1) {
				cursor = temp;
				NotifySelectionChanged ();
			}
		}

		public virtual void Clear ()
		{
			(resultsTreeview.Model as ListStore).Clear ();
			cursor = 0;
			results = null;
		}

		public IUIContext Context
		{
			set {
				if (value == null || value.Results.Length == 0) return;
				
				if (Results.GetHashCode () != value.Results.GetHashCode ()) {
					Results = value.Results;
					Query = value.Query;
				}
				cursor = value.Cursor;
				secondary = value.SecondaryCursors;
				
				UpdateCursors ();
			}
		}

		public int SelectedIndex
		{
			get { return cursor; }
			set { 
				cursor = value;
				
				UpdateCursors ();
			}
		}
		
		private void UpdateCursors () 
		{
			resultsTreeview.Selection.UnselectAll ();

			Gtk.TreePath path;
			/*foreach (int i in secondary) {
				path = new TreePath (i.ToString ());
				resultsTreeview.Selection.SelectPath (path);
			}*/
			
			path = new TreePath (cursor.ToString ());
			resultsTreeview.Selection.SelectPath (path);
			resultsTreeview.ScrollToCell (path, null, false, 0.0F, 0.0F);
		}
		
		public IObject SelectedObject
		{
			get {
				try {
					return results[SelectedIndex];
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
