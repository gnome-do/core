// DockArea_Rendering.cs
// 
// Copyright (C) 2009 GNOME Do
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
using System.Linq;

using Cairo;
using Gdk;
using Gtk;

using Do.Interface;
using Do.Interface.CairoUtils;

using Docky.Core;
using Docky.Utilities;
using Docky.Interface.Painters;

namespace Docky.Interface
{
	
	
	internal partial class DockArea
	{
		class PreviousRenderData {
			public bool ForceFullRender { get; set; }
			public Gdk.Point LastCursor { get; set; }
			public double ZoomIn { get; set; }
		}
		
		public static DateTime RenderTime { get; private set; }
		
		const int IndicatorSize = 9;
		const int UrgentIndicatorSize = 12;
		
		Dictionary<IDockPainter, Surface> painter_surfaces;
		bool fast_render_fail, first_render_set;
		
		Surface backbuffer, input_area_buffer, dock_icon_buffer;
		Surface indicator, urgent_indicator;
		IDockPainter painter, last_painter;
		Matrix default_matrix;
		
		DateTime ActiveIconChangeTime { get; set; }
		
		DateTime FirstRenderTime { get; set; }
		
		PreviousRenderData RenderData { get; set; }

		bool PainterOverlayVisible { get; set; }

		bool CanFastRender {
			get {
				bool canFastRender = !RenderData.ForceFullRender && 
					    RenderData.ZoomIn == 1 && 
						ZoomIn == 1 && 
						!AnimationState [Animations.IconInsert] &&
						!AnimationState [Animations.UrgencyChanged] &&
						!AnimationState [Animations.Bounce];
				
				fast_render_fail = canFastRender;
				return canFastRender && !fast_render_fail;
			}
		}
		
		/// <value>
		/// Determins the opacity of the icons on the normal dock
		/// </value>
		double DockIconOpacity {
			get {
				if (SummonTime < RenderTime - interface_change_time) {
					if (PainterOverlayVisible)
						return 0;
					return 1;
				}

				double total_time = (RenderTime - interface_change_time).TotalMilliseconds;
				if (PainterOverlayVisible) {
					return 1 - (total_time / SummonTime.TotalMilliseconds);
				} else {
					return total_time / SummonTime.TotalMilliseconds;
				}
			}
		}
		
		/// <value>
		/// Icon Size used for the dock
		/// </value>
		int IconSize { 
			get { return DockPreferences.IconSize; } 
		}
		
		IDockPainter LastPainter { 
			get {
				return last_painter;
			}
			set {
				if (last_painter == value)
					return;
				if (last_painter != null)
					last_painter.PaintNeeded -= HandlePaintNeeded;
				last_painter = value;
			}
		}
		
		IDockPainter Painter { 
			get {
				return painter;
			}
			set {
				if (value == painter)
					return;
				LastPainter = painter;
				painter = value;
				if (painter != null)
					painter.PaintNeeded += HandlePaintNeeded;
			}
		}

		/// <summary>
		/// The opacity of the painter surface
		/// </summary>
		double PainterOpacity {
			get { return 1 - DockIconOpacity; }
		}

		//// <value>
		/// The overall offset of the dock as a whole
		/// </value>
		int VerticalOffset {
			get {
				double offset = 0;
				// we never hide in these conditions
				if (!DockPreferences.AutoHide || drag_resizing || PainterOpacity == 1) {
					if ((RenderTime - FirstRenderTime) > SummonTime)
						return 0;
					offset = 1 - Math.Min (1, (DateTime.UtcNow - FirstRenderTime).TotalMilliseconds / SummonTime.TotalMilliseconds);
					return (int) (offset * PositionProvider.DockHeight * 1.5);
				}

				if (PainterOpacity > 0) {
					if (CursorIsOverDockArea) {
						return 0;
					} else {
						offset = Math.Min (1, (RenderTime - enter_time).TotalMilliseconds / 
						                   SummonTime.TotalMilliseconds);
						offset = Math.Min (offset, Math.Min (1, 
						                                     (RenderTime - interface_change_time)
						                                     .TotalMilliseconds / SummonTime.TotalMilliseconds));
					}
					
					if (PainterOverlayVisible)
						offset = 1 - offset;
				} else {
					offset = Math.Min (1, (RenderTime - enter_time).TotalMilliseconds / 
					                   SummonTime.TotalMilliseconds);
					if (CursorIsOverDockArea)
						offset = 1 - offset;
				}
				return (int) (offset * PositionProvider.DockHeight * 1.5);
			}
		}
		
