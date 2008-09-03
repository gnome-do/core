// BeezelDrawingArea.cs
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
using System.Collections.Generic;

using Cairo;
using Gdk;
using Gtk;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	public enum PointLocation {
		Window,
		Close,
		Preferences,
		Outside,
	}
	
	public class BezelDrawingArea : Gtk.DrawingArea
	{
		enum DrawState {
			Normal,
			Text,
			NoResult,
			None,
		}
		
		const int IconSize     = 128;
		const int BoxWidth     = 160;
		const int BoxHeight    = IconSize + 15 + TextHeight;
		const int BoxLineWidth = 1;
		const int TextHeight   = 11;
		const int BorderWidth  = 15;
		const int WindowWidth  = (2 * WindowBorder) - BorderWidth + ((BoxWidth + (BorderWidth)) * 3);
		const int TwoPaneWidth = (2 * WindowBorder) - BorderWidth + ((BoxWidth + (BorderWidth)) * 2);
		const int TitleBarHeight = 21;
		const int WindowRadius = 6;
		const int WindowHeight = BoxHeight + (2 * WindowBorder) + TextHeight + TitleBarHeight;
		const int WindowBorder = 21;
		const int fade_ms      = 150;
		const string HighlightFormat = "<span foreground=\"#5599ff\">{0}</span>";
		
		bool third_pane_visible;
		BezelDrawingContext context, old_context;
		Pane focus;
		DateTime delta_time;
		uint timer;
		
		Gdk.Rectangle drawing_area;
		Dictionary <string, Cairo.Color> colors;
		Dictionary <string, Surface> surface_buffer;
		Surface border_buffer;
		Surface surface;
		
		double text_box_scale;
		double[] icon_fade = new double [] {1, 1, 1};
		bool[] entry_mode = new bool[3];
		
		private BezelDrawingContext Context {
			get {
				return context ?? context = new BezelDrawingContext ();
			}
		}
		
		private BezelDrawingContext OldContext {
			get {
				return old_context ?? old_context = new BezelDrawingContext ();
			}
		}
		
		public bool ThirdPaneVisible {
			get { return third_pane_visible; }
			set { 
				third_pane_visible = value; 
				AnimatedDraw ();
			}
		}
		
		public Pane Focus {
			get { return focus; }
			set { 
				if (focus == value)
					return;
				focus = value;
				AnimatedDraw ();
			}
		}
		
		public BezelDrawingArea() : base ()
		{
			surface_buffer = new Dictionary <string,Surface> ();
			drawing_area  = new Gdk.Rectangle ((WindowWidth - TwoPaneWidth) / 2, 0, TwoPaneWidth, WindowHeight);
			icon_fade = new double [3];
			
			BuildColors ();
			SetSizeRequest (WindowWidth, WindowHeight);
		}
		
		private void BuildColors ()
		{
			colors = new Dictionary<string,Cairo.Color> ();
			colors["focused_box"]    = new Cairo.Color (0.3, 0.3, 0.3, 0.6);
			colors["unfocused_box"]  = new Cairo.Color (0.0, 0.0, 0.0, 0.2);
			colors["focused_line"]   = new Cairo.Color (1.0, 1.0, 1.0, 0.3);
			colors["unfocused_line"] = new Cairo.Color (1.0, 1.0, 1.0, 0.2);
			colors["focused_text"]   = new Cairo.Color (0.0, 0.0, 0.0, 0.85);
			colors["unfocused_text"] = new Cairo.Color (0.3, 0.3, 0.3, 0.7);
			colors["titlebar_step1"] = new Cairo.Color (0.45, 0.45, 0.45);
			colors["titlebar_step2"] = new Cairo.Color (0.33, 0.33, 0.33);
			colors["titlebar_step3"] = new Cairo.Color (0.28, 0.28, 0.28);
		}
		
		private bool AnimationNeeded {
			get {
				return ExpandNeeded || ShrinkNeeded || TextScaleNeeded || FadeNeeded;
			}
		}
		
		private bool ExpandNeeded {
			get {
				return (ThirdPaneVisible || entry_mode[(int) Focus]) && 
					drawing_area.Width != WindowWidth; 
			}
		}
		
		private bool ShrinkNeeded {
			get {
				return (!ThirdPaneVisible && !entry_mode[(int) focus])  && 
					drawing_area.Width != TwoPaneWidth && Focus != Pane.Third;
			}
		}
		
		private bool FadeNeeded {
			get {
				return (icon_fade[0] != 1 || icon_fade[1] != 1 || icon_fade[2] != 1);
			}
		}
		
		private bool TextScaleNeeded {
			get {
				return (entry_mode[(int) Focus] && text_box_scale != 1) ||
					(!entry_mode[(int) Focus] && text_box_scale != 0);
			}
		}
		
		private void AnimatedDraw ()
		{
			if (!IsDrawable || timer > 0)
				return;
			
			Paint ();
			
			if (!AnimationNeeded)
				return;
			
			delta_time = DateTime.Now;
			timer = GLib.Timeout.Add (20, delegate {
				
				double change = DateTime.Now.Subtract (delta_time).TotalMilliseconds / fade_ms;
				delta_time = DateTime.Now;
				
				if (ExpandNeeded) {
					drawing_area.Width += (int)((WindowWidth-TwoPaneWidth)*change);
					drawing_area.Width = (drawing_area.Width > WindowWidth) ? WindowWidth : drawing_area.Width;
					drawing_area.X = (WindowWidth - drawing_area.Width) / 2;
				} else if (ShrinkNeeded) {
					drawing_area.Width -= (int)((WindowWidth-TwoPaneWidth)*change);
					drawing_area.Width = (drawing_area.Width < TwoPaneWidth) ? TwoPaneWidth : drawing_area.Width;
					drawing_area.X = (WindowWidth - drawing_area.Width) / 2;
				}
				
				if (TextScaleNeeded) {
					if (entry_mode[(int) Focus]) {
						text_box_scale += change;
						text_box_scale = (text_box_scale > 1) ? 1 : text_box_scale;
					} else {
						text_box_scale -= change;
						text_box_scale = (text_box_scale < 0) ? 0 : text_box_scale;
					}
				}
				
				if (FadeNeeded) {
					icon_fade[0] += change;
					icon_fade[1] += change;
					icon_fade[2] += change;
					
					icon_fade[0] = (icon_fade[0] > 1) ? 1 : icon_fade[0];
					icon_fade[1] = (icon_fade[1] > 1) ? 1 : icon_fade[1];
					icon_fade[2] = (icon_fade[2] > 1) ? 1 : icon_fade[2];
				}
				
				Paint ();
				
				if (AnimationNeeded) {
					return true;
				} else {
					timer = 0;
					return false;
				}
			});
		}
		
		private DrawState PaneDrawState (Pane pane)
		{
			if (pane == Pane.Third && !ThirdPaneVisible)
				return DrawState.None;
			
			if (Context.GetPaneTextMode (pane))
				return DrawState.Text;
			
			if (Context.GetPaneObject (pane) != null)
				return DrawState.Normal;
			
			if (!string.IsNullOrEmpty (Context.GetPaneQuery (pane))) {
				return DrawState.NoResult;
			}
			
			return DrawState.None;
		}
		
		public void BezelSetPaneObject (Pane pane, IObject obj)
		{
			if (Context.GetPaneObject (pane) == obj && obj != null)
				return;
			
			OldContext.SetPaneObject (pane, Context.GetPaneObject (pane));
			Context.SetPaneObject (pane, obj);
			
			icon_fade[(int) pane] = 0;
		}
		
		public void BezelSetTextMode (Pane pane, bool textMode)
		{
			if (Context.GetPaneTextMode (pane) == textMode)
				return;
			
			OldContext.SetPaneTextMode (pane, Context.GetPaneTextMode (pane));
			Context.SetPaneTextMode (pane, textMode);
		}
		
		public void BezelSetEntryMode (Pane pane, bool entryMode)
		{
			entry_mode[(int) pane] = entryMode;
		}
		
		public void BezelSetQuery (Pane pane, string query)
		{
			if (Context.GetPaneQuery (pane) == query)
				return;
			
			OldContext.SetPaneQuery (pane, Context.GetPaneQuery (pane));
			Context.SetPaneQuery (pane, query);
		}
		
		public void Clear ()
		{
			context = new BezelDrawingContext ();
			old_context = new BezelDrawingContext ();
			entry_mode = new bool [3];
			foreach (Surface s in surface_buffer.Values)
				(s as IDisposable).Dispose ();
			
			surface_buffer = new Dictionary<string,Surface> ();
			AnimatedDraw ();
		}
		
		public void Draw ()
		{
			AnimatedDraw ();
		}
		
		private void SetRoundedPath (Cairo.Context cr, bool strokePath)
		{
			int radius = WindowRadius;
			double x = drawing_area.X;
			double y = drawing_area.Y;
			double h = drawing_area.Height;
			double w = drawing_area.Width;
			
			if (strokePath) {
				x += .5;
				y += .5;
				h--;
				w--;
			}
			cr.MoveTo (x+radius, y);
			cr.Arc (x+w-radius, y+radius, radius, Math.PI*1.5, Math.PI*2);
			cr.Arc (x+w-radius, h-radius, radius, 0, Math.PI*.5);
			cr.Arc (x+radius, h-radius, radius, Math.PI*.5, Math.PI);
			cr.Arc (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
		}
		
		private void SetTitlePath (Cairo.Context cr)
		{
			int radius = WindowRadius;
			double x = .5;
			double y = .5;
			double width = TwoPaneWidth - 1;
			cr.MoveTo (x+radius, y);
			cr.Arc (x+width-radius, y+radius, radius, Math.PI*1.5, Math.PI*2);
			cr.LineTo (x+width, TitleBarHeight);
			cr.LineTo (x, TitleBarHeight);
			cr.Arc (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
		}
		
		void Paint () 
		{
			Cairo.Context cr2 = Gdk.CairoHelper.Create (GdkWindow);
			
			//Much kudos to Ian McIntosh
			if (surface == null)
				surface = cr2.Target.CreateSimilar (cr2.Target.Content, WindowWidth, WindowHeight);
			
			Context cr = new Context (surface);
			cr.Save ();
			cr.Color = new Cairo.Color (0, 0, 0, 0);
			cr.Operator = Cairo.Operator.Source;
			cr.Paint ();
			
			cr.Operator = Cairo.Operator.Over;
			SetRoundedPath (cr, false);
			cr.Color = new Cairo.Color (.15, .15, .15, .95);
			cr.Fill ();
			
			SetRoundedPath (cr, true);
			cr.Color = new Cairo.Color (.35, .35, .35);
			cr.LineWidth = 1;
			cr.Stroke ();
			
			RenderTitleBar (cr);
			
			do {
				
				RenderDescriptionText (cr);
				//--------------First Pane---------------
				RenderPane (Pane.First, cr);
			
				//------------Second Pane----------------
				RenderPane (Pane.Second, cr);
			
				//------------Third Pane-----------------
				if (ThirdPaneVisible && drawing_area.Width == WindowWidth) {
					RenderPane (Pane.Third, cr);
				}
			
				if (text_box_scale > 0) {
					RenderTextModeOverlay (cr);
					if (text_box_scale == 1)
						RenderTextModeText (cr);
				}
				
			} while (false);

			cr2.SetSourceSurface (surface, 0, 0);
			cr2.Operator = Operator.Source;
			cr2.Paint ();
			
			(cr2 as IDisposable).Dispose ();
//			(surface as IDisposable).Dispose ();
			(cr as IDisposable).Dispose ();
		}
		
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			AnimatedDraw ();
			return base.OnExposeEvent (evnt);
		}
		
		private int PaneOffset (Pane pane) 
		{
			return WindowBorder + ((int) pane * (BoxWidth + BorderWidth));
		}
		
		private void RenderCloseCircle (Context cr)
		{
			cr.Arc (12,
			        TitleBarHeight / 2, 6, 0, Math.PI*2);
			cr.Color = new Cairo.Color (1, 1, 1, .8);
			cr.Fill ();
			
			cr.MoveTo (15, (TitleBarHeight / 2) - 3);
			cr.LineTo (9, (TitleBarHeight / 2) + 3);
			cr.MoveTo (9, (TitleBarHeight / 2) - 3);
			cr.LineTo (15, (TitleBarHeight / 2) + 3);
			
			cr.Color = new Cairo.Color (0.2, 0.2, 0.2, .8);
			cr.LineWidth = 2;
			cr.Stroke ();
		}
		
		private void RenderDownCircle (Context cr)
		{
			cr.Arc (TwoPaneWidth - 12,
			        TitleBarHeight / 2, 6, 0, Math.PI*2);
			cr.Color = new Cairo.Color (1, 1, 1, .8);
			cr.Fill ();
			
			cr.MoveTo (TwoPaneWidth - 15, (TitleBarHeight / 2) - 2);
			cr.LineTo (TwoPaneWidth - 9, (TitleBarHeight / 2) - 2);
			cr.LineTo (TwoPaneWidth - 12, (TitleBarHeight / 2) + 3);
			cr.Color = new Cairo.Color (0.2, 0.2, 0.2, .8);
			cr.Fill ();
		}

		private void RenderPane (Pane pane, Context cr)
		{
			RenderPaneOutline (pane, cr);
			
			switch (PaneDrawState (pane)) {
			case DrawState.Normal:
				RenderPixbuf (pane, cr);
				RenderPaneText (pane, cr);
				break;
			case DrawState.NoResult:
				RenderPixbuf (pane, cr, "gtk-question-dialog", 1);
				RenderPaneText (pane, cr, "No results for: " + Context.GetPaneQuery (pane));
				break;
			case DrawState.Text:
				if (text_box_scale < 1) {
					RenderPixbuf (pane, cr, "gnome-mime-text", .1);
					RenderPaneText (pane, cr);
				}
				break;
			case DrawState.None:
				if (pane == Pane.First) {
					RenderPixbuf (pane, cr, "search", 1);
					RenderPaneText (pane, cr, "Type To Search");
				}
				break;
			}
			
		}
		
		private void RenderPaneOutline (Pane pane, Context cr)
		{
			int offset = PaneOffset (pane);
			cr.Rectangle (drawing_area.X + offset, WindowBorder + TitleBarHeight, BoxWidth, BoxHeight);
			cr.Color = (Focus == pane) ? colors["focused_box"] : colors["unfocused_box"];
			cr.Fill ();
			cr.Rectangle (drawing_area.X + offset - .5, WindowBorder + TitleBarHeight - .5, BoxWidth + 1, BoxHeight + 1);
			cr.Color = (Focus == pane) ? colors["focused_line"] : colors["unfocused_line"];
			cr.LineWidth = BoxLineWidth;
			cr.Stroke ();
		}
		
		private void RenderPixbuf (Pane pane, Context cr)
		{
			RenderPixbuf (pane, cr, Context.GetPaneObject (pane).Icon, 1);
		}
		
		private void RenderPixbuf (Pane pane, Context cr, string icon, double alpha)
		{
			int offset = PaneOffset (pane);
			if (!surface_buffer.ContainsKey (icon)) {
				BufferIcon (cr, icon);
			}
			
			string sec_icon = "";
			if (OldContext.GetPaneObject (pane) != null)
				sec_icon = OldContext.GetPaneObject (pane).Icon;
			
			double calc_alpha = (sec_icon != icon) ? icon_fade[(int) pane] : 1;
			cr.SetSource (surface_buffer[icon], drawing_area.X + offset + ((BoxWidth/2)-(IconSize/2)),
				                                 drawing_area.Y + WindowBorder + TitleBarHeight + 3);
			cr.PaintWithAlpha (calc_alpha * alpha);
			
			if (!string.IsNullOrEmpty (sec_icon) && calc_alpha < 1) {
				if (!surface_buffer.ContainsKey (OldContext.GetPaneObject (pane).Icon)) {
					BufferIcon (cr, OldContext.GetPaneObject (pane).Icon);
				}
				cr.SetSource (surface_buffer[OldContext.GetPaneObject (pane).Icon], drawing_area.X + offset + ((BoxWidth/2)-(IconSize/2)),
				                                 drawing_area.Y + WindowBorder + TitleBarHeight + 3);
				cr.PaintWithAlpha (alpha * (1 - calc_alpha));
			}
		}
		
		private void BufferIcon (Context cr, string icon)
		{
			Surface sr;
			Gdk.Pixbuf pixbuf;
			pixbuf = IconProvider.PixbufFromIconName (icon, IconSize);
			if (pixbuf.Height != IconSize && pixbuf.Width != IconSize) {
				Gdk.Pixbuf temp = pixbuf.ScaleSimple (IconSize, IconSize, InterpType.Bilinear);
				pixbuf.Dispose ();
				pixbuf = temp;
			}
			sr = cr.Target.CreateSimilar (cr.Target.Content, IconSize, IconSize);
			Context cr2 = new Context (sr);
			Gdk.CairoHelper.SetSourcePixbuf (cr2, pixbuf, 0, 0);
			cr2.Paint ();
			surface_buffer[icon] = sr;
			(cr2 as IDisposable).Dispose ();
			pixbuf.Dispose ();
		}
		
		void RenderDescriptionText (Context cr)
		{
			if (Context.GetPaneObject (Focus) == null)
				return;
			
			RenderLayoutText (cr, Context.GetPaneObject (Focus).Description, drawing_area.X + 10,
			                  WindowHeight - 25, drawing_area.Width - 20);
		}
		
		void RenderPaneText (Pane pane, Context cr)
		{
			if (Context.GetPaneObject (pane) != null)
				RenderPaneText (pane, cr, Context.GetPaneObject (pane).Name);
		}
		
		void RenderPaneText (Pane pane, Context cr, string text)
		{
			if (text.Length == 0) return;
			
			if (Context.GetPaneTextMode (pane)) {
				Pango.Color color = new Pango.Color ();
				color.Blue = color.Green = color.Red = ushort.MaxValue;
				int y = drawing_area.Y + WindowBorder + TitleBarHeight + 6;
				RenderLayoutText (cr, text, drawing_area.X + PaneOffset (pane) + 5, y, BoxWidth - 10, 
				                  color, Pango.Alignment.Left, Pango.EllipsizeMode.None);
			} else {
				text = (!string.IsNullOrEmpty (Context.GetPaneQuery (pane))) ? 
					Util.FormatCommonSubstrings 
						(text, Context.GetPaneQuery (pane), HighlightFormat) : text;
				int y = drawing_area.Y + WindowBorder + TitleBarHeight + IconSize + 6;
				RenderLayoutText (cr, text, drawing_area.X + PaneOffset (pane) + 5, y, BoxWidth - 10);
			}
		}
		
		void RenderTextModeText (Context cr)
		{
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = (ushort) (ushort.MaxValue * text_box_scale);
			Gdk.Rectangle cursor = RenderLayoutText (cr, Context.GetPaneQuery (Focus), 
			                                         drawing_area.X + 10, TitleBarHeight + 5, 
			                                         drawing_area.Width - 20, color, 
			                                         Pango.Alignment.Left, Pango.EllipsizeMode.None);
			
			if (cursor.X == cursor.Y && cursor.X == 0) return;
			
			cr.Rectangle (cursor.X, cursor.Y, 2, cursor.Height);
			cr.Color = new Cairo.Color (.4, .5, 1, .85);
			cr.Fill ();
		}
		
		void RenderLayoutText (Context cr, string text, int x, int y, int width)
		{
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			RenderLayoutText (cr, text, x, y, width, color, Pango.Alignment.Center, Pango.EllipsizeMode.End);
		}
		
		Gdk.Rectangle RenderLayoutText (Context cr, string text, int x, int y, int width, 
		                       Pango.Color color, Pango.Alignment align, Pango.EllipsizeMode ellipse)
		{
			if (string.IsNullOrEmpty (text)) return new Gdk.Rectangle ();
	
			Pango.Layout layout = new Pango.Layout (this.PangoContext);
			layout.Width = Pango.Units.FromPixels (width);
			layout.SetMarkup (text);
			
			layout.Ellipsize = ellipse;
				
			layout.FontDescription = Pango.FontDescription.FromString ("normal bold");
			layout.FontDescription.AbsoluteSize = Pango.Units.FromPixels (TextHeight);
			layout.Alignment = align;
			
			if (ellipse == Pango.EllipsizeMode.None) {
				layout.Wrap = Pango.WrapMode.WordChar;
				int offset = 0;
				int count = 10;
				bool modified = (layout.LineCount > 10);
				
				while (layout.LineCount > 10) {
					offset += count;
					layout.SetMarkup (text.Substring (0, text.Length - offset) + "...");
					if (layout.LineCount == 10 && count != 1) {
						offset -= count;
						layout.SetMarkup (text.Substring (0, text.Length - offset) + "...");
						count = 1;
					}
				}
				if (modified) {
					text = text.Substring (0, text.Length - offset) + "...";
				}
			}
			
			text = string.Format ("<span foreground=\"{0}\">{1}</span>", color, text);
			layout.SetMarkup (text);
			
			cr.MoveTo (x, y);
			Pango.CairoHelper.ShowLayout (cr, layout);
			Pango.Rectangle strong, weak;
			layout.GetCursorPos (layout.Lines [layout.LineCount-1].StartIndex + 
			                     layout.Lines [layout.LineCount-1].Length, 
			                     out strong, out weak);
			layout.FontDescription.Dispose ();
			return new Gdk.Rectangle (Pango.Units.ToPixels (weak.X) + x,
			                          Pango.Units.ToPixels (weak.Y) + y,
			                          Pango.Units.ToPixels (weak.Width),
			                          Pango.Units.ToPixels (weak.Height));
		}
		
		void RenderTitleText (Context cr) {
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			RenderLayoutText (cr, "GNOME Do", 0, 5, WindowWidth, color, 
			                  Pango.Alignment.Center, Pango.EllipsizeMode.End);
		}
		
		void RenderTextModeOverlay (Context cr) 
		{
			cr.Rectangle (drawing_area.X, drawing_area.Y + TitleBarHeight, drawing_area.Width,
			              (WindowHeight - TitleBarHeight - 10)); 
			cr.Color = new Cairo.Color (colors["focused_text"].R, 
			                            colors["focused_text"].G, 
			                            colors["focused_text"].B, 
			                            colors["focused_text"].A * text_box_scale);
			cr.Fill ();
		}
		
		void RenderTitleBar (Context cr)
		{
			if (border_buffer == null) {
				
				Surface surface = cr.Target.CreateSimilar (cr.Target.Content, TwoPaneWidth, TitleBarHeight);
				Context cr2 = new Context (surface);
				
				SetTitlePath (cr2);
				cr2.Operator = Cairo.Operator.Source;
				LinearGradient title_grad = new LinearGradient (0, 0, 0, TitleBarHeight);
				title_grad.AddColorStop (0.0, colors["titlebar_step1"]);
				title_grad.AddColorStop (0.5, colors["titlebar_step2"]);
				title_grad.AddColorStop (0.5, colors["titlebar_step3"]);
				cr2.Pattern = title_grad;
				cr2.FillPreserve ();
				cr2.Operator = Cairo.Operator.Over;
			
				LinearGradient grad = new LinearGradient (0, 0, 0, TitleBarHeight);
				grad.AddColorStop (0, new Cairo.Color (1, 1, 1, .6));
				grad.AddColorStop (.6, new Cairo.Color (1, 1, 1, 0));
				cr2.Pattern = grad;
				cr2.LineWidth = 1;
				cr2.Stroke ();
			
				RenderDownCircle (cr2);
				RenderCloseCircle (cr2);
				
				border_buffer = surface;
				(cr2 as IDisposable).Dispose ();
			}
			
			if (drawing_area.Width == TwoPaneWidth) {
				cr.SetSource (border_buffer, drawing_area.X, drawing_area.Y);
				cr.Rectangle (drawing_area.X, drawing_area.Y, drawing_area.Width, TitleBarHeight);
				cr.Fill ();
			} else {
				cr.SetSource (border_buffer, drawing_area.X, drawing_area.Y);
				cr.Rectangle (drawing_area.X, drawing_area.Y, 200, TitleBarHeight);
				cr.Fill ();
				
				cr.SetSource (border_buffer, drawing_area.X + drawing_area.Width - TwoPaneWidth, drawing_area.Y);
				cr.Rectangle (drawing_area.X + 200, drawing_area.Y, drawing_area.Width - 200, TitleBarHeight);
				cr.Fill ();
			}
			RenderTitleText (cr);
		}
		
		public PointLocation GetPointLocation (Gdk.Point point)
		{
			Gdk.Rectangle close_circle = new Gdk.Rectangle (drawing_area.X + 6, drawing_area.Y + 2,
			                                                12, 15);
			Gdk.Rectangle pref_circle = new Gdk.Rectangle (drawing_area.X + drawing_area.Width - 18, 
			                                               drawing_area.Y + 2, 12, 15);
			if (!drawing_area.Contains (point))
				return PointLocation.Outside;
			if (close_circle.Contains (point))
				return PointLocation.Close;
			if (pref_circle.Contains (point))
				return PointLocation.Preferences;
			return PointLocation.Window;
		}
	}
}

				

			
			
			