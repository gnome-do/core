// BezelResultsDrawingArea.cs
// 
// Copyright (C) 2008 GNOME-Do
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

using Cairo;
using Gdk;

using Do.Universe;
using Do.Platform.Linux;
using Do.Interface.CairoUtils;

namespace Do.Interface.AnimationBase
{
	
	
	public class BezelResultsDrawingArea : Gtk.DrawingArea
	{
		
		const int SurfaceHeight = 20;
		const int IconSize = 16;
		const int FadeTime = 100;
		
		int num_results;
		int width, height;
		Dictionary <Do.Universe.Item, Surface> surface_cache;
		Surface highlight_surface, backbuffer, child_inout_surface, triplebuffer;
		
		DateTime delta_time;
		double scroll_offset, highlight_offset, child_scroll_offset;
		int cursor, prev_cursor, delta;
		uint timer, delta_reset;
		
		Cairo.Color odd_color, even_color;
		
		Item[] results;
		
		
		public Item[] Results {
			get {
				return results;
			}
			set {
				if (results != null && value != null && results.GetHashCode () == value.GetHashCode ())
					return;
				results = value;
				scroll_offset = 0;
				highlight_offset = 0;
			}
		}
		
		public int Cursor {
			get {
				return cursor;
			}
			set {
				if (cursor == value)
					return;
				
				int oldStart = StartResult;
				prev_cursor = cursor;
				cursor = value;
				if (oldStart == StartResult) {
					highlight_offset -= value-prev_cursor;
				} else {
					scroll_offset += value-prev_cursor;
				}
				delta++;
			}
		}
		
		private int StartResult {
			get {
				int result = Math.Max (Cursor - (num_results / 2), 0);
				if (results == null)
					return 0;
				while (result+num_results > results.Length && result > 1)
					result--;
				return result;
			}
		}
		
		private bool AnimationNeeded {
			get {
				return CursorMoveNeeded || ScrollNeeded || ChildScrollNeeded;
			}
		}
		
		private bool CursorMoveNeeded {
			get {
				return highlight_offset != 0;
			}
		}
		
		private bool ScrollNeeded {
			get {
				return scroll_offset != 0;
			}
		}
		
		private bool ChildScrollNeeded {
			get {
				return child_scroll_offset != 0;
			}
		}
		
		public BezelResultsDrawingArea(int numberResults, int width) : base ()
		{
			num_results = numberResults;
			surface_cache = new Dictionary <Item,Surface> ();
			
			this.width = width;
			height = num_results * SurfaceHeight;
			SetSizeRequest (width, height);
			
			odd_color       = new Cairo.Color (0.0, 0.0, 0.0, 0.0);
			even_color      = new Cairo.Color (.2, .2, .2, .3);
			
			DoubleBuffered = false;
			
			Realized += delegate {
				GdkWindow.SetBackPixmap (null, false);
			};
			
			StyleSet += delegate {
				if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			};
		}
		
		private void AnimatedDraw ()
		{
			if (!IsDrawable || timer > 0) return;
			
			Paint ();
			
			if (!AnimationNeeded)
				return;
			
			delta_time = DateTime.Now;
			timer = GLib.Timeout.Add (20, delegate {
				double change = DateTime.Now.Subtract (delta_time).TotalMilliseconds / FadeTime;
				delta_time = DateTime.Now;
				
				double move = Math.Max (change*delta, change);
				
				if (ScrollNeeded) {
					if (scroll_offset > 0)
						scroll_offset = Math.Max (0, scroll_offset - move);
					else
						scroll_offset = Math.Min (0, scroll_offset + move);
				}
				
				if (CursorMoveNeeded) {
					if (highlight_offset > 0)
						highlight_offset = Math.Max (0, highlight_offset - move);
					else
						highlight_offset = Math.Min (0, highlight_offset + move);
				}
				
				if (ChildScrollNeeded) {
					if (child_scroll_offset > 0)
						child_scroll_offset = Math.Max (0, child_scroll_offset - change);
					else
						child_scroll_offset = Math.Min (0, child_scroll_offset + change);
				}
				
				Paint ();
				
				if (!AnimationNeeded) {
					timer = 0;
					if (delta_reset > 0)
						GLib.Source.Remove (delta_reset);
					delta_reset = GLib.Timeout.Add (50, delegate {
						if (timer == 0)
							delta = 0;
						delta_reset = 0;
						return false;
					});
					return false;
				}
				return true;
			});
		}
		
		public void Clear ()
		{
			Results = null;
			Cursor = 0;
			foreach (Surface s in surface_cache.Values)
				s.Dispose ();
			
			surface_cache = new Dictionary<Item,Surface> ();
		}
		
		private void Paint ()
		{
			if (!IsDrawable) return;
			
			using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
			
				if (backbuffer == null) {
					backbuffer = cr.CreateSimilarToTarget (width, height);
				}
			
			
				DrawContextOnSurface (backbuffer);
				if (child_scroll_offset == 0) {
				
					cr.SetSource (backbuffer);
					cr.Operator = Operator.Source;
					cr.Paint ();
				} else {
					if (triplebuffer == null) {
						triplebuffer = cr.CreateSimilarToTarget (width, height);
					}
				
					int old_x, new_x;
					if (child_scroll_offset > 0) {
						old_x = (int)(-width * (1 - child_scroll_offset));
						new_x = old_x + width;
					} else {
						old_x = (int)(-width * (-1 - child_scroll_offset));
						new_x = old_x - width;
					}
				
					DrawSlideContexts (child_inout_surface, backbuffer, triplebuffer, old_x, new_x);

					cr.SetSource (triplebuffer);
					cr.Operator = Operator.Source;
					cr.Paint ();
				}
			}
		}
		
