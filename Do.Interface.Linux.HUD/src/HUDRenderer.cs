// BezelRenderClasses.cs
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

using Do.Interface.AnimationBase;
using Do.Interface.CairoUtils;

namespace Do.Interface
{

	public class HUDTopBar : IBezelTitleBarRenderElement
	{
		BezelDrawingArea parent;
		Surface border_buffer;
		
		public int Height { get { return 21; } }
		public Cairo.Color BackgroundColor {get { return new Cairo.Color (0, 0, 0); } }
		
		public HUDTopBar (BezelDrawingArea parent)
		{
			this.parent = parent;
			BezelDrawingArea.ThemeChanged += delegate {
				if (border_buffer == null)
					return;
				border_buffer.Dispose ();
				border_buffer = null;
			};
		}

		// TODO: What? We're using a finalizer without the IDisposable pattern?		
		~HUDTopBar ()
		{
			border_buffer.Dispose ();
		}
		
		private int buttons_offset { get { return Math.Min (10, parent.WindowRadius + 3); } }
		
		private void RenderCloseCircle (Context cr)
		{
			cr.Arc (buttons_offset+6, Height / 2, 6, 0, Math.PI*2);
			cr.SetSourceRGBA (1, 1, 1, .8);
			cr.Fill ();
			
			cr.MoveTo (buttons_offset+9, (Height / 2) - 3);
			cr.LineTo (buttons_offset+3,  (Height / 2) + 3);
			cr.MoveTo (buttons_offset+3,  (Height / 2) - 3);
			cr.LineTo (buttons_offset+9, (Height / 2) + 3);
			
			cr.SetSourceRGBA (0.2, 0.2, 0.2, .8);
			cr.LineWidth = 2;
			cr.Stroke ();
		}
		
		private void RenderDownCircle (Context cr)
		{
			cr.Arc (parent.ThreePaneWidth - (buttons_offset + 6),
			        Height / 2, 6, 0, Math.PI*2);
			cr.SetSourceRGBA (1, 1, 1, .8);
			cr.Fill ();
			
			cr.MoveTo (parent.ThreePaneWidth - (buttons_offset + 9), (Height / 2) - 2);
			cr.LineTo (parent.ThreePaneWidth - (buttons_offset + 3),  (Height / 2) - 2);
			cr.LineTo (parent.ThreePaneWidth - (buttons_offset + 6), (Height / 2) + 3);
			cr.SetSourceRGBA (0.2, 0.2, 0.2, .8);
			cr.Fill ();
		}
		
		public void RenderItem (Context cr, Gdk.Rectangle drawing_area)
		{
			if (border_buffer == null) {
				border_buffer = cr.CreateSimilarToTarget (parent.ThreePaneWidth, Height);
				using (Context cr2 = new Context (border_buffer)) {
					SetTitlePath (cr2);
					cr2.Operator = Cairo.Operator.Source;
					using (var title_grad = new LinearGradient (0, 0, 0, Height)) {
						title_grad.AddColorStop (0.0, parent.Colors.TitleBarGlossLight);
						title_grad.AddColorStop (0.5, parent.Colors.TitleBarGlossDark);
						title_grad.AddColorStop (0.5, parent.Colors.TitleBarBase);
						cr2.SetSource (title_grad);
						cr2.FillPreserve ();
						cr2.Operator = Cairo.Operator.Over;
					}

					using (var grad = new LinearGradient (0, 0, 0, Height)) {
						grad.AddColorStop (0, new Cairo.Color (1, 1, 1, .6));
						grad.AddColorStop (.6, new Cairo.Color (1, 1, 1, 0));
						cr2.SetSource (grad);
						cr2.LineWidth = 1;
						cr2.Stroke ();
					}
			
					RenderDownCircle (cr2);
					RenderCloseCircle (cr2);
				}
			}
			
			if (drawing_area.Width == parent.ThreePaneWidth) {
				cr.SetSource (border_buffer, drawing_area.X, drawing_area.Y);
				cr.Rectangle (drawing_area.X, drawing_area.Y, drawing_area.Width, Height);
				cr.Fill ();
			} else {
				//sliding door effect
				cr.SetSource (border_buffer, drawing_area.X, drawing_area.Y);
				cr.Rectangle (drawing_area.X, drawing_area.Y, 200, Height);
				cr.Fill ();
				
				cr.SetSource (border_buffer, drawing_area.X + drawing_area.Width - parent.ThreePaneWidth, drawing_area.Y);
				cr.Rectangle (drawing_area.X + 200, drawing_area.Y, drawing_area.Width - 200, Height);
				cr.Fill ();
			}
			RenderTitleText (cr, drawing_area);
		}
		