		/// <value>
		/// Returns the zoom in percentage (0 through 1)
		/// </value>
		double ZoomIn {
			get {
				if (drag_resizing && drag_start_point != Cursor)
					return 0;
				
				double zoom = Math.Min (1, (RenderTime - enter_time).TotalMilliseconds / 
					                 BaseAnimationTime.TotalMilliseconds);
				if (CursorIsOverDockArea) {
					if (DockPreferences.AutoHide)
						zoom = 1;
				} else {
					zoom = 1 - zoom;
				}
				
				if (PainterOverlayVisible)
					zoom = zoom * DockIconOpacity;
				
				return zoom;
			}
		}

		void BuildRendering ()
		{
			RenderData = new PreviousRenderData ();
			painter_surfaces = new Dictionary<IDockPainter, Surface> ();
			default_matrix = new Matrix ();
		}
		
		void DrawDrock (Context cr)
		{
			Gdk.Rectangle dockArea = GetDockArea ();
			DockBackgroundRenderer.RenderDockBackground (cr, dockArea);

			IDockPainter dpaint = (Painter == null) ? LastPainter : Painter;
			
			if (PainterOpacity > 0 && dpaint != null) {
				Surface overlay_surface = GetOverlaySurface (cr);
				
				using (Context input_cr = new Context (overlay_surface)) {
					if (!dpaint.DoubleBuffer)
						input_cr.AlphaFill ();
					dpaint.Paint (input_cr, dockArea, Cursor);
				}

				cr.SetSource (overlay_surface);
				cr.PaintWithAlpha (PainterOpacity);
			}
			
			bool isNotSummonTransition = PainterOpacity == 0 || CursorIsOverDockArea || !DockPreferences.AutoHide;
			if (DockIconOpacity > 0 && isNotSummonTransition) {
				if (dock_icon_buffer == null)
					dock_icon_buffer = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				
				using (Context input_cr = new Context (dock_icon_buffer)) {
					DrawIcons (input_cr);
				}

				int offset =  (int) (IconSize * (1 - DockIconOpacity));
				Gdk.Point iconBufferLocation = new Gdk.Point (0, 0).RelativeMovePoint (offset, RelativeMove.Outward);
				cr.SetSource (dock_icon_buffer, iconBufferLocation.X, iconBufferLocation.Y);
				cr.PaintWithAlpha (DockIconOpacity);
			}
		}
		
