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
		int width, height;
		uint timer;
		
		ShowCaseDrawingContext context;
		ShowCaseDrawingContext old_context;
		Pango.Color focused_color, unfocused_color;
		
		double[] fade_alpha;
		bool draw_tertiary;
		
		Dictionary<string, Surface> surface_buffer;
		Surface background_surface;
		
		Pane focus;
	
		const string HighlightFormat = "<span foreground=\"#5599ff\">{0}</span>";
		
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
				QueueDraw ();
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
			
			if (timer > 0)
				return;
			
			QueueDraw ();
			timer = GLib.Timeout.Add (50, delegate {
				fade_alpha[0] -= .25;
				fade_alpha[1] -= .25;
				fade_alpha[2] -= .25;
				
				fade_alpha[0] = (fade_alpha[0] < 0) ? 0 : fade_alpha[0];
				fade_alpha[1] = (fade_alpha[1] < 0) ? 0 : fade_alpha[1];
				fade_alpha[2] = (fade_alpha[2] < 0) ? 0 : fade_alpha[2];

				QueueDraw ();
				
				if (fade_alpha[0] > 0 || fade_alpha[1] > 0 || fade_alpha[2] > 0) {
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
			
			this.QueueDraw ();
		}
		
		void OnRealized (object o, EventArgs args)
		{
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Context cr = CairoHelper.Create (GdkWindow);
			cr.Save ();
			cr.SetSource (GetBackgroundSurface ());
			cr.Operator = Cairo.Operator.Source;
			cr.Paint ();
			cr.Restore ();
			
			do {
				if (Context == null) continue;
				Pango.Color color;
				int third_pane_startx = width / 2;
				int small_text_height = height - 55;
				string text;
				double alpha;
				
				/***************First Icon***************/
				color = (Focus == Pane.First) ? focused_color : unfocused_color;
				alpha = (Focus == Pane.First) ? 1 : .8;
				if (Context.Main == null && !Context.TextMode[0]) {
					if (string.IsNullOrEmpty (Context.Queries[0])) {
						RenderReflectedIcon (cr, "search", 128, width / 3 * 2, 20);
						RenderText (cr, focused_color, "Type To Begin Searching", height / 2 - 8, 16, 
						            width / 3 * 2, 18, Pango.Alignment.Right);
					} else {
						RenderReflectedIcon (cr, "gtk-question-dialog", 96, width - 120, 10, alpha);
						RenderText (cr, color, "No results for: " +Context.Queries[0], 
						            height / 4, 0, width - 130, 16, Pango.Alignment.Right);
					}
					continue;
				}
				
				if (Context.TextMode[0]) {
					RenderReflectedIcon (cr, "gnome-mime-text", 96, (width / 2) - 48, 10, .35 * alpha);
					text = (string.IsNullOrEmpty (Context.Queries[0])) ? "" : Context.Queries[0];
					RenderText (cr, color, text, 10, 10, width - 10, 12, Pango.Alignment.Left, true);
				} else {
					if (OldContext.Main != null && Context.Main.Icon == OldContext.Main.Icon) {
						RenderReflectedIcon (cr, Context.Main.Icon, 96, width - 120, 10, alpha);
					} else {
						RenderReflectedIcon (cr, Context.Main.Icon, 96, width - 120, 10, alpha - (alpha * fade_alpha[0]));
						if (OldContext.Main != null)
							RenderReflectedIcon (cr, OldContext.Main.Icon, 96, width - 120, 10, alpha * fade_alpha[0]);
					}
					text = (Context.Queries[0] != string.Empty) ? 
						Util.FormatCommonSubstrings 
							(Context.Main.Name, Context.Queries[0], HighlightFormat) : Context.Main.Name;
					
					RenderText (cr, color, text, height / 4, 10, width - 130, 16, Pango.Alignment.Right);
					RenderText (cr, color, Context.Main.Description, height / 4 + 20, 10, width-130, 12, Pango.Alignment.Right);
				}
				
				/***************Second Icon***************/
				int right_bound = (!DrawTertiary) ? width : third_pane_startx;
				color = (Focus == Pane.Second) ? focused_color : unfocused_color;
				alpha = (Focus == Pane.Second) ? 1 : .8;
				if (Context.Secondary == null && !Context.TextMode[1]) {
					if (Context.Queries[1] != null && Context.Queries[1].Length > 0) {
						RenderReflectedIcon (cr, "gtk-dialog-question", 48, 20, height - 70, alpha);
						RenderText (cr, color, "No results for: " + Context.Queries[1], small_text_height,
						            78, right_bound, 13, Pango.Alignment.Left);
					}
					continue;
				}

				if (Context.TextMode[1]) {
					RenderReflectedIcon (cr, "gnome-mime-text", 48, 20, height - 70, .35 * alpha);
					text = (string.IsNullOrEmpty (Context.Queries[1])) ? "" : Context.Queries[1];
					RenderText (cr, color, text, small_text_height-15, 20, right_bound, 10, Pango.Alignment.Left, true);
				} else {
					if (OldContext.Secondary != null && Context.Secondary.Icon == OldContext.Secondary.Icon) {
						RenderReflectedIcon (cr, Context.Secondary.Icon, 48, 20, height - 70, alpha);
					} else {
						RenderReflectedIcon (cr, Context.Secondary.Icon, 48, 20, height - 70, alpha - (alpha * fade_alpha[1]));
						if (OldContext.Secondary != null)
							RenderReflectedIcon (cr, OldContext.Secondary.Icon, 48, 20, height - 70, alpha * fade_alpha[1]);
					}
					text = (Context.Queries[1] != string.Empty) ? 
						Util.FormatCommonSubstrings 
							(Context.Secondary.Name, Context.Queries[1], HighlightFormat) : Context.Secondary.Name;
					RenderText (cr, color, text, small_text_height,
					            78, right_bound, 13, Pango.Alignment.Left);
					RenderText (cr, color, Context.Secondary.Description, small_text_height + 15,
				            78, right_bound, 10, Pango.Alignment.Left);
				}
				/***************Third Icon***************/
				color = (Focus == Pane.Third) ? focused_color : unfocused_color;
				alpha = (Focus == Pane.Third) ? 1 : .8;
				if ((Context.Tertiary == null && !Context.TextMode[2]) || !DrawTertiary) {
					if (Context.Queries[2].Length > 0 && DrawTertiary) {
						RenderReflectedIcon (cr, "gtk-dialog-question", 48, third_pane_startx, height - 70, alpha);
						RenderText (cr, color, "No results for: " + Context.Queries[2], small_text_height,
						            third_pane_startx + 58, width, 13, Pango.Alignment.Left);
					}
					continue;
				}
				
				if (Context.TextMode[2]) {
					RenderReflectedIcon (cr, "gnome-mime-text", 48, third_pane_startx, height - 70, .35 * alpha);
					text = (string.IsNullOrEmpty (Context.Queries[2])) ? "" : Context.Queries[2];
					RenderText (cr, color, text, small_text_height-15, third_pane_startx, width, 10, Pango.Alignment.Left, true);
				} else {
					if (OldContext.Tertiary != null && Context.Tertiary.Icon == OldContext.Tertiary.Icon) {
						RenderReflectedIcon (cr, Context.Tertiary.Icon, 48, third_pane_startx, height - 70, alpha);
					} else {
						RenderReflectedIcon (cr, Context.Tertiary.Icon, 48, third_pane_startx, height - 70, alpha - (alpha * fade_alpha[2]));
						if (OldContext.Tertiary != null)
							RenderReflectedIcon (cr, OldContext.Tertiary.Icon, 48, third_pane_startx, height - 70, alpha * fade_alpha[2]);
					}
					text = (Context.Queries[2] != string.Empty) ? 
						Util.FormatCommonSubstrings 
							(Context.Tertiary.Name, Context.Queries[2], HighlightFormat) : Context.Tertiary.Name;				
					RenderText (cr, color, text, small_text_height, 
					            third_pane_startx + 58, width, 13, Pango.Alignment.Left);
					RenderText (cr, color, Context.Tertiary.Description, small_text_height + 15,
					            third_pane_startx + 58, width, 10, Pango.Alignment.Left);
				}
					
			} while (false);
			
			(cr as IDisposable).Dispose ();
			return base.OnExposeEvent (evnt);
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
			cr.Save ();
			cr.SetSource (surface, x, y);
			cr.PaintWithAlpha (alpha);
			cr.Restore ();
		}
		
		void RenderText (Context cr, Pango.Color color, string text, int heightOffset, 
		                 int leftBound, int rightBound, int size, Pango.Alignment align)
		{
			RenderText (cr, color, text, heightOffset, leftBound, rightBound, size, align, false);
		}
		
		void RenderText (Context cr, Pango.Color color, string text, int heightOffset, 
		                 int leftBound, int rightBound, int size, Pango.Alignment align, bool wrap)
		{
			if (text.Length == 0) return;
			cr.Save ();
			
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
			
			layout.FontDescription.Dispose ();
			cr.Restore ();
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
