// DockArea.cs
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
using System.Linq;
using System.Text.RegularExpressions;

using System.Diagnostics;

using Cairo;
using Gdk;
using Gtk;

using Do.Platform;
using Do.Interface;
using Do.Universe;
using Do.Interface.CairoUtils;
using Do;

using Docky.Utilities;
using Docky.Interface.Renderers;

namespace Docky.Interface
{
	
	
	public class DockArea : Gtk.DrawingArea
	{
		public const int BaseAnimationTime = 150;
		public const int VerticalBuffer = 5;
		public const int HorizontalBuffer = 7;
		const int BounceTime = 700;
		const int SummonTime = 100;
		const int InsertAnimationTime = BaseAnimationTime*5;
		const int WindowHeight = 300;
		const int IconBorderWidth = 2;
		const string HighlightFormat = "<span foreground=\"#5599ff\">{0}</span>";
		
		Gdk.Point cursor;
		
		Gdk.Rectangle minimum_dock_area;
		
		DateTime last_click = DateTime.UtcNow;
		DateTime enter_time = DateTime.UtcNow;
		DateTime interface_change_time = DateTime.UtcNow;
		
		bool cursor_is_handle = false;
		bool drag_resizing = false;
		
		int monitor_width;
		int drag_start_y = 0;
		int drag_start_icon_size = 0;
		int remove_drag_start_x;
		uint animation_timer = 0;
		
		double previous_zoom = 0;
		int previous_item_count = 0;
		
		
		DockWindow window;
		DockItemProvider item_provider;
		Surface backbuffer, input_area_buffer, dock_icon_buffer;
		
		#region public properties
		public bool InputInterfaceVisible { get; set; }
		
		/// <value>
		/// The width of the docks window, but not the visible dock
		/// </value>
		public int Width {
			get { return monitor_width; }
		}
		
		/// <value>
		/// The height of the docks window
		/// </value>
		public int Height { 
			get { return WindowHeight; } 
		}
		
		/// <value>
		/// The width of the visible dock
		/// </value>
		public int DockWidth {
			get {
				int val = 2 * HorizontalBuffer;
				foreach (IDockItem di in DockItems)
					val += 2 * IconBorderWidth + di.Width;
				return val;
			}
		}
		
		/// <summary>
		/// The height of the visible dock
		/// </summary>
		public int DockHeight {
			get {
				return DockPreferences.AutoHide ? 0 : MinimumDockArea.Height;
			}
		}
		
		public Pane CurrentPane {
			get {
				return State.CurrentPane;
			}
			set {
				State.CurrentPane = value;
				AnimatedDraw ();
			}
		}
		
		public bool ThirdPaneVisible { 
			get { return State.ThirdPaneVisible; }
			set { 
				if (State.ThirdPaneVisible == value)
					return;
				State.ThirdPaneVisible = value;
				AnimatedDraw ();
			}
		}
		#endregion
		
		public new DockState State { get; set; }
		
		SummonModeRenderer SummonRenderer { get; set; }
		
		IDockItem [] DockItems { 
			get { return item_provider.DockItems.ToArray (); } 
		}
		
		IDockItem CurrentDockItem {
			get {
				try { return DockItems [DockItemForX (Cursor.X)]; }
				catch { return null; }
			}
		}
		
		#region Zoom Properties
		/// <value>
		/// Returns the zoom in percentage (0 through 1)
		/// </value>
		double ZoomIn {
			get {
				double zoom = Math.Min (1, (DateTime.UtcNow - enter_time).TotalMilliseconds / BaseAnimationTime);
				if (!CursorIsOverDockArea)
					zoom = 1 - zoom;
				if (InputInterfaceVisible) {
					zoom = zoom * DockIconOpacity;
				}
				return zoom;
			}
		}
		
		/// <value>
		/// Returns the width of the zoom (ZoomIn * ZoomSize)
		/// </value>
		int ZoomPixels {
			get {
				return (int) (DockPreferences.ZoomSize * ZoomIn);
			}
		}
		
		#endregion
		
