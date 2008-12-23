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
		const string HighlightFormat = "<span foreground=\"#5599ff\">{0}</span>";
		const int IconSize = 16;
		
		DockArea parent;
		
		PixbufSurfaceCache LargeIconCache { get; set; }
		TextRenderer TextUtility { get; set; }
		
		DockState State {
			get { return parent.State; }
		}
		
		bool ShouldRenderButton {
			get {
				return State.CurrentPane != Pane.Third && State [State.CurrentPane] != null && State [State.CurrentPane] is Item;
			}
		}
		
		int TextOffset {
			get {
				return (int) (DockPreferences.IconSize * 3.5);
			}
		}
		
		public SummonModeRenderer (DockArea parent)
		{
			this.parent = parent;
			TextUtility = new TextRenderer (parent);
			
			DockPreferences.IconSizeChanged += delegate {
				if (LargeIconCache != null)
					LargeIconCache.Dispose ();
				LargeIconCache = null;
			};
		}
		
		public SummonClickEvent GetClickEvent (Gdk.Rectangle dockArea)
		{
			if (!ShouldRenderButton) return SummonClickEvent.None;
			
			Gdk.Point center = GetButtonCenter (ref dockArea);
			Gdk.Rectangle rect = new Gdk.Rectangle (center.X - IconSize / 2, center.Y - IconSize / 2, IconSize, IconSize);
			if (rect.Contains (parent.Cursor))
				return SummonClickEvent.AddItemToDock;
			return SummonClickEvent.None;
		}
		
		Gdk.Point GetButtonCenter (ref Gdk.Rectangle dockArea)
		{
			return new Gdk.Point (dockArea.X + IconSize / 2 + 5, dockArea.Y + dockArea.Height - (IconSize / 2 + 5));
		}
			
		
		
		public void RenderSummonMode (Context cr, Gdk.Rectangle dockArea)
		{
			if (LargeIconCache == null)
				LargeIconCache = new PixbufSurfaceCache (10, 2 * DockPreferences.IconSize, 2 * DockPreferences.IconSize, cr.Target);
				
			for (int i=0; i<3; i++) {
				Pane pane  = (Pane)i;
				int left_x;
				double zoom;
				GetXForPane (ref dockArea, pane, out left_x, out zoom);
				
				if (pane == Pane.Third && !State.ThirdPaneVisible)
					continue;
				
				string icon = null;
				double opacity = .6 + zoom * .4;
				switch (PaneDrawState (pane)) {
				case DrawState.NoResult:
					icon = "gtk-delete";
					break;
				case DrawState.Normal:
					icon = State[pane].Icon;
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
				              ((dockArea.Y + dockArea.Height) - (DockPreferences.IconSize * 2 * zoom) - DockArea.VerticalBuffer) * (1 / zoom));
				cr.PaintWithAlpha (opacity);
				cr.Scale (1 / zoom, 1 / zoom);
			}
			
			switch (PaneDrawState (State.CurrentPane))
			{
			case DrawState.NoResult:
				RenderText (cr, Catalog.GetString ("No result found for") + ": " + State.GetPaneQuery (State.CurrentPane), ref dockArea);
				break;
			case DrawState.Normal:
				RenderNormalText (cr, ref dockArea);
				if (ShouldRenderButton)
					RenderAddButton (cr, ref dockArea);
				break;
			case DrawState.Text:
				RenderTextModeText (cr, ref dockArea);
				break;
			case DrawState.ExplicitText:
				RenderExplicitText (cr, ref dockArea);
				break;
			case DrawState.None:
				// do nothing
				break;
			}
		}
		
		void RenderNormalText (Context cr, ref Gdk.Rectangle dockArea)
		{
			int base_x = dockArea.X + 30;
			string text = GLib.Markup.EscapeText (State[State.CurrentPane].Name);
			text = Do.Interface.Util.FormatCommonSubstrings (text, State.GetPaneQuery (State.CurrentPane), HighlightFormat);
			
			double text_scale = (DockPreferences.IconSize / 64.0);
			
			int text_height;
			if ((int) (12 * text_scale) > 8)
				text_height = (int) (20 * text_scale);
			else
				text_height = (int) (35 * text_scale);
				
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			
			TextUtility.RenderLayoutText (cr, text, base_x + TextOffset, 
			                              dockArea.Y + (int) (15 * text_scale), (int) (500 * text_scale), text_height,
			                              color, Pango.Alignment.Left, Pango.EllipsizeMode.End);
			
			if ((int) (12 * text_scale) > 8) {
				text_height = (int) (12 * text_scale);
				TextUtility.RenderLayoutText (cr, GLib.Markup.EscapeText (State[State.CurrentPane].Description), 
				                              base_x + TextOffset, dockArea.Y + (int) (42 * text_scale), 
				                              (int) (500 * text_scale), text_height, color, Pango.Alignment.Left, Pango.EllipsizeMode.End);
			}
		}
		
		void RenderExplicitText (Context cr, ref Gdk.Rectangle dockArea)
		{
			int base_x = dockArea.X + 15;
			
			string text;
			Element current = State [State.CurrentPane];
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
		
		void RenderTextModeText (Context cr, ref Gdk.Rectangle dockArea)
		{
			int base_x = dockArea.X + 15;
			
			string text;
			Element current = State [State.CurrentPane];
			if (current is ITextItem)
				text = GLib.Markup.EscapeText ((current as ITextItem).Text);
			else
				text = GLib.Markup.EscapeText (current.Name);
			
			double text_scale = (DockPreferences.IconSize / 64.0);
			int text_height = (int) (15 * text_scale);
				
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			
			TextUtility.RenderLayoutText (cr, text, base_x + TextOffset, 
			                              dockArea.Y + (int) (15 * text_scale), (dockArea.X + dockArea.Width) - (base_x + TextOffset + 40), 
			                              text_height, color, Pango.Alignment.Left, Pango.EllipsizeMode.None);
		}
		
		void RenderText (Context cr, string text, ref Gdk.Rectangle dockArea)
		{
			int base_x = dockArea.X + 15;
			
			double text_scale = (DockPreferences.IconSize / 64.0);
			
			int text_height = (int) (20 * text_scale);
				
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			
			TextUtility.RenderLayoutText (cr, text, base_x + TextOffset, 
			                              dockArea.Y + (int) (15 * text_scale), (dockArea.X + dockArea.Width) - (base_x + TextOffset + 40), 
			                              text_height, color, Pango.Alignment.Left, Pango.EllipsizeMode.End);
		}
		
		void RenderAddButton (Context cr, ref Gdk.Rectangle dockArea)
		{
			Gdk.Point buttonCenter = GetButtonCenter (ref dockArea);
			
			int x = buttonCenter.X - IconSize / 2;
			int y = buttonCenter.Y - IconSize / 2;
			
			cr.SetRoundedRectanglePath (x, y, IconSize, IconSize, IconSize / 2);
			cr.LineWidth = 2;
			switch (GetClickEvent (dockArea)) {
			case SummonClickEvent.AddItemToDock:
				cr.Color = new Cairo.Color (1, 1, 1);
				break;
			case SummonClickEvent.None:
				cr.Color = new Cairo.Color (1, 1, 1, .7);
				break;
			}
			cr.Stroke ();
			
			cr.MoveTo (x + IconSize / 2, y + 4);
			cr.LineTo (x + IconSize / 2, y + (IconSize - 4));
			cr.MoveTo (x + 4, y + IconSize / 2);
			cr.LineTo (x + (IconSize - 4), y + IconSize / 2);
			cr.Stroke ();
		}
		
		void GetXForPane (ref Gdk.Rectangle dockArea, Pane pane, out int left_x, out double zoom)
		{
			int base_x = dockArea.X + 15;
			double zoom_value = .3;
			double slide_state = Math.Min (1, (DateTime.UtcNow - State.CurrentPaneTime).TotalMilliseconds / DockArea.BaseAnimationTime);
			
			double growing_zoom = zoom_value + slide_state * (1 - zoom_value);
			double shrinking_zoom = zoom_value + (1 - slide_state) * (1 - zoom_value);
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
					left_x = base_x + (int) ((DockPreferences.IconSize * 2) * (growing_zoom));
				} else if (State.PreviousPane == Pane.Second && State.CurrentPane == Pane.Third) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value);
				} else if (State.PreviousPane == Pane.First && State.CurrentPane == Pane.Second) {
					zoom = growing_zoom;
					left_x = base_x + (int) ((DockPreferences.IconSize * 2) * (shrinking_zoom));
				} else if (State.PreviousPane == Pane.First && State.CurrentPane == Pane.Third) {
					zoom = zoom_value;
					left_x = base_x + (int) ((DockPreferences.IconSize * 2) * (shrinking_zoom));
				} else if (State.PreviousPane == Pane.Third && State.CurrentPane == Pane.First) {
					zoom = zoom_value;
					left_x = base_x + (int) ((DockPreferences.IconSize * 2) * (growing_zoom));
				} else {// (State.PreviousPane == Pane.Third && State.CurrentPane == Pane.Second) {
					zoom = growing_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value);
				}
				break;
			default:
				if (State.PreviousPane == Pane.Second && State.CurrentPane == Pane.First) {
					zoom = zoom_value;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * (1 + zoom_value));
				} else if (State.PreviousPane == Pane.Second && State.CurrentPane == Pane.Third) {
					zoom = growing_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value) + (int) ((DockPreferences.IconSize * 2) * (shrinking_zoom));
				} else if (State.PreviousPane == Pane.First && State.CurrentPane == Pane.Second) {
					zoom = zoom_value;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * (1 + zoom_value));
				} else if (State.PreviousPane == Pane.First && State.CurrentPane == Pane.Third) {
					zoom = growing_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value) + (int) ((DockPreferences.IconSize * 2) * (shrinking_zoom));
				} else if (State.PreviousPane == Pane.Third && State.CurrentPane == Pane.First) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value) + (int) ((DockPreferences.IconSize * 2) * (growing_zoom));
				} else {// (State.PreviousPane == Pane.Third && State.CurrentPane == Pane.Second) {
					zoom = shrinking_zoom;
					left_x = base_x + (int) (DockPreferences.IconSize * 2 * zoom_value) + (int) ((DockPreferences.IconSize * 2) * (growing_zoom));
				}
				break;
			}
			double offset_scale = 1;
			left_x = (int) (left_x * offset_scale + base_x * (1 - offset_scale));
		}
		
		DrawState PaneDrawState (Pane pane)
		{
			if (pane != State.CurrentPane && (State.GetTextModeType (State.CurrentPane) == TextModeType.Explicit))
				return DrawState.None;
			
			if (pane == Pane.Third && !State.ThirdPaneVisible)
				return DrawState.None;
			
			if (State.GetTextModeType (pane) == TextModeType.Explicit)
				return DrawState.ExplicitText;
			
			if (State.GetTextMode (pane))
				return DrawState.Text;
			
			if (State.GetPaneItem (pane) != null)
				return DrawState.Normal;
			
			if (!string.IsNullOrEmpty (State.GetPaneQuery (pane))) {
				return DrawState.NoResult;
			}
			
			return DrawState.None;
		}
	}
}
