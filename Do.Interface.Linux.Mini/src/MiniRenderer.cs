// MiniRenderer.cs
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
using Do.Universe;

namespace Do.Interface
{
	public class MiniTopBar: IBezelTitleBarRenderElement
	{
		BezelDrawingArea parent;
		
		public int Height { get { return 7; } }
		
		public MiniTopBar (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderItem (Context cr, Gdk.Rectangle drawing_area)
		{
			int radius = parent.WindowRadius;
			double x = drawing_area.X;
			double y = drawing_area.Y;
			double w = drawing_area.Width;
			int glaze_offset = 15;

			cr.MoveTo  (x+radius, y);
			cr.Arc     (x+w-radius, y+radius, radius, Math.PI*1.5, Math.PI*2);
			cr.LineTo  (x+w, y+glaze_offset);
			cr.CurveTo (x+2*(w/3), glaze_offset+15,
			            x+(w/3), glaze_offset+15,
			            x, y+glaze_offset);
			cr.Arc     (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
			cr.SetSourceRGBA (1, 1, 1, .2);
			cr.Fill ();

			cr.MoveTo (x + w - 30, y + 7);
			cr.LineTo (x + w - 20,  y + 7);
			cr.LineTo (x + w - 25, y + 12);
			cr.SetSourceRGBA (1, 1, 1, .95);
			cr.Fill ();
		}
		
		public PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point)
		{
			Gdk.Rectangle pref_circle = new Gdk.Rectangle (drawing_area.X + drawing_area.Width - 32,
				                                 drawing_area.Y +5, 15, 15);
			if (pref_circle.Contains (point))
				return PointLocation.Preferences;
			return PointLocation.Window;
		}
	}
	
	public class MiniPaneOutlineRenderer : IBezelPaneRenderElement
	{
		BezelDrawingArea parent;
		Surface sr_active;
		int surface_height = 0;
		
		public int Width { get { return IconSize+125; } }
		public int Height { get { return IconSize + 20; } }
		public int IconSize { get { return 48; } }
		public bool StackIconText { get { return false; } }
		
		public MiniPaneOutlineRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderItem (Context cr, Gdk.Rectangle render_region, bool focused)
		{
			if (sr_active == null || surface_height != Height) {
				if (sr_active != null) {
					sr_active.Dispose ();
				}
				surface_height = Height;
				sr_active = cr.CreateSimilarToTarget (Width, Height);
				using (var c2 = new Context (sr_active)) {
					c2.SetRoundedRectanglePath (0, 0, Width, Height, parent.WindowRadius);
					using (var lg = new LinearGradient (0, 0, 0, Height)) {
						lg.AddColorStop (0, new Cairo.Color (1, 1, 1, 0));
						lg.AddColorStop (0.4, new Cairo.Color (1, 1, 1, 0));
						lg.AddColorStop (1, new Cairo.Color (1, 1, 1, .3));
						c2.SetSource (lg);
						c2.Fill ();
					}
				}
			}
			
			if (!focused)
				return;
			cr.SetSource (sr_active, render_region.X, render_region.Y);
			cr.Paint ();
		}

	}
	
	public class MiniBackgroundRenderer : IBezelWindowRenderElement
	{
		BezelDrawingArea parent;
		
		public Cairo.Color BackgroundColor {
			get {
				Gdk.Color bgColor;
				using (Gtk.Style rcstyle = Gtk.Rc.GetStyle (parent)) {
					bgColor = rcstyle.Backgrounds[(int) StateType.Selected];
				}
				bgColor = bgColor.SetMaximumValue (65);
				
				return bgColor.ConvertToCairo (.95);
			}
		}

		public MiniBackgroundRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderItem (Context cr, Gdk.Rectangle drawing_area)
		{
			cr.SetRoundedRectanglePath (drawing_area, parent.WindowRadius, false);
			using (var lg = new LinearGradient (0, drawing_area.Y, 0, drawing_area.Height)) {
				lg.AddColorStop (0, parent.Colors.BackgroundDark);
				lg.AddColorStop (1, parent.Colors.BackgroundLight);
				cr.SetSource (lg);
				cr.Fill ();
			}
		}
		
		public PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point)
		{
			if (drawing_area.Contains (point))
				return PointLocation.Window;
			return PointLocation.Outside;
		}
	}
	
	public class MiniTextOverlayRenderer : IBezelOverlayRenderElement
	{
		BezelDrawingArea parent;
		
		public MiniTextOverlayRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}

		public void RenderItem (Context cr, Gdk.Rectangle drawing_area, double overlay)
		{
			cr.SetRoundedRectanglePath (drawing_area, parent.WindowRadius, false);
			cr.SetSourceRGBA (parent.Colors.FocusedText.R, 
			                            parent.Colors.FocusedText.G, 
			                            parent.Colors.FocusedText.B, 
			                            parent.Colors.FocusedText.A * overlay);
			cr.Fill ();
		}
	}
	
	public class MiniDefaults : IBezelDefaults
	{
		
		
		#region IBezelDefaults implementation 
		
		public int WindowBorder {
			get {
				return 5;
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
