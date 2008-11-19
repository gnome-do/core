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
		
		enum IconSource {
			Statistics,
			Custom,
			Application,
			Unknown,
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
		
		bool third_pane_visible = false;
		DateTime third_pane_visibility_change = DateTime.UtcNow;
		public bool ThirdPaneVisible { 
			get { return third_pane_visible; }
			set { 
				if (third_pane_visible == value)
					return;
				third_pane_visible = value;
				third_pane_visibility_change = DateTime.UtcNow;
				AnimatedDraw ();
			}
		}
		#endregion
		
		IStatistics Statistics { get; set; }
		
		IList<IDockItem> DockItems {
			get {
				List<IDockItem> out_items = new List<IDockItem> (dock_items);
				
				if (CustomDockItems.DockItems.Any ()) {
					out_items.Add (Separator);
					out_items.AddRange (CustomDockItems.DockItems);
				}
				
				if (window_items.Any ()) {
					out_items.Add (Separator);
					out_items.AddRange (window_items);
				}
				return out_items.ToArray ();
			}
		}
		
		SeparatorItem separator;
		SeparatorItem Separator {
			get {
				return separator ?? separator = new SeparatorItem ();
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
		
		int IconBorderWidth {get{ return 4; }}
		
		int IconSize { get { return DockItem.IconSize + IconBorderWidth; } }
		
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
		
		Gdk.Point StickIconCenter {
			get {
				Gdk.Rectangle rect = GetDockArea ();
				return new Gdk.Point (rect.X+rect.Width-7, rect.Y+8);
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
		
		bool ThirdPaneVisibilityAnimationNeeded {
			get {
				return (DateTime.UtcNow - third_pane_visibility_change).TotalMilliseconds < BaseAnimationTime;
			}
		}
		
		bool AnimationNeeded {
			get { return PaneChangeAnimationNeeded || ZoomAnimationNeeded || BounceAnimationNeeded || 
				InputModeChangeAnimationNeeded || InputModeSlideAnimationNeeded || IconInsertionAnimationNeeded || 
				OpenAnimationNeeded || ThirdPaneVisibilityAnimationNeeded; }
		}
		#endregion
		
		public DockArea(DockWindow window, IStatistics statistics) : base ()
		{
			Statistics = statistics;
			this.window = window;
			dock_items = new List<IDockItem> ();
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
			
			RegisterEvents ();
		}
		
		void RegisterEvents ()
		{
			Realized += delegate {
				GdkWindow.SetBackPixmap (null, false);
			};
			
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
			
			ItemMenu.Instance.RemoveClicked += delegate (Gdk.Point point) {
				int item = DockItemForX (point.X);
				if (GetIconSource (DockItems[item]) == IconSource.Custom)
					CustomDockItems.RemoveApplication (DockItems[item]);
				AnimatedDraw ();
			};
			
			ItemMenu.Instance.Hidden += delegate {
				int x, y;
				Display.GetPointer (out x, out y);
				
				Gdk.Rectangle geo;
				window.GetPosition (out geo.X, out geo.Y);
				
				x -= geo.X;
				y -= geo.Y;
				
				Cursor = new Gdk.Point (x, y);
				AnimatedDraw ();
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
			cr.SetRoundedRectanglePath (dock_area.X+.5, dock_area.Y+.5, dock_area.Width-1, dock_area.Height+40, 5); //fall off the bottom
			cr.Color = new Cairo.Color (0.1, 0.1, 0.1, .75);
			cr.FillPreserve ();
			
			//gives the dock a "lifted" look and feel
			cr.Color = new Cairo.Color (0, 0, 0, .6);
			cr.LineWidth = 1;
			cr.Stroke ();
			
			cr.SetRoundedRectanglePath (dock_area.X+1.5, dock_area.Y+1.5, dock_area.Width-3, dock_area.Height+40, 5);
			LinearGradient lg = new LinearGradient (0, dock_area.Y+1.5, 0, dock_area.Y+10);
			lg.AddColorStop (0, new Cairo.Color (1, 1, 1, .4));
			lg.AddColorStop (1, new Cairo.Color (1, 1, 1, 0));
			cr.Pattern = lg;
			cr.LineWidth = 1;
			cr.Stroke ();
			
			lg.Destroy ();
			
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
					
					if (CursorIsOverDockArea)
						DrawThumbnailIcon (input_cr);
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
				
				if (GetIconSource (DockItems[i]) == IconSource.Application) {
					cr.MoveTo (center, Height - 6);
					cr.LineTo (center+4, Height);
					cr.LineTo (center-4, Height);
					cr.ClosePath ();
					
					cr.Color = new Cairo.Color (1, 1, 1, .7);
					cr.Fill ();
				}
				
				if (DockItemForX (Cursor.X) == i && CursorIsOverDockArea && DockItems[i].GetTextSurface () != null) {
					cr.SetSource (DockItems[i].GetTextSurface (), IconNormalCenterX (i)-(DockItem.TextWidth/2), 15);
					cr.Paint ();
				}
			}
		}
		
		void DrawThumbnailIcon (Context cr)
		{
			Gdk.Point center = StickIconCenter;
			
			double opacity = 1.0/Math.Abs (center.X-Cursor.X)*30 - .2;
			
			cr.Arc (center.X, center.Y, 3.5, 0, Math.PI*2);
			cr.LineWidth = 1;
			cr.Color = new Cairo.Color (1, 1, 1, opacity);
			cr.Stroke ();
			
			if (autohide) {
				cr.Arc (center.X, center.Y, 1.5, 0, Math.PI*2);
				cr.Color = new Cairo.Color (1, 1, 1, opacity);
				cr.Fill ();
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
			if (State[pane] == null || State.GetPaneResults (pane) == null)
				return;
			
			int center = (int) GetXForPane (pane);
			IObject item = State[pane];
			int cursor = State.GetPaneCursor (pane);
			IObject[] results = State.GetPaneResults (pane).ToArray ();
			double alpha = (pane == CurrentPane) ? 1 : .7;
			double slide_state = Math.Min (1,(DateTime.UtcNow - State.GetPaneCursorTime (pane)).TotalMilliseconds / BaseAnimationTime); 
			int slide_offset; 
			if (State.GetPaneCursor (pane) > State.GetPanePreviousCursor (pane))  { 
				//we moved right, the animation should slide the right hand object to the center.
				//the cursor object therefor needs to be offset to the right
				slide_offset = (int) (100*(1-slide_state));
			} else {
				slide_offset = (int) (-100*(1-slide_state));
			}
			
			cr.Rectangle (center-150, 0, 300, Height);
			cr.Clip ();
			
			for (int i=Math.Max (0, cursor-2); i<= cursor+2 && i<results.Length; i++) {
				int offset = (i-cursor)*100+slide_offset;
				if (!LargeIconCache.ContainsKey (results[i].Icon))
					LargeIconCache.AddPixbufSurface (results[i].Icon, results[i].Icon);
				
				double zoom = 1 - Math.Min (.5, Math.Min (1,((Math.Abs (offset)/100.0) * .5)));
				cr.Scale (zoom, zoom);
				cr.SetSource (LargeIconCache.GetSurface (results[i].Icon), (1/zoom)*((center+offset)-(64*zoom)), (1/zoom)*(Height-YBuffer/2-128*zoom));
				cr.PaintWithAlpha (alpha);
				cr.Scale (1/zoom, 1/zoom);
			}
			
			cr.Rectangle (center-150, 0, 300, Height);
			LinearGradient lg = new LinearGradient (center-150, 0, center+150, 0);
			lg.AddColorStop (0, new Cairo.Color (0, 0, 0, 1));
			lg.AddColorStop (.1, new Cairo.Color (0, 0, 0, 0));
			lg.AddColorStop (.9, new Cairo.Color (0, 0, 0, 0));
			lg.AddColorStop (1, new Cairo.Color (0, 0, 0, 1));
			cr.Operator = Operator.DestOut;
			cr.Pattern = lg;
			cr.Fill ();
			cr.Operator = Operator.Over;
			
			lg.Destroy ();
			
			cr.ResetClip ();
			
			string text = GLib.Markup.EscapeText (item.Name);
			text = Do.Addins.Util.FormatCommonSubstrings (text, State.GetPaneQuery (pane), HighlightFormat);
			Surface text_surface = UI.Util.GetBorderedTextSurface (text, 300);
			cr.SetSource (text_surface, center-150, Height-128-30);
			cr.Paint ();
		}
		
		double GetXForPane (Pane pane)
		{
			double position;
			double slide_state = Math.Min (1,(DateTime.UtcNow - third_pane_visibility_change).TotalMilliseconds/BaseAnimationTime);
			switch (pane) {
			case Pane.First:
				if (ThirdPaneVisible)
					position = 1+(1-slide_state);
				else
					position = 1+slide_state;
				break;
			case Pane.Second:
				if (ThirdPaneVisible)
					position = 3+(1-slide_state);
				else
					position = 3+slide_state;
			break;
			default:
				if (ThirdPaneVisible)
					position = 5 + (5*(1-slide_state));
				else
					position = 5 + 5*slide_state;
				break;
			}
			Gdk.Rectangle dock_area = GetDockArea ();
			return dock_area.X + position * dock_area.Width/6.0;
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
			return -1;
		}
		
		IconSource GetIconSource (IDockItem item) {
			if (window_items.Contains (item))
				return IconSource.Application;
			
			if (dock_items.Contains (item))
				return IconSource.Statistics;
			
			if (CustomDockItems.DockItems.Contains (item))
				return IconSource.Custom;
			
			return IconSource.Unknown;
		}
		
		double zoom_percent = 2;
		void IconPositionedCenterX (int icon, out int x, out double zoom)
		{
			int center = IconNormalCenterX (icon);
			int offset = Math.Min (Math.Abs (Cursor.X - center), ZoomPixels/2);
			
			if (ZoomPixels/2 == 0)
				zoom = 1;
			else {
				zoom = zoom_percent - (offset/(double)(ZoomPixels/2))*(zoom_percent-1);
				zoom = (zoom-1)*ZoomIn+1;
			}
			
			offset = (int) ((offset*Math.Sin ((Math.PI/4)*zoom)) * (zoom_percent-1));
			
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
		
		#region Drag To Code
		protected override bool OnDragMotion (Gdk.DragContext context, int x, int y, uint time_)
		{
			Cursor = new Gdk.Point (x, y);
			AnimatedDraw ();
			return base.OnDragMotion (context, x, y, time_);
		}

		protected override void OnDragDataReceived (Gdk.DragContext context, int x, int y, Gtk.SelectionData selection_data, uint info, uint time_)
		{
			string data = System.Text.Encoding.UTF8.GetString ( selection_data.Data );
			data = data.TrimEnd ('\0'); //sometimes we get a null at the end, and it crashes us
			
			string[] uriList = Regex.Split (data, "\r\n");
			foreach (string uri in uriList) {
				if (uri.EndsWith (".desktop")) {
					AddCustomItem (uri.Substring (7));
				}
			} 
			
			base.OnDragDataReceived (context, x, y, selection_data, info, time_);
		}
		#endregion
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
			bool ret_val = base.OnButtonPressEvent (evnt);
			Gdk.Rectangle stick_rect = new Gdk.Rectangle (StickIconCenter.X-4, StickIconCenter.Y-4, 8, 8);
			if (stick_rect.Contains (Cursor)) {
				autohide = !autohide;
				window.SetStruts ();
				AnimatedDraw ();
				return ret_val;
			}
			
			int item = DockItemForX ((int) evnt.X);
			if (item < 0 || item >= DockItems.Count || !CursorIsOverDockArea || input_interface)
				return ret_val;
			if (evnt.Button == 3) {
				if (GetIconSource (DockItems[item]) == IconSource.Custom)
					ItemMenu.Instance.PopupAtPosition ((int) evnt.XRoot, (int) evnt.YRoot);
				return ret_val;
			}
			
			if ((DateTime.UtcNow - DockItems[item].LastClick).TotalMilliseconds > BounceTime) {
				last_click = DockItems[item].LastClick = DateTime.UtcNow;
				if (DockItems[item] is DockItem) {
					IItem doItem = (DockItems[item] as DockItem).IObject as IItem;
					if (doItem != null)
						window.Controller.PerformDefaultAction (doItem);
				}
				DockItems[item].Clicked (evnt.Button);
				AnimatedDraw ();
			}
			return ret_val;
		}

		protected override bool OnLeaveNotifyEvent (Gdk.EventCrossing evnt)
		{
			Cursor = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			return base.OnLeaveNotifyEvent (evnt);
		}
		
		void AddCustomItem (string desktopFile)
		{
			CustomDockItems.AddApplication (desktopFile);
			AnimatedDraw ();
		}
		
		void UpdateIcons ()
		{
			List<IDockItem> new_items = new List<IDockItem> ();
			foreach (IItem i in Statistics.GetMostUsedItems (10)) {
				IDockItem di = new DockItem (i);
				new_items.Add (di);
				
				bool is_set = false;
				foreach (IDockItem item in dock_items) {
					if (item.Equals (di)) {
						di.DockAddItem = item.DockAddItem;
						is_set = true;
						break;
					}
				}
				if (!is_set)
					di.DockAddItem = DateTime.UtcNow;
			}
			foreach (IDockItem dock_item in dock_items)
				dock_item.Dispose ();
			
			dock_items = new_items;
			AnimatedDraw ();
		}
		
		void UpdateWindowItems ()
		{
			if (Wnck.Screen.Default.ActiveWorkspace == null)
				return;
			IList<IDockItem> out_items = new List<IDockItem> ();
			
			foreach (Wnck.Application app in WindowUtils.GetApplications ()) {
				bool good = false;
				foreach (Wnck.Window w in app.Windows) {
					if (!w.IsSkipTasklist && w.IsInViewport (Wnck.Screen.Default.ActiveWorkspace))
						good = true;
				}
				if (good) {
					ApplicationDockItem api = new ApplicationDockItem (app);
					bool is_set = false;
					foreach (ApplicationDockItem di in window_items) {
						if (api.Equals (di)) {
							api.DockAddItem = di.DockAddItem;
							is_set = true;
							break;
						}
					}
					if (!is_set)
						api.DockAddItem = DateTime.UtcNow;
					out_items.Add (new ApplicationDockItem (app));
				}
			}
			
			foreach (IDockItem item in window_items)
				item.Dispose ();
					
			window_items = out_items;
			
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
			foreach (IDockItem ditem in dock_items)
				ditem.Dispose ();
			
			foreach (IDockItem i in items) {
				i.DockAddItem = DateTime.UtcNow;
			}
			dock_items = new List<IDockItem> (items);
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