		//// <value>
		/// The overall offset of the dock as a whole
		/// </value>
		int VerticalOffset {
			get {
				double offset = 0;
				if (!DockPreferences.AutoHide || cursor_is_handle)
					return 0;

				if (InputAreaOpacity == 1) {
					offset = 0;
				} else if (InputAreaOpacity > 0) {
					if (CursorIsOverDockArea) {
						return 0;
					} else {
						offset = Math.Min (1, (DateTime.UtcNow - enter_time).TotalMilliseconds / SummonTime);
						offset = Math.Min (offset, Math.Min (1, (DateTime.UtcNow - interface_change_time).TotalMilliseconds / SummonTime));
					}
					
					if (InputInterfaceVisible)
						offset = 1 - offset;
				} else {
					offset = Math.Min (1, (DateTime.UtcNow - enter_time).TotalMilliseconds / SummonTime);
					if (CursorIsOverDockArea)
						offset = 1 - offset;
				}
				return (int) (offset * MinimumDockArea.Height);
			}
		}
		
		/// <value>
		/// Determins the opacity of the icons on the normal dock
		/// </value>
		double DockIconOpacity {
			get {
				double total_time = (DateTime.UtcNow - interface_change_time).TotalMilliseconds;
				if (SummonTime < total_time) {
					if (InputInterfaceVisible)
						return 0;
					return 1;
				}
				
				if (InputInterfaceVisible) {
					return 1 - (total_time/SummonTime);
				} else {
					return total_time/SummonTime;
				}
			}
		}
		
		double InputAreaOpacity {
			get { return 1 - DockIconOpacity; }
		}
		
		int IconSize { 
			get { return DockPreferences.IconSize; } 
		}
		
		/// <value>
		/// The current cursor as known to the dock.
		/// </value>
		public Gdk.Point Cursor {
			get {
				return cursor;
			}
			set {
				bool tmp = CursorIsOverDockArea;
				cursor = value;
				
				if (CursorIsOverDockArea != tmp) {
					SetParentInputMask ();
					enter_time = DateTime.UtcNow;
					AnimatedDraw ();
				}
			}
		}
		
		/// <value>
		/// The center of the stick icon
		/// </value>
		Gdk.Point StickIconCenter {
			get {
				Gdk.Rectangle rect = GetDockArea ();
				return new Gdk.Point (rect.X + rect.Width - 7, rect.Y + 8);
			}
		}
		
		Gdk.Rectangle MinimumDockArea {
			get {
				if (minimum_dock_area.X == 0 && minimum_dock_area.Y == 0 && 
				    minimum_dock_area.Width == 0 && minimum_dock_area.Height == 0) {
					// we never have a zero dock area, zeroing it is a signal to force a recalculation.  Since
					// we use this value a LOT we dont want to recalculate it more than once per draw
					
					int x_offset = (Width - DockWidth) / 2;
					minimum_dock_area = new Gdk.Rectangle (x_offset, Height - IconSize - 2 * VerticalBuffer, DockWidth, 
					                                       IconSize + 2 * VerticalBuffer);
				}
				return minimum_dock_area;
			}
		}
		
		bool CursorNearDraggableEdge {
			get {
				return CursorIsOverDockArea && Math.Abs (Cursor.Y - MinimumDockArea.Y) < 5 && CurrentDockItem is SeparatorItem;
			}
		}
		
		#region Animation properties
		bool CursorIsOverDockArea {
			get {
				Gdk.Rectangle rect = MinimumDockArea;
				rect.Inflate (0, 55);
				return rect.Contains (Cursor); 
			}
		}
		
		bool IconInsertionAnimationNeeded {
			get {
				return DockItems.Any (di => (DateTime.UtcNow - di.DockAddItem).TotalMilliseconds < InsertAnimationTime);
			}
		}
		
		bool PaneChangeAnimationNeeded {
			get {
				return (DateTime.UtcNow - State.CurrentPaneTime).TotalMilliseconds < BaseAnimationTime;
			}
		}
		
		bool ZoomAnimationNeeded {
			get {
				bool is_zoomed_fully_in = CursorIsOverDockArea && ZoomIn == 1;
				bool is_zoomed_fully_out = !CursorIsOverDockArea && ZoomIn == 0;
				return !(is_zoomed_fully_in || is_zoomed_fully_out); 
			}
		}
		
		bool OpenAnimationNeeded {
			get { 
				return (DateTime.UtcNow - enter_time).TotalMilliseconds < BaseAnimationTime ||
					(DateTime.UtcNow - interface_change_time).TotalMilliseconds < BaseAnimationTime;
			}
		}
		
