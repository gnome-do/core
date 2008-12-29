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
		public const int HorizontalBuffer = 7;
		const int BounceTime = 700;
		const int InsertAnimationTime = BaseAnimationTime*5;
		const int WindowHeight = 300;
		const int IconBorderWidth = 2;
		const string HighlightFormat = "<span foreground=\"#5599ff\">{0}</span>";
		
		#region private variables
		Gdk.Point cursor;
		
		Gdk.Rectangle minimum_dock_area;
		
		DateTime enter_time = DateTime.UtcNow;
		DateTime interface_change_time = DateTime.UtcNow;
		
		bool cursor_is_handle;
		bool drag_resizing;
		bool gtk_drag_source_set;
		bool gtk_drag_dest_set;
		
		int monitor_width;
		int drag_start_y;
		int drag_start_icon_size;
		int remove_drag_start_x;
		uint animation_timer;
		
		double previous_zoom;
		int previous_item_count;
		int previous_x = -1;
		bool previous_icon_animation_needed = true;
		
		
		DockWindow window;
		DockItemProvider item_provider;
		Surface backbuffer, input_area_buffer, dock_icon_buffer, urgent_buffer;
		DockItemMenu dock_item_menu;
		#endregion
		
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
		
		public int VerticalBuffer {
			get {
				return DockPreferences.Reflections ? 10 : 5;
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
		
		DockAnimationState AnimationState { get; set; }
		
		bool GtkDragging { get; set; }
		
		bool FullRenderFlag { get; set; }
		
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
		
		int SummonTime {
			get {
				return DockPreferences.SummonTime;
			}
		}
		
		#region Zoom Properties
		/// <value>
		/// Returns the zoom in percentage (0 through 1)
		/// </value>
		double ZoomIn {
			get {
				double zoom = Math.Min (1, (DateTime.UtcNow - enter_time).TotalMilliseconds / BaseAnimationTime);
				if (!CursorIsOverDockArea) {
					zoom = 1 - zoom;
				} else {
					if (DockPreferences.AutoHide)
						zoom = 1;
				}
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
				return (int) (offset * MinimumDockArea.Height * 1.5);
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
				bool cursorIsOverDockArea = CursorIsOverDockArea;
				cursor = value;
				
				// When we change over this boundry, it will normally trigger an animation, we need to be sure to catch it
				if (CursorIsOverDockArea != cursorIsOverDockArea) {
					enter_time = DateTime.UtcNow;
					AnimatedDraw ();
				}
				SetParentInputMask ();
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
		
		bool UrgentRecentChange {
			get {
				return DockItems.Where (di => di is IDockAppItem)
					.Cast<IDockAppItem> ()
					.Any (dai => (DateTime.UtcNow - dai.AttentionRequestStartTime).TotalMilliseconds < BounceTime);
			}
		}
		
		bool IconAnimationNeeded {
			get {
				return AnimationState.CheckCondition ("BounceAnimationNeeded") ||
					AnimationState.CheckCondition ("IconInsertAnimationNeeded") ||
					AnimationState.CheckCondition ("UrgentAnimationNeeded");
			}
		}
		
		bool CanFastRender {
			get {
				// Some conditions are not good for doing partial draws.
				// we have a couple conditions were this render peformance boost will result in "badness".
				// in these cases we need to do a full render.
				return ZoomIn == 1 && 
					previous_zoom == 1 && 
					previous_item_count == DockItems.Length && 
					!IconAnimationNeeded &&
					!previous_icon_animation_needed &&
					!drag_resizing &&
					!FullRenderFlag;
			}
		}
		#endregion
		
		public DockArea (DockWindow window) : base ()
		{
			this.window = window;
			
			item_provider = new DockItemProvider ();
			State = new DockState ();
			
			AnimationState = new DockAnimationState ();
			BuildAnimationStateEngine ();
			
			SummonRenderer = new SummonModeRenderer (this);
			dock_item_menu = new DockItemMenu ();
			
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
			           (int) EventMask.ButtonReleaseMask |
			           (int) EventMask.FocusChangeMask);
			
			DoubleBuffered = false;
			
			RegisterEvents ();
			RegisterGtkDragDest ();
			RegisterGtkDragSource ();
			
			GLib.Timeout.Add (20, () => {
				//must be done after construction is complete
				SetParentInputMask ();
				return false;
			});
		}
		
		void RegisterEvents ()
		{
			item_provider.DockItemsChanged += OnDockItemsChanged;
			
			item_provider.ItemNeedsUpdate += HandleItemNeedsUpdate;
			
			dock_item_menu.Hidden += OnDockItemMenuHidden;
			
			dock_item_menu.Shown += OnDockItemMenuShown;
			
			Wnck.Screen.Default.ViewportsChanged += OnWnckViewportsChanged;
			
			Realized += (o, a) => GdkWindow.SetBackPixmap (null, false);
			
			StyleSet += (o, a) => { 
				if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			};
		}
		
		void BuildAnimationStateEngine ()
		{
			AnimationState.AddCondition ("IconInsertAnimationNeeded", 
			                             () => DockItems.Any (di => (DateTime.UtcNow - di.DockAddItem).TotalMilliseconds < InsertAnimationTime));
			
			AnimationState.AddCondition ("PaneChangeAnimationNeeded",
			                             () => (DateTime.UtcNow - State.CurrentPaneTime).TotalMilliseconds < BaseAnimationTime);
		
			AnimationState.AddCondition ("ZoomAnimationNeeded",
			                             () => !(CursorIsOverDockArea && ZoomIn == 1) || (!CursorIsOverDockArea && ZoomIn == 0));
			
			AnimationState.AddCondition ("OpenAnimationNeeded",
			                             () => (DateTime.UtcNow - enter_time).TotalMilliseconds < SummonTime ||
			                             (DateTime.UtcNow - interface_change_time).TotalMilliseconds < SummonTime);
			
			AnimationState.AddCondition ("BounceAnimationNeeded",
			                             () => DockItems.Any (di => (DateTime.UtcNow - di.LastClick).TotalMilliseconds <= BounceTime));
			
			AnimationState.AddCondition ("UrgentAnimationNeeded",
			                             () => DockItems.Where (di => di is IDockAppItem)
			                             .Cast<IDockAppItem> ()
			                             .Where (dai => dai.NeedsAttention)
			                             .Any (dai => (DateTime.UtcNow - dai.AttentionRequestStartTime).TotalMilliseconds < BounceTime));
			
			AnimationState.AddCondition ("UrgentRecentChange",
			                             () => DockItems.Where (di => di is IDockAppItem)
			                             .Cast<IDockAppItem> ()
			                             .Any (dai => (DateTime.UtcNow - dai.AttentionRequestStartTime).TotalMilliseconds < BounceTime));
			
			AnimationState.AddCondition ("InputModeChangeAnimationNeeded",
			                             () => (DateTime.UtcNow - interface_change_time).TotalMilliseconds < SummonTime);
			
			AnimationState.AddCondition ("InputModeSlideAnimationNeeded",
			                             () => (DateTime.UtcNow - State.LastCursorChange).TotalMilliseconds < BaseAnimationTime);
			
			AnimationState.AddCondition ("ThirdPaneVisibilityAnimationNeeded",
			                             () => (DateTime.UtcNow - State.ThirdChangeTime).TotalMilliseconds < BaseAnimationTime);
		}

		void HandleItemNeedsUpdate (object sender, UpdateRequestArgs args)
		{
			FullRenderFlag = true;
			AnimatedDraw ();
		}
		
		void RegisterGtkDragSource ()
		{
			gtk_drag_source_set = true;
			TargetEntry te = new TargetEntry ("text/uri-list", TargetFlags.OtherApp, 0);
			Gtk.Drag.SourceSet (this, Gdk.ModifierType.Button1Mask, new [] {te}, DragAction.Copy);
		}
		
		void RegisterGtkDragDest ()
		{
			gtk_drag_dest_set = true;
			TargetEntry dest_te = new TargetEntry ("text/uri-list", 0, 0);
			Gtk.Drag.DestSet (this, DestDefaults.Motion | DestDefaults.Drop, new [] {dest_te}, Gdk.DragAction.Copy);
		}
		
		void UnregisterGtkDragSource ()
		{
			gtk_drag_source_set = false;
			Gtk.Drag.SourceUnset (this);
		}
		
		void AnimatedDraw ()
		{
			if (0 < animation_timer)
				return;
			
			// the presense of this queue draw has caused some confusion, so I will explain.
			// first its here to draw the "first frame".  Without it, we have a 16ms delay till that happens,
			// however minor that is.  We do everything after 16ms (about 60fps) so we will keep this up.
			QueueDraw ();
			animation_timer = GLib.Timeout.Add (16, OnDrawTimeoutElapsed);
		}
		
		bool OnDrawTimeoutElapsed ()
		{
			QueueDraw ();
			if (AnimationState.AnimationNeeded)
				return true;
			
			//reset the timer to 0 so that the next time AnimatedDraw is called we fall back into
			//the draw loop.
			animation_timer = 0;
			return false;
		}
		
		void DrawDrock (Context cr)
		{
			// We need to initilize this the first time we use it. However we cant initialize it until our
			// very first draw starts, and after that it must maintain state, so we signal this with -1;
			if (previous_x == -1)
				previous_x = Cursor.X;
			
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
			
			// To enable this render optimization, we have to keep track of several state items that otherwise
			// are unimportant.  This is an unfortunate reality we must live with.
			previous_zoom = ZoomIn;
			previous_item_count = DockItems.Length;
			previous_x = Cursor.X;
			previous_icon_animation_needed = IconAnimationNeeded;
		}
		
		void DrawIcons (Context cr)
		{
			if (CanFastRender) {
				do {
					// If the cursor has not moved and the dock_item_menu is not visible (this causes a render change without moving the cursor)
					// we can do no rendering at all and just take our previous frame as our current result.
					if (previous_x == Cursor.X && !dock_item_menu.Visible && !UrgentRecentChange)
						break;
					
					// we need to know the left and right items for the parabolic zoom.  These items represent the only icons that are
					// actually undergoing change.  By noting what these icons are, we can only draw these icons and those between them.
					int left_item = Math.Max (0, DockItemForX (Math.Min (Cursor.X, previous_x) - DockPreferences.ZoomSize / 2));
					int right_item = DockItemForX (Math.Max (Cursor.X, previous_x) + DockPreferences.ZoomSize / 2);
					if (right_item == -1) 
						right_item = DockItems.Length - 1;
					
					int left_x, right_x;
					double left_zoom, right_zoom;
					
					// calculates the actual x postions of the borders of the left and right most changing icons
					if (left_item == 0) {
						left_x = 0;
					} else {
						IconPositionedCenterX (left_item, out left_x, out left_zoom);
						left_x -= (int) (left_zoom * DockItems [left_item].Width / 2) + IconBorderWidth;
					}
					
					if (right_item == DockItems.Length - 1) {
						right_x = Width;
					} else {
						IconPositionedCenterX (right_item, out right_x, out right_zoom);
						right_x += (int) (right_zoom * DockItems [right_item].Width / 2) + IconBorderWidth;
					}
					
					// only clear that area for which we are going to redraw.  If we land this in the middle of an icon
					// things are going to look ugly, so this calculation MUST be correct.
					cr.Rectangle (left_x, 0, right_x - left_x, Height);
					cr.Color = new Cairo.Color (1, 1, 1, 0);
					cr.Operator = Operator.Source;
					cr.Fill ();
					cr.Operator = Operator.Over;
					
					for (int i=left_item; i<=right_item; i++)
						DrawIcon (cr, i);
				} while (false);
			} else {
				FullRenderFlag = false;
				// less code, twice as slow...
				cr.AlphaFill ();
				for (int i=0; i<DockItems.Length; i++)
					DrawIcon (cr, i);
			}
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
			double y = (Height - (zoom * DockItems [icon].Height)) - VerticalBuffer;
			
			int bounce_ms = (int) (DateTime.UtcNow - DockItems [icon].LastClick).TotalMilliseconds;
			
			// we will set this flag now
			bool draw_urgency = false;
			if (bounce_ms < BounceTime) {
				// bounces twice
				y -= Math.Abs (30 * Math.Sin (bounce_ms * Math.PI / (BounceTime / 2)));
			} else {
				IDockAppItem dai = DockItems [icon] as IDockAppItem;
				if (dai != null && dai.NeedsAttention) {
					draw_urgency = true;
					int urgent_ms = (int) (DateTime.UtcNow - dai.AttentionRequestStartTime).TotalMilliseconds;
					if (urgent_ms < BounceTime)
						y -= 100 * Math.Sin (urgent_ms * Math.PI / (BounceTime));
				}
			}
			
			double scale = zoom/DockPreferences.IconQuality;
			
			if (DockItems [icon].Scalable) {
				if (DockPreferences.Reflections) {
					cr.Scale (scale, 0-scale);
					
					// get us into a "normal" reflected position
					double reflect_y = y + 2 * (MinimumDockArea.Height + DockItems [icon].Height) * scale;
					
					// move us up a bit based on the vertial buffer and the zoom
					reflect_y -= VerticalBuffer * 2.7 * zoom;
					cr.SetSource (DockItems [icon].GetIconSurface (cr.Target),  x * (1 / scale), reflect_y * (-1 / scale));
					cr.PaintWithAlpha (.25);	
					cr.Scale (1 / scale, 1 / (0-scale));
				}
				
				cr.Scale (scale, scale);
				// we need to multiply x and y by 1 / scale to undo the scaling of the context.  We only want to zoom
				// the icon, not move it around.
				cr.SetSource (DockItems [icon].GetIconSurface (cr.Target), x / scale, y / scale);
				cr.Paint ();
				
				if (GtkDragging && DockItems [icon].IsAcceptingDrops && icon == DockItemForX (Cursor.X)) {
					cr.Rectangle (x / scale, y / scale, DockPreferences.FullIconSize, DockPreferences.FullIconSize);
					cr.Color = new Cairo.Color (.8, .85, 1, .5);
					cr.Operator = Operator.Atop;
					cr.Fill ();
					cr.Operator = Operator.Over;
				}
				
				if (draw_urgency) {
					cr.SetSource (GetUrgentSurface (cr), x / scale, y / scale);
					cr.PaintWithAlpha (.8);
				}
				cr.Scale (1 / scale, 1 / scale);
			} else {
				// since these dont scale, we have some extra work to do to keep them centered
				double startx = x + (zoom*DockItems [icon].Width - DockItems [icon].Width) / 2;
				cr.SetSource (DockItems [icon].GetIconSurface (cr.Target), (int) startx, 
				              Height - DockItems [icon].Height - (MinimumDockArea.Height - DockItems [icon].Height) / 2);
				cr.Paint ();
			}
			
			if (DockItems [icon].WindowCount > 0) {
				// draws a simple triangle indicator.  Should be replaced by something nicer some day
				int indicator_y = Height - 1;
				DrawGlowIndicator (cr, center, indicator_y);
			}
			
			// we do a null check here to allow things like separator items to supply a null.  This allows us to draw nothing
			// at all instead of rendering a blank surface (which is slow)
			if (!dock_item_menu.Visible &&
			    DockItemForX (Cursor.X) == icon && 
			    CursorIsOverDockArea && 
			    DockItems [icon].GetTextSurface (cr.Target) != null) {
				int textx = IconNormalCenterX (icon) - (DockPreferences.TextWidth / 2);
				int texty = Height - 2 * IconSize - 28;
				cr.SetSource (DockItems [icon].GetTextSurface (cr.Target), textx, texty);
				cr.Paint ();
			}
		}
		
		Surface GetUrgentSurface (Context cr)
		{
			if (urgent_buffer == null) {
				double scale = .75;
				int size = (int) (DockPreferences.FullIconSize * scale);
				urgent_buffer = cr.Target.CreateSimilar (cr.Target.Content, DockPreferences.FullIconSize, DockPreferences.FullIconSize);
				using (cr = new Context (urgent_buffer)) {
					cr.AlphaFill ();
					Pixbuf pbuf = IconProvider.PixbufFromIconName ("emblem-important", size);
					CairoHelper.SetSourcePixbuf (cr, pbuf, DockPreferences.FullIconSize * (1 - scale) / 2, 
					                             DockPreferences.FullIconSize * (1 - scale) / 2);
					cr.Paint ();
					pbuf.Dispose ();
				}
			}
			return urgent_buffer;
		}
		
		void DrawGlowIndicator (Context cr, int x, int y)
		{
			cr.MoveTo (x, y);
			cr.Arc (x, y, 4, 0, Math.PI * 2);
			
			RadialGradient rg = new RadialGradient (x, y, 0, x, y, 4);
			rg.AddColorStop (0, new Cairo.Color (1, 1, 1, 1));
			rg.AddColorStop (.45, new Cairo.Color (.5, .6, 1, 1));
			rg.AddColorStop (.7, new Cairo.Color (.5, .6, 1, .8));
			rg.AddColorStop (1, new Cairo.Color (.5, .6, 1, 0));
			
			cr.Pattern = rg;
			cr.Fill ();
			rg.Destroy ();
		}
		
		void DrawThumbnailIcon (Context cr)
		{
			Gdk.Point center = StickIconCenter;
			
			// calculates an opacity randing from 0 to 1 depending on how far from the cursor the icon is (only X is calculated)
			double opacity = 1.0 / Math.Abs (center.X - Cursor.X) * 30 - .2;
			
			// draw concentric circles from here on
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
			// the first icons center is at dock X + border + IconBorder + half its width
			// it is subtle, but it *is* a mistake to add the half width until the end.  adding
			// premature will add the wrong width.  It hurts the brain.
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
			// this method is more than somewhat slow on the complexity scale, we want to avoid doing it
			// more than we have to.  Further, when we do call it, we should always check for this shortcut.
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
			FullRenderFlag = true;
			SetIconRegions ();
			AnimatedDraw ();
		}
		
		void OnDockItemMenuHidden (object o, System.EventArgs args)
		{
			// While an popup menus are being showing, the dock does not recieve mouse updates.  This is
			// both a good thing and a bad thing.  We must at the very least update the cursor position once the
			// popup is no longer in view.
			ManualCursorUpdate ();
			AnimatedDraw ();
		}
		
		void OnWnckViewportsChanged (object o, EventArgs e)
		{
			ManualCursorUpdate ();
			AnimatedDraw ();
		}
		
		/// <summary>
		/// Only purpose is to trigger one last redraw to eliminate the hover text
		/// </summary>
		void OnDockItemMenuShown (object o, EventArgs args)
		{
			AnimatedDraw ();
		}
		
		public void ManualCursorUpdate ()
		{
			int x, y;
			Display.GetPointer (out x, out y);
			
			Gdk.Rectangle geo;
			window.GetPosition (out geo.X, out geo.Y);
			
			x -= geo.X;
			y -= geo.Y;
			
			Cursor = new Gdk.Point (x, y);
		}
		
		#region Drag Code

		
		protected override bool OnDragMotion (Gdk.DragContext context, int x, int y, uint time)
		{
			GtkDragging = true;
			
			Cursor = new Gdk.Point (x, y);
			AnimatedDraw ();
			return base.OnDragMotion (context, x, y, time);
		}

		protected override void OnDragDataReceived (Gdk.DragContext context, int x, int y, Gtk.SelectionData selectionData, 
		                                            uint info, uint time)
		{
			if (!CursorIsOverDockArea) return;
			
			string data = System.Text.Encoding.UTF8.GetString ( selectionData.Data );
			data = System.Uri.UnescapeDataString (data);
			//sometimes we get a null at the end, and it crashes us
			data = data.TrimEnd ('\0'); 
			
			string [] uriList = Regex.Split (data, "\r\n");
			if (CurrentDockItem != null && CurrentDockItem.IsAcceptingDrops) {
				uriList.Where (uri => uri.StartsWith ("file://"))
					.ForEach (uri => CurrentDockItem.ReceiveItem (uri.Substring (7)));
			} else {
				uriList.Where (uri => uri.StartsWith ("file://"))
					.ForEach (uri => item_provider.AddCustomItem (uri.Substring (7)));
			}
			
			base.OnDragDataReceived (context, x, y, selectionData, info, time);
		}
		
		protected override void OnDragBegin (Gdk.DragContext context)
		{
			// the user might not end the drag on the same horizontal position they start it on
			remove_drag_start_x = Cursor.X;
			int item = DockItemForX (Cursor.X);
			if (DockItems [item].GetDragPixbuf () != null)
				Gtk.Drag.SetIconPixbuf (context, DockItems [item].GetDragPixbuf (), 0, 0);
			base.OnDragBegin (context);
		}
		
		protected override void OnDragEnd (Gdk.DragContext context)
		{
			if (context.DestWindow != window.GdkWindow || !CursorIsOverDockArea) {
				item_provider.RemoveItem (DockItemForX (remove_drag_start_x));
			} else if (CursorIsOverDockArea) {
				item_provider.MoveItemToPosition (DockItemForX (remove_drag_start_x), DockItemForX (Cursor.X));
			}
			base.OnDragEnd (context);
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
				Context cursorIsOverDockArea = Gdk.CairoHelper.Create (GdkWindow);
				backbuffer = cursorIsOverDockArea.Target.CreateSimilar (cursorIsOverDockArea.Target.Content, Width, Height);
				(cursorIsOverDockArea as IDisposable).Dispose ();
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
			GtkDragging = false;
			
			bool cursorIsOverDockArea = CursorIsOverDockArea;
			bool cursorNearDraggableEdge = CursorNearDraggableEdge;
			
			Gdk.Point old_cursor_location = Cursor;
			Cursor = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			
			// we do this so that our custom drag isn't destroyed by gtk's drag
			if (cursorNearDraggableEdge && gtk_drag_source_set) {
				UnregisterGtkDragSource ();
			} else if (!cursorNearDraggableEdge && !gtk_drag_source_set && !drag_resizing) {
				RegisterGtkDragSource ();
			}
			
			if (cursorNearDraggableEdge && !cursor_is_handle) {
				Gdk.Cursor top_cursor = new Gdk.Cursor (CursorType.TopSide);
				GdkWindow.Cursor = top_cursor;
				top_cursor.Dispose ();
				cursor_is_handle = true;
			} else if (!cursorNearDraggableEdge && cursor_is_handle && !drag_resizing) {
				Gdk.Cursor normal_cursor = new Gdk.Cursor (CursorType.LeftPtr);
				GdkWindow.Cursor = normal_cursor;
				normal_cursor.Dispose ();
				cursor_is_handle = false;
			}

			if (drag_resizing)
				DockPreferences.IconSize = drag_start_icon_size + (drag_start_y - Cursor.Y);
			
			bool cursorMoveWarrantsDraw = CursorIsOverDockArea && (old_cursor_location.X != Cursor.X);

			if (cursorIsOverDockArea != CursorIsOverDockArea || drag_resizing || cursorMoveWarrantsDraw) 
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
				
				//handling right clicks for those icons which request simple right click handling
				if (evnt.Button == 3) {
					if (CurrentDockItem is IRightClickable && (CurrentDockItem as IRightClickable).GetMenuItems ().Any ()) {
						
						int item_x;
						double item_zoom;
						IconPositionedCenterX (DockItemForX (Cursor.X), out item_x, out item_zoom);
						int menu_y = Screen.GetMonitorGeometry (0).Height - (int) (DockPreferences.IconSize * item_zoom);
						dock_item_menu.PopUp ((CurrentDockItem as IRightClickable).GetMenuItems (), 
						                      ((int) evnt.XRoot - Cursor.X) + item_x, menu_y);
						return ret_val;
					}
				}
				
				//send off the clicks
				DockItems [item].Clicked (evnt.Button);
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
			Gdk.Rectangle pos, geo;
			window.GetPosition (out pos.X, out pos.Y);
			
			geo = Screen.GetMonitorGeometry (0);
			// we use geo here instead of our position for the Y value because we know the parent window
			// may offset us when hidden. This is not desired...
			for (int i=0; i<DockItems.Length; i++) {
				int x = IconNormalCenterX (i);
				DockItems [i].SetIconRegion (new Gdk.Rectangle (pos.X + (x - IconSize / 2), 
				                                               geo.Y + geo.Height - VerticalBuffer - IconSize, IconSize, IconSize));
			}
		}
		
		void SetParentInputMask ()
		{
			if (window == null)
				return;
			
			int offset;
			if (InputInterfaceVisible) {
				offset = Height;
			} else if (CursorIsOverDockArea) {
				offset = GetDockArea ().Height * 2 + 10;
			} else {
				if (DockPreferences.AutoHide)
					offset = 1;
				else
					offset = GetDockArea ().Height;
			}
			if (window.CurrentOffsetMask != offset)
				window.SetInputMask (offset);
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
			
			GLib.Timeout.Add (500, () => { item_provider.ForceUpdate (); return false; });
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
