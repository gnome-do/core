// SummonModeRenderer.cs
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
using System.Diagnostics;

using Cairo;
using Gdk;
using Mono.Unix;

using Do.Interface;
using Do.Interface.AnimationBase;
using Do.Interface.CairoUtils;

using Do.Platform;
using Do.Universe;

using Docky.Utilities;

namespace Docky.Interface.Renderers
{
	public enum SummonClickEvent {
		AddItemToDock,
		None,
	}
	
	public class SummonModeRenderer
	{
		Gtk.Widget widget;
		const string HighlightFormat = "<span foreground=\"#5599ff\">{0}</span>";
		
		PixbufSurfaceCache LargeIconCache { get; set; }
		TextRenderer TextUtility { get; set; }
		
		public SummonModeRenderer (Gtk.Widget referenceWidget)
		{
			widget = referenceWidget;
			TextUtility = new TextRenderer (widget);
			
			DockPreferences.IconSizeChanged += delegate {
				if (LargeIconCache != null)
					LargeIconCache.Dispose ();
				LargeIconCache = null;
			};
		}
		
		public SummonClickEvent GetClickEvent (Gdk.Point cursor, DockState state, Gdk.Rectangle dockArea)
		{
			if (!ShouldRenderButton (state)) return SummonClickEvent.None;
			
			Gdk.Point center = GetButtonCenter (dockArea);
			Gdk.Rectangle rect = new Gdk.Rectangle (center.X - 10, center.Y - 10, 20, 20);
			if (rect.Contains (cursor))
				return SummonClickEvent.AddItemToDock;
			return SummonClickEvent.None;
		}
		
		Gdk.Point GetButtonCenter (Gdk.Rectangle dockArea)
		{
			return new Gdk.Point (dockArea.X + dockArea.Width - 20, dockArea.Y + 18);
		}
			
		bool ShouldRenderButton (DockState state)
		{
			return state [state.CurrentPane] != null && state [state.CurrentPane] is Item;
		}
		
		public void RenderSummonMode (Context cr, DockState state, Gdk.Rectangle dockArea, int VerticalBuffer)
		{
			if (LargeIconCache == null)
				LargeIconCache = new PixbufSurfaceCache (10, 2 * DockPreferences.IconSize, 2 * DockPreferences.IconSize, cr.Target);
				
			for (int i=0; i<3; i++) {
				Pane pane  = (Pane)i;
				int left_x;
				double zoom;
				GetXForPane (dockArea, state, pane, out left_x, out zoom);
				
				if (pane == Pane.Third && !state.ThirdPaneVisible)
					continue;
				
				string icon = null;
				double opacity = .6 + zoom * .4;
				switch (PaneDrawState (pane, state)) {
				case DrawState.NoResult:
					icon = "gtk-delete";
					break;
				case DrawState.Normal:
					icon = state[pane].Icon;
					break;
				case DrawState.Text:
					icon = "gnome-mime-text";
					break;
				case DrawState.ExplicitText:
					icon = "gnome-mime-text";
					opacity = .2 * opacity;
					break;
				case DrawState.None:
					continue;
				}
				
				if (icon == null)
					continue;
				
				if (!LargeIconCache.ContainsKey (icon))
				    LargeIconCache.AddPixbufSurface (icon, icon);
				
				cr.Scale (zoom, zoom);
				cr.SetSource (LargeIconCache.GetSurface (icon), 
				              left_x * (1 / zoom), 
				              ((dockArea.Y + dockArea.Height) - (DockPreferences.IconSize * 2 * zoom) - VerticalBuffer) * (1 / zoom));
				cr.PaintWithAlpha (opacity);
				cr.Scale (1 / zoom, 1 / zoom);
			}
			
			switch (PaneDrawState (state.CurrentPane, state))
			{
			case DrawState.NoResult:
				RenderText (cr, Catalog.GetString ("No result found for") + ": " + state.GetPaneQuery (state.CurrentPane), dockArea);
				break;
			case DrawState.Normal:
				RenderNormalText (cr, state, dockArea);
				if (ShouldRenderButton (state))
					RenderAddButton (cr, state, dockArea);
				break;
			case DrawState.Text:
				RenderTextModeText (cr, state, dockArea);
				break;
			case DrawState.ExplicitText:
				RenderExplicitText (cr, state, dockArea);
				break;
			case DrawState.None:
				// do nothing
				break;
			}
		}
		
