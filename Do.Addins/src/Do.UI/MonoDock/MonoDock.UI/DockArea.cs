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

using Cairo;
using Gdk;
using Gtk;

using Do.Addins;
using Do.Universe;
using Do.UI;
using Do.Addins.CairoUtils;

using MonoDock.Util;

namespace MonoDock.UI
{
	
	
	public class DockArea : Gtk.DrawingArea
	{
		enum DrawState {
			Normal,
			Text,
			NoResult,
			None,
		}
		
		const int BounceTime = 700;
		const int BaseAnimationTime = 150;
		const int InsertAnimationTime = BaseAnimationTime*5;
		const int YBuffer = 3;
		const int XBuffer = 5;
		const int PaneSize = 168;
		
		IList<IDockItem> dock_items;
		IList<IDockItem> window_items;
		Gdk.Point cursor;
		DateTime enter_time = DateTime.UtcNow;
		DateTime last_render = DateTime.UtcNow;
		int monitor_width;
		
		DockState state;
		Surface backbuffer, input_area_buffer, dock_icon_buffer;
		DockWindow window;
		PixbufSurfaceCache large_icon_cache, small_icon_cache;
		
		bool autohide = true;
		
		#region Public properties
		public int Width {
			get {
				return monitor_width;
			}
		}
		
		public int Height {
			get {
				return 2*IconSize + 40;
			}
		}
		
		public int DockWidth {
			get {
				int out_width = 2*XBuffer;//(2*XBuffer)+(dock_items.Count*IconBorderWidth);
				foreach (IDockItem di in DockItems) {
					out_width += di.Width;
				}
				return out_width;
			}
		}
		
		public int DockHeight {
			get {
				if (autohide)
					return 0;
				return MinimumDockArea.Height;
			}
		}
		
