// ShowCaseDrawingArea.cs
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

using Cairo;
using Gdk;
using Gtk;
using System;
using System.Collections.Generic;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	
	
	public class ShowCaseDrawingArea : Gtk.DrawingArea
	{
		enum DrawState {
			NoDraw,
			NoResultFoundDraw,
			NormalDraw,
			TextMode,
		}
		
		int width, height, small_text_height, third_pane_startx;
		uint timer;
		
		DateTime delta_time;
		ShowCaseDrawingContext context;
		ShowCaseDrawingContext old_context;
		Pango.Color focused_color, unfocused_color;
		
		double[] fade_alpha;
		double focus_size;
		bool draw_tertiary;
		
		Dictionary<string, Surface> surface_buffer;
		Surface background_surface;
		Surface highlight_surface;
		
		Pane focus;
	
		const string HighlightFormat = "<span foreground=\"#5599ff\">{0}</span>";
		const int HighlightHeight = 4;
		const uint fade_ms = 150;
		
		public bool DrawTertiary {
			get {
				return draw_tertiary;
			}
			set {
				draw_tertiary = value;
				QueueDraw ();
			}
		}
		
		public Pane Focus {
			get {
				return focus;
			}
			set {
				focus = value;
				focus_size = .01;
				AnimatedDraw ();
			}
		}
		
		private ShowCaseDrawingContext OldContext {
			get {
				return old_context ?? old_context = new ShowCaseDrawingContext (null, null, null, Pane.First);
			}
		}
		
		private ShowCaseDrawingContext Context { 
			get {
				return context ?? context = new ShowCaseDrawingContext (null, null, null, Pane.First);
			}
		}
		
		public ShowCaseDrawingArea(int width, int height)
		{
			this.width = width;
			this.height = height;
			small_text_height = height - 55;
			third_pane_startx = width / 2;
			fade_alpha = new double[3];
			surface_buffer = new Dictionary<string,Surface> ();
			
			focused_color = new Pango.Color ();
			focused_color.Blue = focused_color.Red = focused_color.Green = ushort.MaxValue * 1;
			unfocused_color = new Pango.Color ();
			unfocused_color.Blue = unfocused_color.Red = unfocused_color.Green = (ushort) (ushort.MaxValue * .8);
			
			SetSizeRequest (width, height);
			Realized += OnRealized;
		}
		
		public void SetPaneObject (Pane pane, IObject item)
		{
			if (Context.GetObjectForPane (pane) != null && Context.GetObjectForPane(pane).Equals (item))
					return;
				
			OldContext.SetObjectForPane (pane, Context.GetObjectForPane (pane));
			Context.SetObjectForPane (pane, item);
			
			
			fade_alpha[(int) pane] = 1;
			
			AnimatedDraw ();
		}
		
		private void AnimatedDraw ()
		{
			if (timer > 0)
				return;
			
			QueueDraw ();
			delta_time = DateTime.Now;
			timer = GLib.Timeout.Add (20, delegate {
				
				double change = DateTime.Now.Subtract (delta_time).TotalMilliseconds / fade_ms;
				delta_time = DateTime.Now;
				fade_alpha[0] -= change;
				fade_alpha[1] -= change;
				fade_alpha[2] -= change;
				focus_size += change;
				
				fade_alpha[0] = (fade_alpha[0] < 0) ? 0 : fade_alpha[0];
				fade_alpha[1] = (fade_alpha[1] < 0) ? 0 : fade_alpha[1];
				fade_alpha[2] = (fade_alpha[2] < 0) ? 0 : fade_alpha[2];
				focus_size = (focus_size > 1) ? 1 : focus_size;

				QueueDraw ();
				
				
				if (fade_alpha[0] > 0 || fade_alpha[1] > 0 || fade_alpha[2] > 0 || focus_size < 1) {
					return true;
				} else {
					timer = 0;
					return false;
				}
			});
		}
		
		public void SetQuery (Pane pane, string query)
		{
			if (Context.Queries[(int) pane] == query)
				return;
			
			OldContext.Queries[(int) pane] = Context.Queries[(int) pane];
			Context.Queries[(int) pane] = query;
			QueueDraw ();
		}
		
		public void SetTextMode (Pane pane, bool textMode)
		{
			if (Context.TextMode[(int) pane] == textMode) 
				return;
			
			OldContext.TextMode[(int) pane] = Context.TextMode[(int) pane];
			Context.TextMode[(int) pane] = textMode;
			QueueDraw ();
		}
		
		public void Clear ()
		{
			Context.Main = Context.Secondary = Context.Tertiary = null;
			Context.Queries = new string [3];
			Context.TextMode = new bool [3];
			foreach (Surface s in surface_buffer.Values) {
				(s as IDisposable).Dispose ();
			}
			surface_buffer.Clear ();
			
			QueueDraw ();
		}
		
		private DrawState GetPaneDrawState (Pane pane) {
			if (pane == Pane.Third && !DrawTertiary)
				return DrawState.NoDraw;
			
			if (Context.TextMode[(int) pane])
				return DrawState.TextMode;
			
			if (Context.GetObjectForPane (pane) != null)
				return DrawState.NormalDraw;
			
			if (!string.IsNullOrEmpty (Context.Queries[(int) pane]))
				return DrawState.NoResultFoundDraw;
			
			return DrawState.NoDraw;
		}
		
		void OnRealized (object o, EventArgs args)
		{
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Context cr = CairoHelper.Create (GdkWindow);
			
			cr.SetSource (GetBackgroundSurface ());
			cr.Operator = Cairo.Operator.Source;
			cr.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
			cr.Fill ();
			cr.Operator = Cairo.Operator.Over;
			
			do {
				if (Context == null) continue;
				Pango.Color color;
				double alpha;
				
				/***************First Icon***************/
				color = (Focus == Pane.First) ? focused_color : unfocused_color;
				alpha = (Focus == Pane.First) ? 1 : .8;
				
				RenderFirstPane (cr, evnt.Area, color, alpha);
				switch (GetPaneDrawState (Pane.First)) {
				case DrawState.NoResultFoundDraw:
				case DrawState.NoDraw:
					continue;
				}
				
				/***************Second Icon***************/
				color = (Focus == Pane.Second) ? focused_color : unfocused_color;
				alpha = (Focus == Pane.Second) ? 1 : .8;
				
				RenderSecondPane (cr, evnt.Area, color, alpha);
				switch (GetPaneDrawState (Pane.Second)) {
				case DrawState.NoDraw:
				case DrawState.NoResultFoundDraw:
					continue;
				}
				
				/***************Third Icon***************/
				color = (Focus == Pane.Third) ? focused_color : unfocused_color;
				alpha = (Focus == Pane.Third) ? 1 : .8;
				
				if (DrawTertiary)
					RenderThirdPane (cr, evnt.Area, color, alpha);
			} while (false);
			
			(cr as IDisposable).Dispose ();
			return base.OnExposeEvent (evnt);
		}
		
		void RenderFirstPane (Cairo.Context cr, Gdk.Rectangle evntRegion, Pango.Color color, double alpha)
		{
			int icon_size = 96;
			Gdk.Rectangle rectp = new Gdk.Rectangle (width-120, 10, icon_size, icon_size*2);
			Gdk.Rectangle rectt = new Gdk.Rectangle (10, height / 4, width - 130 - 10, 40);
			string text;
			
			switch (GetPaneDrawState (Pane.First)) {
			case DrawState.NormalDraw:
				
				text = (!string.IsNullOrEmpty (Context.Queries[0])) ? 
					Util.FormatCommonSubstrings 
						(Context.Main.Name, Context.Queries[0], HighlightFormat) : Context.Main.Name;
				
				if (evntRegion.IntersectsWith (rectt)) {
					RenderText (cr, color, text, rectt.Y, rectt.X, 
					            rectt.X + rectt.Width, 16, Pango.Alignment.Right);
					int highlight = 
						RenderText (cr, color, Context.Main.Description, rectt.Y+20, rectt.X, 
						            rectt.X + rectt.Width, 12, Pango.Alignment.Right);
					if (highlight > 0 && Focus == Pane.First) {
						Gdk.Rectangle highlight_area 
							= new Gdk.Rectangle (width - 130 - highlight, height / 4 + 34, highlight, HighlightHeight);
						if (evntRegion.IntersectsWith (highlight_area))
							RenderHighlightRegion (cr, highlight_area);
					}
				}
				if (evntRegion.IntersectsWith (rectp)) {
					RenderReflectedIcon (cr, Context.Main.Icon, icon_size, rectp.X, 
					                     rectp.Y, alpha - (alpha * fade_alpha[0]));
					if (OldContext.Main != null && fade_alpha[0] > 0)
						RenderReflectedIcon (cr, OldContext.Main.Icon, icon_size, rectp.X, 
						                     rectp.Y, alpha * fade_alpha[0]);
				}
				break;
			case DrawState.TextMode:
				RenderReflectedIcon (cr, "gnome-mime-text", icon_size, (width / 2) - 48, 10, .35 * alpha);
				text = (string.IsNullOrEmpty (Context.Queries[0])) ? "" : Context.Queries[0];
				RenderText (cr, color, text, 10, 10, width - 10, 12, Pango.Alignment.Left, true);
				if (Focus == Pane.First)
					RenderHighlightRegion (cr, new Gdk.Rectangle (10, height - 80, width - 20, HighlightHeight));
				break;
			case DrawState.NoResultFoundDraw:
				RenderReflectedIcon (cr, "gtk-question-dialog", icon_size, rectp.X, rectp.Y, alpha);
				int highlight = RenderText (cr, color, "No results for: " +Context.Queries[0], rectt.Y, 
				            rectt.X, rectt.X + rectt.Width, 16, Pango.Alignment.Right);
				if (Focus == Pane.First)
					RenderHighlightRegion (cr, new Gdk.Rectangle (rectt.X +rectt.Width - highlight, 
					                                              rectt.Y+18, highlight, HighlightHeight));
				break;
			case DrawState.NoDraw:
				RenderReflectedIcon (cr, "search", 128, width / 3 * 2, 20);
				RenderText (cr, focused_color, "Type To Begin Searching", height / 2 - 8, 16, 
				            width / 3 * 2, 18, Pango.Alignment.Right);
				break;
			}
		}
		
		void RenderSecondPane (Cairo.Context cr, Gdk.Rectangle evntRegion, Pango.Color color, double alpha)
		{
			int icon_size = 48;
			Gdk.Rectangle rectp;
			Gdk.Rectangle rectt;
			
			rectp = new Gdk.Rectangle (20, height - 70, icon_size, icon_size * 2);
			
			int right_bound = (GetPaneDrawState (Pane.Third) != DrawState.NoDraw) ? third_pane_startx - 10 : width - 10;
			rectt = new Gdk.Rectangle (78, small_text_height, right_bound - 78, height - small_text_height);
			
			RenderPane (Pane.Second, cr, evntRegion, rectt, rectp, color, alpha, icon_size);
		}
		
		void RenderThirdPane (Cairo.Context cr, Gdk.Rectangle evntRegion, Pango.Color color, double alpha)
		{
			int icon_size = 48;
			Gdk.Rectangle rectp;
			Gdk.Rectangle rectt;
			
			rectp = new Gdk.Rectangle (third_pane_startx, height - 70, icon_size, icon_size * 2);
			rectt = new Gdk.Rectangle (third_pane_startx + 58, small_text_height, 
			                           width - (third_pane_startx + 58), height - small_text_height);
			
			RenderPane (Pane.Third, cr, evntRegion, rectt, rectp, color, alpha, icon_size);
		}
		
		void RenderPane (Pane pane, Cairo.Context cr, Gdk.Rectangle evntRegion, Gdk.Rectangle rectt,
		                 Gdk.Rectangle rectp, Pango.Color color, double alpha, int icon_size)
		{
			string text;
			
			switch (GetPaneDrawState (pane)) {
			case DrawState.NormalDraw:
				RenderReflectedIcon (cr, Context.GetObjectForPane (pane).Icon, icon_size, rectp.X, 
				                     rectp.Y, alpha - (alpha * fade_alpha[(int) pane]));
				if (OldContext.GetObjectForPane (pane) != null && fade_alpha[(int) pane] > 0)
					RenderReflectedIcon (cr, OldContext.GetObjectForPane (pane).Icon, icon_size, 
					                     rectp.X, rectp.Y, alpha * fade_alpha[(int) pane]);
				
				text = (!string.IsNullOrEmpty (Context.Queries[(int) pane])) ? 
					Util.FormatCommonSubstrings 
						(Context.GetObjectForPane (pane).Name, 
						 Context.Queries[(int) pane], HighlightFormat) : Context.GetObjectForPane (pane).Name;
					
				RenderText (cr, color, text, rectt.Y,  rectt.X, rectt.X + rectt.Width, 13, Pango.Alignment.Left);
				int highlight = 
					RenderText (cr, color, Context.GetObjectForPane (pane).Description, rectt.Y + 15,
					            rectt.X, rectt.X + rectt.Width, 10, Pango.Alignment.Left);
				
				if (highlight > 0 && Focus == pane)
					RenderHighlightRegion (cr, new Gdk.Rectangle (rectt.X, rectt.Y + 27, highlight, HighlightHeight));
				break;
			case DrawState.TextMode:
				RenderReflectedIcon (cr, "gnome-mime-text", icon_size, rectp.X, rectp.Y, .35 * alpha);
				text = (string.IsNullOrEmpty (Context.Queries[(int) pane])) ? "" : Context.Queries[(int) pane];
				RenderText (cr, color, text, rectt.Y-15, rectp.X, rectt.X + rectt.Width, 10, Pango.Alignment.Left, true);
				if (Focus == pane)
					RenderHighlightRegion (cr, new Gdk.Rectangle (rectp.X, rectt.Y+40, rectp.Width+rectt.Width, HighlightHeight));
				break;
			case DrawState.NoResultFoundDraw:
				RenderReflectedIcon (cr, "gtk-dialog-question", icon_size, rectp.X, rectp.Y, alpha);
				highlight = RenderText (cr, color, "No results for: " + Context.Queries[(int) pane], rectt.Y,
				            rectt.X, rectt.X + rectt.Width, 13, Pango.Alignment.Left);
				if (Focus == pane)
					RenderHighlightRegion (cr, new Gdk.Rectangle (rectt.X, rectt.Y+15, highlight, HighlightHeight));
				break;
			}
		}
		
		void RenderHighlightRegion (Cairo.Context cr, Gdk.Rectangle region) 
		{
			if (highlight_surface == null) {
				highlight_surface = cr.Target.CreateSimilar (cr.Target.Content, region.Height * 3, region.Height);
				Cairo.Context cr2 = new Context (highlight_surface);
				cr2.Scale (3, 1);
				double ht = region.Height / 2;
				cr2.Arc (ht, ht, ht, 0, Math.PI * 2);
				RadialGradient rad = new RadialGradient (ht, ht, 0, ht, ht, ht);
				rad.AddColorStop (0, new Cairo.Color (.9, .95, 1, 1));
				rad.AddColorStop (.3, new Cairo.Color (.7, .85, 1, 1));
				rad.AddColorStop (1, new Cairo.Color (.7, .85, 1, 0));
				cr2.Pattern = rad;
				
				cr2.Fill ();
				(cr2 as IDisposable).Dispose ();
			}
			
			cr.Save ();
			double scale_no_focus = (double) region.Width/((double) region.Height * 3);
			double scale = (double) region.Width/((double) region.Height * 3 * focus_size);
			cr.Scale (scale_no_focus, 1);
			cr.SetSource (highlight_surface, 0, 0);
			
			Matrix matrix = new Matrix ();
			matrix.InitTranslate (-((double) region.X / scale) - ((double) region.X * (1 - focus_size) / scale), -region.Y);
			
			cr.Source.Matrix = matrix;
			
			cr.Scale ((double) (1/scale), 1);
			cr.Rectangle (region.X, region.Y, region.Width, region.Height);
			cr.Fill ();
			
			cr.Restore ();
		}
		
		void RenderReflectedIcon (Cairo.Context cr, string icon, int size, int x, int y)
		{
			RenderReflectedIcon (cr, icon, size, x, y, 1);
		}
		
		void RenderReflectedIcon (Cairo.Context cr, string icon, int size, int x, int y, double alpha)
		{
			Surface surface;
			if (surface_buffer.ContainsKey (icon + size.ToString ())) {
				surface = surface_buffer[icon + size.ToString ()];
			} else {
				surface = CreateReflectedSurface (icon, size);
				surface_buffer[icon + size.ToString ()] = surface;
			}
			
			cr.SetSource (surface, x, y);
			cr.PaintWithAlpha (alpha);
		}
		
		int RenderText (Context cr, Pango.Color color, string text, int heightOffset, 
		                 int leftBound, int rightBound, int size, Pango.Alignment align)
		{
			return RenderText (cr, color, text, heightOffset, leftBound, rightBound, size, align, false);
		}
		
		int RenderText (Context cr, Pango.Color color, string text, int heightOffset, 
		                 int leftBound, int rightBound, int size, Pango.Alignment align, bool wrap)
		{
			if (text.Length == 0) return 0;
//			cr.Save ();
			
			int max_width = rightBound - leftBound;
			
			Pango.Layout layout = new Pango.Layout (this.PangoContext);
			layout.Width = Pango.Units.FromPixels (max_width);
			
			
			if (wrap) {
				layout.Ellipsize = Pango.EllipsizeMode.None;
				layout.Wrap = Pango.WrapMode.WordChar;
			} else {
				layout.Ellipsize = Pango.EllipsizeMode.End;
			}
			
			layout.Alignment = align;
			text = string.Format ("<span foreground=\"{0}\">{1}</span>", color, text);
			layout.SetMarkup (text);
			layout.FontDescription = new Pango.FontDescription ();
			layout.FontDescription.Weight = Pango.Weight.Bold;
			layout.FontDescription.AbsoluteSize = Pango.Units.FromPixels (size);
			
			GdkWindow.DrawLayout (this.Style.TextGC (StateType.Normal), leftBound, heightOffset, layout);
			
			int width, height;
			layout.GetSize (out width, out height);
			layout.FontDescription.Dispose ();
			
			return Pango.Units.ToPixels (width);
//			cr.Restore ();
		}
		
		Surface GetBackgroundSurface ()
		{
			if (background_surface == null) {
				double alpha = .95;
				Context cr = CairoHelper.Create (GdkWindow);
				background_surface = cr.Target.CreateSimilar (cr.Target.Content, width, height);
				
				(cr as IDisposable).Dispose ();
				cr = new Context (background_surface);
				
				cr.Save ();
				LinearGradient grad = new LinearGradient (0, 0, 0, height);
				grad.AddColorStop (0, new Cairo.Color (0.2, 0.2, 0.2, alpha));
				grad.AddColorStop (1, new Cairo.Color (0.0, 0.0, 0.0, alpha));
				cr.Pattern = grad;
				cr.Rectangle (0, 0, width, height);
				cr.Operator = Cairo.Operator.Source;
				cr.Fill ();
				cr.Restore ();
				
				(cr as IDisposable).Dispose ();
			}
			
			return background_surface;
		}
		
		Surface CreateReflectedSurface (string icon, int size)
		{
			// Based on the work of Aaron Bockover <abockover@novell.com> in Banshee
			Gdk.Pixbuf pbuf;
			pbuf = IconProvider.PixbufFromIconName (icon, size);

			if (pbuf.Height != size && pbuf.Width != size) {
				Gdk.Pixbuf temp = pbuf.ScaleSimple (size, size, InterpType.Bilinear);
				pbuf.Dispose ();
				pbuf = temp;
			}
			Surface surface;
			int reflect = (int) (pbuf.Height * .6);
			
			Context cr = CairoHelper.Create (GdkWindow);
			surface = cr.Target.CreateSimilar (cr.Target.Content, pbuf.Width, pbuf.Height + reflect);
			(cr as IDisposable).Dispose ();
			
			cr = new Context (surface);
			
			cr.Save ();
			CairoHelper.SetSourcePixbuf (cr, pbuf, 0, 0);
			cr.Rectangle (0, 0, pbuf.Width, pbuf.Height);
			cr.Operator = Cairo.Operator.Source;
			cr.Fill ();
			
			cr.Restore ();
			
			Matrix matrix = new Matrix ();
			matrix.InitScale (1, -1);
			matrix.Translate (0, -(2 * pbuf.Height) + 1);
			cr.Transform (matrix);
		
			LinearGradient mask = new LinearGradient (0, pbuf.Height, 0, pbuf.Height + reflect);
			mask.AddColorStop (0, new Cairo.Color (0, 0, 0, .6));
			mask.AddColorStop (1, new Cairo.Color (0, 0, 0, 0));
			mask.Matrix = matrix;

			Gdk.CairoHelper.SetSourcePixbuf (cr, pbuf, 0, 0);
			
			cr.Rectangle (0, pbuf.Height, pbuf.Width, reflect);
			cr.Mask (mask);
				
			pbuf.Dispose ();
			
			(cr as IDisposable).Dispose ();
			return surface;
		}
	}
}
