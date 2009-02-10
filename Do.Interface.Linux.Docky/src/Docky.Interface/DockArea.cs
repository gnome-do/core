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

using Docky.Core;
using Docky.Utilities;
using Docky.Interface.Menus;
using Docky.Interface.Painters;

namespace Docky.Interface
{
	
	
	public partial class DockArea : Gtk.DrawingArea
	{
		public event System.Action CursorUpdated;
		
		public static readonly TimeSpan BaseAnimationTime = new TimeSpan (0, 0, 0, 0, 150);
		
		const uint OffDockWakeupTime = 250;
		const uint OnDockWakeupTime = 20;

		TimeSpan BounceTime = new TimeSpan (0, 0, 0, 0, 700);
		TimeSpan InsertAnimationTime = new TimeSpan (0, 0, 0, 0, 150*5);
		
		#region Private Variables
		Gdk.Point cursor;
		
		DateTime enter_time = new DateTime (0);
		DateTime interface_change_time = new DateTime (0);
		
		bool disposed;
		
		uint animation_timer;
		uint cursor_timer;
		
		DockWindow window;
		
		#endregion
		
		#region Public Properties
		
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

		#endregion
		
		DockAnimationState AnimationState { get; set; }
		
		ItemPositionProvider PositionProvider { get; set; }

		new DockItemMenu PopupMenu { get; set; }
		
		bool CursorIsOverDockArea {	get; set; }

		PainterService PainterService { get; set; }

		ModifierType CursorModifier { get; set; }
		
		ReadOnlyCollection<AbstractDockItem> DockItems { 
			get { return DockServices.ItemsService.DockItems; } 
		}
		
		AbstractDockItem CurrentDockItem {
			get {
				try { return DockItems [PositionProvider.IndexAtPosition (Cursor)]; }
				catch { return null; }
			}
		}
		
		TimeSpan SummonTime {
			get { return DockPreferences.SummonTime; }
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

				if (CursorUpdated != null)
					CursorUpdated ();
				
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

				DragCursorUpdate ();
			}
		}
		
		Gdk.Rectangle MinimumDockArea {
			get {
				return PositionProvider.MinimumDockArea;
			}
		}
		
		public DockArea (DockWindow window) : base ()
		{
			this.window = window;
			
			SetSize ();
			SetSizeRequest (Width, Height);
			
			PainterService = new PainterService (this);
			PainterService.BuildPainters ();
			DockServices.RegisterService (PainterService);
			
			PositionProvider = new ItemPositionProvider (this);
			
			AnimationState = new DockAnimationState ();
			BuildAnimationStateEngine ();
			
			PopupMenu = new DockItemMenu ();

			Cursor = new Gdk.Point (-1, -1);

			this.SetCompositeColormap ();

			// fixme, we should be using the PointerMotionHintMask
			AddEvents ((int) EventMask.PointerMotionMask |
			           (int) EventMask.EnterNotifyMask |
			           (int) EventMask.ButtonPressMask | 
			           (int) EventMask.ButtonReleaseMask |
			           (int) EventMask.ScrollMask |
			           (int) EventMask.FocusChangeMask);
			
			DoubleBuffered = false;

			BuildRendering ();
			BuildDragAndDrop ();
			
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
			DockServices.ItemsService.DockItemsChanged += OnDockItemsChanged;
			DockServices.ItemsService.ItemNeedsUpdate += HandleItemNeedsUpdate;

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
			DockServices.ItemsService.DockItemsChanged -= OnDockItemsChanged;
			DockServices.ItemsService.ItemNeedsUpdate -= HandleItemNeedsUpdate;

			PopupMenu.Hidden -= OnDockItemMenuHidden;
			PopupMenu.Shown -= OnDockItemMenuShown;

			Wnck.Screen.Default.ViewportsChanged -= OnWnckViewportsChanged;
		}
		