		void DrawIcons (Context cr)
		{
			if (!CanFastRender) {
				cr.AlphaFill ();
				for (int i = 0; i < DockItems.Count; i++)
					DrawIcon (cr, i);
			} else {
			
				Gdk.Rectangle renderArea = Gdk.Rectangle.Zero;
				Gdk.Rectangle dockArea = GetDockArea ();
				
				int startItemPosition;
				startItemPosition = Math.Min (Cursor.X, RenderData.LastCursor.X) - 
					(DockPreferences.ZoomSize / 2 + DockPreferences.IconSize);
				
				int endItemPosition;
				endItemPosition = Math.Max (Cursor.X, RenderData.LastCursor.X) + 
					(DockPreferences.ZoomSize / 2 + DockPreferences.IconSize);
				
				int startItem = PositionProvider.IndexAtPosition (startItemPosition, Cursor.Y);
				int endItem = PositionProvider.IndexAtPosition (endItemPosition, Cursor.Y);
				
				PointD firstPosition, lastPosition;
				double firstZoom, lastZoom;
				
				// set up our X value
				if (startItem == -1) {
					startItem = 0;
					renderArea.X = dockArea.X;
				} else {
					IconZoomedPosition (startItem, out firstPosition, out firstZoom);
					renderArea.X = (int) (firstPosition.X - (DockItems [startItem].Width * firstZoom) / 2) - 2;
				}
				
				if (endItem == -1) {
					endItem = DockItems.Count - 1;
					renderArea.Width = dockArea.Width - (renderArea.X - dockArea.X);
				} else {
					IconZoomedPosition (endItem, out lastPosition, out lastZoom);
				
					// Add/Sub 2 to provide a good "buffer" into the dead zone between icons
					renderArea.Width = (int) (lastPosition.X + (DockItems [endItem].Width * lastZoom) / 2) + 2 - renderArea.X;
				}
				
				renderArea.Height = Height;
				
				cr.Rectangle (renderArea.X, renderArea.Y, renderArea.Width, renderArea.Height);
				switch (DockPreferences.Orientation) {
				case DockOrientation.Bottom:
					cr.Rectangle (0, Width, 0, Height - dockArea.Height);
					break;
				case DockOrientation.Top:
					cr.Rectangle (0, Width, dockArea.Height, Height - dockArea.Height);
					break;
				}
				cr.Operator = Operator.Clear;
				cr.Fill ();
				cr.Operator = Operator.Over;
				
				for (int i = startItem; i <= endItem; i++)
					DrawIcon (cr, i);
			}
			
			RenderData.LastCursor = Cursor;
			RenderData.ZoomIn = ZoomIn;
			RenderData.ForceFullRender = false;
		}
		