		bool BounceAnimationNeeded {
			get { return (DateTime.UtcNow - last_click).TotalMilliseconds < BounceTime; }
		}
		
		bool InputModeChangeAnimationNeeded {
			get { return (DateTime.UtcNow - interface_change_time).TotalMilliseconds < SummonTime; }
		}
		
		bool InputModeSlideAnimationNeeded {
			get { return (DateTime.UtcNow - State.LastCursorChange).TotalMilliseconds < BaseAnimationTime; }
		}
		
		bool ThirdPaneVisibilityAnimationNeeded {
			get { return (DateTime.UtcNow - State.ThirdChangeTime).TotalMilliseconds < BaseAnimationTime; }
		}
		
		bool AnimationNeeded {
			get { 
				return OpenAnimationNeeded || 
					   ZoomAnimationNeeded || 
					   BounceAnimationNeeded || 
					   InputModeChangeAnimationNeeded || 
					   InputModeSlideAnimationNeeded || 
					   IconInsertionAnimationNeeded || 
					   PaneChangeAnimationNeeded || 
					   ThirdPaneVisibilityAnimationNeeded; 
			}
		}
		#endregion
		
		public DockArea (DockWindow window) : base ()
		{
			this.window = window;
			item_provider = new DockItemProvider ();
			State = new DockState ();
			SummonRenderer = new SummonModeRenderer (this);
			
			Cursor = new Gdk.Point (-1, -1);
			minimum_dock_area = new Gdk.Rectangle ();
			
			Gdk.Rectangle geo;
			geo = Screen.GetMonitorGeometry (0);
			
			monitor_width = geo.Width;
			SetSizeRequest (geo.Width, Height);
			
			this.SetCompositeColormap ();
			
			AddEvents ((int) EventMask.PointerMotionMask | 
			           (int) EventMask.LeaveNotifyMask |
			           (int) EventMask.ButtonPressMask | 
			           (int) EventMask.ButtonReleaseMask);
			
			DoubleBuffered = false;
			
			RegisterEvents ();
			
			GLib.Timeout.Add (20, () => {
				//must be done after construction is complete
				SetParentInputMask ();
				return false;
			});
		}
		
		public Surface GetSimilar (int width, int height)
		{
			if (backbuffer == null)
				throw new Exception ("DockArea is not fully initialized");
			return backbuffer.CreateSimilar (backbuffer.Content, width, height);
		}
		
		void RegisterEvents ()
		{
			item_provider.DockItemsChanged += OnDockItemsChanged;
			
			ItemMenu.Instance.Hidden += OnItemMenuHidden;
		}
		
		void AnimatedDraw ()
		{
			if (0 < animation_timer)
				return;
			
			QueueDraw ();
			animation_timer = GLib.Timeout.Add (16, OnDrawTimeoutElapsed);
		}
		
		bool OnDrawTimeoutElapsed ()
		{
			QueueDraw ();
			if (AnimationNeeded)
				return true;
			
			//reset the timer to 0 so that the next time AnimatedDraw is called we fall back into
			//the draw loop.
			animation_timer = 0;
			return false;
		}
		
		void DrawDrock (Context cr)
		{
			Gdk.Rectangle dockArea = GetDockArea ();
			DockBackgroundRenderer.RenderDockBackground (cr, dockArea);
			
			if (InputAreaOpacity > 0) {
				if (input_area_buffer == null)
					input_area_buffer = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				
				using (Context input_cr = new Context (input_area_buffer)) {
					input_cr.AlphaFill ();
					SummonRenderer.RenderSummonMode (input_cr, dockArea);
				}
				
				cr.SetSource (input_area_buffer);
				cr.PaintWithAlpha (InputAreaOpacity);
			}
			
			bool isNotSummonTransition = InputAreaOpacity == 0 || CursorIsOverDockArea || !DockPreferences.AutoHide;
			if (DockIconOpacity > 0 && isNotSummonTransition) {
				if (dock_icon_buffer == null)
					dock_icon_buffer = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				
				using (Context input_cr = new Context (dock_icon_buffer)) {
					DrawIcons (input_cr);
					
					if (CursorIsOverDockArea)
						DrawThumbnailIcon (input_cr);
				}
				
				cr.SetSource (dock_icon_buffer, 0, IconSize * (1 - DockIconOpacity));
				cr.PaintWithAlpha (DockIconOpacity);
			}
		}
		