		void BuildAnimationStateEngine ()
		{
			AnimationState.AddCondition (Animations.IconInsert, 
			                             () => DockItems.Any (di => di.TimeSinceAdd < InsertAnimationTime));
			
			AnimationState.AddCondition (Animations.Zoom,
			                             () => (CursorIsOverDockArea && ZoomIn != 1) || (!CursorIsOverDockArea && ZoomIn != 0));
			
			AnimationState.AddCondition (Animations.Open,
			                             () => DateTime.UtcNow - enter_time < SummonTime ||
			                             DateTime.UtcNow - interface_change_time < SummonTime);
			
			AnimationState.AddCondition (Animations.Bounce,
			                             () => DockItems.Any (di => di.TimeSinceClick <= BounceTime));
			
			AnimationState.AddCondition (Animations.Urgency,
			                             () => DockItems
			                             .Where (di => di.NeedsAttention)
			                             .Any (di => DateTime.UtcNow - di.AttentionRequestStartTime < BounceTime));
			
			AnimationState.AddCondition (Animations.UrgencyChanged,
			                             () => DockItems.Any (di => DateTime.UtcNow - di.AttentionRequestStartTime < BounceTime));
			
			AnimationState.AddCondition (Animations.InputModeChanged,
			                             () => DateTime.UtcNow - interface_change_time < SummonTime);
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

		void HandlePaintNeeded (object sender, PaintNeededArgs args)
		{
			if (sender != Painter && sender != LastPainter) return;
			
			if (args.Animated) {
				if (AnimationState.Contains (Animations.Painter))
					AnimationState.RemoveCondition (Animations.Painter);
			
				DateTime current_time = DateTime.UtcNow;
				AnimationState.AddCondition (Animations.Painter, () => DateTime.UtcNow - current_time < args.AnimationLength);
			}
			AnimatedDraw ();
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
		
		void OnDockItemsChanged (IEnumerable<AbstractDockItem> items)
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
		
		void ManualCursorUpdate ()
		{
			int x, y;
			ModifierType mod;

			Display.GetPointer (out x, out y, out mod);
			CursorModifier = mod;
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

		protected override bool OnMotionNotifyEvent (Gdk.EventMotion evnt)
		{
			GtkDragging = false;
			return base.OnMotionNotifyEvent (evnt);
		}
		
		protected override bool OnEnterNotifyEvent (Gdk.EventCrossing evnt)
		{
			ManualCursorUpdate ();
			return base.OnEnterNotifyEvent (evnt);
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

			if (PainterOverlayVisible) {
				Painter.Clicked (GetDockArea (), Cursor);
				if (PainterOverlayVisible && !GetDockArea ().Contains (Cursor)) {
					InteruptPainter ();
				}
			} else {
				int item = PositionProvider.IndexAtPosition ((int) evnt.X, (int) evnt.Y); //sometimes clicking is not good!
				if (item < 0 || item >= DockItems.Count || !CursorIsOverDockArea || PainterOverlayVisible)
					return ret_val;
				
				//handling right clicks for those icons which request simple right click handling
				if (evnt.Button == 3) {
					if (CurrentDockItem is IRightClickable && (CurrentDockItem as IRightClickable).GetMenuItems ().Any ()) {
						PointD itemPosition_;
						double itemZoom;
						IconZoomedPosition (PositionProvider.IndexAtPosition (Cursor), out itemPosition_, out itemZoom);

						Gdk.Point itemPosition = new Gdk.Point ((int) itemPosition_.X, (int) itemPosition_.Y);
						
						itemPosition = itemPosition.RelativeMovePoint ((int) (IconSize * itemZoom * .9) - IconSize / 2, RelativeMove.Inward);
						itemPosition = itemPosition.RelativePointToRootPoint (window);
						
						PopupMenu.PopUp ((CurrentDockItem as IRightClickable).GetMenuItems (), itemPosition.X, itemPosition.Y);
						return ret_val;
					}
				}
				
				//send off the clicks
				Gdk.Point relative_point = Gdk.Point.Zero;
				DockItems [item].Clicked (evnt.Button, evnt.State, relative_point);
				
				AnimatedDraw ();
			}
			return ret_val;
		}

		protected override bool OnScrollEvent (Gdk.EventScroll evnt)
		{
			int item = PositionProvider.IndexAtPosition ((int) evnt.X, (int) evnt.Y);
			DockItems [item].Scrolled (evnt.Direction);
			return base.OnScrollEvent (evnt);
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
			if (PainterOverlayVisible) {
				offset = 0;
			} else if (CursorIsOverDockArea) {
				offset = (DockPreferences.DockIsHorizontal) ? GetDockArea ().Height : GetDockArea ().Width;
				offset = offset * 2 + 10;
			} else {
				if (DockPreferences.AutoHide && !drag_resizing) {
					// setting the offset to 2 will trigger the parent window to unhide us if we are hidden.
					if (AnimationState [Animations.Urgency])
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

		public bool RequestShowPainter (IDockPainter painter)
		{
			if (Painter == painter)
				return true;
			
			if (Painter == null || Painter.Interuptable) {
				if (Painter != null)
					Painter.Interupt ();
				Painter = painter;
				PainterOverlayVisible = true;
				interface_change_time = DateTime.UtcNow;

				SetParentInputMask ();
				AnimatedDraw ();
			} else {
				return false;
			}
			UnregisterGtkDragSource ();
			return true;
		}

		public bool RequestHidePainter (IDockPainter painter)
		{
			if (Painter != painter)
				return false;

			Painter = null;
			PainterOverlayVisible = false;
			interface_change_time = DateTime.UtcNow;

			SetParentInputMask ();
			AnimatedDraw ();

			GLib.Timeout.Add (500, () => { 
				DockServices.ItemsService.ForceUpdate (); 
				return false; 
			});

			RegisterGtkDragSource ();
			return true;
		}

		void InteruptPainter ()
		{
			if (Painter == null) return;

			Painter.Interupt ();
			Painter = null;
			PainterOverlayVisible = false;
			interface_change_time = DateTime.UtcNow;

			SetParentInputMask ();
			AnimatedDraw ();
		}
		
		public override void Dispose ()
		{
			disposed = true;
			UnregisterEvents ();
			UnregisterGtkDragSource ();

			PositionProvider.Dispose ();
			AnimationState.Dispose ();
			PopupMenu.Destroy ();

			PositionProvider = null;
			AnimationState = null;
			PopupMenu = null;

			if (backbuffer != null)
				backbuffer.Destroy ();

			if (input_area_buffer != null)
				input_area_buffer.Destroy ();

			if (dock_icon_buffer != null)
				dock_icon_buffer.Destroy ();
			
			DockServices.UnregisterService (PainterService);
			PainterService.Dispose ();

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