		void DrawIcon (Context cr, int icon)
		{
			// Don't draw the icon we are dragging around
			if (GtkDragging && !DragState.IsFinished) {
				int item = DockItems.IndexOf (DragState.DragItem);
				if (item == icon && DockServices.ItemsService.ItemCanBeMoved (item))
					return;
			}
			
			AbstractDockItem dockItem = DockItems [icon];
			if (dockItem == null) return;
			
			PointD center;
			double zoom;
			IconZoomedPosition (icon, out center, out zoom);
			
			// This gives the actual x,y coordinates of the icon
			PointD iconPosition = new PointD (center.X - zoom * (dockItem.Width >> 1),
			                                  center.Y - zoom * (dockItem.Width >> 1));
			
			ClickAnimationType animationType = IconAnimation (icon);
			
			// we will set this flag now
			if (animationType == ClickAnimationType.Bounce) {
				// bounces twice
				double delta = Math.Abs (30 * Math.Sin 
				                         (dockItem.TimeSinceClick.TotalMilliseconds * Math.PI / 
				                          (BounceTime.TotalMilliseconds / 2)));
				
				iconPosition = iconPosition.RelativeMovePoint (delta, RelativeMove.Inward);
			} else {
				if (RenderTime - dockItem.AttentionRequestStartTime < BounceTime) {
					double urgentMs = (RenderTime - dockItem.AttentionRequestStartTime)
						.TotalMilliseconds;
					
					double delta = 100 * Math.Sin (urgentMs * Math.PI / (BounceTime.TotalMilliseconds));
					iconPosition = iconPosition.RelativeMovePoint (delta, RelativeMove.Inward);
				}
			}
			
			int size;
			Surface iconSurface = dockItem.GetIconSurface (cr.Target, (int) (IconSize * zoom), out size);
			
			if (dockItem.ScalingType != ScalingType.None) {
				double scale;
				
				if (size == DockPreferences.FullIconSize) {
					scale = zoom / DockPreferences.IconQuality;
				} else if (size == DockPreferences.IconSize) {
					scale = zoom;
				} else {
					Do.Platform.Log<DockArea>.Error ("Icon provided in unexpected size");
					return;
				}
				
				if (scale != 1)
					cr.Scale (scale, scale);
				// we need to multiply x and y by 1 / scale to undo the scaling of the context.  We only want to zoom
				// the icon, not move it around.
				
				double fadeInOpacity = Math.Min (dockItem.TimeSinceAdd.TotalMilliseconds / 
				                                 InsertAnimationTime.TotalMilliseconds, 1);
				cr.SetSource (iconSurface, 
				              iconPosition.X / scale, iconPosition.Y / scale);
				cr.PaintWithAlpha (fadeInOpacity);
				
				bool shade_light = GtkDragging && 
					dockItem.IsAcceptingDrops && icon == PositionProvider.IndexAtPosition (Cursor);
				
				bool shade_dark = animationType == ClickAnimationType.Darken;
				if (shade_dark || shade_light) {
					cr.Rectangle (iconPosition.X / scale, iconPosition.Y / scale, 
					              DockPreferences.FullIconSize, DockPreferences.FullIconSize);
					
					if (shade_light) {
						cr.Color = new Cairo.Color (.9, .95, 1, .5);
					} else {
						double opacity = (BounceTime - dockItem.TimeSinceClick).TotalMilliseconds / 
							BounceTime.TotalMilliseconds - .7;
						
						cr.Color = new Cairo.Color (0, 0, 0, opacity);
					}
						
					cr.Operator = Operator.Atop;
					cr.Fill ();
					cr.Operator = Operator.Over;
				}
				
				if (scale != 1)
					cr.Matrix = default_matrix;
			} else {
				// since these dont scale, we have some extra work to do to keep them
				// centered
				cr.SetSource (iconSurface, 
				              (int) iconPosition.X, (int) center.Y - (dockItem.Height >> 1));
				cr.Paint ();
			}
			
			if (0 < dockItem.WindowCount) {
				Gdk.Point location;
				switch (DockPreferences.Orientation) {
				case DockOrientation.Bottom:
					location = new Gdk.Point ((int) center.X, Height - 1);	
					break;
				case DockOrientation.Top:
				default:
					location = new Gdk.Point ((int) center.X, 1);
					break;
				}
				DrawGlowIndicator (cr, location, dockItem.NeedsAttention, dockItem.WindowCount);
			}
			
			// we do a null check here to allow things like separator items to supply
			// a null.  This allows us to draw nothing at all instead of rendering a
			// blank surface (which is slow)
			if (!PopupMenu.Visible && PositionProvider.IndexAtPosition (Cursor) == icon &&
			    CursorIsOverDockArea && dockItem.GetTextSurface (cr.Target) != null && !GtkDragging) {

				Gdk.Point textPoint;
				textPoint.X = PositionProvider.IconUnzoomedPosition (icon).X - (DockPreferences.TextWidth >> 1);
				
				if (DockPreferences.Orientation == DockOrientation.Top)
					textPoint.Y = (int) (DockPreferences.ZoomPercent * IconSize) + 10;
				else
					textPoint.Y = Height - (int) (DockPreferences.ZoomPercent * IconSize) - 38;
				
				dockItem.GetTextSurface (cr.Target).Show (cr, textPoint.X, textPoint.Y);
			}
		}
		
		void DrawGlowIndicator (Context cr, Gdk.Point location, bool urgent, int numberOfWindows)
		{
			if (DockPreferences.IndicateMultipleWindows && 1 < numberOfWindows) {
				DrawSingleIndicator (cr, location.RelativeMovePoint (3, RelativeMove.RelativeLeft), urgent);
				DrawSingleIndicator (cr, location.RelativeMovePoint (3, RelativeMove.RelativeRight), urgent);
			} else if (0 < numberOfWindows) {
				DrawSingleIndicator (cr, location, urgent);
			}
		}
		