		void DrawIcons (Context cr)
		{
			if (ZoomIn == 1 && previous_zoom == 1 && !AnimationNeeded && previous_item_count == DockItems.Length) {
				int current_item = DockItemForX (Cursor.X);
				
				int left_item = Math.Max (0, DockItemForX (Cursor.X - DockPreferences.ZoomSize / 2));
				
				int right_item = DockItemForX (Cursor.X + DockPreferences.ZoomSize / 2);
				if (right_item == -1) 
					right_item = DockItems.Length - 1;
				
				int left_x, right_x;
				double d1, d2;
				
				if (left_item == 0) {
					left_x = 0;
				} else {
					IconPositionedCenterX (left_item, out left_x, out d1);
					left_x -= (int) (d1 * DockItems [left_item].Width / 2) + IconBorderWidth;
				}
				
				if (right_item == DockItems.Length - 1) {
					right_x = Width;
				} else {
					IconPositionedCenterX (right_item, out right_x, out d2);
					right_x += (int) (d2 * DockItems [right_item].Width / 2) + IconBorderWidth;
				}
					
				cr.Rectangle (left_x, 0, right_x - left_x, Height);
				cr.Color = new Cairo.Color (1, 1, 1, 0);
				cr.Operator = Operator.Source;
				cr.Fill ();
				cr.Operator = Operator.Over;
				
				for (int i=left_item; i<=right_item; i++)
					DrawIcon (cr, i);
			} else {
				cr.AlphaFill ();
				for (int i=0; i<DockItems.Length; i++)
					DrawIcon (cr, i);
			}
			previous_zoom = ZoomIn;
			previous_item_count = DockItems.Length;
		}
		
		void DrawIcon (Context cr, int icon)
		{
			int center;
			double zoom;
			IconPositionedCenterX (icon, out center, out zoom);
			
			double insertion_ms = (DateTime.UtcNow - DockItems [icon].DockAddItem).TotalMilliseconds;
			if (insertion_ms < InsertAnimationTime) {
				// if we just inserted the icon, we scale it down the newer it is.  This gives the nice
				// zoom in effect for newly inserted icons
				zoom *= insertion_ms / InsertAnimationTime;
			}
			
			// This gives the actual x,y coordinates of the icon 
			double x = (center - zoom * DockItems [icon].Width / 2);
			double y = (Height - (zoom * DockItems [icon].Width)) - VerticalBuffer;
			
			int total_ms = (int) (DateTime.UtcNow - DockItems [icon].LastClick).TotalMilliseconds;
			if (total_ms < BounceTime) {
				y -= Math.Abs (20 * Math.Sin (total_ms * Math.PI / (BounceTime / 2)));
			}
			
			double scale = zoom/DockPreferences.IconQuality;
			
			if (DockItems [icon].Scalable) {
				cr.Scale (scale, scale);
				// we need to multiply x and y by 1 / scale to undo the scaling of the context.  We only want to zoom
				// the icon, not move it around.
				cr.SetSource (DockItems [icon].GetIconSurface (cr.Target), x * (1 / scale), y * (1 / scale));
				cr.Paint ();
				cr.Scale (1 / scale, 1 / scale);
			} else {
				// since these dont scale, we have some extra work to do to keep them centered
				double startx = x + (zoom*DockItems [icon].Width - DockItems [icon].Width) / 2;
				cr.SetSource (DockItems [icon].GetIconSurface (cr.Target), (int) startx, 
				              Height - DockItems [icon].Height - (MinimumDockArea.Height - DockItems [icon].Height) / 2);
				cr.Paint ();
			}
			
			if (DockItems [icon].DrawIndicator) {
				// draws a simple triangle indicator.  Should be replaced by something nicer some day
				cr.MoveTo (center, Height - 6);
				cr.LineTo (center + 4, Height);
				cr.LineTo (center - 4, Height);
				cr.ClosePath ();
				
				cr.Color = new Cairo.Color (1, 1, 1, .7);
				cr.Fill ();
			}
			
			if (DockItemForX (Cursor.X) == icon && CursorIsOverDockArea && DockItems [icon].GetTextSurface (cr.Target) != null) {
				int textx = IconNormalCenterX (icon) - (DockPreferences.TextWidth / 2);
				int texty = Height - 2 * IconSize - 28;
				cr.SetSource (DockItems [icon].GetTextSurface (cr.Target), textx, texty);
				cr.Paint ();
			}
		}
		
