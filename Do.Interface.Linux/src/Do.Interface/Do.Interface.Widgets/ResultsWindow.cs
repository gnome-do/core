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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Gtk;
using Gdk;

using Do.Universe;
using Do.Platform.Linux;
using Do.Interface;

namespace Do.Interface.Widgets
{
	public delegate void OnSelectionChanged (object sender, 
	                                         ResultsWindowSelectionEventArgs args);

	public class ResultsWindow : Gtk.Window
	{
		protected int DefaultResultIconSize = 32;
		protected int DefaultWindowWidth = 352;
		protected int NumberResultsDisplayed = 6;
		
		protected int offset;
		
		public string ResultInfoFormat
		{
			set { resultInfoFormat = value; }
			get { return resultInfoFormat; }
		}
		
		protected string resultInfoFormat = "<b>{0}</b>\n<small>{1}</small>";
		const string QueryLabelFormat = "<b>{0}</b>";

		public event OnSelectionChanged SelectionChanged;

		protected enum Column {
			ItemColumn = 0,
			NameColumn = 1,
			NumberColumns = 2
		}

		protected ScrolledWindow resultsScrolledWindow;
		protected TreeView resultsTreeview;
		protected IList<Do.Universe.Item> results, stunted_results;
		protected int startResult, endResult;
		protected Frame frame;
		protected string query;
		protected Gdk.Color backgroundColor;
		protected VBox vbox;
		protected Toolbar toolbar;
		protected Label resultsLabel, queryLabel;
		protected IUIContext context = null;
		
		protected int cursor;
		protected int[] secondary = new int[0];
		
		protected bool pushedUpdate, clearing, update_needed = false;


		public ResultsWindow (Gdk.Color backgroundColor, int NumberResults) 
			: base (Gtk.WindowType.Toplevel)
		{
			this.backgroundColor = backgroundColor;
			this.NumberResultsDisplayed = NumberResults;
			
			Build ();
			results = new Do.Universe.Item[0];
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
			results = new Do.Universe.Item[0];
		}

		protected void NotifySelectionChanged ()
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

		protected virtual void Build ()
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
			resultsScrolledWindow.SetPolicy (PolicyType.Never, PolicyType.Never);
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
				typeof (Do.Universe.Item),				
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
						
		protected void IconDataFunc (TreeViewColumn column, CellRenderer cell, 
		                           TreeModel model, TreeIter iter)
		{			
			CellRendererPixbuf renderer = cell as CellRendererPixbuf;
			Do.Universe.Item o = (resultsTreeview.Model as ListStore).GetValue (iter, 0) as Do.Universe.Item;
			bool isSecondary = false;
			foreach (int i in secondary)
				if (model.GetStringFromIter (iter) == i.ToString ())
					isSecondary = true;
			
			Gdk.Pixbuf final;
			if (isSecondary) {
				using (Gdk.Pixbuf source = IconProvider.PixbufFromIconName (o.Icon, DefaultResultIconSize))
				using (Gdk.Pixbuf emblem = IconProvider.PixbufFromIconName ("gtk-add", DefaultResultIconSize)) {
					final = new Pixbuf (Colorspace.Rgb, 
					                    true, 
					                    8,
					                    DefaultResultIconSize,
					                    DefaultResultIconSize);
					
					source.CopyArea (0, 0, source.Width, source.Height, final, 0, 0);
					
					emblem.Composite (final, 
					                  0, 
					                  0, 
					                  DefaultResultIconSize, 
					                  DefaultResultIconSize, 
					                  0, 
					                  0, 
					                  1,
					                  1, 
					                  InterpType.Bilinear, 
					                  220);
				}
			} else {
				final = IconProvider.PixbufFromIconName (o.Icon, DefaultResultIconSize);
			}
			renderer.Pixbuf = final;
			final.Dispose ();
		}

		protected void OnResultRowSelected (object sender, EventArgs args)
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
			queryLabel.Markup = "";
			update_needed = false;
		}