		void DrawSingleIndicator (Context cr, Gdk.Point location, bool urgent)
		{
			if (urgent) {
				cr.SetSource (GetUrgentIndicator (cr.Target), location.X - UrgentIndicatorSize, location.Y - UrgentIndicatorSize);
			} else {
				cr.SetSource (GetIndicator (cr.Target), location.X - IndicatorSize, location.Y - IndicatorSize);
			}

			cr.Paint ();
		}

		Gdk.Rectangle GetDockArea ()
		{
			Gdk.Rectangle rect;
			
			// this method is more than somewhat slow on the complexity scale, we want to avoid doing it
			// more than we have to.  Further, when we do call it, we should always check for this shortcut.
			if (DockIconOpacity == 0 || ZoomIn == 0)
				rect = MinimumDockArea;
			else
				rect = PositionProvider.DockArea (ZoomIn, Cursor);

			int minSize; 
				
			if (PainterOverlayVisible && Painter != null) {
				minSize = Math.Max (Painter.MinimumWidth, 10 * rect.Height);
			} else if (!PainterOverlayVisible && LastPainter != null) {
				minSize = Math.Max (LastPainter.MinimumWidth, 10 * rect.Height);
			} else {
				minSize = 10 * rect.Height;
			}
			
			if (rect.Width < minSize && DockIconOpacity < 1) {
				int difference = minSize - rect.Width;
				int alpha = (int) (difference * PainterOpacity);
				rect.X -= alpha / 2;
				rect.Width += alpha;
			}
			
			
			return rect;
		}
		
		Surface GetIndicator (Surface similar)
		{
			if (indicator == null) {
				Style style = Docky.Interface.DockWindow.Window.Style;
				Gdk.Color color = style.Backgrounds [(int) StateType.Selected].SetMinimumValue (100);

				indicator = similar.CreateSimilar (similar.Content, IndicatorSize * 2, IndicatorSize * 2);
				Context cr = new Context (indicator);

				double x = IndicatorSize;
				double y = x;
				
				cr.MoveTo (x, y);
				cr.Arc (x, y, IndicatorSize, 0, Math.PI * 2);
				
				RadialGradient rg = new RadialGradient (x, y, 0, x, y, IndicatorSize);
				rg.AddColorStop (0, new Cairo.Color (1, 1, 1, 1));
				rg.AddColorStop (.10, color.ConvertToCairo (1.0));
				rg.AddColorStop (.20, color.ConvertToCairo (.60));
				rg.AddColorStop (.25, color.ConvertToCairo (.25));
				rg.AddColorStop (.50, color.ConvertToCairo (.15));
				rg.AddColorStop (1.0, color.ConvertToCairo (0.0));
				
				cr.Pattern = rg;
				cr.Fill ();
				rg.Destroy ();

				(cr as IDisposable).Dispose ();
			}
			return indicator;
		}