		void DrawThumbnailIcon (Context cr)
		{
			Gdk.Point center = StickIconCenter;
			
			double opacity = 1.0/Math.Abs (center.X - Cursor.X) * 30 - .2;
			
			cr.Arc (center.X, center.Y, 3.5, 0, Math.PI*2);
			cr.LineWidth = 1;
			cr.Color = new Cairo.Color (1, 1, 1, opacity);
			cr.Stroke ();
			
			if (!DockPreferences.AutoHide) {
				cr.Arc (center.X, center.Y, 1.5, 0, Math.PI*2);
				cr.Color = new Cairo.Color (1, 1, 1, opacity);
				cr.Fill ();
			}
		}
		
		int IconNormalCenterX (int icon)
		{
			//the first icons center is at dock X + border + IconBorder + half its width
			if (!DockItems.Any ())
				return 0;
			int start_x = MinimumDockArea.X + HorizontalBuffer + IconBorderWidth;
			for (int i=0; i<icon; i++)
				start_x += DockItems [i].Width + 2 * IconBorderWidth;
			
			return start_x + DockItems [icon].Width / 2;
		}
		
		int DockItemForX (int x)
		{
			int start_x = MinimumDockArea.X + HorizontalBuffer;
			for (int i=0; i<DockItems.Length; i++) {
				if (x >= start_x && x <= start_x + DockItems [i].Width + 2 * IconBorderWidth)
					return i;
				start_x += DockItems [i].Width + 2 * IconBorderWidth;
			}
			return -1;
		}
		
		void IconPositionedCenterX (int icon, out int x, out double zoom)
		{
			int center = IconNormalCenterX (icon);
			int offset = Math.Min (Math.Abs (Cursor.X - center), ZoomPixels / 2);
			
			if (ZoomPixels / 2 == 0) {
				zoom = 1;
			} else {
				zoom = DockPreferences.ZoomPercent - (offset / (double)(ZoomPixels / 2)) * (DockPreferences.ZoomPercent - 1);
				zoom = (zoom - 1) * ZoomIn + 1;
			}
			
			offset = (int) ((offset*Math.Sin ((Math.PI/4)*zoom)) * (DockPreferences.ZoomPercent-1));
			
			if (Cursor.X > center) {
				center -= offset;
			} else {
				center += offset;
			}
			x = center;
		}
		
		Gdk.Rectangle GetDockArea ()
		{
			if (DockIconOpacity == 0 || ZoomIn == 0)
				return MinimumDockArea;

			int start_x, end_x;
			double start_zoom, end_zoom;
			IconPositionedCenterX (0, out start_x, out start_zoom);
			IconPositionedCenterX (DockItems.Length - 1, out end_x, out end_zoom);
			
			int x = start_x - (int)(start_zoom * (IconSize / 2)) - HorizontalBuffer;
			int end = end_x + (int)(end_zoom * (IconSize / 2)) + HorizontalBuffer;
			
			return new Gdk.Rectangle (x, Height - IconSize - 2 * VerticalBuffer, end - x, IconSize + 2 * VerticalBuffer);
		}
		
		void OnDockItemsChanged (IEnumerable<IDockItem> items)
		{
			SetIconRegions ();
			AnimatedDraw ();
		}
		
		void OnItemMenuHidden (object o, System.EventArgs args)
		{
			int x, y;
			Display.GetPointer (out x, out y);
			
			Gdk.Rectangle geo;
			window.GetPosition (out geo.X, out geo.Y);
			
			x -= geo.X;
			y -= geo.Y;
			
			Cursor = new Gdk.Point (x, y);
			AnimatedDraw ();
		}
		
		#region Drag Code
		
		protected override bool OnDragMotion (Gdk.DragContext context, int x, int y, uint time)
		{
			Cursor = new Gdk.Point (x, y);
			AnimatedDraw ();
			return base.OnDragMotion (context, x, y, time);
		}

