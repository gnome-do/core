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

using Do.Addins;

namespace Do.UI
{
	public interface IBezelWindowRenderElement
	{
		int Height {get;}
		
		void RenderElement (Context cr, Gdk.Rectangle drawing_area);
		PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point);
	}
	
	public interface IBezelOverlayRenderElement
	{
		int Height {get;}
		
		void RenderElement (Context cr, Gdk.Rectangle drawing_area, double overlay);
	}
	
	public interface IBezelPaneRenderElement
	{
		int Width  {get;}
		int Height {get;}
		
		void RenderElement (Context cr, Gdk.Rectangle drawing_area, Pane pane, bool focused);
	}

	public class HUDTopBar : IBezelWindowRenderElement
	{
		BezelDrawingArea parent;
		Surface border_buffer;
		
		public int Height { get { return 21; } }
		
		public HUDTopBar (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		private void RenderCloseCircle (Context cr)
		{
			cr.Arc (12, Height / 2, 6, 0, Math.PI*2);
			cr.Color = new Cairo.Color (1, 1, 1, .8);
			cr.Fill ();
			
			cr.MoveTo (15, (Height / 2) - 3);
			cr.LineTo (9,  (Height / 2) + 3);
			cr.MoveTo (9,  (Height / 2) - 3);
			cr.LineTo (15, (Height / 2) + 3);
			
			cr.Color = new Cairo.Color (0.2, 0.2, 0.2, .8);
			cr.LineWidth = 2;
			cr.Stroke ();
		}
		
		private void RenderDownCircle (Context cr)
		{
			cr.Arc (parent.TwoPaneWidth - 12,
			        Height / 2, 6, 0, Math.PI*2);
			cr.Color = new Cairo.Color (1, 1, 1, .8);
			cr.Fill ();
			
			cr.MoveTo (parent.TwoPaneWidth - 15, (Height / 2) - 2);
			cr.LineTo (parent.TwoPaneWidth - 9,  (Height / 2) - 2);
			cr.LineTo (parent.TwoPaneWidth - 12, (Height / 2) + 3);
			cr.Color = new Cairo.Color (0.2, 0.2, 0.2, .8);
			cr.Fill ();
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area)
		{
			if (border_buffer == null) {
				
				Surface surface = cr.Target.CreateSimilar (cr.Target.Content, parent.TwoPaneWidth, Height);
				Context cr2 = new Context (surface);
				
				SetTitlePath (cr2);
				cr2.Operator = Cairo.Operator.Source;
				LinearGradient title_grad = new LinearGradient (0, 0, 0, Height);
				title_grad.AddColorStop (0.0, BezelColors.Colors["titlebar_step1"]);
				title_grad.AddColorStop (0.5, BezelColors.Colors["titlebar_step2"]);
				title_grad.AddColorStop (0.5, BezelColors.Colors["titlebar_step3"]);
				cr2.Pattern = title_grad;
				cr2.FillPreserve ();
				cr2.Operator = Cairo.Operator.Over;
			
				LinearGradient grad = new LinearGradient (0, 0, 0, Height);
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
			
			if (drawing_area.Width == parent.TwoPaneWidth) {
				cr.SetSource (border_buffer, drawing_area.X, drawing_area.Y);
				cr.Rectangle (drawing_area.X, drawing_area.Y, drawing_area.Width, Height);
				cr.Fill ();
			} else {
				cr.SetSource (border_buffer, drawing_area.X, drawing_area.Y);
				cr.Rectangle (drawing_area.X, drawing_area.Y, 200, Height);
				cr.Fill ();
				
				cr.SetSource (border_buffer, drawing_area.X + drawing_area.Width - parent.TwoPaneWidth, drawing_area.Y);
				cr.Rectangle (drawing_area.X + 200, drawing_area.Y, drawing_area.Width - 200, Height);
				cr.Fill ();
			}
			RenderTitleText (cr, drawing_area);
		}
		
		void RenderTitleText (Context cr, Gdk.Rectangle drawing_area) {
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			string s = "GNOME Do";
			if (DateTime.Now.Day == 25 && DateTime.Now.Month == 12)
				s = "Merry Christmas!!!";
			BezelTextUtils.RenderLayoutText (cr, s, 0, drawing_area.Y + 5, parent.WindowWidth, color, 
			                  Pango.Alignment.Center, Pango.EllipsizeMode.End, parent);
		}
		
		private void SetTitlePath (Cairo.Context cr)
		{
			int radius = parent.WindowRadius;
			double x = .5;
			double y = .5;
			double width = parent.TwoPaneWidth - 1;
			cr.MoveTo (x+radius, y);
			cr.Arc (x+width-radius, y+radius, radius, Math.PI*1.5, Math.PI*2);
			cr.LineTo (x+width, Height);
			cr.LineTo (x, Height);
			cr.Arc (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
		}
		
		public PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point)
		{
			Gdk.Rectangle close_circle = new Gdk.Rectangle (drawing_area.X + 6, drawing_area.Y + 2,
			                                                12, 15);
			Gdk.Rectangle pref_circle = new Gdk.Rectangle (drawing_area.X + drawing_area.Width - 18, 
			                                               drawing_area.Y + 2, 12, 15);
			if (close_circle.Contains (point))
				return PointLocation.Close;
			else if (pref_circle.Contains (point))
				return PointLocation.Preferences;
			else
				return PointLocation.Window;
		}
	}
	
	public class ClassicTopBar: IBezelWindowRenderElement
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
//			double h = drawing_area.Height;
			double w = drawing_area.Width;
			int glaze_offset = 90;

			cr.MoveTo (x+radius, y);
			cr.Arc (x+w-radius, y+radius, radius, Math.PI*1.5, Math.PI*2);
			cr.LineTo (x+w, y+glaze_offset);
			cr.CurveTo (x+2*(w/3), glaze_offset-25,
			            x+(w/3), glaze_offset-25,
			            x, glaze_offset);
			cr.Arc (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
			LinearGradient lg = new LinearGradient (x, y, x, glaze_offset);
			lg.AddColorStop (0, new Cairo.Color (1, 1, 1, 0));
			lg.AddColorStop (1, new Cairo.Color (1, 1, 1, .25));
			cr.Pattern = lg;
			cr.Fill ();

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
	
	public class HUDPaneOutlineRenderer : IBezelPaneRenderElement
	{
		BezelDrawingArea parent;
		
		public int Width { get { return 160; } }

		public int Height { get { return BezelDrawingArea.IconSize + 15 + BezelTextUtils.TextHeight; } }

		public HUDPaneOutlineRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area, Pane pane, bool focused)
		{
			int offset = parent.PaneOffset (pane);
			cr.Rectangle (drawing_area.X + offset, 
			              drawing_area.Y + parent.WindowBorder + parent.TitleBarHeight, 
			              Width, 
			              Height);
			cr.Color = (focused) ? BezelColors.Colors["focused_box"] : BezelColors.Colors["unfocused_box"];
			cr.Fill ();
			cr.Rectangle (drawing_area.X + offset - .5, 
			              drawing_area.Y + parent.WindowBorder + parent.TitleBarHeight - .5, 
			              Width + 1, 
			              Height + 1);
			cr.Color = (focused) ? BezelColors.Colors["focused_line"] : BezelColors.Colors["unfocused_line"];
			cr.LineWidth = 1;
			cr.Stroke ();
		}

		
	}
	
	public class ClassicPaneOutlineRenderer : IBezelPaneRenderElement
	{
		BezelDrawingArea parent;
		
		public int Width { get { return 175; } }

		public int Height { get { return BezelDrawingArea.IconSize + 25 + BezelTextUtils.TextHeight; } }

		public ClassicPaneOutlineRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area, Pane pane, bool focused)
		{
			int offset = parent.PaneOffset (pane);
			cr.MoveTo (drawing_area.X + offset + parent.WindowRadius, 
			           drawing_area.Y + parent.WindowBorder + parent.TitleBarHeight);
			cr.Arc (drawing_area.X + offset + Width - parent.WindowRadius, 
			        drawing_area.Y + parent.WindowBorder + parent.TitleBarHeight + parent.WindowRadius, 
			        parent.WindowRadius, 
			        Math.PI*1.5, 
			        Math.PI*2);
			cr.Arc (drawing_area.X + offset + Width - parent.WindowRadius,
			        drawing_area.Y + parent.WindowBorder + parent.TitleBarHeight + Height - parent.WindowRadius,
			        parent.WindowRadius,
			        0,
			        Math.PI*.5);
			cr.Arc (drawing_area.X + offset + parent.WindowRadius,
			        drawing_area.Y + parent.WindowBorder + parent.TitleBarHeight + Height - parent.WindowRadius,
			        parent.WindowRadius,
			        Math.PI*.5,
			        Math.PI);
			cr.Arc (drawing_area.X + offset + parent.WindowRadius,
			        drawing_area.Y + parent.WindowBorder + parent.TitleBarHeight + parent.WindowRadius, 
			        parent.WindowRadius,
			        Math.PI,
			        Math.PI*1.5);
			cr.Color = (focused) ? BezelColors.Colors["focused_box"] : BezelColors.Colors["unfocused_box"];
			cr.Fill ();
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

		public HUDBackgroundRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area)
		{
			SetRoundedPath (cr, drawing_area, false);
			cr.Color = BezelColors.Colors["background"];
			cr.Fill ();
				
			SetRoundedPath (cr, drawing_area, true);
			cr.Color = BezelColors.Colors["outline"];
			cr.LineWidth = 1;
			cr.Stroke ();
		}
		
		private void SetRoundedPath (Cairo.Context cr, Gdk.Rectangle drawing_area, bool strokePath)
		{
			int radius = parent.WindowRadius;
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
			cr.Arc (x+w-radius, y+h-radius, radius, 0, Math.PI*.5);
			cr.Arc (x+radius, y+h-radius, radius, Math.PI*.5, Math.PI);
			cr.Arc (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
		}
		
		public PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point)
		{
			if (drawing_area.Contains (point))
				return PointLocation.Window;
			return PointLocation.Outside;
		}
	}
	
	public class ClassicBackgroundRenderer : IBezelWindowRenderElement
	{
		private BezelDrawingArea parent;
		
		public int Height {
			get {
				return 0;
			}
		}

		public ClassicBackgroundRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}
		
		public void RenderElement (Context cr, Gdk.Rectangle drawing_area)
		{
			SetRoundedPath (cr, drawing_area, false);
			LinearGradient lg = new LinearGradient (0, drawing_area.Y, 0, drawing_area.Height);
			lg.AddColorStop (0, BezelColors.Colors["background_dk"]);
			lg.AddColorStop (1, BezelColors.Colors["background_lt"]);
			cr.Pattern = lg;
			cr.Fill ();
		}
		
		private void SetRoundedPath (Cairo.Context cr, Gdk.Rectangle drawing_area, bool strokePath)
		{
			int radius = parent.WindowRadius;
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
			cr.Arc (x+w-radius, y+h-radius, radius, 0, Math.PI*.5);
			cr.Arc (x+radius, y+h-radius, radius, Math.PI*.5, Math.PI);
			cr.Arc (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
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
		
		public int Height {
			get {
				return 0;
			}
		}
		
		public HUDTextOverlayRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}

		public void RenderElement (Context cr, Gdk.Rectangle drawing_area, double overlay)
		{
			cr.Rectangle (drawing_area.X, drawing_area.Y + parent.TextModeOffset, drawing_area.Width,
				              (parent.InternalHeight - parent.TextModeOffset - parent.WindowRadius)); 
			cr.Color = new Cairo.Color (BezelColors.Colors["focused_text"].R, 
			                            BezelColors.Colors["focused_text"].G, 
			                            BezelColors.Colors["focused_text"].B, 
			                            BezelColors.Colors["focused_text"].A * overlay);
			cr.Fill ();
		}
	}
	
	public class ClassicTextOverlayRenderer : IBezelOverlayRenderElement
	{
		BezelDrawingArea parent;
		
		public int Height {
			get {
				return 0;
			}
		}
		
		public ClassicTextOverlayRenderer (BezelDrawingArea parent)
		{
			this.parent = parent;
		}

		public void RenderElement (Context cr, Gdk.Rectangle drawing_area, double overlay)
		{
			SetRoundedPath (cr, drawing_area, false);
			cr.Color = new Cairo.Color (BezelColors.Colors["focused_text"].R, 
			                            BezelColors.Colors["focused_text"].G, 
			                            BezelColors.Colors["focused_text"].B, 
			                            BezelColors.Colors["focused_text"].A * overlay);
			cr.Fill ();
		}
		
		private void SetRoundedPath (Cairo.Context cr, Gdk.Rectangle drawing_area, bool strokePath)
		{
			int radius = parent.WindowRadius;
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
			cr.Arc (x+w-radius, y+h-radius, radius, 0, Math.PI*.5);
			cr.Arc (x+radius, y+h-radius, radius, Math.PI*.5, Math.PI);
			cr.Arc (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
		}
	}
}
