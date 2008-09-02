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
		protected int DefaultResultIconSize = 16;
		protected int NumberResultsDisplayed = 8;
		protected int offset;
		
		const string QueryLabelFormat = "<b>{0}</b>";
		const int WindowRadius = 6;
		const uint TitleBarHeight = 22;


		protected BezelResultsDrawingArea brda;
		protected IObject[] results;
		protected string query;
		protected Label resultsLabel, queryLabel;
		protected IUIContext context = null;
		
		protected int cursor;
		protected int[] secondary = new int[0];
		
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
			
			VBox vbox;
			
			vbox = new VBox ();
			vbox.WidthRequest = 340;
			vbox.Show ();
			Add(vbox);
			
			//---------Top Spacer
			vbox.PackStart (new HBox (), false, false, 10);

			//---------Results Window
			brda = new BezelResultsDrawingArea (9, 340);
			vbox.PackStart (brda, false, false, 0);
			
			//---------The breadcrum bar---------
			resultsLabel = new Label ();
			queryLabel = new Label ();
			queryLabel.Ellipsize = Pango.EllipsizeMode.End;
			queryLabel.WidthRequest = 332;
			queryLabel.Xalign = 0f;
			queryLabel.HeightRequest = 25;
			
			resultsLabel.Ellipsize = Pango.EllipsizeMode.Middle;
			resultsLabel.WidthRequest = 332;
			resultsLabel.Xalign = 0f;
			resultsLabel.Xpad = 4;
			resultsLabel.HeightRequest = 25;
			
			vbox.PackStart (resultsLabel, true, true, 0);
		
			resultsLabel.ModifyFg (StateType.Normal, new Gdk.Color (0xff, 0xff, 0xff));
			queryLabel.ModifyFg   (StateType.Normal, new Gdk.Color (0xff, 0xff, 0xff));
			
			vbox.ShowAll ();
			
			Shown += OnShown;
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

				//Main Window Outline
				cr.MoveTo (geo.X + WindowRadius, geo.Y);
				cr.Arc (geo.X + geo.Width - WindowRadius, geo.Y + WindowRadius, WindowRadius, Math.PI*1.5, Math.PI*2);
				cr.Arc (geo.X + geo.Width - WindowRadius, geo.Y + geo.Height - WindowRadius, WindowRadius, 0, Math.PI*.5);
				cr.Arc (geo.X + WindowRadius, geo.Y + geo.Height - WindowRadius, WindowRadius, Math.PI*.5, Math.PI);
				cr.Arc (geo.X + WindowRadius, geo.Y + WindowRadius, WindowRadius, Math.PI, Math.PI*1.5);
				cr.Color = new Cairo.Color (.15, .15, .15, .95);
				cr.Fill ();
				
				//Titlebar
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
				
				//Base Line
				cr.MoveTo (geo.X, geo.Y + geo.Height - 24.5);
				cr.LineTo (geo.X + geo.Width, geo.Y + geo.Height - 24.5);
				cr.Operator = Operator.Over;
				cr.Color = new Cairo.Color (1, 1, 1, .35);
				cr.LineWidth = 1;
				cr.Stroke ();
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
			brda.Clear ();
			resultsLabel.Markup = string.Empty;
			queryLabel.Markup = string.Empty;
		}
		
		public IUIContext Context
		{
			set {
				IUIContext tmp = context;
				context = value;
				
				if (!Visible)
					return;

				if (value == null || value.Results.Length == 0) {
					Clear ();
					return;
				}
				
				if (context.ParentContext != null && tmp != null &&
				    context.ParentContext.Query == tmp.Query && 
				    tmp.Results.GetHashCode () == context.ParentContext.Results.GetHashCode ()) {
					brda.InitChildInAnimation ();
				} else if (tmp.ParentContext != null && context != null &&
				    tmp.ParentContext.Query == context.Query && 
				    tmp.ParentContext.Results.GetHashCode () == context.Results.GetHashCode ()) {
					brda.InitChildOutAnimation ();
				}
				
				brda.Cursor = value.Cursor;
				brda.Results = value.Results;
				Query = value.Query;
				secondary = value.SecondaryCursors;
				
				UpdateQueryLabel (value);
				string desc = "";
				if (value.Selection != null)
					desc = value.Selection.Description;
				resultsLabel.Markup = string.Format ("<b>{1} of {0}  ▸  {2}</b>", 
				                                     value.Results.Length, 
				                                     value.Cursor + 1,
				                                     desc);
				brda.Draw ();
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
			Context = context;
			brda.Draw ();
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
