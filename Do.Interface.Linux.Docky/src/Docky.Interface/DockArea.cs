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
using Docky.Interface.Menus;
using Docky.Interface.Renderers;

namespace Docky.Interface
{
	
	
	public class DockArea : Gtk.DrawingArea
	{
		enum DragEdge {
			None = 0,
			Top,
			Left,
			Right,
		}

		public readonly TimeSpan BaseAnimationTime = new TimeSpan (0, 0, 0, 0, 150);
		
		const uint OffDockWakeupTime = 250;
		const uint OnDockWakeupTime = 20;

		TimeSpan BounceTime = new TimeSpan (0, 0, 0, 0, 700);
		TimeSpan InsertAnimationTime = new TimeSpan (0, 0, 0, 0, 150*5);
		
		#region private variables
		Gdk.Point cursor, drag_start_point, remove_drag_start_point;
		
		Gdk.CursorType cursor_type = CursorType.LeftPtr;
		
		DateTime enter_time = DateTime.UtcNow - new TimeSpan (0, 10, 0);
		DateTime interface_change_time = DateTime.UtcNow - new TimeSpan (0, 10, 0);
		
		bool drag_resizing;
		bool gtk_drag_source_set;
		bool disposed;
		
		int drag_start_icon_size;
		
		uint animation_timer;
		uint cursor_timer;
		
		DragEdge drag_edge;
		
		DockWindow window;
		DockItemProvider item_provider;
		Surface backbuffer, input_area_buffer, dock_icon_buffer;
		
		Matrix default_matrix;
		#endregion
		
		#region public properties
		public bool InputInterfaceVisible { get; set; }
		
		/// <value>
		/// The width of the docks window, but not the visible dock
		/// </value>
		public int Width { get; private set; }
		
		/// <value>
		/// The height of the docks window
		/// </value>
		public int Height { get; private set; }
		
		/// <value>
		/// The width of the visible dock
		/// </value>
		public int DockWidth {
			get { return PositionProvider.DockWidth; }
		}
		
		/// <summary>
		/// The height of the visible dock
		/// </summary>
		public int DockHeight {
			get { return PositionProvider.DockHeight; }
		}

