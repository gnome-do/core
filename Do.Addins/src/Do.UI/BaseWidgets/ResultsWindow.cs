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
using System.Text;

using Gtk;
using Gdk;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	public delegate void OnSelectionChanged (object sender, 
	                                         ResultsWindowSelectionEventArgs args);

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
		Frame frame;
		string query;
		Gdk.Color backgroundColor;
		VBox vbox;
		Toolbar toolbar;
		Label resultsLabel, queryLabel;
		
		int cursor;
		int[] secondary = new int[0];
		
		bool pushedUpdate, clearing = false;


		public ResultsWindow (Gdk.Color backgroundColor, int NumberResults) 
			: base (Gtk.WindowType.Toplevel)
		{
			this.backgroundColor = backgroundColor;
			this.NumberResultsDisplayed = NumberResults;
			
			Build ();
			results = new IObject[0];
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
			results = new IObject[0];
		}

		private void NotifySelectionChanged ()
		{
			TreeIter iter;
			if (!resultsTreeview.Selection.GetSelected (out iter)) return;
			try {
				cursor = Convert.ToInt32 (resultsTreeview.Model.GetStringFromIter (iter));
			} catch { return; }
			
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
			HBox hbox;

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
			vbox.BorderWidth = 3;
			vbox.Show ();
			
			//---------The breadcrum bar---------
			hbox = new HBox ();
			toolbar = new Toolbar ();
			align = new Alignment (0, .5f, 0, 0);
			resultsLabel = new Label ();
			queryLabel = new Label ();
			align.Add (queryLabel);
			hbox.PackStart (align, true, true, 4);
			hbox.PackStart (resultsLabel, false, false, 0);
			hbox.WidthRequest = DefaultWindowWidth - 10;
			toolbar.Add (hbox);
			toolbar.ShowAll ();
			vbox.PackStart (toolbar, false, false, 0);
			
			
			//---------Results Window
			resultsScrolledWindow = new ScrolledWindow ();
			resultsScrolledWindow.SetPolicy (PolicyType.Never, PolicyType.Always);
			resultsScrolledWindow.ShadowType = ShadowType.None;
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
			                     (height + 2) * NumberResultsDisplayed + 
			                     (int) (vbox.BorderWidth * 2) + 20);
			
			cell = new CellRendererText ();
			(cell as CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
			column.PackStart (cell, true);
			column.AddAttribute (cell, "markup", (int) Column.NameColumn);
			
			resultsTreeview.AppendColumn (column);


			resultsTreeview.Selection.Changed += OnResultRowSelected;
			Shown += OnShown;
		}
		
			
		public void UpdateColors (Gdk.Color backgroundColor)
		{
			this.backgroundColor = backgroundColor;
			frame.FillColor = backgroundColor;
		}
						
		private void IconDataFunc (TreeViewColumn column, CellRenderer cell, 
		                           TreeModel model, TreeIter iter)
		{			
			CellRendererPixbuf renderer = cell as CellRendererPixbuf;
			IObject o = (resultsTreeview.Model as ListStore).GetValue (iter, 0) as IObject;
			bool isSecondary = false;
			foreach (int i in secondary)
				if (model.GetStringFromIter (iter) == i.ToString ())
					isSecondary = true;
			
			Gdk.Pixbuf final;
			if (isSecondary) {
				final = 
					IconProvider.PixbufFromIconName (o.Icon, DefaultResultIconSize);
				Gdk.Pixbuf emblem = 
					IconProvider.PixbufFromIconName ("gtk-add", DefaultResultIconSize / 2);
				
				emblem.Composite (final, 
				                  0, 
				                  0, 
				                  DefaultResultIconSize / 2, 
				                  DefaultResultIconSize / 2, 
				                  0, 
				                  0, 
				                  1,
				                  1, 
				                  InterpType.Bilinear, 
				                  255);
				emblem.Dispose ();
				
			} else {
				final = IconProvider.PixbufFromIconName (o.Icon, DefaultResultIconSize);
			}
			renderer.Pixbuf = final;
			final.Dispose ();
		}

		private void OnResultRowSelected (object sender, EventArgs args)
		{
			if (!clearing && !pushedUpdate) {
				NotifySelectionChanged ();
			}
		}

		public virtual void Clear ()
		{
			(resultsTreeview.Model as ListStore).Clear ();
			cursor = 0;
			resultsLabel.Markup = "--/--";
			queryLabel.Markup = string.Empty;
		}

		public IUIContext Context
		{
			set {
				pushedUpdate = true;
				if (value == null || value.Results.Length == 0) {
					Results = new IObject [0];
					return;
				}
				
				if (results.GetHashCode () != value.Results.GetHashCode ()) {
					results = value.Results;
					Results = results;
				}
				
				Query = value.Query;
				
				cursor = value.Cursor;// - offset;

				
				secondary = value.SecondaryCursors;
				
				
				UpdateCursors ();
				
				UpdateQueryLabel (value);
				
				resultsLabel.Markup = string.Format ("{1}/{0}", 
				                                     value.Results.Length, 
				                                     value.Cursor + 1);
				
				Gtk.Application.Invoke (delegate {
					pushedUpdate = false;
				});
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
			
		private void UpdateQueryLabel (IUIContext context)
		{
			string query = context.Query;
			StringBuilder builder = new StringBuilder ();
			
			int count = 0;
			while (context.ParentContext != null && count < 2) {
				builder.Insert (0, context.ParentContext.Selection.Name + " > ");
				context = context.ParentContext;
				count++;
			}
			queryLabel.Markup = string.Format ("{0}<b>{1}</b>", builder.ToString (), query);
		}
		
		private void UpdateCursors () 
		{
			resultsTreeview.Selection.UnselectAll ();

			Gtk.TreePath path;
			
			path = new TreePath (cursor.ToString ());
			
			//makes this just a tiny bit smoother overall
			Gtk.Application.Invoke (delegate {
				resultsTreeview.ScrollToCell (path, null, false, 0.5F, 0.0F);
				resultsTreeview.Selection.SelectPath (path);
			});
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
			//get { return results ?? results = new IObject[0]; }
			set {
				ListStore store;
				string info;

				clearing = true;
				Clear ();
				store = resultsTreeview.Model as ListStore;

				foreach (IObject result in value) {					
					
					info = string.Format (ResultInfoFormat, result.Name, result.Description);
					info = Util.Appearance.MarkupSafeString (info);
					store.AppendValues (new object[] {
						result,
						info,
					});
							
				}
				clearing = false;
			}
		}


		public string Query
		{
			set {
				query = value;
				queryLabel.Markup = string.Format (QueryLabelFormat, value ?? string.Empty);
			}
			get { return query; }
		}
		
		protected void OnShown (object o, EventArgs args)
		{
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
			colormap.Dispose ();
		}
	}
}