		void RenderNormalText (Context cr, DockState state, Gdk.Rectangle dockArea)
		{
			int base_x = dockArea.X + 15;
			string text = GLib.Markup.EscapeText (state[state.CurrentPane].Name);
			text = Do.Interface.Util.FormatCommonSubstrings (text, state.GetPaneQuery (state.CurrentPane), HighlightFormat);
			
			double text_scale = (DockPreferences.IconSize / 64.0);
			int text_offset = (int) (DockPreferences.IconSize * 3);
			
			int text_height;
			if ((int) (12 * text_scale) > 8)
				text_height = (int) (20 * text_scale);
			else
				text_height = (int) (35 * text_scale);
				
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			
			TextUtility.RenderLayoutText (cr, text, base_x + text_offset, 
			                              dockArea.Y + (int) (15 * text_scale), (int) (500 * text_scale), text_height,
			                              color, Pango.Alignment.Left, Pango.EllipsizeMode.End);
			
			if ((int) (12 * text_scale) > 8) {
				text_height = (int) (12 * text_scale);
				TextUtility.RenderLayoutText (cr, GLib.Markup.EscapeText (state[state.CurrentPane].Description), 
				                              base_x + text_offset, dockArea.Y + (int) (42 * text_scale), 
				                              (int) (500 * text_scale), text_height, color, Pango.Alignment.Left, Pango.EllipsizeMode.End);
			}
		}
		
		void RenderExplicitText (Context cr, DockState state, Gdk.Rectangle dockArea)
		{
			int base_x = dockArea.X + 15;
			
			string text;
			Element current = state [state.CurrentPane];
			if (current is ITextItem)
				text = GLib.Markup.EscapeText ((current as ITextItem).Text);
			else
				text = GLib.Markup.EscapeText (current.Name);
			
			double text_scale = (DockPreferences.IconSize / 64.0);
			int text_height = (int) (15 * text_scale);
				
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			
			Gdk.Rectangle rect = TextUtility.RenderLayoutText (cr, text, base_x, 
			                                                   dockArea.Y + (int) (15 * text_scale), 
			                                                   (dockArea.X + dockArea.Width) - (base_x + 15), text_height,
			                                                   color, Pango.Alignment.Left, Pango.EllipsizeMode.None);
			
			cr.Rectangle (rect.X, rect.Y, 2, rect.Height);
			cr.Color = new Cairo.Color (1, 1, 1);
			cr.Fill ();
		}
		
		void RenderTextModeText (Context cr, DockState state, Gdk.Rectangle dockArea)
		{
			int base_x = dockArea.X + 15;
			
			string text;
			Element current = state [state.CurrentPane];
			if (current is ITextItem)
				text = GLib.Markup.EscapeText ((current as ITextItem).Text);
			else
				text = GLib.Markup.EscapeText (current.Name);
			
			double text_scale = (DockPreferences.IconSize / 64.0);
			int text_offset = (int) (DockPreferences.IconSize * 3);
			int text_height = (int) (15 * text_scale);
				
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			
			TextUtility.RenderLayoutText (cr, text, base_x + text_offset, 
			                              dockArea.Y + (int) (15 * text_scale), (dockArea.X + dockArea.Width) - (base_x + text_offset + 40), 
			                              text_height, color, Pango.Alignment.Left, Pango.EllipsizeMode.None);
		}
		
		void RenderText (Context cr, string text, Gdk.Rectangle dockArea)
		{
			int base_x = dockArea.X + 15;
			
			double text_scale = (DockPreferences.IconSize / 64.0);
			int text_offset = (int) (DockPreferences.IconSize * 3);
			
			int text_height = (int) (20 * text_scale);
				
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			
			TextUtility.RenderLayoutText (cr, text, base_x + text_offset, 
			                              dockArea.Y + (int) (15 * text_scale), (dockArea.X + dockArea.Width) - (base_x + text_offset + 40), 
			                              text_height, color, Pango.Alignment.Left, Pango.EllipsizeMode.End);
		}
		
		void RenderAddButton (Context cr, DockState state, Gdk.Rectangle dockArea)
		{
			Gdk.Point buttonCenter = GetButtonCenter (dockArea);
			int x = buttonCenter.X - 10;
			int y = buttonCenter.Y - 10;
			
			cr.SetRoundedRectanglePath (x, y, 20, 20, 10);
			cr.LineWidth = 2;
			cr.Color = new Cairo.Color (1, 1, 1);
			cr.Stroke ();
			
			cr.MoveTo (x+10, y+4);
			cr.LineTo (x+10, y+16);
			cr.MoveTo (x+4, y+10);
			cr.LineTo (x+16, y+10);
			cr.Stroke ();
			
			TextUtility.RenderLayoutText (cr, "<b>" + Catalog.GetString ("Add To Dock") + "</b>", x - 125, y, 115, 16); 
		}
		
