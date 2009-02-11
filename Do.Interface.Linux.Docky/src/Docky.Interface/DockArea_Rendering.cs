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

using Do.Interface;
using Do.Interface.CairoUtils;

using Docky.Core;
using Docky.Utilities;
using Docky.Interface.Painters;

namespace Docky.Interface
{
	
	
	public partial class DockArea
	{
		Dictionary<IDockPainter, Surface> painter_surfaces;
		
		Surface backbuffer, input_area_buffer, dock_icon_buffer;
		IDockPainter painter, last_painter;
		Matrix default_matrix;
		long render_count = 0;
		
		DateTime ActiveIconChangeTime { get; set; }

		bool PainterOverlayVisible { get; set; }
		
		/// <value>
		/// Returns the zoom in percentage (0 through 1)
		/// </value>
		double ZoomIn {
			get {
				if (drag_resizing && drag_start_point != Cursor)
					return 0;
				
				double zoom = Math.Min (1, (DateTime.UtcNow - enter_time).TotalMilliseconds / 
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
		
		//// <value>
		/// The overall offset of the dock as a whole
		/// </value>
		int VerticalOffset {
			get {
				double offset = 0;
				// we never hide in these conditions
				if (!DockPreferences.AutoHide || drag_resizing || PainterOpacity == 1)
					return 0;

				if (PainterOpacity > 0) {
					if (CursorIsOverDockArea) {
						return 0;
					} else {
						offset = Math.Min (1, (DateTime.UtcNow - enter_time).TotalMilliseconds / 
						                   SummonTime.TotalMilliseconds);
						offset = Math.Min (offset, Math.Min (1, 
						                                     (DateTime.UtcNow - interface_change_time)
						                                     .TotalMilliseconds / SummonTime.TotalMilliseconds));
					}
					
					if (PainterOverlayVisible)
						offset = 1 - offset;
				} else {
					offset = Math.Min (1, (DateTime.UtcNow - enter_time).TotalMilliseconds / 
					                   SummonTime.TotalMilliseconds);
					if (CursorIsOverDockArea)
						offset = 1 - offset;
				}
				return (int) (offset * PositionProvider.DockHeight * 1.5);
			}
		}
		
		/// <value>
		/// Determins the opacity of the icons on the normal dock
		/// </value>
		double DockIconOpacity {
			get {
				if (SummonTime < DateTime.UtcNow - interface_change_time) {
					if (PainterOverlayVisible)
						return 0;
					return 1;
				}

				double total_time = (DateTime.UtcNow - interface_change_time).TotalMilliseconds;
				if (PainterOverlayVisible) {
					return 1 - (total_time / SummonTime.TotalMilliseconds);
				} else {
					return total_time / SummonTime.TotalMilliseconds;
				}
			}
		}

		/// <summary>
		/// The opacity of the painter surface
		/// </summary>
		double PainterOpacity {
			get { return 1 - DockIconOpacity; }
		}

		/// <value>
		/// Icon Size used for the dock
		/// </value>
		int IconSize { 
			get { return DockPreferences.IconSize; } 
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

		void BuildRendering ()
		{
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
			cr.AlphaFill ();
			Console.WriteLine ("Drawing icons: {0} {1}", DockItems.Count, render_count++);
			for (int i = 0; i < DockItems.Count; i++)
				DrawIcon (cr, i);
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
			PointD center;
			double zoom;
			IconZoomedPosition (icon, out center, out zoom);
			
			// This gives the actual x,y coordinates of the icon
			PointD iconPosition = new PointD (center.X - zoom * dockItem.Width / 2,
			                                  center.Y - zoom * dockItem.Width / 2);
			
			ClickAnimationType animationType = IconAnimation (icon);
			
			// we will set this flag now
			bool drawUrgency = false;
			if (animationType == ClickAnimationType.Bounce) {
				// bounces twice
				double delta = Math.Abs (30 * Math.Sin 
				                         (dockItem.TimeSinceClick.TotalMilliseconds * Math.PI / 
				                          (BounceTime.TotalMilliseconds / 2)));
				
				iconPosition = iconPosition.RelativeMovePoint (delta, RelativeMove.Inward);
			} else {
				if (dockItem != null && dockItem.NeedsAttention) {
					drawUrgency = true;
					if (DateTime.UtcNow - dockItem.AttentionRequestStartTime < BounceTime) {
						double urgentMs = (DateTime.UtcNow - dockItem.AttentionRequestStartTime)
							.TotalMilliseconds;
						
						double delta = 100 * Math.Sin (urgentMs * Math.PI / (BounceTime.TotalMilliseconds));
						iconPosition = iconPosition.RelativeMovePoint (delta, RelativeMove.Inward);
					}
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
				if (DockPreferences.DockIsHorizontal) {
					// why this fails to center right... i dont know...
					cr.SetSource (iconSurface, 
					              (int) iconPosition.X, (int) center.Y - dockItem.Height / 2);
				} else {
					cr.SetSource (iconSurface, 
					              (int) iconPosition.X - IconSize / 2 + 5, (int) iconPosition.Y);
				}
				cr.Paint ();
			}
			
			if (0 < dockItem.WindowCount) {
				Gdk.Point location;
				switch (DockPreferences.Orientation) {
				case DockOrientation.Bottom:
					location = new Gdk.Point ((int) center.X, Height - 1);	
					break;
				case DockOrientation.Left:
					location = new Gdk.Point (1, (int) center.Y);
					break;
				case DockOrientation.Right:
					location = new Gdk.Point (Width - 1, (int) center.Y);
					break;
				case DockOrientation.Top:
				default:
					location = new Gdk.Point ((int) center.X, 1);
					break;
				}
				Util.DrawGlowIndicator (cr, location, drawUrgency, dockItem.WindowCount);
			}
			
			// we do a null check here to allow things like separator items to supply
			// a null.  This allows us to draw nothing at all instead of rendering a
			// blank surface (which is slow)
			if (!PopupMenu.Visible && PositionProvider.IndexAtPosition (Cursor) == icon &&
			    CursorIsOverDockArea && dockItem.GetTextSurface (cr.Target) != null && !GtkDragging) {

				Gdk.Point textPoint;
				if (DockPreferences.DockIsHorizontal) {
					textPoint.X = PositionProvider.IconUnzoomedPosition (icon).X - (DockPreferences.TextWidth / 2);
					if (DockPreferences.Orientation == DockOrientation.Top)
						textPoint.Y = (int) (DockPreferences.ZoomPercent * IconSize) + 10;
					else
						textPoint.Y = Height - (int) (DockPreferences.ZoomPercent * IconSize) - 38;
				} else {
					PointD tmp = center.RelativeMovePoint ((IconSize / 2) * DockPreferences.ZoomPercent + 10, 
					                                          RelativeMove.Inward);
					
					textPoint.X = (int) tmp.X;
					textPoint.Y = (int) tmp.Y;
					if (DockPreferences.Orientation == DockOrientation.Right)
						textPoint = textPoint.RelativeMovePoint (DockPreferences.TextWidth, RelativeMove.Inward);
					textPoint = textPoint.RelativeMovePoint (10, RelativeMove.RealUp);
				}
				dockItem.GetTextSurface (cr.Target).Show (cr, textPoint.X, textPoint.Y);
			}
		}

		Gdk.Rectangle GetDockArea ()
		{
			Gdk.Rectangle rect;
			
			// this method is more than somewhat slow on the complexity scale, we want to avoid doing it
			// more than we have to.  Further, when we do call it, we should always check for this shortcut.
			if (DockIconOpacity == 0 || ZoomIn == 0)
				rect = MinimumDockArea;

			rect = PositionProvider.DockArea (ZoomIn, Cursor);

			// fixme this does not work for side dock
			if (rect.Width < 10 * rect.Height && DockIconOpacity < 1) {
				int difference = 10 * rect.Height - rect.Width;
				int alpha = (int) (difference * PainterOpacity);
				rect.X -= alpha / 2;
				rect.Width += alpha;
			}
			return rect;
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
			bool result = base.OnExposeEvent (evnt);
			
			if (!IsDrawable || window.IsRepositionHidden)
				return result;
			
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

			if (DockServices.ItemsService.UpdatesEnabled)
				DrawDrock (cr);
			(cr as IDisposable).Dispose ();
			
			Context cr2 = Gdk.CairoHelper.Create (GdkWindow);
			
			Gdk.Point finalTarget = new Gdk.Point (0, 0).RelativeMovePoint (VerticalOffset, RelativeMove.Outward);
			cr2.SetSource (backbuffer, finalTarget.X, finalTarget.Y);
			
			cr2.Operator = Operator.Source;
			cr2.Paint ();
			
			cr2.Target.Destroy ();
			((IDisposable)cr2.Target).Dispose ();
			((IDisposable)cr2).Dispose ();
			
			return result;
		}
	}
}
