// ClassicRenderer.cs
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
	public class ClassicTopBar: IBezelTitleBarRenderElement
	{
		BezelDrawingArea parent;
		
		public int Height { get { return 7; } }
		
		public ClassicTopBar (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area)
		{
			int radius = parent.WindowRadius;
			double x = drawing_area.X;
			double y = drawing_area.Y;
			double w = drawing_area.Width;
			int glaze_offset = 85;

			cr.MoveTo  (x+radius, y);
			cr.Arc     (x+w-radius, y+radius, radius, Math.PI*1.5, Math.PI*2);
			cr.LineTo  (x+w, y+glaze_offset);
			cr.CurveTo (x+2*(w/3), glaze_offset-15,
			            x+(w/3), glaze_offset-15,
			            x, y+glaze_offset);
			cr.Arc     (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
			LinearGradient lg = new LinearGradient (x, y, x, glaze_offset);
			lg.AddColorStop (0, new Cairo.Color (1, 1, 1, 0));
			lg.AddColorStop (1, new Cairo.Color (1, 1, 1, .25));
			cr.Pattern = lg;
			cr.Fill ();
			lg.Destroy ();

			cr.MoveTo (x + w - 30, y + 7);
			cr.LineTo (x + w - 20,  y + 7);
			cr.LineTo (x + w - 25, y + 12);
			cr.Color = new Cairo.Color (1, 1, 1, .95);
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
	
	public class ClassicPaneOutlineRenderer : IBezelPaneRenderElement
	{
		BezelDrawingArea parent;
		Surface sr_active, sr_inactive;
		int surface_height = 0;
		
		public int Width { get { return IconSize+47; } }
		public int Height { get { return IconSize + 25 + BezelTextUtils.TextHeight; } }
		public int IconSize { get { return 128; } }
		public bool StackIconText { get { return true; } }
		
		public ClassicPaneOutlineRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area, Pane pane, bool focused)
		{
			if (sr_active == null || sr_inactive == null || surface_height != Height) {
				surface_height = Height;
				sr_active = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				sr_inactive = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				Context c2 = new Context (sr_active);
				CairoUtils.SetRoundedRectanglePath (c2, 0, 0, Width, Height, parent.WindowRadius);
				c2.Color = new Cairo.Color (1.0, 1.0, 1.0, 0.4);
				c2.Fill ();
				(c2 as IDisposable).Dispose ();
				
				c2 = new Context (sr_inactive);
				CairoUtils.SetRoundedRectanglePath (c2, 0, 0, Width, Height, parent.WindowRadius);
				c2.Color = new Cairo.Color (1.0, 1.0, 1.0, 0.1);
				c2.Fill ();
				(c2 as IDisposable).Dispose ();
			}
			int offset = parent.PaneOffset (pane);
			if (pane == parent.Focus)
				cr.SetSource (sr_active, drawing_area.X + offset, drawing_area.Y + parent.WindowBorder + parent.TitleBarHeight);
			else
				cr.SetSource (sr_inactive, drawing_area.X + offset, drawing_area.Y + parent.WindowBorder + parent.TitleBarHeight);
			cr.Paint ();
		}

	}
	
	public class ClassicBackgroundRenderer : IBezelWindowRenderElement
	{
		BezelDrawingArea parent;
		
		public Cairo.Color BackgroundColor {
			get {
				Gdk.Color bgColor;
				using (Gtk.Style rcstyle = Gtk.Rc.GetStyle (parent)) {
					bgColor = rcstyle.Backgrounds[(int) StateType.Selected];
				}
				bgColor = CairoUtils.SetMaximumValue (bgColor, 65);
				
				return CairoUtils.ConvertToCairo (bgColor, .95);
			}
		}

		public ClassicBackgroundRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area)
		{
			CairoUtils.SetRoundedRectanglePath (cr, drawing_area, parent.WindowRadius, false);
			LinearGradient lg = new LinearGradient (0, drawing_area.Y, 0, drawing_area.Height);
			lg.AddColorStop (0, parent.Colors.BackgroundDark);
			lg.AddColorStop (1, parent.Colors.BackgroundLight);
			cr.Pattern = lg;
			lg.Destroy ();
			cr.Fill ();
		}
		
		public PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point)
		{
			if (drawing_area.Contains (point))
				return PointLocation.Window;
			return PointLocation.Outside;
		}
	}
	
	public class ClassicTextOverlayRenderer : IBezelOverlayRenderElement
	{
		BezelDrawingArea parent;
		
		public ClassicTextOverlayRenderer (BezelDrawingArea parent)
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
}