		void GetXForPane (Gdk.Rectangle dockArea, DockState state, Pane pane, out int left_x, out double zoom)
		{
			int base_x = dockArea.X + 15;
			double zoom_value = .3;
			double slide_state = Math.Min (1, (DateTime.UtcNow - state.CurrentPaneTime).TotalMilliseconds / DockArea.BaseAnimationTime);
			
			double growing_zoom = zoom_value + slide_state * (1 - zoom_value);
			double shrinking_zoom = zoom_value + (1 - slide_state) * (1 - zoom_value);
			switch (pane) {
			case Pane.First:
				left_x = base_x;
				if (state.CurrentPane == Pane.First && (state.PreviousPane == Pane.Second || state.PreviousPane == Pane.Third)) {
					zoom = growing_zoom;
				} else if (state.PreviousPane == Pane.First && (state.CurrentPane == Pane.Second || state.CurrentPane == Pane.Third)) {
					zoom = shrinking_zoom;
				} else {
					zoom = zoom_value;
				}
				break;
			case Pane.Second:
				if (state.PreviousPane == Pane.Second && state.CurrentPane == Pane.First) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) ((DockPreferences.IconSize * 2) * (growing_zoom));
				} else if (state.PreviousPane == Pane.Second && state.CurrentPane == Pane.Third) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value);
				} else if (state.PreviousPane == Pane.First && state.CurrentPane == Pane.Second) {
					zoom = growing_zoom;
					left_x = base_x + (int) ((DockPreferences.IconSize * 2) * (shrinking_zoom));
				} else if (state.PreviousPane == Pane.First && state.CurrentPane == Pane.Third) {
					zoom = zoom_value;
					left_x = base_x + (int) ((DockPreferences.IconSize * 2) * (shrinking_zoom));
				} else if (state.PreviousPane == Pane.Third && state.CurrentPane == Pane.First) {
					zoom = zoom_value;
					left_x = base_x + (int) ((DockPreferences.IconSize * 2) * (growing_zoom));
				} else {// (state.PreviousPane == Pane.Third && state.CurrentPane == Pane.Second) {
					zoom = growing_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value);
				}
				break;
			default:
				if (state.PreviousPane == Pane.Second && state.CurrentPane == Pane.First) {
					zoom = zoom_value;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * (1 + zoom_value));
				} else if (state.PreviousPane == Pane.Second && state.CurrentPane == Pane.Third) {
					zoom = growing_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value) + (int) ((DockPreferences.IconSize * 2) * (shrinking_zoom));
				} else if (state.PreviousPane == Pane.First && state.CurrentPane == Pane.Second) {
					zoom = zoom_value;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * (1 + zoom_value));
				} else if (state.PreviousPane == Pane.First && state.CurrentPane == Pane.Third) {
					zoom = growing_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value) + (int) ((DockPreferences.IconSize * 2) * (shrinking_zoom));
				} else if (state.PreviousPane == Pane.Third && state.CurrentPane == Pane.First) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value) + (int) ((DockPreferences.IconSize * 2) * (growing_zoom));
				} else {// (state.PreviousPane == Pane.Third && state.CurrentPane == Pane.Second) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value) + (int) ((DockPreferences.IconSize * 2) * (growing_zoom));
				}
				break;
			}
			double offset_scale = .9;
			left_x = (int) (left_x * offset_scale + base_x * (1 - offset_scale));
		}
		
		DrawState PaneDrawState (Pane pane, DockState state)
		{
			if (pane != state.CurrentPane && (state.GetTextModeType (state.CurrentPane) == TextModeType.Explicit))
				return DrawState.None;
			
			if (pane == Pane.Third && !state.ThirdPaneVisible)
				return DrawState.None;
			
			if (state.GetTextModeType (pane) == TextModeType.Explicit)
				return DrawState.ExplicitText;
			
			if (state.GetTextMode (pane))
				return DrawState.Text;
			
			if (state.GetPaneItem (pane) != null)
				return DrawState.Normal;
			
			if (!string.IsNullOrEmpty (state.GetPaneQuery (pane))) {
				return DrawState.NoResult;
			}
			
			return DrawState.None;
		}
	}
}
