// GlassFrameRenderer.cs
// 
// Copyright (C) 2008 GNOME Do
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
	public class GlassFrameTopBar: IBezelTitleBarRenderElement
	{
		BezelDrawingArea parent;
		
		public int Height { get { return 0; } }
		
		public GlassFrameTopBar (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area)
		{
//			int radius = parent.WindowRadius;
			double x = drawing_area.X;
			double y = drawing_area.Y;
			double w = drawing_area.Width;
//			int glaze_offset = 85;
//
//			cr.MoveTo  (x+radius, y);
//			cr.Arc     (x+w-radius, y+radius, radius, Math.PI*1.5, Math.PI*2);
//			cr.LineTo  (x+w, y+glaze_offset);
//			cr.CurveTo (x+2*(w/3), glaze_offset-15,
//			            x+(w/3), glaze_offset-15,
//			            x, y+glaze_offset);
//			cr.Arc     (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
//			LinearGradient lg = new LinearGradient (x, y, x, glaze_offset);
//			lg.AddColorStop (0, new Cairo.Color (1, 1, 1, 0));
//			lg.AddColorStop (1, new Cairo.Color (1, 1, 1, .25));
//			cr.Pattern = lg;
//			cr.Fill ();
//			lg.Destroy ();

			cr.MoveTo (x + w - 30, y + 17);
			cr.LineTo (x + w - 20,  y + 17);
			cr.LineTo (x + w - 25, y + 22);
			cr.Color = new Cairo.Color (1, 1, 1, .95);
			cr.Fill ();
		}
		
		public PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point)
		{
			Gdk.Rectangle pref_circle = new Gdk.Rectangle (drawing_area.X + drawing_area.Width - 32,
				                                 drawing_area.Y +15, 15, 15);
			if (pref_circle.Contains (point))
				return PointLocation.Preferences;
			return PointLocation.Window;
		}
	}
	
	public class GlassFramePaneOutlineRenderer : IBezelPaneRenderElement
	{
		BezelDrawingArea parent;
		Surface sr_active;
		int surface_height = 0;
		
		public int Width { get { return IconSize+140; } }
		public int Height { get { return IconSize + 16; } }
		public int IconSize { get { return 64; } }
		public bool StackIconText { get { return false; } }
		
		public GlassFramePaneOutlineRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area, Pane pane, bool focused)
		{
			if (sr_active == null || surface_height != Height) {
				surface_height = Height;
				sr_active = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				Context c2 = new Context (sr_active);
				CairoUtils.SetRoundedRectanglePath (c2, 0, 0, Width, Height, parent.WindowRadius*.6);
				LinearGradient lg = new LinearGradient (0, 0, 0, Height);
				lg.AddColorStop (0, new Cairo.Color (1, 1, 1, 0));
				lg.AddColorStop (0.4, new Cairo.Color (1, 1, 1, 0));
				lg.AddColorStop (1, new Cairo.Color (1, 1, 1, .3));
				c2.Pattern = lg;
				c2.Fill ();
				lg.Destroy ();
				(c2 as IDisposable).Dispose ();
			}
			
			
			if (pane != parent.Focus)
				return;
			int offset = parent.PaneOffset (pane);
			cr.SetSource (sr_active, drawing_area.X + offset, drawing_area.Y + parent.WindowBorder + parent.TitleBarHeight);
			cr.Paint ();
		}

	}
	
	public class GlassFrameBackgroundRenderer : IBezelWindowRenderElement
	{
		BezelDrawingArea parent;
		
		public Cairo.Color BackgroundColor {
			get {
				return new Cairo.Color (.13, .13, .13, 1);
			}
		}

		public GlassFrameBackgroundRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area)
		{
			CairoUtils.SetRoundedRectanglePath (cr, drawing_area, parent.WindowRadius, true);
			cr.Color = new Cairo.Color (.7, .7, .7, .55);
			cr.FillPreserve ();
			
			cr.LineWidth = 1;
			cr.Color = new Cairo.Color (0, 0, 0, .4);
			cr.Stroke ();
			
			Gdk.Rectangle inter_rect = new Gdk.Rectangle (drawing_area.X+parent.WindowBorder-5,
			                                              drawing_area.Y+parent.WindowBorder-5,
			                                              drawing_area.Width-(2*parent.WindowBorder)+10,
			                                              drawing_area.Height-(2*parent.WindowBorder)+10);
			CairoUtils.SetRoundedRectanglePath (cr, inter_rect, parent.WindowRadius*.7, false);
			LinearGradient lg = new LinearGradient (0, inter_rect.Y, 0, inter_rect.Y+inter_rect.Height);
			lg.AddColorStop (0, CairoUtils.ShadeColor (parent.Colors.Background, 2));
			lg.AddColorStop (1, CairoUtils.ShadeColor (parent.Colors.Background, .3));
			cr.Pattern = lg;
			cr.Fill ();
			lg.Destroy ();
		}
		
		public PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point)
		{
			if (drawing_area.Contains (point))
				return PointLocation.Window;
			return PointLocation.Outside;
		}
	}
	
	public class GlassFrameTextOverlayRenderer : IBezelOverlayRenderElement
	{
		BezelDrawingArea parent;
		
		public GlassFrameTextOverlayRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}

		public void RenderElement (Context cr, Gdk.Rectangle drawing_area, double overlay)
		{
			CairoUtils.SetRoundedRectanglePath (cr, drawing_area, parent.WindowRadius, false);
			cr.Color = new Cairo.Color (parent.Colors.FocusedText.R, 
			                            parent.Colors.FocusedText.G, 
			                            parent.Colors.FocusedText.B, 
			                            parent.Colors.FocusedText.A * overlay);
			cr.Fill ();
		}
	}
	
	public class GlassFrameDefaults : IBezelDefaults
	{
		#region IBezelDefaults implementation 
		
		public int WindowBorder {
			get {
				return 17;
			}
		}
		
		public int WindowRadius {
			get {
				return 10;
			}
		}
		
		public string HighlightFormat {
			get {
				return "<span underline=\"single\">{0}</span>";
			}
		}
		
		public bool RenderDescriptionText {
			get {
				return false;
			}
		}
		
		#endregion 
		
	}
}
