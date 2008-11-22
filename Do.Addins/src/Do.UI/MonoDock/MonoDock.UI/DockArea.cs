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
		const int YBuffer = 5;
		const int XBuffer = 7;
		
		IList<IDockItem> dock_items;
		IList<IDockItem> window_items;
		Gdk.Point cursor;
		DateTime enter_time = DateTime.UtcNow;
		DateTime last_render = DateTime.UtcNow;
		int monitor_width;
		
		DockState state;
		Surface backbuffer, input_area_buffer, dock_icon_buffer;
		DockWindow window;
		PixbufSurfaceCache large_icon_cache;
		
		#region Public properties
		public int Width {
			get {
				return monitor_width;
			}
		}
		
		public int Height {
			get; set;
		}
		
		public int DockWidth {
			get {
				int out_width = 2*XBuffer;
				foreach (IDockItem di in DockItems) {
					out_width += di.Width + 2*IconBorderWidth; //each icon has a left and right border
				}
				return out_width;
			}
		}
		
		public int DockHeight {
			get {
				if (Preferences.AutoHide)
					return 0;
				return MinimumDockArea.Height;
			}
		}
		
		public int IconBorderWidth {
			get {
				return 2;
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
				if (!Preferences.AutoHide || cursor_is_handle)
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
				return "<span foreground=\"#5599ff\">{0}</span>";
			} 
		}
		
		double InputAreaOpacity {
			get {
				return 1-DockIconOpacity;
			}
		}
		
		int IconSize { get { return Preferences.IconSize; } }
		
		Gdk.Point Cursor {
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
				return large_icon_cache ?? large_icon_cache = new PixbufSurfaceCache (10, 2*IconSize, 2*IconSize);
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
			Height = 300;
			Statistics = statistics;
			this.window = window;
			dock_items = new List<IDockItem> ();
			window_items = new List<IDockItem> ();
			
			GLib.Timeout.Add (3000, delegate {
				UpdateIcons ();
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
			
			GLib.Timeout.Add (20, delegate {
				SetParentInputMask ();
				return false;
			});
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
			
			Wnck.Screen.Default.WindowOpened += delegate (object o, Wnck.WindowOpenedArgs args) {
				if (!args.Window.IsSkipTasklist)
					UpdateWindowItems ();
			};
			
			Wnck.Screen.Default.WindowClosed += delegate(object o, Wnck.WindowClosedArgs args) {
				if (!args.Window.IsSkipTasklist)
					UpdateWindowItems ();
			};
			
			ItemMenu.Instance.RemoveClicked += delegate (Gdk.Point point) {
				int item = DockItemForX (point.X);
				if (GetIconSource (DockItems[item]) == IconSource.Custom) {
					CustomDockItems.RemoveItem (DockItems[item]);
				} else if (GetIconSource (DockItems[item]) == IconSource.Statistics) {
					DockItem di = DockItems[item] as DockItem;
					if (di != null)
						Preferences.AddBlacklistItem (di.IObject.Name + di.IObject.Description + di.IObject.Icon);
					UpdateIcons ();
				}
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
			
			Preferences.IconSizeChanged += delegate {
				if (large_icon_cache != null) {
					large_icon_cache.Dispose ();
					large_icon_cache = null;
				}
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
				double y = (1/zoom)*(Height-(zoom*IconSize)) - YBuffer;
				
				int total_ms = (int) (DateTime.UtcNow - DockItems[i].LastClick).TotalMilliseconds;
				if (total_ms < BounceTime) {
					y -= Math.Abs (20*Math.Sin (total_ms*Math.PI/(BounceTime/2)));
				}
				
				double scale = zoom/Preferences.IconQuality;
				if (DockItems[i].Scalable) {
					cr.Scale (scale, scale);
					cr.SetSource (DockItems[i].GetIconSurface (), x*Preferences.IconQuality, y*Preferences.IconQuality);
					cr.Paint ();
					cr.Scale (1/scale, 1/scale);
				} else {
					cr.SetSource (DockItems[i].GetIconSurface (), x*zoom, Height-DockItems[i].Height-(MinimumDockArea.Height-DockItems[i].Height)/2);
					cr.Paint ();
				}
				
				if (DockItems[i].DrawIndicator) {
					cr.MoveTo (center, Height - 6);
					cr.LineTo (center+4, Height);
					cr.LineTo (center-4, Height);
					cr.ClosePath ();
					
					cr.Color = new Cairo.Color (1, 1, 1, .7);
					cr.Fill ();
				}
				
				if (DockItemForX (Cursor.X) == i && CursorIsOverDockArea && DockItems[i].GetTextSurface () != null) {
					cr.SetSource (DockItems[i].GetTextSurface (), IconNormalCenterX (i)-(Preferences.TextWidth/2), Height-2*IconSize-28);
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
			
			if (!Preferences.AutoHide) {
				cr.Arc (center.X, center.Y, 1.5, 0, Math.PI*2);
				cr.Color = new Cairo.Color (1, 1, 1, opacity);
				cr.Fill ();
			}
		}
		
		#region Input Area drawing code
		void DrawInputArea (Context cr)
		{
			DrawPanes (cr);
		}
		
		void DrawPanes (Context cr)
		{
			int base_x = GetDockArea ().X + 15;
			
			for (int i=0; i<3; i++) {
				Pane pane  = (Pane)i;
				int left_x;
				double zoom;
				GetXForPane (pane, out left_x, out zoom);
				
				if (State[pane] == null || (pane == Pane.Third && !ThirdPaneVisible))
					continue;
				if (!LargeIconCache.ContainsKey (State[pane].Icon))
				    LargeIconCache.AddPixbufSurface (State[pane].Icon, State[pane].Icon);
				
				cr.Scale (zoom, zoom);
				cr.SetSource (LargeIconCache.GetSurface (State[pane].Icon), left_x*(1/zoom), (Height-IconSize*2*zoom-YBuffer)*(1/zoom));
				cr.Paint ();
				cr.Scale (1/zoom, 1/zoom);
			}
			
			if (State[CurrentPane] == null)
				return;
			
			string text = GLib.Markup.EscapeText (State[CurrentPane].Name);
			text = Do.Addins.Util.FormatCommonSubstrings (text, State.GetPaneQuery (CurrentPane), HighlightFormat);
			
			int tmp = BezelTextUtils.TextHeight;
			double text_scale = (IconSize/64.0);
			int text_offset = (int) (IconSize*3);
			
			if ((int) (12*text_scale) > 8)
				BezelTextUtils.TextHeight = (int) (20 * text_scale);
			else
				BezelTextUtils.TextHeight = (int) (35 * text_scale);
				
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			
			BezelTextUtils.RenderLayoutText (cr, text, base_x + text_offset, 
			                                 Height - MinimumDockArea.Height + (int) (15*text_scale), (int) (500*text_scale), 
			                                 color, Pango.Alignment.Left, Pango.EllipsizeMode.End, this);
			if ((int) (12*text_scale) > 8) {
				BezelTextUtils.TextHeight = (int) (12*text_scale);
				BezelTextUtils.RenderLayoutText (cr, GLib.Markup.EscapeText (State[CurrentPane].Description), 
				                                 base_x + text_offset, Height - MinimumDockArea.Height + (int) (42*text_scale), 
				                                 (int) (500*text_scale), color, Pango.Alignment.Left, Pango.EllipsizeMode.End, this);
			}
			BezelTextUtils.TextHeight = tmp;
		}
		
		void GetXForPane (Pane pane, out int left_x, out double zoom)
		{
			int base_x = GetDockArea ().X + 15;
			double zoom_value = .3;
			double slide_state = Math.Min (1,(DateTime.UtcNow - State.CurrentPaneTime).TotalMilliseconds/BaseAnimationTime);
			
			double growing_zoom = zoom_value + slide_state*(1-zoom_value);
			double shrinking_zoom = zoom_value + (1-slide_state)*(1-zoom_value);
			switch (pane) {
			case Pane.First:
				left_x = base_x;
				if (State.CurrentPane == Pane.First && (State.PreviousPane == Pane.Second || State.PreviousPane == Pane.Third)) {
					zoom = growing_zoom;
				} else if (State.PreviousPane == Pane.First && (State.CurrentPane == Pane.Second || State.CurrentPane == Pane.Third)) {
					zoom = shrinking_zoom;
				} else {
					zoom = zoom_value;
				}
				break;
			case Pane.Second:
				if (State.PreviousPane == Pane.Second && State.CurrentPane == Pane.First) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) ((IconSize*2) * (growing_zoom));
				} else if (State.PreviousPane == Pane.Second && State.CurrentPane == Pane.Third) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) (IconSize*2*zoom_value);
				} else if (State.PreviousPane == Pane.First && State.CurrentPane == Pane.Second) {
					zoom = growing_zoom;
					left_x = base_x + (int) ((IconSize*2) * (shrinking_zoom));
				} else if (State.PreviousPane == Pane.First && State.CurrentPane == Pane.Third) {
					zoom = zoom_value;
					left_x = base_x + (int) ((IconSize*2) * (shrinking_zoom));
				} else if (State.PreviousPane == Pane.Third && State.CurrentPane == Pane.First) {
					zoom = zoom_value;
					left_x = base_x + (int) ((IconSize*2) * (growing_zoom));
				} else {// (State.PreviousPane == Pane.Third && State.CurrentPane == Pane.Second) {
					zoom = growing_zoom;
					left_x = base_x + (int) (IconSize*2*zoom_value);
				}
				break;
			default:
				if (State.PreviousPane == Pane.Second && State.CurrentPane == Pane.First) {
					zoom = zoom_value;
					left_x = base_x + (int) (IconSize*2*(1+zoom_value));
				} else if (State.PreviousPane == Pane.Second && State.CurrentPane == Pane.Third) {
					zoom = growing_zoom;
					left_x = base_x + (int) (IconSize*2*zoom_value) + (int) ((IconSize*2) * (shrinking_zoom));
				} else if (State.PreviousPane == Pane.First && State.CurrentPane == Pane.Second) {
					zoom = zoom_value;
					left_x = base_x + (int) (IconSize*2*(1+zoom_value));
				} else if (State.PreviousPane == Pane.First && State.CurrentPane == Pane.Third) {
					zoom = growing_zoom;
					left_x = base_x + (int) (IconSize*2*zoom_value) + (int) ((IconSize*2) * (shrinking_zoom));
				} else if (State.PreviousPane == Pane.Third && State.CurrentPane == Pane.First) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) (IconSize*2*zoom_value) + (int) ((IconSize*2) * (growing_zoom));
				} else {// (State.PreviousPane == Pane.Third && State.CurrentPane == Pane.Second) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) (IconSize*2*zoom_value) + (int) ((IconSize*2) * (growing_zoom));
				}
				break;
			}
			double offset_scale = .9;
			left_x = (int) (left_x *offset_scale + base_x*(1-offset_scale));
		}
		#endregion
		
		int IconNormalCenterX (int icon)
		{
			//the first icons center is at dock X + border + IconBorder + half its width
			if (DockItems.Count == 0)
				return 0;
			int start_x = MinimumDockArea.X + XBuffer + IconBorderWidth + (DockItems[0].Width/2);
			for (int i=0; i<icon; i++)
				start_x += DockItems[i].Width + 2*IconBorderWidth;
			return start_x;
		}
		
		int DockItemForX (int x)
		{
			int start_x = MinimumDockArea.X + XBuffer;
			for (int i=0; i<DockItems.Count; i++) {
				if (x >= start_x && x <= start_x+DockItems[i].Width+2*IconBorderWidth)
					return i;
				start_x += DockItems[i].Width + 2*IconBorderWidth;
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
		
		void IconPositionedCenterX (int icon, out int x, out double zoom)
		{
			int center = IconNormalCenterX (icon);
			int offset = Math.Min (Math.Abs (Cursor.X - center), ZoomPixels/2);
			
			if (ZoomPixels/2 == 0)
				zoom = 1;
			else {
				zoom = Preferences.ZoomPercent - (offset/(double)(ZoomPixels/2))*(Preferences.ZoomPercent-1);
				zoom = (zoom-1)*ZoomIn+1;
			}
			
			offset = (int) ((offset*Math.Sin ((Math.PI/4)*zoom)) * (Preferences.ZoomPercent-1));
			
			if (Cursor.X > center) {
				center -= offset;
			} else {
				center += offset;
			}
			x = center;
		}
		
		Gdk.Rectangle GetDockArea ()
		{
			if (!CursorIsOverDockArea && ZoomIn == 0 && InputAreaOpacity == 0)
				return MinimumDockArea;

			int start_x, end_x;
			double start_zoom, end_zoom;
			IconPositionedCenterX (0, out start_x, out start_zoom);
			IconPositionedCenterX (DockItems.Count - 1, out end_x, out end_zoom);
			
			int x = start_x - (int)(start_zoom*(IconSize/2)) - XBuffer;
			int end = end_x + (int)(end_zoom*(IconSize/2)) + XBuffer;
			
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
				if (uri.StartsWith ("file://")) {
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
 
		bool cursor_is_handle = false;
		int drag_start_y = 0;
		int drag_start_icon_size = 0;
		protected override bool OnMotionNotifyEvent(EventMotion evnt)
		{
			bool tmp = CursorIsOverDockArea;
			Cursor = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			
			if (cursor_is_handle && !((evnt.State & ModifierType.Button1Mask) == ModifierType.Button1Mask))
				EndDrag ();
			
			if (Math.Abs (Cursor.Y - MinimumDockArea.Y) < 5 || (cursor_is_handle && (evnt.State & ModifierType.Button1Mask) == ModifierType.Button1Mask)) {
				int item = DockItemForX (Cursor.X);
				if (!cursor_is_handle && item > 0 && DockItems[item] is SeparatorItem) {
					GdkWindow.Cursor = new Gdk.Cursor (CursorType.TopSide);
					cursor_is_handle = true;
					drag_start_y = Cursor.Y;
					drag_start_icon_size = Preferences.IconSize;
				}
			}
			if (cursor_is_handle && (evnt.State & ModifierType.Button1Mask) == ModifierType.Button1Mask) {
				Preferences.IconSize = drag_start_icon_size + (drag_start_y - Cursor.Y);
			}
			
			if (tmp != CursorIsOverDockArea || CursorIsOverDockArea && DateTime.UtcNow.Subtract (last_render).TotalMilliseconds > 20) 
				AnimatedDraw ();
			return base.OnMotionNotifyEvent (evnt);
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			bool ret_val = base.OnButtonPressEvent (evnt);
			
			// lets not do anything in this case
			if (cursor_is_handle) {
				EndDrag ();
				return ret_val;
			}
			
			// we are hovering over the pin icon
			Gdk.Rectangle stick_rect = new Gdk.Rectangle (StickIconCenter.X-4, StickIconCenter.Y-4, 8, 8);
			if (stick_rect.Contains (Cursor)) {
				Preferences.AutoHide = !Preferences.AutoHide;
				window.SetStruts ();
				AnimatedDraw ();
				return ret_val;
			}
			
			int item = DockItemForX ((int) evnt.X); //sometimes clicking is not good!
			if (item < 0 || item >= DockItems.Count || !CursorIsOverDockArea || input_interface)
				return ret_val;
			
			//handling right clicks
			if (evnt.Button == 3) {
				if (GetIconSource (DockItems[item]) == IconSource.Custom || GetIconSource (DockItems[item]) == IconSource.Statistics)
					ItemMenu.Instance.PopupAtPosition ((int) evnt.XRoot, (int) evnt.YRoot);
				return ret_val;
			}
			
			//send off the clicks
			DockItems[item].Clicked (evnt.Button, window.Controller);
			if (DockItems[item].LastClick > last_click)
				last_click = DockItems[item].LastClick;
			AnimatedDraw ();
			
			return ret_val;
		}

		ModifierType leave_mask = ModifierType.Button1Mask | ModifierType.Button2Mask | 
				ModifierType.Button3Mask | ModifierType.Button4Mask | ModifierType.Button5Mask;
		protected override bool OnLeaveNotifyEvent (Gdk.EventCrossing evnt)
		{
			Cursor = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			
			if (CursorIsOverDockArea && (int) (evnt.State & leave_mask) == 0 && evnt.Mode == CrossingMode.Normal)
				Cursor = new Gdk.Point ((int) evnt.X, -1);
			return base.OnLeaveNotifyEvent (evnt);
		}
		
		void EndDrag ()
		{
			GdkWindow.Cursor = new Gdk.Cursor (CursorType.LeftPtr);
			cursor_is_handle = false;
		}
		
		void AddCustomItem (string file)
		{
			if (file.EndsWith (".desktop"))
			    CustomDockItems.AddApplication (file);
			else
				CustomDockItems.AddFile (file);
			AnimatedDraw ();
		}
		
		void SetParentInputMask ()
		{
			if (CursorIsOverDockArea) {
				window.SetInputMask (GetDockArea ().Height*2 + 10);
			} else {
				if (Preferences.AutoHide)
					window.SetInputMask (1);
				else
					window.SetInputMask (GetDockArea ().Height);
			}
		}
		
		void UpdateIcons ()
		{
			List<IDockItem> new_items = new List<IDockItem> ();
			foreach (IItem i in Statistics.GetMostUsedItems (Preferences.AutomaticIcons)) {
				if (Preferences.ItemBlacklist.Contains (i.Name + i.Description + i.Icon))
					continue;
				IDockItem di = new DockItem (i);
				if (CustomDockItems.DockItems.Contains (di))
					continue;
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
			UpdateWindowItems ();
			AnimatedDraw ();
		}
		
		void UpdateWindowItems ()
		{
			foreach (IDockItem di in dock_items.Concat (CustomDockItems.DockItems)) {
				if (!(di is DockItem))
					continue;
				(di as DockItem).UpdateApplication ();
			}
			
			if (Wnck.Screen.Default.ActiveWorkspace == null)
				return;
			IList<IDockItem> out_items = new List<IDockItem> ();
			
			foreach (Wnck.Application app in WindowUtils.GetApplications ()) {
				bool good = false;
				foreach (Wnck.Window w in app.Windows) {
					if (!w.IsSkipTasklist)
						good = true;
				}
				
				foreach (IDockItem di in dock_items.Concat (CustomDockItems.DockItems)) {
					if (!(di is DockItem) || (di as DockItem).Apps.Count () == 0)
						continue;
					if ((di as DockItem).Pids.Contains (app.Pid)) {
						good = false;
						break;
					}
				}
						
				if (!good) 
					continue;
				
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
			
			foreach (IDockItem item in window_items)
				item.Dispose ();
					
			window_items = out_items;
			
			Gdk.Rectangle geo, main;
			geo = Gdk.Screen.Default.GetMonitorGeometry (0);
			window.GetSize (out main.Width, out main.Height);
			window.GetPosition (out main.X, out main.Y);
			
			foreach (IDockItem idi in DockItems) {
				DockItem di = (idi as DockItem);
				if (di == null)
					continue;
				foreach (Wnck.Application app in di.Apps) {
					foreach (Wnck.Window w in app.Windows) {
						w.SetIconGeometry (geo.X + main.X + IconNormalCenterX (DockItems.IndexOf (di))-IconSize/2, 
						                   geo.Y + main.Y + (Height-DockHeight),
						                   IconSize, IconSize);
					}
				}
			}
			
			foreach (ApplicationDockItem di in window_items) {
				foreach (Wnck.Window w in di.App.Windows) {
					w.SetIconGeometry (geo.X + main.X + IconNormalCenterX (DockItems.IndexOf (di))-IconSize/2, 
					                   geo.Y + main.Y + (Height-DockHeight),
					                   IconSize, IconSize);
				}
			}
			
			AnimatedDraw ();
		}
		
		public void SetPaneContext (IUIContext context, Pane pane)
		{
			State.SetContext (context, pane);
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
