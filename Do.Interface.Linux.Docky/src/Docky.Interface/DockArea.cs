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
using System.Collections.ObjectModel;
using System.Linq;

using Cairo;
using Gdk;
using Gtk;

using Do.Platform;
using Do.Interface.Wink;
using Do.Interface.Xlib;

using Docky.Core;
using Docky.Utilities;
using Docky.Interface.Menus;
using Docky.Interface.Painters;

namespace Docky.Interface
{
	
	
	internal partial class DockArea : Gtk.DrawingArea
	{
		public static readonly TimeSpan BaseAnimationTime = new TimeSpan (0, 0, 0, 0, 150);
		
		static DateTime UpdateTimeStamp (DateTime lastStamp, TimeSpan animationLength)
		{
			TimeSpan delta = DateTime.UtcNow - lastStamp;
			if (delta < animationLength)
				return DateTime.UtcNow.Subtract (animationLength - delta);
			return DateTime.UtcNow;
		}
		
		const uint OffDockWakeupTime = 250;
		const uint OnDockWakeupTime = 20;
		
		const int UrgentBounceHeight = 80;
		const int LaunchBounceHeight = 30;

		TimeSpan BounceTime = new TimeSpan (0, 0, 0, 0, 700);
		TimeSpan InsertAnimationTime = new TimeSpan (0, 0, 0, 0, 150*5);
		
		#region Private Variables
		DateTime enter_time = new DateTime (0);
		DateTime interface_change_time = new DateTime (0);
		DateTime last_draw_timeout = new DateTime (0);
		DateTime showhide_time = new DateTime (0);
		DateTime painter_time = new DateTime (0);
		
		TimeSpan painter_span;
		
		uint animation_timer;
		
		DockWindow window;
		
		DrawingService drawing_service;
		
		#endregion
		
		#region Public Properties
		//// <value>
		/// Returns true if the cursor is over the visible part of the dock or the pixel closest to its autohide edge
		/// </value>
		public bool CursorIsOverDockArea { get; private set; }
		
		/// <value>
		/// The width of the docks window, but not the visible dock
		/// </value>
		public int Width { get; private set; }
		
		/// <value>
		/// The height of the docks window
		/// </value>
		public int Height { get; private set; }
		
		//// <value>
		/// Returns true if the dock is currently being overlayed by an IDockPainter
		/// </value>
		public bool PainterOverlayVisible { get; private set; }
		
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
				Gdk.Rectangle geo = LayoutUtils.MonitorGeometry ();
				
				if (DockPreferences.AutohideType != AutohideType.None)
					return values;
				