		protected override void OnDragDataReceived (Gdk.DragContext context, int x, int y, Gtk.SelectionData selectionData, 
		                                            uint info, uint time)
		{
			string data = System.Text.Encoding.UTF8.GetString ( selectionData.Data );
			//sometimes we get a null at the end, and it crashes us
			data = data.TrimEnd ('\0'); 
			
			string [] uriList = Regex.Split (data, "\r\n");
			uriList.Where (uri => uri.StartsWith ("file://"))
				.ForEach (uri => item_provider.AddCustomItem (uri.Substring (7)));
			
			base.OnDragDataReceived (context, x, y, selectionData, info, time);
		}
		
		protected override void OnDragBegin (Gdk.DragContext context)
		{
			remove_drag_start_x = Cursor.X;
			base.OnDragBegin (context);
		}

		protected override bool OnDragDrop(DragContext context, int x, int y, uint time)
		{
			Cursor = new Gdk.Point (x, y);
			int item = DockItemForX (remove_drag_start_x);
			if (!CursorIsOverDockArea)
				item_provider.RemoveItem (item);
			return base.OnDragDrop (context, x, y, time);
		}

		#endregion
		
		protected override bool OnExposeEvent(EventExpose evnt)
		{
			bool ret_val = base.OnExposeEvent (evnt);
			// clear the dock area cache... this will cause it to recalculate.
			minimum_dock_area = new Gdk.Rectangle ();
			if (!IsDrawable)
				return ret_val;
			
			if (backbuffer == null) {
				Context tmp = Gdk.CairoHelper.Create (GdkWindow);
				backbuffer = tmp.Target.CreateSimilar (tmp.Target.Content, Width, Height);
				(tmp as IDisposable).Dispose ();
			}
			
			Context cr = new Cairo.Context (backbuffer);
			cr.AlphaFill ();
			cr.Operator = Operator.Over;
			
			DrawDrock (cr);
			(cr as IDisposable).Dispose ();
			
			Context cr2 = Gdk.CairoHelper.Create (GdkWindow);
			cr2.SetSource (backbuffer, 0, VerticalOffset);
			cr2.Operator = Operator.Source;
			cr2.Paint ();
			(cr2 as IDisposable).Dispose ();
			
			return ret_val;
		}
		
		protected override bool OnMotionNotifyEvent(EventMotion evnt)
		{
			bool tmp = CursorIsOverDockArea;
			
			Gdk.Point old_cursor_location = Cursor;
			Cursor = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			
			if (CursorNearDraggableEdge && !cursor_is_handle) {
				Gdk.Cursor top_cursor = new Gdk.Cursor (CursorType.TopSide);
				GdkWindow.Cursor = top_cursor;
				top_cursor.Dispose ();
				cursor_is_handle = true;
			} else if (!CursorNearDraggableEdge && cursor_is_handle && !drag_resizing) {
				Gdk.Cursor normal_cursor = new Gdk.Cursor (CursorType.LeftPtr);
				GdkWindow.Cursor = normal_cursor;
				normal_cursor.Dispose ();
				cursor_is_handle = false;
			}

			if (drag_resizing)
				DockPreferences.IconSize = drag_start_icon_size + (drag_start_y - Cursor.Y);
			
			bool cursorMoveWarrantsDraw = CursorIsOverDockArea && (old_cursor_location.X != Cursor.X);

			if (tmp != CursorIsOverDockArea || drag_resizing || cursorMoveWarrantsDraw) 
				AnimatedDraw ();
			
			return base.OnMotionNotifyEvent (evnt);
		}
		
		protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
		{
			if (CursorNearDraggableEdge)
				StartDrag ();
			
			return base.OnButtonPressEvent (evnt);
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			bool ret_val = base.OnButtonPressEvent (evnt);
			
			// lets not do anything in this case
			if (drag_resizing) {
				EndDrag ();
				return ret_val;
			}
			
			if (InputInterfaceVisible) {
				switch (SummonRenderer.GetClickEvent (GetDockArea ())) {
				case SummonClickEvent.AddItemToDock:
					item_provider.AddCustomItem (State [State.CurrentPane]);
					window.RequestClickOff ();
					break;
				case SummonClickEvent.None:
					// Do nothing
					break;
				}
				if (!CursorIsOverDockArea)
					window.RequestClickOff ();
			} else {
				// we are hovering over the pin icon
				Gdk.Rectangle stick_rect = new Gdk.Rectangle (StickIconCenter.X - 4, StickIconCenter.Y - 4, 8, 8);
				if (stick_rect.Contains (Cursor)) {
					DockPreferences.AutoHide = !DockPreferences.AutoHide;
					window.SetStruts ();
					AnimatedDraw ();
					return ret_val;
				}
				
				int item = DockItemForX ((int) evnt.X); //sometimes clicking is not good!
				if (item < 0 || item >= DockItems.Length || !CursorIsOverDockArea || InputInterfaceVisible)
					return ret_val;
				
				//handling right clicks
				if (evnt.Button == 3) {
					if (CurrentDockItem is IRightClickable && (CurrentDockItem as IRightClickable).GetMenuItems ().Any ())
						ItemMenu.Instance.PopupAtPosition ((CurrentDockItem as IRightClickable).GetMenuItems (), 
						                                   (int) evnt.XRoot, (int) evnt.YRoot);
					return ret_val;
				}
				
				//send off the clicks
				DockItems [item].Clicked (evnt.Button, window.Controller);
				if (DockItems [item].LastClick > last_click)
					last_click = DockItems [item].LastClick;
				AnimatedDraw ();
			}
			return ret_val;
		}
		
		protected override bool OnLeaveNotifyEvent (Gdk.EventCrossing evnt)
		{
			Cursor = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			ModifierType leave_mask = ModifierType.Button1Mask | ModifierType.Button2Mask | 
				ModifierType.Button3Mask | ModifierType.Button4Mask | ModifierType.Button5Mask;
			
			if (CursorIsOverDockArea && (int) (evnt.State & leave_mask) == 0 && evnt.Mode == CrossingMode.Normal)
				Cursor = new Gdk.Point ((int) evnt.X, -1);
			return base.OnLeaveNotifyEvent (evnt);
		}
		
		protected override void OnRealized ()
		{
			base.OnRealized ();
			if (IsRealized)
				GdkWindow.SetBackPixmap (null, false);
		}
		
		protected override void OnStyleSet (Gtk.Style previous_style)
		{
			if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			base.OnStyleSet (previous_style);
		}
		
		void StartDrag ()
		{
			drag_start_y = Cursor.Y;
			drag_start_icon_size = DockPreferences.IconSize;
			drag_resizing = true;
		}
		
		void EndDrag ()
		{
			drag_resizing = false;
		}
		
		void SetIconRegions ()
		{
			Gdk.Rectangle pos;
			window.GetPosition (out pos.X, out pos.Y);
			
			for (int i=0; i<DockItems.Length; i++) {
				int x = IconNormalCenterX (i);
				DockItems [i].SetIconRegion (new Gdk.Rectangle (pos.X + (x - IconSize / 2), 
				                                               pos.Y + (Height - VerticalBuffer - IconSize), IconSize, IconSize));
			}
		}
		
		void SetParentInputMask ()
		{
			if (InputInterfaceVisible) {
				window.SetInputMask (Height);
			} else if (CursorIsOverDockArea) {
				window.SetInputMask (GetDockArea ().Height*2 + 10);
			} else {
				if (DockPreferences.AutoHide)
					window.SetInputMask (1);
				else
					window.SetInputMask (GetDockArea ().Height);
			}
		}
		
		public void SetPaneContext (IUIContext context, Pane pane)
		{
			State.SetContext (context, pane);
			AnimatedDraw ();
		}
		
		public void ShowInputInterface ()
		{
			interface_change_time = DateTime.UtcNow;
			InputInterfaceVisible = true;
			
			SetParentInputMask ();
			AnimatedDraw ();
		}
		
		public void HideInputInterface ()
		{
			interface_change_time = DateTime.UtcNow;
			InputInterfaceVisible = false;
			
			SetParentInputMask ();
			AnimatedDraw ();
		}
		
		public void Reset ()
		{
			State.Clear ();
			AnimatedDraw ();
		}
		
		public void ClearPane (Pane pane)
		{
			State.ClearPane (pane);
		}
	}
}