		void RenderTitleText (Context cr, Gdk.Rectangle drawing_area) {
//			Pango.Color color = new Pango.Color ();
//			color.Blue = color.Red = color.Green = ushort.MaxValue;
//			string s = "GNOME Do";
//			if (DateTime.Now.Day == 25 && DateTime.Now.Month == 12)
//				s = "Merry Christmas!!!";
//			BezelTextUtils.RenderLayoutText (cr, s, 0, drawing_area.Y + 5, parent.WindowWidth, color, 
//			                  Pango.Alignment.Center, Pango.EllipsizeMode.End);
		}
		
		private void SetTitlePath (Cairo.Context cr)
		{
			int radius = parent.WindowRadius;
			double x = .5;
			double y = .5;
			double width = parent.ThreePaneWidth - 1;
			cr.MoveTo (x+radius, y);
			cr.Arc (x+width-radius, y+radius, radius, Math.PI*1.5, Math.PI*2);
			cr.LineTo (x+width, Height);
			cr.LineTo (x, Height);
			cr.Arc (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
		}
		
		public PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point)
		{
			Gdk.Rectangle close_circle = new Gdk.Rectangle (drawing_area.X + buttons_offset, drawing_area.Y + 2,
			                                                12, 15);
			Gdk.Rectangle pref_circle = new Gdk.Rectangle (drawing_area.X + drawing_area.Width - (buttons_offset + 12), 
			                                               drawing_area.Y + 2, 12, 15);
			if (close_circle.Contains (point))
				return PointLocation.Close;
			else if (pref_circle.Contains (point))
				return PointLocation.Preferences;
			else
				return PointLocation.Window;
		}
	}
	
	
	
	public class HUDPaneOutlineRenderer : IBezelPaneRenderElement
	{
		BezelDrawingArea parent;
		
		public int Width { get { return 160; } }
		public int IconSize { get { return 128; } }
		public int Height { get { return IconSize + 15 + BezelDrawingArea.TextHeight; } }
		public bool StackIconText { get { return true; } }

		public HUDPaneOutlineRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderItem (Context cr, Gdk.Rectangle render_region, bool focused)
		{
			cr.Rectangle (render_region.X, render_region.Y, render_region.Width, render_region.Height); 
			if (focused) {
				var focused_color = new Cairo.Color (0.3, 0.3, 0.3, 0.6).ColorizeColor (parent.Colors.Background);
				cr.SetSourceRGBA (focused_color);
			} else {
				cr.SetSourceRGBA (0.0, 0.0, 0.0, 0.2);
			}
			cr.Fill ();
			cr.Rectangle (render_region.X + .5, 
			              render_region.Y + .5, 
			              render_region.Width - 1, 
			              render_region.Height - 1);
			if (focused) {
				cr.SetSourceRGBA (parent.Colors.FocusedLine);
			} else {
				cr.SetSourceRGBA (parent.Colors.UnfocusedLine);
			}
			cr.LineWidth = 1;
			cr.Stroke ();
		}

		
	}
	
	public class HUDBackgroundRenderer : IBezelWindowRenderElement
	{
		private BezelDrawingArea parent;
		
		public int Height {
			get {
				return 0;
			}
		}
		public Cairo.Color BackgroundColor {get { return new Cairo.Color (.15, .15, .15, .95); } }

		public HUDBackgroundRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderItem (Context cr, Gdk.Rectangle drawing_area)
		{
			cr.SetRoundedRectanglePath (drawing_area, parent.WindowRadius, false);
			cr.SetSourceRGBA(parent.Colors.Background);
			cr.Fill ();
				
			cr.SetRoundedRectanglePath (drawing_area, parent.WindowRadius, true);
			cr.SetSourceRGB (.35, .35, .35);
			cr.LineWidth = 1;
			cr.Stroke ();
		}
		
		public PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point)
		{
			if (drawing_area.Contains (point))
				return PointLocation.Window;
			return PointLocation.Outside;
		}
	}
	
	
	
	public class HUDTextOverlayRenderer : IBezelOverlayRenderElement
	{
		BezelDrawingArea parent;
		
		public HUDTextOverlayRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}

		public void RenderItem (Context cr, Gdk.Rectangle drawing_area, double overlay)
		{
			cr.Rectangle (drawing_area.X, drawing_area.Y + parent.TextModeOffset, drawing_area.Width,
				              (parent.InternalHeight - parent.TextModeOffset - parent.WindowRadius)); 
			cr.SetSourceRGBA (parent.Colors.FocusedText.R, 
			                            parent.Colors.FocusedText.G, 
			                            parent.Colors.FocusedText.B, 
			                            parent.Colors.FocusedText.A * overlay);
			cr.Fill ();
		}
	}
}