		public bool InputInterfaceVisible {
			get {
				return input_interface;
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
		
		public bool ThirdPaneVisible { get; set; }
		#endregion
		
		IStatistics Statistics { get; set; }
		
		IList<IDockItem> DockItems {
			get {
				List<IDockItem> out_items = new List<IDockItem> (dock_items);
				
				
				out_items.Add (new SeperatorItem ());
				out_items.AddRange (window_items);
				
				return out_items;
			}
			set {
				dock_items = value;
			}
		}
		
		#region Zoom Properties
		double ZoomIn {
			get {
				if (!CursorIsOverDockArea)
					return Math.Max (0, 1-DateTime.UtcNow.Subtract (enter_time).TotalMilliseconds/BaseAnimationTime);
				return Math.Min (1, DateTime.UtcNow.Subtract (enter_time).TotalMilliseconds/BaseAnimationTime);
			}
		}
		
		int ZoomPixels {
			get {
				return (int) (ZoomSize*ZoomIn);
			}
		}
		
		int ZoomSize {
			get {
				return Preferences.ZoomSize;
			}
		}
		#endregion
		
		int YOffset {
			get {
				if (!autohide)
					return 0;
				double offset = 0;
				if (CursorIsOverDockArea) {
					offset = 1 - Math.Min (1,(DateTime.UtcNow - enter_time).TotalMilliseconds / BaseAnimationTime);
					return (int) (offset*MinimumDockArea.Height);
				} else {
					offset = Math.Min (Math.Min (1,(DateTime.UtcNow - enter_time).TotalMilliseconds / BaseAnimationTime),
					                  Math.Min (1, (DateTime.UtcNow - interface_change_time).TotalMilliseconds / BaseAnimationTime));
					if (input_interface)
						offset = 1-offset;
					return (int) (offset*MinimumDockArea.Height);
				}
			}
		}
		
		double DockIconOpacity {
			get {
				double total_time = (DateTime.UtcNow - interface_change_time).TotalMilliseconds;
				if (total_time > BaseAnimationTime) {
					if (input_interface)
						return 0;
					return 1;
				}
				
				if (input_interface) {
					return 1-(total_time/BaseAnimationTime);
				} else {
					return total_time/BaseAnimationTime;
				}
			}
		}
		
		string HighlightFormat { 
			get { 
				return "<span underline=\"single\">{0}</span>";
			} 
		}
		
		double InputAreaOpacity {
			get {
				return 1-DockIconOpacity;
			}
		}
		
		double InputAreaSlideStatus {
			get {
				return Math.Min (1,(DateTime.UtcNow - State.CurrentPaneTime).TotalMilliseconds / BaseAnimationTime);
			}
		}
		
		int IconBorderWidth {get{ return 4; }}
		
		int IconSize { get { return DockItem.IconSize + IconBorderWidth; } }
		
		int GapSize { get { return GetDockArea ().Width - 10 - PaneSize*3; } }
		
		Gdk.Point Cursor {
			get {
				return cursor;
			}
			set {
				bool tmp = CursorIsOverDockArea;
				cursor = value;
				if (CursorIsOverDockArea != tmp) {
					if (CursorIsOverDockArea) {
						window.SetInputMask (0);
					} else {
						if (autohide)
							window.SetInputMask (Height-1);
						else
							window.SetInputMask (Height-IconSize);
					}
					enter_time = DateTime.UtcNow;
					AnimatedDraw ();
				}
			}
		}
		
		Gdk.Rectangle MinimumDockArea {
			get {
				int x_offset = (Width-DockWidth)/2;
				Gdk.Rectangle minimum_dock_size = 
					new Gdk.Rectangle (x_offset, Height-IconSize-2*YBuffer, DockWidth, IconSize+2*YBuffer);
					
				return minimum_dock_size;
			}
		}
		
		PixbufSurfaceCache LargeIconCache {
			get {
				return large_icon_cache ?? large_icon_cache = new PixbufSurfaceCache (10, 128, 128);
			}
		}
		
		PixbufSurfaceCache SmallIconCache {
			get {
				return small_icon_cache ?? small_icon_cache = new PixbufSurfaceCache (40, 64, 64);
			}
		}
		
		DockState State {
			get { return state ?? state = new DockState (); }
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
				foreach (IDockItem di in DockItems) {
					if ((DateTime.UtcNow-di.DockAddItem).TotalMilliseconds < InsertAnimationTime)
						return true;
				}
				return false;
			}
		}
		
		bool PaneChangeAnimationNeeded {
			get {
				return (DateTime.UtcNow - State.CurrentPaneTime).TotalMilliseconds < BaseAnimationTime;
			}
		}
		
		bool ZoomAnimationNeeded {
			get { return !((CursorIsOverDockArea && ZoomIn == 1) || (!CursorIsOverDockArea && ZoomIn == 0)); }
		}
		
		bool OpenAnimationNeeded {
			get { return (DateTime.UtcNow - enter_time).TotalMilliseconds < BaseAnimationTime ||
				(DateTime.UtcNow - interface_change_time).TotalMilliseconds < BaseAnimationTime;
			}
		}
		
		DateTime last_click = DateTime.UtcNow;
		bool BounceAnimationNeeded {
			get { return (DateTime.UtcNow - last_click).TotalMilliseconds < BounceTime; }
		}
		
		bool InputModeChangeAnimationNeeded {
			get { return (DateTime.UtcNow - interface_change_time).TotalMilliseconds < BaseAnimationTime; }
		}
		
		bool InputModeSlideAnimationNeeded {
			get {
				return (DateTime.UtcNow - State.LastCursorChange).TotalMilliseconds < BaseAnimationTime;
			}
		}
		
		bool AnimationNeeded {
			get { return PaneChangeAnimationNeeded || ZoomAnimationNeeded || BounceAnimationNeeded || 
				InputModeChangeAnimationNeeded || InputModeSlideAnimationNeeded || IconInsertionAnimationNeeded || OpenAnimationNeeded; }
		}
		#endregion
		
		public DockArea(DockWindow window, IStatistics statistics) : base ()
		{
			Statistics = statistics;
			this.window = window;
			DockItems = new List<IDockItem> ();
			window_items = new List<IDockItem> ();
			
			
			GLib.Timeout.Add (3000, delegate {
				UpdateWindowItems ();
				List<IDockItem> items = new List<IDockItem> ();
				foreach (IItem item in Statistics.GetMostUsedItems (10)) {
					items.Add (new DockItem (item));
				}
				SetIcons (items);
				return false;
			});
			
			Cursor = new Gdk.Point (-1, -1);
			
			Gdk.Rectangle geo;
			geo = Screen.GetMonitorGeometry (0);
			monitor_width = geo.Width;
			SetSizeRequest (geo.Width, Height);
			this.SetCompositeColormap ();
			
			AddEvents ((int) Gdk.EventMask.PointerMotionMask | (int) Gdk.EventMask.LeaveNotifyMask |
			           (int) Gdk.EventMask.ButtonPressMask | (int) Gdk.EventMask.ButtonReleaseMask);
			
			DoubleBuffered = false;
			
			StyleSet += delegate {
				if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			};
			
			Wnck.Screen.Default.WindowOpened += delegate {
				UpdateWindowItems ();
			};
			
			Wnck.Screen.Default.WindowClosed += delegate {
				UpdateWindowItems ();
			};
			
			Wnck.Screen.Default.ViewportsChanged += delegate {
				UpdateWindowItems ();
			}; 
		}
		
		uint timer = 0;
		void AnimatedDraw ()
		{
			if (timer > 0)
				return;
			
			QueueDraw ();
			
			timer = GLib.Timeout.Add (16, delegate {
				QueueDraw ();
				if (AnimationNeeded)
					return true;
				
				timer = 0;
				return false;
			});
		}
		
		void DrawDrock (Context cr)
		{
			Gdk.Rectangle dock_area = GetDockArea ();
			cr.Rectangle (dock_area.X+.5, dock_area.Y+.5, dock_area.Width-1, dock_area.Height);
			cr.Color = new Cairo.Color (.1, .1, .1, .7);
			cr.FillPreserve ();
			
			cr.Color = new Cairo.Color (1, 1, 1, .4);
			cr.LineWidth = 1;
			cr.Stroke ();
			
			if (InputAreaOpacity > 0) {
				if (input_area_buffer == null)
					input_area_buffer = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				
				using (Context input_cr = new Context (input_area_buffer)) {
					input_cr.AlphaFill ();
					DrawInputArea (input_cr);
				}
				
				cr.SetSource (input_area_buffer);
				cr.PaintWithAlpha (InputAreaOpacity);
			}
			
			if (DockIconOpacity > 0) {
				if (dock_icon_buffer == null)
					dock_icon_buffer = cr.Target.CreateSimilar (cr.Target.Content, Width, Height);
				
				using (Context input_cr = new Context (dock_icon_buffer)) {
					input_cr.AlphaFill ();
					DrawIcons (input_cr);
				}
				
				cr.SetSource (dock_icon_buffer, 0, IconSize * (1-DockIconOpacity));
				cr.PaintWithAlpha (DockIconOpacity);
			}
		}
		
		void DrawIcons (Context cr)
		{
			for (int i=0; i<DockItems.Count; i++) {
				int center;
				double zoom;
				
				IconPositionedCenterX (i, out center, out zoom);
				
				double insertion_ms = (DateTime.UtcNow - DockItems[i].DockAddItem).TotalMilliseconds;
				if (insertion_ms < InsertAnimationTime) {
					zoom *= insertion_ms/InsertAnimationTime;
				}
				
				double x = (1/zoom)*(center - zoom*IconSize/2);
				double y = (1/zoom)*(Height-(zoom*IconSize)) + IconBorderWidth/2 - YBuffer;
				
				int total_ms = (int) (DateTime.UtcNow - DockItems[i].LastClick).TotalMilliseconds;
				if (total_ms < BounceTime) {
					y -= Math.Abs (20*Math.Sin (total_ms*Math.PI/(BounceTime/2)));
				}
				
				double scale = zoom/DockItem.IconQuality;
				if (DockItems[i].Scalable) {
					cr.Scale (scale, scale);
					cr.SetSource (DockItems[i].GetIconSurface (), x*DockItem.IconQuality, y*DockItem.IconQuality);
					cr.Paint ();
					cr.Scale (1/scale, 1/scale);
				} else {
					cr.SetSource (DockItems[i].GetIconSurface (), x*zoom, Height-DockItems[i].Height-(MinimumDockArea.Height-DockItems[i].Height)/2);
					cr.Paint ();
				}
				
				if (DockItemForX (Cursor.X) == i && CursorIsOverDockArea && DockItems[i].GetTextSurface () != null) {
					cr.SetSource (DockItems[i].GetTextSurface (), IconNormalCenterX (i)-(DockItem.TextWidth/2), 15);
					cr.Paint ();
				}
			}
		}
		
		#region Input Area drawing code
		void DrawInputArea (Context cr)
		{
			DrawPane (cr, Pane.First);
			
			DrawPane (cr, Pane.Second);
			
			if (ThirdPaneVisible) {
				DrawPane (cr, Pane.Third);
			}
		}
		
		void DrawPane (Context cr, Pane pane)
		{
			int start_x = GetXForPane (pane);
			
			cr.SetRoundedRectanglePath (start_x, Height - 162, PaneSize - 10, PaneSize - 10, 20);
			if (pane == CurrentPane)
				cr.Color = new Cairo.Color (.15, .15, .15, .8);
			else
				cr.Color = new Cairo.Color (0, 0, 0, .4);
			cr.FillPreserve ();
			
			if (pane == State.CurrentPane)
				cr.Color = new Cairo.Color (1, 1, 1, .8);
			else
				cr.Color = new Cairo.Color (1, 1, 1, .3);
			cr.Stroke ();
			
			if (State.GetPaneResults (pane) == null)
				return;
			
			double results_slide_state = GetResultSlidePercentage (pane);
			
			int num_icons = GapSize/IconSize+1;
			int cursor = State.GetPaneCursor (pane);
			if (State.GetPanePreviousCursor (pane) > State.GetPaneCursor (pane)) {
			    results_slide_state = 1-results_slide_state;
				cursor++;
				num_icons--;
			}
			
			double gap_offset;
			if (State.CurrentPane != pane && State.PreviousPane != pane)
				gap_offset = 0;
			else if (State.CurrentPane == pane)
				gap_offset = Math.Min (1, (DateTime.UtcNow-State.CurrentPaneTime).TotalMilliseconds/BaseAnimationTime);
			else
				gap_offset = 1-Math.Min (1, (DateTime.UtcNow-State.CurrentPaneTime).TotalMilliseconds/BaseAnimationTime);
			
			int animation_start_x = start_x+15+PaneSize+IconSize/2;
			int pane_center = start_x+30+IconSize;
			double cursor_center_x = animation_start_x-IconSize*results_slide_state;
			//fixme the extra plus 10 in here is due to a calculation error.
			cr.Rectangle (start_x+10, 0, PaneSize+GapSize*gap_offset, Height);
			cr.Clip ();
			
			for (int i=cursor-1; i<State.GetPaneResults (pane).Count && i < cursor+num_icons; i++) {
				if (i<0)
					continue;
				double base_center_x = cursor_center_x + (i-cursor)*IconSize;
				if (base_center_x < 0)
					continue;
				double icon_size;
				if (base_center_x < animation_start_x) {
					base_center_x -= IconSize*results_slide_state + (IconSize)*(cursor-i);
					icon_size = Math.Min (2, 1+((animation_start_x-base_center_x)/(animation_start_x-(double)pane_center)));
				} else {
					icon_size=1;
				}
					
				IObject item = State.GetPaneResults (pane)[i];
				
				double scale;
				Surface sr;
				if (icon_size < 1.5) {
					if (!SmallIconCache.ContainsKey (item.Icon))
						SmallIconCache.AddPixbufSurface (item.Icon, item.Icon);
					scale = icon_size;
					sr = SmallIconCache.GetSurface (item.Icon);
				} else {
					if (!LargeIconCache.ContainsKey (item.Icon))
						LargeIconCache.AddPixbufSurface (item.Icon, item.Icon);
					scale = icon_size/2;
					sr = LargeIconCache.GetSurface (item.Icon);
				}
					
				cr.Scale (scale, scale);
				cr.SetSource (sr, (base_center_x-(IconSize*icon_size)/2)/scale, (Height-IconSize*icon_size-YBuffer)/scale);
				cr.Paint ();
				
				cr.Scale (1/scale, 1/scale);
			}
			cr.ResetClip ();
			
			string text = GLib.Markup.EscapeText (State.GetPaneItem (pane).Name);
			text = Do.Addins.Util.FormatCommonSubstrings (text, State.GetPaneQuery (pane), HighlightFormat);
			BezelTextUtils.RenderLayoutText (cr, text, start_x + 5, Height-PaneSize+12, PaneSize-20, this);
		}
		
		int GetXForPane (Pane pane)
		{
			Gdk.Rectangle dock_area = GetDockArea ();
			
			double transit_state = InputAreaSlideStatus;
			if (transit_state == 1 || pane == Pane.First) {
				switch (pane) {
				case Pane.First:
					return dock_area.X + 10;
				case Pane.Second:
					if (CurrentPane == Pane.First)
						return dock_area.X + 10 + PaneSize + GapSize;
					return dock_area.X + 10 + PaneSize;
				default:
					if (CurrentPane == Pane.Third)
						return dock_area.X + 10 + 2*PaneSize;
					return dock_area.X + 10 + GapSize + 2*PaneSize;
				}
			}
			
			//It should be impossible to get to this point with a Pane.First
			if (pane == Pane.First)
				throw new Exception ("You have beat the programmer");
			
			if (pane == Pane.Second) {
				if (State.PreviousPane == Pane.First) {
					return dock_area.X + 10 + PaneSize + (int) (GapSize*(1-transit_state));
				} else if (State.CurrentPane == Pane.First) {
					return dock_area.X + 10 + PaneSize + (int) (GapSize*transit_state);
				} else {
					return dock_area.X + 10 + PaneSize;
				}
			} else {
				if (State.CurrentPane == Pane.Third) {
					return dock_area.X + 10 + (int) (GapSize*(1-transit_state)) + 2*PaneSize;
				} else if (State.PreviousPane == Pane.Third) {
					return dock_area.X + 10 + (int) (GapSize*transit_state) + 2*PaneSize;
				} else {
					return dock_area.X + 10 + GapSize + 2*PaneSize;
				}
			}
			
		}
		
		DrawState PaneDrawState (Pane pane)
		{
			if (pane == Pane.Third && !ThirdPaneVisible)
				return DrawState.None;
			
//			if (Context.GetPaneTextMode (pane))
//				return DrawState.Text;
			
			if (State[pane] != null)
				return DrawState.Normal;
			
			if (!string.IsNullOrEmpty (State.GetPaneQuery (pane))) {
				return DrawState.NoResult;
			}
			
			return DrawState.None;
		}
		
		double GetResultSlidePercentage (Pane pane)
		{
			if (State.GetPaneCursor (pane) == State.GetPanePreviousCursor (pane))
				return 1;
			return Math.Min ((DateTime.UtcNow - State.GetPaneCursorTime (pane)).TotalMilliseconds/BaseAnimationTime,1);
		}
		#endregion
		
		int IconNormalCenterX (int icon)
		{
			int start_x = MinimumDockArea.X + XBuffer + (IconSize/2);
			for (int i=0; i<icon; i++)
				start_x += DockItems[i].Width;
			return start_x;
		}
		
		int DockItemForX (int x)
		{
			int start_x = MinimumDockArea.X + XBuffer;
			for (int i=0; i<DockItems.Count; i++) {
				if (x >= start_x && x < start_x+DockItems[i].Width)
					return i;
				start_x += DockItems[i].Width;
			}
			return 0;
		}
		
		void IconPositionedCenterX (int icon, out int x, out double zoom)
		{
			int center = IconNormalCenterX (icon);
			int offset = Math.Min (Math.Abs (Cursor.X - center), ZoomPixels/2);
			
			if (ZoomPixels/2 == 0)
				zoom = 1;
			else {
				zoom = 2 - (offset/(double)(ZoomPixels/2));
				zoom = (zoom-1)*ZoomIn+1;
			}
			
			offset = (int) (offset*Math.Sin ((Math.PI/4)*zoom));
			
			if (Cursor.X > center) {
				center -= offset;
			} else {
				center += offset;
			}
			x = center;
		}
		
		Gdk.Rectangle GetDockArea ()
		{
			if (!CursorIsOverDockArea && ZoomIn == 0)
				return MinimumDockArea;

			int start_x, end_x;
			double start_zoom, end_zoom;
			IconPositionedCenterX (0, out start_x, out start_zoom);
			IconPositionedCenterX (DockItems.Count - 1, out end_x, out end_zoom);
			
			int x = start_x - (int)(start_zoom*(IconSize/2)) - XBuffer;
			int end = end_x + (int)(end_zoom*(IconSize/2));
			
			return new Gdk.Rectangle (x, Height-IconSize-2*YBuffer, end-x, IconSize+2*YBuffer);
		}
		
		protected override bool OnExposeEvent(EventExpose evnt)
		{
			bool ret_val = base.OnExposeEvent (evnt);
			if (!IsDrawable)
				return ret_val;
			last_render = DateTime.UtcNow;
			
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
//			cr2.AlphaFill ();
			cr2.SetSource (backbuffer, 0, YOffset);
			cr2.Operator = Operator.Source;
			cr2.Paint ();
			(cr2 as IDisposable).Dispose ();
			
			return ret_val;
		}
 
		protected override bool OnMotionNotifyEvent(EventMotion evnt)
		{
			bool tmp = CursorIsOverDockArea;
			Cursor = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			
			if (tmp != CursorIsOverDockArea || CursorIsOverDockArea && DateTime.UtcNow.Subtract (last_render).TotalMilliseconds > 20) 
				AnimatedDraw ();
			return base.OnMotionNotifyEvent (evnt);
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			int item = DockItemForX ((int) evnt.X);
			if (item < 0 || item >= DockItems.Count)
				return base.OnButtonReleaseEvent (evnt);
			if ((DateTime.UtcNow - DockItems[item].LastClick).TotalMilliseconds > BounceTime) {
				last_click = DockItems[item].LastClick = DateTime.UtcNow;
				if (DockItems[item] is DockItem) {
					IItem doItem = (DockItems[item] as DockItem).IObject as IItem;
					if (doItem != null)
						window.Controller.PerformDefaultAction (doItem);
				}
				DockItems[item].Clicked ();
				AnimatedDraw ();
			}
			return base.OnButtonReleaseEvent (evnt);
		}

		protected override bool OnLeaveNotifyEvent (Gdk.EventCrossing evnt)
		{
			Cursor = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			return base.OnLeaveNotifyEvent (evnt);
		}
		
		void UpdateIcons ()
		{
			List<IDockItem> new_items = new List<IDockItem> ();
			foreach (IItem i in Statistics.GetMostUsedItems (10)) {
				IDockItem di = new DockItem (i);
				new_items.Add (di);
				
				bool is_set = false;
				foreach (IDockItem item in DockItems) {
					if (item.Equals (di)) {
						di.DockAddItem = item.DockAddItem;
						is_set = true;
						break;
					}
				}
				if (!is_set)
					di.DockAddItem = DateTime.UtcNow;
			}
			foreach (IDockItem dock_item in DockItems)
				dock_item.Dispose ();
			
			DockItems = new_items;
			AnimatedDraw ();
		}
		
		void UpdateWindowItems ()
		{
			foreach (IDockItem item in window_items)
				item.Dispose ();
			
			window_items.Clear ();
			
			foreach (Wnck.Application app in WindowUtils.GetApplications ()) {
				bool good = false;
				foreach (Wnck.Window w in app.Windows) {
					if (!w.IsSkipTasklist && w.IsInViewport (Wnck.Screen.Default.ActiveWorkspace))
						good = true;
				}
				if (good)
					window_items.Add (new ApplicationDockItem (app));
			}
			
			foreach (IDockItem item in window_items)
				item.DockAddItem = DateTime.UtcNow;
			
			Gdk.Rectangle geo, main;
			geo = Gdk.Screen.Default.GetMonitorGeometry (0);
			window.GetSize (out main.Width, out main.Height);
			window.GetPosition (out main.X, out main.Y);
			
			foreach (ApplicationDockItem di in window_items) {
				foreach (Wnck.Window w in di.App.Windows) {
					w.SetIconGeometry (geo.X + main.X + IconNormalCenterX (DockItems.IndexOf (di))-IconSize/2, 
					                   geo.Y + main.Y + (Height-DockHeight),
					                   IconSize, IconSize);
				}
			}
			
			AnimatedDraw ();
		}
		
		public void SetIcons (IEnumerable<IDockItem> items)
		{
			foreach (IDockItem ditem in DockItems)
				ditem.Dispose ();
			
			foreach (IDockItem i in items) {
				i.DockAddItem = DateTime.UtcNow;
			}
			DockItems = new List<IDockItem> (items);
			AnimatedDraw ();
		}
		
		public void SetPaneContext (IUIContext context, Pane pane)
		{
			State[pane] = context.Selection;
			State.SetPaneQuery (context.Query, pane);
			State.SetPaneResults (context.Results, pane);
			State.SetPaneCursor (context.Cursor, pane);
			AnimatedDraw ();
		}
		
		DateTime interface_change_time = DateTime.UtcNow;
		bool input_interface = false;
		public void ShowInputInterface ()
		{
			interface_change_time = DateTime.UtcNow;
			input_interface = true;
			
			AnimatedDraw ();
		}
		
		uint update_timer = 0;
		public void HideInputInterface ()
		{
			interface_change_time = DateTime.UtcNow;
			input_interface = false;
		
			if (update_timer != 0)
				return;
			
			update_timer = GLib.Timeout.Add (1000, delegate {
				UpdateIcons ();
				update_timer = 0;
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
	}
}