		public uint[] StrutRequest {
			get {
				uint[] values = new uint[12];
				Gdk.Rectangle geo = LayoutUtils.MonitorGemonetry ();
				
				if (DockPreferences.AutoHide || DockPreferences.AllowOverlap)
					return values;
				
				switch (DockPreferences.Orientation) {
				case DockOrientation.Bottom:
					values [(int) XLib.Struts.Bottom] = (uint) DockHeight;
					values [(int) XLib.Struts.BottomStart] = (uint) geo.X;
					values [(int) XLib.Struts.BottomEnd] = (uint) (geo.X + geo.Width - 1);
					break;
				case DockOrientation.Left:
					values [(int) XLib.Struts.Left] = (uint) DockHeight;
					values [(int) XLib.Struts.LeftStart] = (uint) geo.Y;
					values [(int) XLib.Struts.LeftEnd] = (uint) (geo.Y + geo.Height - 1);
					break;
				case DockOrientation.Right:
					values [(int) XLib.Struts.Right] = (uint) DockHeight;
					values [(int) XLib.Struts.RightStart] = (uint) geo.Y;
					values [(int) XLib.Struts.RightEnd] = (uint) (geo.Y + geo.Height - 1);
					break;
				case DockOrientation.Top:
					values [(int) XLib.Struts.Top] = (uint) DockHeight;
					values [(int) XLib.Struts.TopStart] = (uint) geo.X;
					values [(int) XLib.Struts.TopEnd] = (uint) (geo.X + geo.Width - 1);
					break;
				}
				return values;
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
		
		ItemPositionProvider PositionProvider { get; set; }
		
		new DockItemMenu PopupMenu { get; set; }
		
		bool GtkDragging { get; set; }
		
		SummonModeRenderer SummonRenderer { get; set; }
		
		List<BaseDockItem> DockItems { 
			get { return item_provider.DockItems; } 
		}
		
		BaseDockItem CurrentDockItem {
			get {
				try { return DockItems [PositionProvider.IndexAtPosition (Cursor)]; }
				catch { return null; }
			}
		}
		
		TimeSpan SummonTime {
			get {
				return DockPreferences.SummonTime;
			}
		}
		
		/// <value>
		/// Returns the zoom in percentage (0 through 1)
		/// </value>
		double ZoomIn {
			get {
				if (drag_resizing && drag_start_point != Cursor)
					return 0;
				
				double zoom = Math.Min (1, (DateTime.UtcNow - enter_time).TotalMilliseconds / BaseAnimationTime.TotalMilliseconds);
				if (CursorIsOverDockArea) {
					if (DockPreferences.AutoHide)
						zoom = 1;
				} else {
					zoom = 1 - zoom;
				}
				
				if (InputInterfaceVisible)
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
				if (!DockPreferences.AutoHide || drag_resizing || InputAreaOpacity == 1)
					return 0;

				if (InputAreaOpacity > 0) {
					if (CursorIsOverDockArea) {
						return 0;
					} else {
						offset = Math.Min (1, (DateTime.UtcNow - enter_time).TotalMilliseconds / SummonTime.TotalMilliseconds);
						offset = Math.Min (offset, Math.Min (1, (DateTime.UtcNow - interface_change_time).TotalMilliseconds / SummonTime.TotalMilliseconds));
					}
					
					if (InputInterfaceVisible)
						offset = 1 - offset;
				} else {
					offset = Math.Min (1, (DateTime.UtcNow - enter_time).TotalMilliseconds / SummonTime.TotalMilliseconds);
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
					if (InputInterfaceVisible)
						return 0;
					return 1;
				}

				double total_time = (DateTime.UtcNow - interface_change_time).TotalMilliseconds;
				if (InputInterfaceVisible) {
					return 1 - (total_time / SummonTime.TotalMilliseconds);
				} else {
					return total_time / SummonTime.TotalMilliseconds;
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
				
				// We set this value here instead of dynamically checking due to performance constraints.
				// Ideally our CursorIsOverDockArea getter would do this fairly simple calculation, but it gets
				// called about 20 to 30 times per render loop, so the savings do add up.
				if (cursorIsOverDockArea) {
					Gdk.Rectangle rect = MinimumDockArea;
					if (DockPreferences.DockIsHorizontal)
						rect.Inflate (0, (int) (IconSize * (DockPreferences.ZoomPercent - 1)) + 22);
					else
						rect.Inflate ((int) (IconSize * (DockPreferences.ZoomPercent - 1)) + 22, 0);
					CursorIsOverDockArea = rect.Contains (cursor);
				} else {
					Gdk.Rectangle small = MinimumDockArea;
					if (DockPreferences.AutoHide) {
						switch (DockPreferences.Orientation) {
						case DockOrientation.Bottom:
							small.Y += small.Height - 1;
							small.Height = 1;
							break;
						case DockOrientation.Left:
							small.Width = 1;
							break;
						case DockOrientation.Right:
							small.X += small.Width - 1;
							small.Width = 1;
							break;
						case DockOrientation.Top:
							small.Height = 1;
							break;
						}
					}
					CursorIsOverDockArea = small.Contains (cursor);
				}
				
				// When we change over this boundry, it will normally trigger an animation, we need to be sure to catch it
				if (CursorIsOverDockArea != cursorIsOverDockArea) {
					ResetCursorTimer ();
					enter_time = DateTime.UtcNow;
					AnimatedDraw ();
				}
			}
		}
		
		Gdk.Rectangle MinimumDockArea {
			get {
				return PositionProvider.MinimumDockArea;
			}
		}
		
		bool CursorNearTopDraggableEdge {
			get {
				return MinimumDockArea.Contains (Cursor) && CurrentDockItem is SeparatorItem;
			}
		}
		
		bool CursorNearLeftEdge {
			get {
				if (DockPreferences.DockIsHorizontal)
					return CursorIsOverDockArea && Math.Abs (Cursor.X - MinimumDockArea.X) < 8;
				return CursorIsOverDockArea && Math.Abs (Cursor.Y - MinimumDockArea.Y) < 8;
			}
		}
		
		bool CursorNearRightEdge {
			get {
				if (DockPreferences.DockIsHorizontal)
					return CursorIsOverDockArea && Math.Abs (Cursor.X - (MinimumDockArea.X + MinimumDockArea.Width)) < 8;
				return CursorIsOverDockArea && Math.Abs (Cursor.Y - (MinimumDockArea.Y + MinimumDockArea.Height)) < 8;
			}
		}
		
		bool CursorNearDraggableEdge {
			get {
				return CursorNearTopDraggableEdge || 
					   CursorNearRightEdge || 
					   CursorNearLeftEdge;
			}
		}
		
		DragEdge CurrentDragEdge {
			get {
				if (CursorNearTopDraggableEdge)
					return DragEdge.Top;
				else if (CursorNearLeftEdge)
					return DragEdge.Left;
				else if (CursorNearRightEdge)
					return DragEdge.Right;
				return DragEdge.None;
			}
		}
		
		#region Animation properties
		bool CursorIsOverDockArea {	get; set; }
		
		bool IconAnimationNeeded {
			get {
				return AnimationState ["BounceAnimationNeeded"] ||
					   AnimationState ["IconInsertAnimationNeeded"] ||
					   AnimationState ["UrgentAnimationNeeded"];
			}
		}
		#endregion
		
		public DockArea (DockWindow window) : base ()
		{
			default_matrix = new Matrix ();
			this.window = window;
			
			SetSize ();
			SetSizeRequest (Width, Height);
			
			item_provider = new DockItemProvider ();
			State = new DockState ();
			PositionProvider = new ItemPositionProvider (item_provider, new Gdk.Rectangle (0, 0, Width, Height));
			
			AnimationState = new DockAnimationState ();
			BuildAnimationStateEngine ();
			
			SummonRenderer = new SummonModeRenderer (this);
			PopupMenu = new DockItemMenu ();
			
			Cursor = new Gdk.Point (-1, -1);
			
			this.SetCompositeColormap ();
			
			AddEvents ((int) EventMask.PointerMotionMask | 
			           (int) EventMask.EnterNotifyMask |
			           (int) EventMask.ButtonPressMask | 
			           (int) EventMask.ButtonReleaseMask |
			           (int) EventMask.FocusChangeMask);
			
			DoubleBuffered = false;
			
			RegisterEvents ();
			RegisterGtkDragDest ();
			RegisterGtkDragSource ();
			
			ResetCursorTimer ();
		}

		void SetSize ()
		{
			Gdk.Rectangle geo;
			geo = LayoutUtils.MonitorGemonetry ();
			
			if (DockPreferences.DockIsHorizontal) {
				Width = geo.Width;
				Height = 300;
			} else {
				Width = 500;
				Height = geo.Height;
			}
		}

		void RegisterEvents ()
		{
			item_provider.DockItemsChanged += OnDockItemsChanged;
			item_provider.ItemNeedsUpdate += HandleItemNeedsUpdate;
			
			PopupMenu.Hidden += OnDockItemMenuHidden;
			PopupMenu.Shown += OnDockItemMenuShown;

			Services.Core.UniverseInitialized += HandleUniverseInitialized;
			
			Wnck.Screen.Default.ViewportsChanged += OnWnckViewportsChanged;

			Realized += (o, e) => SetParentInputMask ();
			Realized += (o, a) => GdkWindow.SetBackPixmap (null, false);
			
			StyleSet += (o, a) => { 
				if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			};
		}

		void UnregisterEvents ()
		{
			item_provider.DockItemsChanged -= OnDockItemsChanged;
			item_provider.ItemNeedsUpdate -= HandleItemNeedsUpdate;
			
			PopupMenu.Hidden -= OnDockItemMenuHidden;
			PopupMenu.Shown -= OnDockItemMenuShown;

			Services.Core.UniverseInitialized -= HandleUniverseInitialized;
			
			Wnck.Screen.Default.ViewportsChanged -= OnWnckViewportsChanged;
		}
		
		void BuildAnimationStateEngine ()
		{
			AnimationState.AddCondition ("IconInsertAnimationNeeded", 
			                             () => DockItems.Any (di => di.TimeSinceAdd < InsertAnimationTime));
			
			AnimationState.AddCondition ("PaneChangeAnimationNeeded",
			                             () => (DateTime.UtcNow - State.CurrentPaneTime) < BaseAnimationTime);
		
			AnimationState.AddCondition ("ZoomAnimationNeeded",
			                             () => (CursorIsOverDockArea && ZoomIn != 1) || (!CursorIsOverDockArea && ZoomIn != 0));
			
			AnimationState.AddCondition ("OpenAnimationNeeded",
			                             () => DateTime.UtcNow - enter_time < SummonTime ||
			                             DateTime.UtcNow - interface_change_time < SummonTime);
			
			AnimationState.AddCondition ("BounceAnimationNeeded",
			                             () => DockItems.Any (di => di.TimeSinceClick <= BounceTime));
			
			AnimationState.AddCondition ("UrgentAnimationNeeded",
			                             () => DockItems.Where (di => di is IDockAppItem)
			                             .Cast<IDockAppItem> ()
			                             .Where (dai => dai.NeedsAttention)
			                             .Any (dai => DateTime.UtcNow - dai.AttentionRequestStartTime < BounceTime));
			
			AnimationState.AddCondition ("UrgentRecentChange",
			                             () => DockItems.Where (di => di is IDockAppItem)
			                             .Cast<IDockAppItem> ()
			                             .Any (dai => DateTime.UtcNow - dai.AttentionRequestStartTime < BounceTime));
			
			AnimationState.AddCondition ("InputModeChangeAnimationNeeded",
			                             () => DateTime.UtcNow - interface_change_time < SummonTime);
			
			AnimationState.AddCondition ("InputModeSlideAnimationNeeded",
			                             () => DateTime.UtcNow - State.LastCursorChange < BaseAnimationTime);
			
			AnimationState.AddCondition ("ThirdPaneVisibilityAnimationNeeded",
			                             () => DateTime.UtcNow - State.ThirdChangeTime < BaseAnimationTime);
		}

		void HandleItemNeedsUpdate (object sender, UpdateRequestArgs args)
		{
			if (args.Type == UpdateRequestType.NeedsAttentionSet) {
				SetParentInputMask ();
			}
			AnimatedDraw ();
		}

		void HandleUniverseInitialized(object sender, EventArgs e)
		{
			GLib.Timeout.Add (2000, delegate {
				SetIconRegions ();
				return false;
			});
		}
		
		void RegisterGtkDragSource ()
		{
			gtk_drag_source_set = true;
			TargetEntry te = new TargetEntry ("text/uri-list", TargetFlags.OtherApp, 0);
			Gtk.Drag.SourceSet (this, Gdk.ModifierType.Button1Mask, new [] {te}, DragAction.Copy);
		}
		
		void RegisterGtkDragDest ()
		{
			TargetEntry dest_te = new TargetEntry ("text/uri-list", 0, 0);
			Gtk.Drag.DestSet (this, DestDefaults.Motion | DestDefaults.Drop, new [] {dest_te}, Gdk.DragAction.Copy);
		}
		
		void UnregisterGtkDragSource ()
		{
			gtk_drag_source_set = false;
			Gtk.Drag.SourceUnset (this);
		}
		
		void ResetCursorTimer ()
		{
			if (disposed)
				return;
			if (cursor_timer > 0)
				GLib.Source.Remove (cursor_timer);
			
			uint time = (CursorIsOverDockArea || drag_resizing) ? OnDockWakeupTime : OffDockWakeupTime;
			cursor_timer = GLib.Timeout.Add (time, OnCursorTimerEllapsed);
		}
		
		bool OnCursorTimerEllapsed ()
		{
			ManualCursorUpdate ();
			return true;
		}
		
		void AnimatedDraw ()
		{
			if (0 < animation_timer)
				return;
			
			// the presense of this queue draw has caused some confusion, so I will explain.
			// first its here to draw the "first frame".  Without it, we have a 16ms delay till that happens,
			// however minor that is.  We do everything after 16ms (about 60fps) so we will keep this up.
			QueueDraw ();
			if (AnimationState.AnimationNeeded)
				animation_timer = GLib.Timeout.Add (1000/50, OnDrawTimeoutElapsed);
		}
		
		bool OnDrawTimeoutElapsed ()
		{
			QueueDraw ();
			// this is a "protected method".  We need to be sure that our input mask is okay on every frame.
			// 99% of the time this means nothing at all will be done
			SetParentInputMask ();
			
			if (AnimationState.AnimationNeeded)
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
				if (input_area_buffer == null) {
					input_area_buffer = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				}
				
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
			for (int i = 0; i < DockItems.Count; i++)
				DrawIcon (cr, i);
		}
		
		void DrawIcon (Context cr, int icon)
		{
			// Don't draw the icon we are dragging around
			if (GtkDragging) {
				int item = PositionProvider.IndexAtPosition (remove_drag_start_point);
				if (item == icon && item_provider.ItemCanBeMoved (item))
					return;
			}
			
			Gdk.Point center;
			double zoom;
			IconZoomedPosition (icon, out center, out zoom);
			
			// This gives the actual x,y coordinates of the icon
			Gdk.Point iconPosition = new Gdk.Point ((int) (center.X - zoom * DockItems [icon].Width / 2),
			                                        (int) (center.Y - zoom * DockItems [icon].Width / 2));
			
			ClickAnimationType animationType = IconAnimation (icon);
			
			// we will set this flag now
			bool drawUrgency = false;
			if (animationType == ClickAnimationType.Bounce) {
				// bounces twice
				int delta = (int) Math.Abs (30 * Math.Sin (DockItems [icon].TimeSinceClick.TotalMilliseconds * Math.PI / (BounceTime.TotalMilliseconds / 2)));
				iconPosition = iconPosition.RelativeMovePoint (delta, RelativeMove.Inward);
			} else {
				IDockAppItem dai = DockItems [icon] as IDockAppItem;
				if (dai != null && dai.NeedsAttention) {
					drawUrgency = true;
					if (DateTime.UtcNow - dai.AttentionRequestStartTime < BounceTime) {
						double urgentMs = (DateTime.UtcNow - dai.AttentionRequestStartTime).TotalMilliseconds;
						int delta = (int) (100 * Math.Sin (urgentMs * Math.PI / (BounceTime.TotalMilliseconds)));
						iconPosition = iconPosition.RelativeMovePoint (delta, RelativeMove.Inward);
					}
				}
			}
			
			double scale = zoom/DockPreferences.IconQuality;
			
			if (DockItems [icon].Scalable) {
				if (scale != 1)
					cr.Scale (scale, scale);
				// we need to multiply x and y by 1 / scale to undo the scaling of the context.  We only want to zoom
				// the icon, not move it around.
				
				double fadeInOpacity = Math.Min (DockItems [icon].TimeSinceAdd.TotalMilliseconds / InsertAnimationTime.TotalMilliseconds, 1);
				cr.SetSource (DockItems [icon].GetIconSurface (cr.Target), iconPosition.X / scale, iconPosition.Y / scale);
				cr.PaintWithAlpha (fadeInOpacity);
				
				bool shade_light = GtkDragging && DockItems [icon].IsAcceptingDrops && icon == PositionProvider.IndexAtPosition (Cursor);
				bool shade_dark = animationType == ClickAnimationType.Darken;
				if (shade_dark || shade_light) {
					cr.Rectangle (iconPosition.X / scale, iconPosition.Y / scale, DockPreferences.FullIconSize, DockPreferences.FullIconSize);
					
					if (shade_light) {
						cr.Color = new Cairo.Color (.9, .95, 1, .5);
					} else {
						double opacity = (BounceTime - DockItems [icon].TimeSinceClick).TotalMilliseconds / BounceTime.TotalMilliseconds - .7;
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
					cr.SetSource (DockItems [icon].GetIconSurface (cr.Target), iconPosition.X, iconPosition.Y - IconSize / 2 + 10);
				} else {
					cr.SetSource (DockItems [icon].GetIconSurface (cr.Target), iconPosition.X - IconSize / 2 + 5, iconPosition.Y);
				}
				cr.Paint ();
			}
			
			if (0 < DockItems [icon].WindowCount) {
				Gdk.Point location;
				switch (DockPreferences.Orientation) {
				case DockOrientation.Bottom:
					location = new Gdk.Point (center.X, Height - 1);	
					break;
				case DockOrientation.Left:
					location = new Gdk.Point (1, center.Y);
					break;
				case DockOrientation.Right:
					location = new Gdk.Point (Width - 1, center.Y);
					break;
				case DockOrientation.Top:
				default:
					location = new Gdk.Point (center.X, 1);
					break;
				}
				Util.DrawGlowIndicator (cr, location, drawUrgency, DockItems [icon].WindowCount);
			}
			
			// we do a null check here to allow things like separator items to supply
			// a null.  This allows us to draw nothing at all instead of rendering a
			// blank surface (which is slow)
			if (!PopupMenu.Visible && PositionProvider.IndexAtPosition (Cursor) == icon && 
			    CursorIsOverDockArea && DockItems [icon].GetTextSurface (cr.Target) != null) {

				Gdk.Point textPoint;
				if (DockPreferences.DockIsHorizontal) {
					textPoint.X = PositionProvider.IconUnzoomedPosition (icon).X - (DockPreferences.TextWidth / 2);
					textPoint.Y = Height - (int) (DockPreferences.ZoomPercent * IconSize) - 32;
					if (DockPreferences.Orientation == DockOrientation.Top)
						textPoint.Y = (int) (DockPreferences.ZoomPercent * IconSize) + 22;
				} else {
					textPoint = center.RelativeMovePoint ((int) ((IconSize / 2) * DockPreferences.ZoomPercent + 10), RelativeMove.Inward);
					if (DockPreferences.Orientation == DockOrientation.Right)
						textPoint = textPoint.RelativeMovePoint (DockPreferences.TextWidth, RelativeMove.Inward);
					textPoint = textPoint.RelativeMovePoint (10, RelativeMove.RealUp);
				}
				DockItems [icon].GetTextSurface (cr.Target).Show (cr, textPoint.X, textPoint.Y);
			}
		}
		
		ClickAnimationType IconAnimation (int icon)
		{
			return (DockItems [icon].TimeSinceClick < BounceTime) ? DockItems [icon].AnimationType : ClickAnimationType.None;
		}
		
		void IconZoomedPosition (int icon, out Gdk.Point center, out double zoom)
		{
			PositionProvider.IconZoomedPosition (icon, ZoomIn, Cursor, out center, out zoom);
		}
		
		Gdk.Rectangle GetDockArea ()
		{
			// this method is more than somewhat slow on the complexity scale, we want to avoid doing it
			// more than we have to.  Further, when we do call it, we should always check for this shortcut.
			if (DockIconOpacity == 0 || ZoomIn == 0)
				return MinimumDockArea;

			return PositionProvider.DockArea (ZoomIn, Cursor);
		}
		
		void OnDockItemsChanged (IEnumerable<BaseDockItem> items)
		{
			DockPreferences.MaxIconSize = (int) (((double) Width / MinimumDockArea.Width) * IconSize);
			
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
			if ((Cursor.X == x && Cursor.Y == y) || PopupMenu.Visible)
				return;
			
			Gdk.Rectangle geo, hide_offset;
			window.GetPosition (out geo.X, out geo.Y);
			window.WindowHideOffset (out hide_offset.X, out hide_offset.Y);

			x -= geo.X - hide_offset.X;
			y -= geo.Y - hide_offset.Y;
			Gdk.Point old_cursor_location = Cursor;
			Cursor = new Gdk.Point (x, y);

			ConfigureCursor ();

			if (drag_resizing)
				HandleDragMotion ();
			
			bool cursorMoveWarrantsDraw = CursorIsOverDockArea && 
				((DockPreferences.DockIsHorizontal && old_cursor_location.X != Cursor.X) ||
					(!DockPreferences.DockIsHorizontal && old_cursor_location.Y != Cursor.Y));

			if (drag_resizing || cursorMoveWarrantsDraw) 
				AnimatedDraw ();
		}
		
		#region Drag Code
		
		protected override bool OnDragMotion (Gdk.DragContext context, int x, int y, uint time)
		{
			GtkDragging = true;
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
					.ForEach (uri => CurrentDockItem.ReceiveItem (uri.Substring ("file://".Length)));
			} else {
				uriList.Where (uri => uri.StartsWith ("file://"))
					.ForEach (uri => item_provider.AddCustomItem (uri.Substring ("file://".Length)));
			}
			
			base.OnDragDataReceived (context, x, y, selectionData, info, time);
		}
		
		protected override void OnDragBegin (Gdk.DragContext context)
		{
			// the user might not end the drag on the same horizontal position they start it on
			remove_drag_start_point = Cursor;
			int item = PositionProvider.IndexAtPosition (Cursor);
			
			Gdk.Pixbuf pbuf;
			if (item == -1 || !item_provider.ItemCanBeMoved (item)) {
				pbuf = IconProvider.PixbufFromIconName ("gtk-remove", DockPreferences.IconSize);
			} else {
				pbuf = DockItems [item].GetDragPixbuf ();
			}
				
			if (pbuf != null)
				Gtk.Drag.SetIconPixbuf (context, pbuf, pbuf.Width / 2, pbuf.Height / 2);
			base.OnDragBegin (context);
		}
		
		protected override void OnDragEnd (Gdk.DragContext context)
		{
			if (PositionProvider.IndexAtPosition (remove_drag_start_point) != -1) {
				GtkDragging = false;
				int draggedPosition = PositionProvider.IndexAtPosition (remove_drag_start_point);
				int currentPosition = PositionProvider.IndexAtPosition (Cursor);
				if (context.DestWindow != window.GdkWindow || !CursorIsOverDockArea) {
					item_provider.RemoveItem (PositionProvider.IndexAtPosition (remove_drag_start_point));
				} else if (CursorIsOverDockArea && currentPosition != draggedPosition) {
					item_provider.MoveItemToPosition (draggedPosition, currentPosition);
				}
				AnimatedDraw ();
			}
			remove_drag_start_point = new Gdk.Point (-1, -1);
			base.OnDragEnd (context);
		}

		#endregion
		
		protected override bool OnEnterNotifyEvent (Gdk.EventCrossing evnt)
		{
			ManualCursorUpdate ();
			return base.OnEnterNotifyEvent (evnt);
		}
		
		protected override bool OnExposeEvent(EventExpose evnt)
		{
			bool ret_val = base.OnExposeEvent (evnt);
			
			if (!IsDrawable || window.IsRepositionHidden)
				return ret_val;
			
			Context cr;
			if (backbuffer == null) {
				cr = Gdk.CairoHelper.Create (GdkWindow);
				backbuffer = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				(cr as IDisposable).Dispose ();
			}
			
			cr = new Cairo.Context (backbuffer);
			cr.AlphaFill ();
			cr.Operator = Operator.Over;
			
			if (item_provider.UpdatesEnabled)
				DrawDrock (cr);
			(cr as IDisposable).Dispose ();
			
			Context cr2 = Gdk.CairoHelper.Create (GdkWindow);
			
			Gdk.Point finalTarget = new Gdk.Point (0, 0).RelativeMovePoint (VerticalOffset, RelativeMove.Outward);
			cr2.SetSource (backbuffer, finalTarget.X, finalTarget.Y);
			
			cr2.Operator = Operator.Source;
			cr2.Paint ();
			(cr2 as IDisposable).Dispose ();
			
			return ret_val;
		}
		
		protected override bool OnMotionNotifyEvent(EventMotion evnt)
		{
			GtkDragging = false;
			return base.OnMotionNotifyEvent (evnt);
		}
		
		void ConfigureCursor ()
		{
			// we do this so that our custom drag isn't destroyed by gtk's drag
			if (gtk_drag_source_set && CursorNearDraggableEdge) {
				UnregisterGtkDragSource ();

				if (cursor_type != CursorType.SbVDoubleArrow && CursorNearTopDraggableEdge) {
					if (DockPreferences.DockIsHorizontal)
						SetCursor (CursorType.SbVDoubleArrow);
					else
						SetCursor (CursorType.SbHDoubleArrow);
					
				} else if (cursor_type != CursorType.LeftSide && CursorNearLeftEdge) {
					if (DockPreferences.DockIsHorizontal)
						SetCursor (CursorType.LeftSide);
					else
						SetCursor (CursorType.TopSide);
					
				} else if (cursor_type != CursorType.RightSide && CursorNearRightEdge) {
					if (DockPreferences.DockIsHorizontal)
						SetCursor (CursorType.RightSide);
					else
						SetCursor (CursorType.BottomSide);
				}
				
			} else if (!gtk_drag_source_set && !drag_resizing && !CursorNearDraggableEdge) {
				RegisterGtkDragSource ();
				if (cursor_type != CursorType.LeftPtr)
					SetCursor (CursorType.LeftPtr);
			}
		}
		
		void SetCursor (Gdk.CursorType type)
		{
			cursor_type = type;
			Gdk.Cursor tmp_cursor = new Gdk.Cursor (type);
			GdkWindow.Cursor = tmp_cursor;
			tmp_cursor.Dispose ();
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
				int item = PositionProvider.IndexAtPosition ((int) evnt.X, (int) evnt.Y); //sometimes clicking is not good!
				if (item < 0 || item >= DockItems.Count || !CursorIsOverDockArea || InputInterfaceVisible)
					return ret_val;
				
				//handling right clicks for those icons which request simple right click handling
				if (evnt.Button == 3) {
					if (CurrentDockItem is IRightClickable && (CurrentDockItem as IRightClickable).GetMenuItems ().Any ()) {
						Gdk.Rectangle geo;
						geo = LayoutUtils.MonitorGemonetry ();
						
						Gdk.Point itemPosition;
						double itemZoom;
						IconZoomedPosition (PositionProvider.IndexAtPosition (Cursor), out itemPosition, out itemZoom);
						
						itemPosition = itemPosition.RelativeMovePoint ((int) (IconSize * itemZoom * .9) - IconSize / 2, RelativeMove.Inward);
						itemPosition = itemPosition.RelativePointToRootPoint (window);
						
						PopupMenu.PopUp ((CurrentDockItem as IRightClickable).GetMenuItems (), itemPosition.X, itemPosition.Y);
						return ret_val;
					}
				}
				
				//send off the clicks
				DockItems [item].Clicked (evnt.Button);
				AnimatedDraw ();
			}
			return ret_val;
		}
		
		void StartDrag ()
		{
			drag_start_point = Cursor;
			drag_start_icon_size = DockPreferences.IconSize;
			drag_resizing = true;
			drag_edge = CurrentDragEdge;
		}
		
		void EndDrag ()
		{
			drag_edge = DragEdge.None;
			drag_resizing = false;
			SetIconRegions ();
			window.SetStruts ();
			
			AnimatedDraw ();
			
			ResetCursorTimer ();
		}
		
		void HandleDragMotion ()
		{
			int movement = 0;
			switch (drag_edge) {
			case DragEdge.Top:
				int delta = DockPreferences.DockIsHorizontal ? drag_start_point.Y - Cursor.Y : drag_start_point.X - Cursor.X;
				if (DockPreferences.Orientation == DockOrientation.Left || DockPreferences.Orientation == DockOrientation.Top)
					delta = 0 - delta;
				DockPreferences.IconSize = Math.Min (drag_start_icon_size + delta, DockPreferences.MaxIconSize);
				return;
			case DragEdge.Left:
				movement = DockPreferences.DockIsHorizontal ? drag_start_point.X - Cursor.X : drag_start_point.Y - Cursor.Y;
				break;
			case DragEdge.Right:
				movement = DockPreferences.DockIsHorizontal ? Cursor.X - drag_start_point.X : Cursor.Y - drag_start_point.Y;
				break;
			}

			if (movement > IconSize / 2 + 2) {
				DockPreferences.AutomaticIcons++;
			} else if (movement < 0 - (IconSize / 2 + 2)) {
				DockPreferences.AutomaticIcons--;
			} else {
				return;
			}
			
			drag_start_point = Cursor;
		}
		
		void SetIconRegions ()
		{
			Gdk.Rectangle pos, area, offset;
			window.GetPosition (out pos.X, out pos.Y);
			window.GetSize (out pos.Width, out pos.Height);
			
			window.WindowHideOffset (out offset.X, out offset.Y);
			// we use geo here instead of our position for the Y value because we know the parent window
			// may offset us when hidden. This is not desired...
			for (int i = 0; i < DockItems.Count; i++) {
				Gdk.Point position = PositionProvider.IconUnzoomedPosition (i);
				area = new Gdk.Rectangle (pos.X + (position.X - IconSize / 2) - offset.X,
				                          pos.Y + (position.Y - IconSize / 2) - offset.Y,
				                          IconSize,
				                          IconSize);
				DockItems [i].SetIconRegion (area);
			}
		}
		
		void SetParentInputMask ()
		{
			if (window == null)
				return;
			
			int offset;
			if (InputInterfaceVisible) {
				offset = (DockPreferences.DockIsHorizontal) ? Height : Width;
			} else if (CursorIsOverDockArea) {
				offset = (DockPreferences.DockIsHorizontal) ? GetDockArea ().Height : GetDockArea ().Width;
				offset = offset * 2 + 10;
			} else {
				if (DockPreferences.AutoHide && !drag_resizing) {
					// setting the offset to 2 will trigger the parent window to unhide us if we are hidden.
					if (AnimationState ["UrgentAnimationNeeded"])
						offset = 2;
					else
						offset = 1;
				} else {
					offset = (DockPreferences.DockIsHorizontal) ? GetDockArea ().Height : GetDockArea ().Width;
				}
			}
			
			int dockSize;
			if (drag_resizing)
				dockSize = (DockPreferences.DockIsHorizontal) ? Width : Height;
			else
				dockSize = (DockPreferences.DockIsHorizontal) ? MinimumDockArea.Width : MinimumDockArea.Height;
			
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				window.SetInputMask (new Gdk.Rectangle ((Width - dockSize) / 2, 
				                                        Height - offset, 
				                                        dockSize, 
				                                        offset));
				break;
			case DockOrientation.Left:
				window.SetInputMask (new Gdk.Rectangle (0, 
				                                        (Height - dockSize) / 2, 
				                                        offset, 
				                                        dockSize));
				break;
			case DockOrientation.Right:
				window.SetInputMask (new Gdk.Rectangle (Width - offset, 
				                                        (Height - dockSize) / 2, 
				                                        offset, 
				                                        dockSize));
				break;
			case DockOrientation.Top:
				window.SetInputMask (new Gdk.Rectangle ((Width - dockSize) / 2, 
				                                        0, 
				                                        dockSize, 
				                                        offset));
				break;
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
			
			GLib.Timeout.Add (500, () => { 
				item_provider.ForceUpdate (); 
				return false; 
			});
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
		
		public override void Dispose ()
		{
			disposed = true;
			UnregisterEvents ();
			UnregisterGtkDragSource ();

			SummonRenderer.Dispose ();
			SummonRenderer = null;
			
			item_provider.Dispose ();
			item_provider = null;

			PositionProvider.Dispose ();
			PositionProvider = null;

			AnimationState.Dispose ();
			AnimationState = null;

			PopupMenu.Destroy ();
			PopupMenu = null;

			if (backbuffer != null)
				backbuffer.Destroy ();

			if (input_area_buffer != null)
				input_area_buffer.Destroy ();

			if (dock_icon_buffer != null)
				dock_icon_buffer.Destroy ();

			window = null;
			
			if (cursor_timer > 0)
				GLib.Source.Remove (cursor_timer);
			
			if (animation_timer > 0)
				GLib.Source.Remove (animation_timer);

			Destroy ();
			base.Dispose ();
		}

	}
}