		private void DrawSlideContexts (Surface old_surface, Surface new_surface, Surface target_surface,
		                                int old_x, int new_x)
		{
			using (Context cr = new Context (target_surface)) {
			
				cr.Operator = Operator.Source;
				cr.SetSource (old_surface, old_x, 0);
				cr.Paint ();
			
				cr.Operator = Operator.Over;
				cr.SetSource (new_surface, new_x, 0);
				cr.Paint ();
			}
		}
		
		private void DrawContextOnSurface (Surface sr)
		{
			using (Context cr = new Context (sr)) {
				cr.Operator = Operator.Source;
				cr.Rectangle (0, 0, width, height);
				cr.SetSourceRGBA (0, 0, 0, .9);
				cr.Fill ();
				cr.Operator = Operator.Over;
			
				if (Results != null) {
					int start_result = StartResult - (int)Math.Ceiling (scroll_offset);
					RenderHighlight (cr);
					for (int i = start_result; i < start_result + num_results + 1 && i < Results.Length; i++) {
						RenderItem (cr, i);
					}
				}
			}
		}
		
		public void Draw ()
		{
			AnimatedDraw ();
		}
		
		public void InitChildInAnimation ()
		{
			if (child_inout_surface == null) {
				using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
					child_inout_surface = cr.CreateSimilarToTarget (width, height);
				}
			}
			DrawContextOnSurface (child_inout_surface);
			child_scroll_offset = 1;
		}
		
		public void InitChildOutAnimation ()
		{
			if (child_inout_surface == null) {
				using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
					child_inout_surface = cr.CreateSimilarToTarget (width, height);
				}
			}
			DrawContextOnSurface (child_inout_surface);
			child_scroll_offset = -1;
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Draw ();
			return base.OnExposeEvent (evnt);
		}

		void PopulateSurfaceCacheForItem (Item item) 
		{
			if (!IsDrawable)
				return;
			
			using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
				// We cache this surface, so must not Dispose of it here.
				var surface = cr.CreateSimilarToTarget (width, SurfaceHeight);
				using (var cr2 = new Context (surface)) {
					cr2.Rectangle (0, 0, width, SurfaceHeight);
					cr2.SetSourceRGBA (0, 0, 0, 0);
					cr2.Operator = Operator.Source;
					cr2.Fill ();
					cr2.Operator = Operator.Over;
			
					using (Gdk.Pixbuf pixbuf = IconProvider.PixbufFromIconName (item.Icon, IconSize)) {
						Gdk.CairoHelper.SetSourcePixbuf (cr2, pixbuf, 2, 2);
						cr2.Paint ();
					}
				
					Pango.Layout layout = new Pango.Layout (this.PangoContext);
					layout.Width = Pango.Units.FromPixels (width - IconSize - 10);
					layout.Ellipsize = Pango.EllipsizeMode.End;
					layout.SetMarkup ("<span foreground=\"#ffffff\">" + item.Name + "</span>");
					layout.FontDescription = Pango.FontDescription.FromString ("normal bold");
					layout.FontDescription.AbsoluteSize = Pango.Units.FromPixels (10);
				
					cr2.MoveTo (IconSize + 6, 4);
					Pango.CairoHelper.ShowLayout (cr2, layout);
			
					if (surface_cache.ContainsKey (item)) {
						surface_cache [item].Dispose ();
					}
					surface_cache [item] = surface;
			
					layout.FontDescription.Dispose ();
				}
			}
		}
		
		Surface GetHighlightSource () 
		{
			if (highlight_surface == null) {
				using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
					highlight_surface = cr.CreateSimilarToTarget (width, SurfaceHeight);
				
					using (var cr2 = new Context (highlight_surface)) {
						using (var grad = new LinearGradient (0, 0, 0, SurfaceHeight)) {
				
							grad.AddColorStop (0, new Cairo.Color (.2, .48, .81, 1));
							grad.AddColorStop (1, new Cairo.Color (0, .21, .57, 1));
				
							cr2.SetSource (grad);
							cr2.Rectangle (0, 0, width, SurfaceHeight);
							cr2.Fill ();
						}
					}
				}
			}
			return highlight_surface;
		}
		
		void RenderItem (Context cr, int item)
		{
			int offset = (int) (SurfaceHeight*scroll_offset);
			if (!surface_cache.ContainsKey (Results[item])) {
				PopulateSurfaceCacheForItem (Results[item]);
			}
				
			cr.Rectangle (0, offset+(item-StartResult)*SurfaceHeight, width, SurfaceHeight);
			if (item % 2 == 1) {
				cr.SetSourceRGBA (odd_color);
			} else {
				cr.SetSourceRGBA (even_color);
			}
			cr.Operator = Operator.DestOver;
			cr.FillPreserve ();
			
			cr.Operator = Operator.Over;
			
			cr.SetSource (surface_cache[Results[item]], 0, offset+(item-StartResult)*SurfaceHeight);
			cr.Fill ();
		}
		
		void RenderHighlight (Context cr)
		{
			int offset = (int) (SurfaceHeight*highlight_offset);
			cr.Rectangle (0, offset+(Cursor-StartResult)*SurfaceHeight, width, SurfaceHeight);
			cr.SetSource (GetHighlightSource (), 0, offset+(Cursor-StartResult)*SurfaceHeight);
			cr.FillPreserve ();
		}
	}
}
