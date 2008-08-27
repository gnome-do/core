// BezelResultsWindow.cs
// 
// Copyright (C) 2008 GNOME-Do
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
using System.Runtime.InteropServices;
using System.Text;

using Cairo;
using Gtk;
using Gdk;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	
	
	public class BezelResultsWindow : Gtk.Window
	{
		protected int DefaultResultIconSize = 32;
		protected int NumberResultsDisplayed = 5;
		protected int offset;
		
		public string ResultInfoFormat
		{
			set { resultInfoFormat = value; }
			get { return resultInfoFormat; }
		}
		
		protected string resultInfoFormat = "<b>{0}</b>\n<small>{1}</small>";
		const string QueryLabelFormat = "<b>{0}</b>";
		
		const int WindowRadius = 6;
		const uint TitleBarHeight = 22;

//		public event OnSelectionChanged SelectionChanged;

		protected enum Column {
			ItemColumn = 0,
			NameColumn = 1,
			NumberColumns = 2
		}

		protected ScrolledWindow resultsScrolledWindow;
		protected TreeView resultsTreeview;
		protected IObject[] results, stunted_results;
		protected int startResult, endResult;
		protected string query;
		protected Label resultsLabel, queryLabel;
		protected IUIContext context = null;
		
		protected int cursor;
		protected int[] secondary = new int[0];
		
		protected bool pushedUpdate, clearing, update_needed = false;
		
		protected Surface buffered_surface;
		
		public BezelResultsWindow () : base (Gtk.WindowType.Toplevel)
		{
			Build ();
			results = new IObject[0];
		}
		
		protected virtual void Build ()
		{
			AppPaintable = true;
			KeepAbove = true;
			Decorated = false;
			AcceptFocus = false;
			
			TypeHint = WindowTypeHint.Splashscreen;
			Gdk.Colormap  colormap;

			colormap = Screen.RgbaColormap;
			if (colormap == null) {
				colormap = Screen.RgbColormap;
				Console.Error.WriteLine ("No alpha support.");
			}
			
			Colormap = colormap;
			colormap.Dispose ();
			
			TreeViewColumn column;
			CellRenderer   cell;
			HBox hbox;
			VBox vbox;
			
			vbox = new VBox ();
			vbox.Show ();
			Add(vbox);
			
			//---------Top Spacer
			vbox.PackStart (new HBox (), false, false, 10);

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
				
			cell = new CellRendererText ();
			(cell as CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
			column.PackStart (cell, true);
			column.AddAttribute (cell, "markup", (int) Column.NameColumn);
			
			resultsTreeview.AppendColumn (column);

//			resultsTreeview.Selection.Changed += OnResultRowSelected;
			Shown += OnShown;
			
			HeightRequest = height * NumberResultsDisplayed + 25 + (int) TitleBarHeight;
			
			//---------The breadcrum bar---------
			hbox = new HBox ();
			resultsLabel = new Label ();
			queryLabel = new Label ();
			hbox.PackStart (queryLabel, false, false, 4);
			hbox.PackStart (new HBox (), true, true, 0);
			hbox.PackStart (resultsLabel, false, false, 4);
			vbox.PackStart (hbox, false, false, 0);
			
			resultsTreeview.ModifyText (StateType.Normal, new Gdk.Color (0xff, 0xff, 0xff));
			resultsTreeview.ModifyBase (StateType.Normal, new Gdk.Color (0x00, 0x00, 0x00));
			
			resultsLabel.ModifyFg (StateType.Normal, new Gdk.Color (0xff, 0xff, 0xff));
			queryLabel.ModifyFg (StateType.Normal, new Gdk.Color (0xff, 0xff, 0xff));
			
			vbox.ShowAll ();
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Context cr2 = Gdk.CairoHelper.Create (GdkWindow);
			if (buffered_surface == null) {
				Gdk.Rectangle geo;
				GetSize (out geo.Width, out geo.Height);
				
				geo.X = geo.Y = 0;
				
				buffered_surface = cr2.Target.CreateSimilar (cr2.Target.Content, geo.Width, geo.Height);
				Context cr = new Context (buffered_surface);
				cr.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
				cr.Color = new Cairo.Color (0, 0, 0, 0);
				cr.Operator = Operator.Source;
				cr.Fill ();
				
				
				
				cr.MoveTo (geo.X + WindowRadius, geo.Y);
				cr.Arc (geo.X + geo.Width - WindowRadius, geo.Y + WindowRadius, WindowRadius, Math.PI*1.5, Math.PI*2);
				cr.LineTo (geo.X + geo.Width, geo.Y + geo.Height);
				cr.LineTo (geo.X, geo.Y + geo.Height);
				cr.Arc (geo.X + WindowRadius, geo.Y + WindowRadius, WindowRadius, Math.PI, Math.PI*1.5);
				cr.Color = new Cairo.Color (.15, .15, .15, .95);
				cr.Fill ();
				
				cr.MoveTo (geo.X + WindowRadius, geo.Y);
				cr.Arc (geo.X + geo.Width - WindowRadius, geo.Y + WindowRadius, WindowRadius, Math.PI*1.5, Math.PI*2);
				cr.LineTo (geo.X + geo.Width, geo.Y + TitleBarHeight);
				cr.LineTo (geo.X, geo.Y + TitleBarHeight);
				cr.Arc (geo.X + WindowRadius, geo.Y + WindowRadius, WindowRadius, Math.PI, Math.PI*1.5);
				LinearGradient title_grad = new LinearGradient (0, 0, 0, TitleBarHeight);
				title_grad.AddColorStop (0.0, new Cairo.Color (0.45, 0.45, 0.45));
				title_grad.AddColorStop (0.5, new Cairo.Color (0.33, 0.33, 0.33));
				title_grad.AddColorStop (0.5, new Cairo.Color (0.28, 0.28, 0.28));
				cr.Pattern = title_grad;
				cr.Fill ();
				(cr as IDisposable).Dispose ();
			}
			cr2.Operator = Operator.Source;
			cr2.SetSource (buffered_surface);
			cr2.Paint ();
			(cr2 as IDisposable).Dispose ();
			
			return base.OnExposeEvent (evnt);
		}

		
		public virtual void Clear ()
		{
			(resultsTreeview.Model as ListStore).Clear ();
			cursor = 0;
			resultsLabel.Markup = "--/--";
			queryLabel.Markup = string.Empty;
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
				if (value == null || value.Results.Length == 0) {
					Results = new IObject [0];
					return;
				}
				
				if (results.GetHashCode () != value.Results.GetHashCode ()) {
					results = value.Results;
				}
				
				startResult = value.Cursor - 2;
				
				if (startResult < 0)
					startResult = 0;
				endResult = startResult + 5;

				while (endResult > value.Results.Length) {
					endResult--;
					if (startResult > 0)
						startResult--;
				}
				
				offset = startResult;
				
				IObject[] resultsArray = new IObject[endResult - startResult];
				Array.Copy (results, startResult, resultsArray, 0, resultsArray.Length); 
				
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
				                                     value.Results.Length, 
				                                     value.Cursor + 1);
				Gtk.Application.Invoke (delegate {
					pushedUpdate = false;
				});
			}
		}
		
		protected void IconDataFunc (TreeViewColumn column, CellRenderer cell, 
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
		
		public string Query
		{
			set {
				query = value;
				queryLabel.Markup = string.Format (QueryLabelFormat, value ?? string.Empty);
			}
			get { return query; }
		}
		
		public IObject[] Results
		{
			get {
				return stunted_results ?? stunted_results = new IObject[0];
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
					
					foreach (IObject result in value) {					
						
						info = string.Format (ResultInfoFormat, result.Name, result.Description);
						info = Util.Appearance.MarkupSafeString (info);
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
		
		protected void OnShown (object o, EventArgs args)
		{
			if (update_needed) {
				Context = context;
			}
			update_needed = false;
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
			queryLabel.Markup = string.Format ("{0}<b>{1}</b>", builder.ToString (), query);
		}
	}
}