		Surface GetUrgentIndicator (Surface similar)
		{
			if (urgent_indicator == null) {
				Style style = Docky.Interface.DockWindow.Window.Style;
				Gdk.Color color = style.Backgrounds [(int) StateType.Selected];
				byte r, g, b; 
				double h, s, v;	

				r = (byte) ((color.Red)   >> 8);
				g = (byte) ((color.Green) >> 8);
				b = (byte) ((color.Blue)  >> 8);
				Do.Interface.Util.Appearance.RGBToHSV (r, g, b, out h, out s, out v);

				// see if the theme color is too close to red and if so use
				// blue instead
				if (h <= 30 || h >= byte.MaxValue - 30)
					color = new Cairo.Color (0.5, 0.6, 1.0, 1.0).ConvertToGdk ();
				else
					color = new Cairo.Color (1.0, 0.3, 0.3, 1.0).ConvertToGdk ();

				urgent_indicator = similar.CreateSimilar (similar.Content, UrgentIndicatorSize * 2, UrgentIndicatorSize * 2);
				Context cr = new Context (urgent_indicator);

				double x = UrgentIndicatorSize;
				double y = x;
				
				cr.MoveTo (x, y);
				cr.Arc (x, y, UrgentIndicatorSize, 0, Math.PI * 2);
				
				RadialGradient rg = new RadialGradient (x, y, 0, x, y, UrgentIndicatorSize);
				rg.AddColorStop (0, new Cairo.Color (1, 1, 1, 1));
				rg.AddColorStop (.10, color.ConvertToCairo (1.0));
				rg.AddColorStop (.20, color.ConvertToCairo (.60));
				rg.AddColorStop (.35, color.ConvertToCairo (.35));
				rg.AddColorStop (.50, color.ConvertToCairo (.25));
				rg.AddColorStop (1.0, color.ConvertToCairo (0.0));
				
				cr.Pattern = rg;
				cr.Fill ();
				rg.Destroy ();

				(cr as IDisposable).Dispose ();
			}
			return urgent_indicator;
		}

		ClickAnimationType IconAnimation (int icon)
		{
			return (DockItems [icon].TimeSinceClick < BounceTime) ? 
				DockItems [icon].AnimationType : ClickAnimationType.None;
		}
		
		void IconZoomedPosition (int icon, out PointD center, out double zoom)
		{
			PositionProvider.IconZoomedPosition (icon, ZoomIn, Cursor, out center, out zoom);
		}

		Surface GetOverlaySurface (Context similar)
		{
			if (Painter != null && Painter.DoubleBuffer) {
				if (!painter_surfaces.ContainsKey (Painter))
					painter_surfaces [Painter] = similar.Target.CreateSimilar (similar.Target.Content, 
					                                                           Width, Height);
				return painter_surfaces [Painter];
			} else {
				if (input_area_buffer == null)
					input_area_buffer = similar.Target.CreateSimilar (similar.Target.Content, 
					                                                  Width, Height);
				return input_area_buffer;
			}
		}

		protected override bool OnExposeEvent(EventExpose evnt)
		{
			if (!IsDrawable || window.IsRepositionHidden)
				return false;
			
			RenderTime = DateTime.UtcNow;
			
			Context cr;
			if (backbuffer == null) {
				cr = Gdk.CairoHelper.Create (GdkWindow);
				backbuffer = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				
				cr.Target.Destroy ();
				(cr.Target as IDisposable).Dispose ();
				(cr as IDisposable).Dispose ();
			}
			
			cr = new Cairo.Context (backbuffer);
			cr.AlphaFill ();

			if (DockServices.ItemsService.UpdatesEnabled) {
				if (!first_render_set) {
					FirstRenderTime = DateTime.UtcNow;
					first_render_set = true;
				}
				DrawDrock (cr);
			}
			(cr as IDisposable).Dispose ();
			
			cr = Gdk.CairoHelper.Create (GdkWindow);
			
			Gdk.Point finalTarget = new Gdk.Point (0, 0).RelativeMovePoint (VerticalOffset, RelativeMove.Outward);
			cr.SetSource (backbuffer, finalTarget.X, finalTarget.Y);
			
			cr.Operator = Operator.Source;
			cr.Paint ();
			
			cr.Target.Destroy ();
			((IDisposable)cr.Target).Dispose ();
			((IDisposable)cr).Dispose ();
			
			return true;
		}
		
		protected override void OnStyleSet (Gtk.Style previous_style)
		{
			if (indicator != null) {
				indicator.Destroy ();
				indicator = null;
			}
			
			if (urgent_indicator != null) {
				urgent_indicator.Destroy ();
				urgent_indicator = null;
			}
			
			RequestFullRender ();
			
			base.OnStyleSet (previous_style);
		}
		
		void RequestFullRender ()
		{
			RenderData.ForceFullRender = true;
		}
	}
}