				switch (DockPreferences.Orientation) {
				case DockOrientation.Bottom:
					values [(int) Struts.Bottom] = (uint) (DockHeight + (Screen.Height - (geo.Y + geo.Height)));
					values [(int) Struts.BottomStart] = (uint) geo.X;
					values [(int) Struts.BottomEnd] = (uint) (geo.X + geo.Width - 1);
					break;
				case DockOrientation.Top:
					values [(int) Struts.Top] = (uint) (DockHeight + geo.Y);
					values [(int) Struts.TopStart] = (uint) geo.X;
					values [(int) Struts.TopEnd] = (uint) (geo.X + geo.Width - 1);
					break;
				}
				return values;
			}
		}

		/// <value>
		/// The current cursor as known to the dock.
		/// </value>
		public Gdk.Point Cursor {
			get {
				return CursorTracker.Cursor;
			}
		}
		
		public Gdk.Rectangle MinimumDockArea {
			get {
				return PositionProvider.MinimumDockArea;
			}
		}
		#endregion
		
		AutohideTracker AutohideTracker { get; set; }
		
		CursorTracker CursorTracker { get; set; }
		
		DnDTracker DnDTracker { get; set; }
		
		DockAnimationState AnimationState { get; set; }
		
		ItemPositionProvider PositionProvider { get; set; }

		new DockItemMenu PopupMenu { get; set; }
		
		ReadOnlyCollection<AbstractDockItem> DockItems { 
			get { return DockServices.ItemsService.DockItems; } 
		}
		
		AbstractDockItem CurrentDockItem {
			get {
				int index = PositionProvider.IndexAtPosition (Cursor);
				if (index >= 0 && index < DockItems.Count)
					return DockItems [index];
				return null;
			}
		}
		
		TimeSpan SummonTime {
			get { return DockPreferences.SummonTime; }
		}
		
		bool WindowIntersectingOther { 
			get { return AutohideTracker.WindowIntersectingOther; }
		}
		
		IEnumerable<Gdk.Window> WindowStack {
			get {
				try {
					return Screen.WindowStack;
				} catch { 
					try {
						return Wnck.Screen.Default.WindowsStacked.Select (wnk => Gdk.Window.ForeignNew ((uint) wnk.Xid));
					} catch {
						return null;
					}
				}
			}
		}
		
		public DockArea (DockWindow window) : base ()
		{
			drawing_service = new DrawingService (this);
			DockServices.RegisterService (drawing_service);
			
			this.window = window;
			
			ScreenUtils.Initialize ();
			WindowUtils.Initialize ();
			
			AnimationState   = new DockAnimationState ();
			AutohideTracker  = new AutohideTracker (this);
			CursorTracker    = new CursorTracker (window, OffDockWakeupTime);
			PositionProvider = new ItemPositionProvider (this);
			DnDTracker       = new DnDTracker (this, PositionProvider, CursorTracker);
			PopupMenu        = new DockItemMenu ();
			
			BuildAnimationStateEngine ();

			this.SetCompositeColormap ();

			AddEvents ((int) EventMask.ButtonPressMask | 
			           (int) EventMask.ButtonReleaseMask |
			           (int) EventMask.ScrollMask |
			           (int) EventMask.FocusChangeMask);
			
			SetSize ();
			DoubleBuffered = false;

			BuildRendering ();
			
			RegisterEvents ();
			
			ResetCursorTimer ();
		}
		
		void SetSize ()
		{
			Gdk.Rectangle geo;
			geo = LayoutUtils.MonitorGeometry ();
			
			Width = geo.Width;
			Height = DockPreferences.FullIconSize + 2 * PositionProvider.VerticalBuffer + UrgentBounceHeight;
			Height = Math.Max (150, Height);
			
			SetSizeRequest (Width, Height);
		}

		void RegisterEvents ()
		{
			DockServices.ItemsService.DockItemsChanged += OnDockItemsChanged;
			DockServices.ItemsService.ItemNeedsUpdate += HandleItemNeedsUpdate;
			
			DockPreferences.MonitorChanged += HandleMonitorChanged; 
			DockPreferences.IconSizeChanged += HandleIconSizeChanged; 
			DockPreferences.OrientationChanged += HandleOrientationChanged; 
			
			DockServices.PainterService.PainterShowRequest += HandlePainterShowRequest;
			DockServices.PainterService.PainterHideRequest += HandlePainterHideRequest;

			Screen.SizeChanged += HandleSizeChanged; 
			
			PopupMenu.Hidden += OnDockItemMenuHidden;
			PopupMenu.Shown += OnDockItemMenuShown;

			Services.Core.UniverseInitialized += HandleUniverseInitialized;
			
			Realized += (o, e) => SetParentInputMask ();
			Realized += (o, a) => GdkWindow.SetBackPixmap (null, false);
			
			AutohideTracker.IntersectionChanged += HandleIntersectionChanged;
			Wnck.Screen.Default.ActiveWindowChanged += HandleActiveWindowChanged;
			
			CursorTracker.CursorUpdated += HandleCursorUpdated;
			
			DnDTracker.DragEnded += HandleDragEnded;
			DnDTracker.DrawRequired += HandleDrawRequired; 
			
			StyleSet += (o, a) => { 
				if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			};
		}

		void UnregisterEvents ()
		{
			DockServices.ItemsService.DockItemsChanged -= OnDockItemsChanged;
			DockServices.ItemsService.ItemNeedsUpdate -= HandleItemNeedsUpdate;
			
			DockPreferences.MonitorChanged -= HandleMonitorChanged;
			DockPreferences.IconSizeChanged -= HandleIconSizeChanged; 
			DockPreferences.OrientationChanged -= HandleOrientationChanged; 
			
			DockServices.PainterService.PainterShowRequest -= HandlePainterShowRequest;
			DockServices.PainterService.PainterHideRequest -= HandlePainterHideRequest;

			Screen.SizeChanged -= HandleSizeChanged;
			
			PopupMenu.Hidden -= OnDockItemMenuHidden;
			PopupMenu.Shown -= OnDockItemMenuShown;
			
			Services.Core.UniverseInitialized -= HandleUniverseInitialized;
			
			AutohideTracker.IntersectionChanged -= HandleIntersectionChanged;
			Wnck.Screen.Default.ActiveWindowChanged -= HandleActiveWindowChanged;
			
			CursorTracker.CursorUpdated -= HandleCursorUpdated;
			
			DnDTracker.DragEnded -= HandleDragEnded;
			DnDTracker.DrawRequired -= HandleDrawRequired; 
		}
		
		void BuildAnimationStateEngine ()
		{
			AnimationState.AddCondition (Animations.IconInsert, 
			                             () => DockItems.Any (di => di.TimeSinceAdd < InsertAnimationTime));
			
			AnimationState.AddCondition (Animations.Zoom,
			                             () => (CursorIsOverDockArea && ZoomIn != 1) || (!CursorIsOverDockArea && ZoomIn != 0));
			
			AnimationState.AddCondition (Animations.Open,
			                             () => DateTime.UtcNow - enter_time < BaseAnimationTime ||
			                             DateTime.UtcNow - interface_change_time < BaseAnimationTime);
			
			AnimationState.AddCondition (Animations.Bounce,
			                             () => DockItems.Any (di => di.TimeSinceClick <= BounceTime));
			
			AnimationState.AddCondition (Animations.UrgencyChanged,
			                             () => DockItems.Any (di => DateTime.UtcNow - di.AttentionRequestStartTime < BounceTime));
			
			AnimationState.AddCondition (Animations.InputModeChanged,
			                             () => DateTime.UtcNow - interface_change_time < SummonTime || 
			                             DateTime.UtcNow - showhide_time < SummonTime);
		}
		
		void HandleCursorUpdated (object sender, CursorUpdatedArgs args)
		{
			if (PopupMenu.Visible)
				return;
			
			UpdateCursorIsOverDockArea ();

			bool cursorMoveWarrantsDraw = CursorIsOverDockArea && args.OldCursor.X != args.NewCursor.X;

			if (DnDTracker.DragResizing || cursorMoveWarrantsDraw) 
				AnimatedDraw ();
		}
		
		void HandleDrawRequired(object sender, EventArgs e)
		{
			AnimatedDraw ();
		}

		void HandleDragEnded(object sender, EventArgs e)
		{
			ResetCursorTimer ();
			Reconfigure ();
		}

		void HandleItemNeedsUpdate (object sender, UpdateRequestArgs args)
		{
			if (args.Type == UpdateRequestType.NeedsAttentionSet) {
				SetParentInputMask ();
				Reconfigure ();
				
				GLib.Timeout.Add ((uint) BounceTime.Milliseconds + 20, delegate {
					Reconfigure ();
					return false;
				});
			}
			
			if (args.Type == UpdateRequestType.IconChanged || args.Type == UpdateRequestType.NameChanged) {
				RequestIconRender (args.Item);
			} else {
				RequestFullRender ();
			}
			AnimatedDraw ();
		}

		void HandleMonitorChanged()
		{
			Reconfigure ();
		}

		void HandleOrientationChanged()
		{
			Reconfigure ();
			window.Reposition ();
		}
		
		void HandleSizeChanged (object sender, EventArgs e)
		{
			Reconfigure ();
		}
		
		public void Reconfigure ()
		{
			if (!Visible || !IsRealized || DnDTracker.DragResizing)
				return;
			
			SetSize ();
			ResetBuffers ();
			PositionProvider.ForceUpdate ();
			SetParentInputMask ();
			SetIconRegions ();
			window.DelaySetStruts ();
			AnimatedDraw ();
		}
		
		void HandleUniverseInitialized (object sender, EventArgs e)
		{
			GLib.Timeout.Add (2000, delegate {
				DockServices.ItemsService.ForceUpdate ();
				SetIconRegions ();
				return false;
			});
		}
		
		void HandleIconSizeChanged()
		{
			Reconfigure ();
			AnimatedDraw ();
		}

		void HandlePaintNeeded (object sender, PaintNeededArgs args)
		{
			if (sender != Painter && sender != LastPainter) return;
			
			if (args.Animated) {
				painter_span = args.AnimationLength;
				painter_time = DateTime.UtcNow;
				if (!AnimationState.Contains (Animations.Painter))
					AnimationState.AddCondition (Animations.Painter, () => DateTime.UtcNow - painter_time < painter_span);
			}
			AnimatedDraw ();
		}
		
		void HandlePainterHideRequest(object sender, EventArgs e)
		{
			IDockPainter painter = sender as IDockPainter;
			if (Painter != painter)
				return;

			Painter = null;
			PainterOverlayVisible = false;
			interface_change_time = UpdateTimeStamp (interface_change_time, SummonTime);

			SetParentInputMask ();
			AnimatedDraw ();

			GLib.Timeout.Add (500, () => { 
				DockServices.ItemsService.ForceUpdate (); 
				return false; 
			});

			window.UnpresentWindow ();
			DnDTracker.Enable ();
		}

		void HandlePainterShowRequest(object sender, EventArgs e)
		{
			IDockPainter painter = sender as IDockPainter;
			if (Painter == painter)
				return;
			
			if (Painter == null || Painter.Interruptable) {
				if (Painter != null)
					Painter.Interrupt ();
				Painter = painter;
				PainterOverlayVisible = true;
				interface_change_time = UpdateTimeStamp (interface_change_time, SummonTime);

				SetParentInputMask ();
				AnimatedDraw ();
			} else {
				painter.Interrupt ();
			}
			
			window.PresentWindow ();
			DnDTracker.Disable ();
		}
		
		void ResetCursorTimer ()
		{
			CursorTracker.TimerLength = (CursorIsOverDockArea || DnDTracker.DragResizing) ? OnDockWakeupTime : OffDockWakeupTime;
		}
		
		void UpdateCursorIsOverDockArea ()
		{
			bool tmp = CursorIsOverDockArea;
			
			Gdk.Rectangle dockRegion;
			if (PainterOverlayVisible)
				dockRegion = GetDockArea ();
			else
				dockRegion = MinimumDockArea;
			
			if (tmp) {
				dockRegion.Inflate (0, (int) (IconSize * (DockPreferences.ZoomPercent - 1)) + 22);
			} else {
				if (IsHidden) {
					switch (DockPreferences.Orientation) {
					case DockOrientation.Bottom:
						dockRegion.Y += dockRegion.Height - 1;
						dockRegion.Height = 1;
						break;
					case DockOrientation.Top:
						dockRegion.Height = 1;
						break;
					}
				}
			}
			
			if (PainterOverlayVisible)
				CursorIsOverDockArea = dockRegion.Contains (new Gdk.Point (dockRegion.X, Cursor.Y));
			else
				CursorIsOverDockArea = dockRegion.Contains (Cursor);
			
			if (CursorIsOverDockArea != tmp) {
				ResetCursorTimer ();
				enter_time = UpdateTimeStamp (enter_time, BaseAnimationTime);
				
				switch (DockPreferences.AutohideType) {
				case AutohideType.Autohide:
					showhide_time = enter_time;
					break;
				case AutohideType.Intellihide:
					if (WindowIntersectingOther)
						showhide_time = enter_time;
					break;
				}
				AnimatedDraw ();
				
				if (PainterOverlayVisible) {
					GLib.Timeout.Add (500, delegate {
						if (!CursorIsOverDockArea && PainterOverlayVisible && (DateTime.UtcNow - enter_time).TotalMilliseconds > 400)
							InterruptPainter ();
						return false;
					});
				}
			}
			
		}
		
		void AnimatedDraw ()
		{
			if (0 < animation_timer) {
				if ((DateTime.UtcNow - last_draw_timeout).TotalMilliseconds > 500) {
					// honestly this should never happen.  I am not sure if it does but we are going
					// to protect against it because there are reports of rendering failing. A condition
					// where the animation_timer is > 0 without an actual callback tied to it would
					// forever block future animations.
					GLib.Source.Remove (animation_timer);
					animation_timer = 0;
				} else {
					return;
				}
			}
			
			// the presense of this queue draw has caused some confusion, so I will explain.
			// first its here to draw the "first frame".  Without it, we have a 16ms delay till that happens,
			// however minor that is.
			MaskAndDraw ();
			
			if (AnimationState.AnimationNeeded)
				animation_timer = GLib.Timeout.Add (1000/60, OnDrawTimeoutElapsed);
		}
		
		bool OnDrawTimeoutElapsed ()
		{
			last_draw_timeout = DateTime.UtcNow;
			
			MaskAndDraw ();
			
			if (AnimationState.AnimationNeeded)
				return true;
			
			//reset the timer to 0 so that the next time AnimatedDraw is called we fall back into
			//the draw loop.
			animation_timer = 0;
			return false;
		}
		
		void MaskAndDraw ()
		{
			QueueDraw ();
			// this is a "protected method".  We need to be sure that our input mask is okay on every frame.
			// 99% of the time this means nothing at all will be done
			SetParentInputMask ();
		}
		
		void OnDockItemsChanged (IEnumerable<AbstractDockItem> items)
		{
			DockPreferences.MaxIconSize = (int) (((double) Width / MinimumDockArea.Width) * IconSize);
			
			SetIconRegions ();
			RequestFullRender ();
			UpdateCursorIsOverDockArea ();
			AnimatedDraw ();
		}
		
		void OnDockItemMenuHidden (object o, System.EventArgs args)
		{
			// While an popup menus are being showing, the dock does not recieve mouse updates.  This is
			// both a good thing and a bad thing.  We must at the very least update the cursor position once the
			// popup is no longer in view.
			CursorTracker.Enabled = true;
			AnimatedDraw ();
		}
		
		/// <summary>
		/// Only purpose is to trigger one last redraw to eliminate the hover text
		/// </summary>
		void OnDockItemMenuShown (object o, EventArgs args)
		{
			CursorTracker.Enabled = false;
			AnimatedDraw ();
		}

		public void ProxyButtonReleaseEvent (Gdk.EventButton evnt)
		{
			HandleButtonReleaseEvent (evnt);
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			bool result = base.OnButtonPressEvent (evnt);
			HandleButtonReleaseEvent (evnt);
			
			return result;
		}
		
		private void HandleButtonReleaseEvent (Gdk.EventButton evnt)
		{
			// lets not do anything in this case
			if (DnDTracker.DragResizing) return;
			
			if (PainterOverlayVisible) {
				Painter.Clicked (GetDockArea (), Cursor);
				if (PainterOverlayVisible && !GetDockArea ().Contains (Cursor)) {
					InterruptPainter ();
				}
			} else {
				int item = PositionProvider.IndexAtPosition ((int) evnt.X, (int) evnt.Y); //sometimes clicking is not good!
				if (item < 0 || item >= DockItems.Count || !CursorIsOverDockArea || PainterOverlayVisible)
					return;
				
				//handling right clicks for those icons which request simple right click handling
				if (evnt.Button == 3) {
					if (CurrentDockItem is IRightClickable && (CurrentDockItem as IRightClickable).GetMenuItems ().Any ()) {
						PointD itemPosition_;
						double itemZoom;
						IconZoomedPosition (PositionProvider.IndexAtPosition (Cursor), out itemPosition_, out itemZoom);

						Gdk.Point itemPosition = new Gdk.Point ((int) itemPosition_.X, (int) itemPosition_.Y);
						
						itemPosition = itemPosition.RelativeMovePoint ((int) (IconSize * itemZoom * .9 * .5), RelativeMove.Inward);
						itemPosition = itemPosition.RelativePointToRootPoint (window);
						
						PopupMenu.PopUp (CurrentDockItem.Description, 
						                 (CurrentDockItem as IRightClickable).GetMenuItems (), 
						                 itemPosition.X, 
						                 itemPosition.Y);
						return;
					}
				}
				
				//send off the clicks
				PointD relative_point = RelativePointOverItem (item);
				DockItems [item].Clicked (evnt.Button, evnt.State, relative_point);
				
				AnimatedDraw ();
			}
			return;
		}
		
		PointD RelativePointOverItem (int item)
		{
			PointD relative_point = new PointD (0,0);
			double zoom;
			PointD center;
			IconZoomedPosition (item, out center, out zoom);
			
			int left = (int) (center.X - DockItems [item].Width * zoom / 2);
			int top = (int) (center.Y - DockItems [item].Height * zoom / 2);
			int right = (int) (center.X + DockItems [item].Width * zoom / 2);
			int bottom = (int) (center.Y + DockItems [item].Height * zoom / 2);
			
			relative_point.X = (Cursor.X - left) / (double) (right - left);
			relative_point.Y = (Cursor.Y - top) / (double) (bottom - top);
			
			return relative_point;
		}

		protected override bool OnScrollEvent (Gdk.EventScroll evnt)
		{
			if (PainterOverlayVisible) {
				Painter.Scrolled (evnt.Direction);
			} else {
				int item = PositionProvider.IndexAtPosition ((int) evnt.X, (int) evnt.Y);
				if (item >= 0 && item < DockItems.Count)
					DockItems [item].Scrolled (evnt.Direction);
			}
			return base.OnScrollEvent (evnt);
		}
		
		void SetIconRegions ()
		{
			Gdk.Rectangle pos, area;
			window.GetPosition (out pos.X, out pos.Y);
			window.GetSize (out pos.Width, out pos.Height);
			
			// we use geo here instead of our position for the Y value because we know the parent window
			// may offset us when hidden. This is not desired...
			for (int i = 0; i < DockItems.Count; i++) {
				Gdk.Point position = PositionProvider.IconUnzoomedPosition (i);
				area = new Gdk.Rectangle (pos.X + (position.X - IconSize / 2),
				                          pos.Y + (position.Y - IconSize / 2),
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
			if (PainterOverlayVisible) {
				offset = 0;
			} else if (CursorIsOverDockArea) {
				offset = GetDockArea ().Height;
				offset = offset * 2 + 10;
			} else {
				if (IsHidden && !DnDTracker.DragResizing) {
					offset = 1;
				} else {
					offset = GetDockArea ().Height;
				}
			}
			
			int dockSize = (DnDTracker.DragResizing) ? Width : MinimumDockArea.Width;
			
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				window.SetInputMask (new Gdk.Rectangle ((Width - dockSize) / 2, 
				                                        Height - offset, 
				                                        dockSize, 
				                                        offset));
				break;
			case DockOrientation.Top:
				window.SetInputMask (new Gdk.Rectangle ((Width - dockSize) / 2, 
				                                        0, 
				                                        dockSize, 
				                                        offset));
				break;
			}
		}

		void InterruptPainter ()
		{
			if (Painter == null || !Painter.Interruptable) return;

			Painter.Interrupt ();
			Painter = null;
			PainterOverlayVisible = false;
			interface_change_time = DateTime.UtcNow;
			
			DnDTracker.Enable ();
			window.UnpresentWindow ();
			
			SetParentInputMask ();
			AnimatedDraw ();
		}
		
		public override void Dispose ()
		{
			DockServices.UnregisterService (drawing_service).Dispose ();
			
			UnregisterEvents ();
			DnDTracker.Disable ();

			AutohideTracker.Dispose ();
			CursorTracker.Dispose ();
			DnDTracker.Dispose ();
			PositionProvider.Dispose ();
			AnimationState.Dispose ();
			PopupMenu.Destroy ();
			PopupMenu.Dispose ();

			PositionProvider = null;
			AnimationState = null;
			PopupMenu = null;

			if (backbuffer != null)
				backbuffer.Destroy ();

			if (input_area_buffer != null)
				input_area_buffer.Destroy ();

			if (dock_icon_buffer != null)
				dock_icon_buffer.Destroy ();
			
			if (LastPainter != null) {
				LastPainter.PaintNeeded -= HandlePaintNeeded;
			}
			
			if (Painter != null) {
				Painter.PaintNeeded -= HandlePaintNeeded;
			}
			
			window = null;
			
			if (animation_timer > 0)
				GLib.Source.Remove (animation_timer);

			Destroy ();
			base.Dispose ();
		}

	}
}