		public IUIContext Context
		{
			set {
				context = value;
				if (!Visible) {
					update_needed = true;
					return;
				}
				
				pushedUpdate = true;
				if (value == null || !value.Results.Any ()) {
					Results = new Do.Universe.Item [0];
					return;
				}
				
				if (results.GetHashCode () != value.Results.GetHashCode ()) {
					results = value.Results;
				}
				
				startResult = value.Cursor - 5;
				
				if (startResult < 0)
					startResult = 0;
				endResult = startResult + 8;
				offset = startResult;
				
				if (endResult > results.Count)
					endResult = results.Count;
				
				Do.Universe.Item[] resultsArray = new Do.Universe.Item[endResult - startResult];
				Array.Copy (results.ToArray (), startResult, resultsArray, 0, resultsArray.Length); 
				
				cursor = value.Cursor - offset;
				
				Results = resultsArray;
				
				Query = value.Query;
				
				
				
				int[] secArray = new int[value.SecondaryCursors.Length];
				for (int i=0; i<secArray.Length; i++) {
					secArray[i] = value.SecondaryCursors[i] - offset;
				}
				
				secondary = secArray;
				
				
				UpdateCursors ();
				UpdateQueryLabel (value);
				resultsLabel.Markup = string.Format ("{1}/{0}", 
				                                     value.Results.Count, 
				                                     value.Cursor + 1);
				Gtk.Application.Invoke (delegate {
					pushedUpdate = false;
				});
			}
		}

		public int SelectedIndex
		{
			get { return cursor + offset; }
			set { 
				cursor = value - offset;
				
				UpdateCursors ();
			}
		}
			
		protected void UpdateQueryLabel (IUIContext context)
		{
			string query = context.Query;
			StringBuilder builder = new StringBuilder ();
			
			int count = 0;
			while (context.ParentContext != null && count < 2) {
				builder.Insert (0, context.ParentContext.Selection.Name + " > ");
				context = context.ParentContext;
				count++;
			}
			queryLabel.Markup = string.Format ("{0}<b>{1}</b>", 
			                                   GLib.Markup.EscapeText (builder.ToString ()),
			                                   GLib.Markup.EscapeText (query)); 
		}
		
		private void UpdateCursors () 
		{

			Gtk.TreePath path;
			
			path = new TreePath (cursor.ToString ());
			
			//makes this just a tiny bit smoother overall
			Gtk.Application.Invoke (delegate {
				resultsTreeview.Selection.UnselectAll ();
				resultsTreeview.Selection.SelectPath (path);
				resultsTreeview.ScrollToCell (path, null, true, 0.5F, 0.0F);
			});
		}
		
		public Do.Universe.Item SelectedObject
		{
			get {
				try {
					return results [SelectedIndex];
				} catch {
					return null;
				}
			}
		}
		
		public IList<Do.Universe.Item> Results
		{
			get {
				if (stunted_results == null)
					stunted_results = new List<Do.Universe.Item> (0);
				return stunted_results;
			}
			set {
				stunted_results = value;
				//some memory hacks.
				foreach (CellRenderer rend in resultsTreeview.Columns[0].CellRenderers) {
					if (rend is CellRendererPixbuf && (rend as CellRendererPixbuf).Pixbuf != null) {
						(rend as CellRendererPixbuf).Pixbuf.Dispose ();
					}
					rend.Dispose ();
				}
				
				ListStore store;
				string info;

				clearing = true;
				Gtk.Application.Invoke (delegate {
					store = resultsTreeview.Model as ListStore;
					store.Clear ();
					
					foreach (Do.Universe.Item result in value) {					
						
						info = string.Format (ResultInfoFormat, 
						                      GLib.Markup.EscapeText (result.Name), 
						                      GLib.Markup.EscapeText (result.Description)); 
						store.AppendValues (new object[] {
							result,
							info,
						});
						
					}
					clearing = false;
				});
//				UpdateCursors ();
			}
		}


		public string Query
		{
			set {
				query = value ?? "";
				queryLabel.Markup = string.Format (QueryLabelFormat, GLib.Markup.EscapeText (query)); 
			}
			get { return query; }
		}
		
		protected void OnShown (object o, EventArgs args)
		{
			if (update_needed)
				Context = context;
			update_needed = false;
		}

		// Draw a border around the window.
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Cairo.Context cairo;
			
			using (cairo = Gdk.CairoHelper.Create (GdkWindow)) {
				cairo.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
				cairo.SetSourceRGBA (1.0, 1.0, 1.0, 0.0);
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
