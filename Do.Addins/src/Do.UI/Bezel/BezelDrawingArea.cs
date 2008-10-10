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
	
	public enum HUDStyle {
		HUD,
		Classic,
	}
	
	public class BezelDrawingArea : Gtk.DrawingArea
	{
		enum DrawState {
			Normal,
			Text,
			NoResult,
			None,
		}
		
		private HUDStyle style;
		
		public const int IconSize     = 128;
		const int BoxLineWidth = 1;
		const int TextHeight   = 11;
		const int BorderWidth  = 15;
		const int fade_ms      = 150;
		const int ShadowRadius = 10;
		
		IBezelWindowRenderElement  TitleBarRenderer;
		IBezelWindowRenderElement  BackgroundRenderer;
		IBezelPaneRenderElement    PaneOutlineRenderer;
		IBezelOverlayRenderElement TextModeOverlayRenderer;
		IBezelDefaults BezelDefaults;
		
		public int BoxWidth {
			get {
				return PaneOutlineRenderer.Width;
			}
		}
		
		public int BoxHeight { 
			get { 
				return PaneOutlineRenderer.Height;
			} 
		}

		public int WindowWidth { 
			get { 
				return ((2 * WindowBorder) - BorderWidth) + ((BoxWidth + (BorderWidth)) * 3) + (2*ShadowRadius); 
			} 
		}
		
		public int ThreePaneWidth { 
			get { 
				return ((2 * WindowBorder) - BorderWidth) + ((BoxWidth + (BorderWidth)) * 3); 
			} 
		}
		
		public int TwoPaneWidth { get { return (2 * WindowBorder) - BorderWidth + ((BoxWidth + (BorderWidth)) * 2); } }
		
		string HighlightFormat {
			get {
				return BezelDefaults.HighlightFormat;
			}
		}
		
		public int WindowBorder {
			get {
				return BezelDefaults.WindowBorder;
			}
		}
		
		public int WindowRadius {
			get {
				return BezelDefaults.WindowRadius;
			}
		}
		
		public int TitleBarHeight {
			get {
				return TitleBarRenderer.Height;
			}
		}
		
		public int WindowHeight {
			get {
				return BoxHeight + (2 * WindowBorder) + TextHeight + TitleBarHeight + 2*ShadowRadius;
			}
		}
		
		public int InternalHeight {
			get {
				return BoxHeight + (2 * WindowBorder) + TextHeight + TitleBarHeight;
			}
		}
		
		public int TextModeOffset { get { return Math.Max (TitleBarHeight, WindowRadius); } }
		
		bool third_pane_visible;
		BezelDrawingContext context, old_context;
		Pane focus;
		DateTime delta_time;
		uint timer;
		
		Gdk.Rectangle drawing_area;
		Dictionary <string, Surface> surface_buffer;
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
		
		public BezelDrawingArea(HUDStyle style) : base ()
		{
			this.style = style;
			BezelColors.InitColors (style, this);
			switch (style) {
			case HUDStyle.HUD:
				TitleBarRenderer        = new HUDTopBar (this);
				PaneOutlineRenderer     = new HUDPaneOutlineRenderer (this);
				BackgroundRenderer      = new HUDBackgroundRenderer (this);
				TextModeOverlayRenderer = new HUDTextOverlayRenderer (this);
				BezelDefaults           = new HUDBezelDefaults ();
				break;
			case HUDStyle.Classic:
				TitleBarRenderer        = new ClassicTopBar (this);
				PaneOutlineRenderer     = new ClassicPaneOutlineRenderer (this);
				BackgroundRenderer      = new ClassicBackgroundRenderer (this);
				TextModeOverlayRenderer = new ClassicTextOverlayRenderer (this);
				BezelDefaults           = new ClassicBezelDefaults ();
				break;
			default:
				throw new NotImplementedException ();
			}
			
			surface_buffer = new Dictionary <string,Surface> ();
			drawing_area  = new Gdk.Rectangle ((WindowWidth - TwoPaneWidth) / 2, ShadowRadius, TwoPaneWidth, InternalHeight);
			icon_fade = new double [3];
			
			SetSizeRequest (WindowWidth, WindowHeight);
		}
		
		private bool AnimationNeeded {
			get {
				return ExpandNeeded || ShrinkNeeded || TextScaleNeeded || FadeNeeded;
			}
		}
		
		private bool ExpandNeeded {
			get {
				return (ThirdPaneVisible || entry_mode[(int) Focus]) && 
					drawing_area.Width != ThreePaneWidth; 
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
			timer = GLib.Timeout.Add (17, delegate {
				
				double change = DateTime.Now.Subtract (delta_time).TotalMilliseconds / fade_ms;
				delta_time = DateTime.Now;
				
				if (ExpandNeeded) {
					drawing_area.Width += (int)((ThreePaneWidth-TwoPaneWidth)*change);
					drawing_area.Width = (drawing_area.Width > ThreePaneWidth) ? ThreePaneWidth : drawing_area.Width;
					drawing_area.X = (WindowWidth - drawing_area.Width) / 2;
				} else if (ShrinkNeeded) {
					drawing_area.Width -= (int)((ThreePaneWidth-TwoPaneWidth)*change);
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
					if (text_box_scale == 1) {
						icon_fade[0] = icon_fade[1] = icon_fade[2] = 1;
					}
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
			
			BackgroundRenderer.RenderElement (cr, drawing_area);
			
			RenderTitleBar (cr);
			
			do {
				if (text_box_scale > 0) {
					RenderTextModeOverlay (cr);
					if (text_box_scale == 1) {
						RenderTextModeText (cr);
						continue;
					}
				}
				
				RenderDescriptionText (cr);
				//--------------First Pane---------------
				RenderPane (Pane.First, cr);
			
				//------------Second Pane----------------
				RenderPane (Pane.Second, cr);
			
				//------------Third Pane-----------------
				if (ThirdPaneVisible && drawing_area.Width == ThreePaneWidth) {
					RenderPane (Pane.Third, cr);
				}
				
				if (text_box_scale > 0) {
					RenderTextModeOverlay (cr);
				}
			} while (false);

			Util.Appearance.DrawShadow (cr, drawing_area.X, drawing_area.Y, drawing_area.Width, 
			                            drawing_area.Height, WindowRadius, new Util.ShadowParameters (.5, ShadowRadius));
			
			cr2.SetSourceSurface (surface, 0, 0);
			cr2.Operator = Operator.Source;
			cr2.Paint ();
			
			(cr2 as IDisposable).Dispose ();
			(cr as IDisposable).Dispose ();
		}
		
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			AnimatedDraw ();
			return base.OnExposeEvent (evnt);
		}
		
		public int PaneOffset (Pane pane) 
		{
			return WindowBorder + ((int) pane * (BoxWidth + BorderWidth));
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
			PaneOutlineRenderer.RenderElement (cr, drawing_area, pane, (Focus == pane));
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
			
			BezelTextUtils.RenderLayoutText (cr, GLib.Markup.EscapeText (Context.GetPaneObject (Focus).Description), drawing_area.X + 10,
			                                 drawing_area.Y + InternalHeight - WindowBorder - 4, drawing_area.Width - 20, this);
		}
		
		void RenderPaneText (Pane pane, Context cr)
		{
			if (Context.GetPaneObject (pane) != null)
				RenderPaneText (pane, cr, GLib.Markup.EscapeText (Context.GetPaneObject (pane).Name));
		}
		
		void RenderPaneText (Pane pane, Context cr, string text)
		{
			if (text.Length == 0) return;
			
			if (Context.GetPaneTextMode (pane)) {
				Pango.Color color = new Pango.Color ();
				color.Blue = color.Green = color.Red = ushort.MaxValue;
				int y = drawing_area.Y + WindowBorder + TitleBarHeight + 6;
				BezelTextUtils.RenderLayoutText (cr, text, drawing_area.X + PaneOffset (pane) + 5, y, BoxWidth - 10, 
				                                 color, Pango.Alignment.Left, Pango.EllipsizeMode.None, this);
			} else {
				text = (!string.IsNullOrEmpty (Context.GetPaneQuery (pane))) ? 
					Util.FormatCommonSubstrings 
						(text, Context.GetPaneQuery (pane), HighlightFormat) : text;
				int y = drawing_area.Y + WindowBorder + TitleBarHeight + BoxHeight - TextHeight - 9;
				BezelTextUtils.RenderLayoutText (cr, text, drawing_area.X + PaneOffset (pane) + 5, y, BoxWidth - 10, this);
			}
		}
		
		void RenderTextModeText (Context cr)
		{
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = (ushort) (ushort.MaxValue * text_box_scale);
			Gdk.Rectangle cursor = BezelTextUtils.RenderLayoutText (cr, GLib.Markup.EscapeText (Context.GetPaneQuery (Focus)), 
			                                                        drawing_area.X + 10, TextModeOffset + 5, 
			                                                        drawing_area.Width - 20, color, 
			                                                        Pango.Alignment.Left, Pango.EllipsizeMode.None, this);
			
			if (cursor.X == cursor.Y && cursor.X == 0) return;
			
			cr.Rectangle (cursor.X, cursor.Y, 2, cursor.Height);
			cr.Color = new Cairo.Color (.4, .5, 1, .85);
			cr.Fill ();
		}
		
		void RenderTextModeOverlay (Context cr) 
		{
			TextModeOverlayRenderer.RenderElement (cr, drawing_area, text_box_scale);
		}
		
		void RenderTitleBar (Context cr)
		{
			TitleBarRenderer.RenderElement (cr, drawing_area);
		}
		
		public PointLocation GetPointLocation (Gdk.Point point)
		{
			Gdk.Rectangle close_circle, pref_circle;

			switch (style) {
			case HUDStyle.HUD:
				close_circle = new Gdk.Rectangle (drawing_area.X + 6, drawing_area.Y + 2,
				                                  12, 15);
				pref_circle = new Gdk.Rectangle (drawing_area.X + drawing_area.Width - 18, 
				                                 drawing_area.Y + 2, 12, 15);
				break;
			case HUDStyle.Classic:
				close_circle = new Gdk.Rectangle ();
				pref_circle = new Gdk.Rectangle (drawing_area.X + drawing_area.Width - 32,
				                                 drawing_area.Y +5, 15, 15);
				break;
			default:
				throw new NotImplementedException ();
			}
			
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

				

			
			
			