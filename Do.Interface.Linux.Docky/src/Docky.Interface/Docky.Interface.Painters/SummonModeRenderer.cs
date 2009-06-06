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

using Docky.Core;
using Docky.Interface;
using Docky.Utilities;

namespace Docky.Interface.Painters
{
	public class SummonModeRenderer : IDockPainter
	{
		const int IconSize = 16;
		
		bool paint;
		Gdk.Rectangle previous_area;

		public event EventHandler<PaintNeededArgs> PaintNeeded;

		public event EventHandler ShowRequested;
		public event EventHandler HideRequested;
		
		PixbufSurfaceCache LargeIconCache { get; set; }
		TextRenderer TextUtility { get; set; }
		
		DockState State {
			get { return DockState.Instance; }
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
		
		TimeSpan RenderBuffer {
			get {
				return DockArea.BaseAnimationTime.Add (new TimeSpan (0, 0, 0, 0, 50));
			}
		}

		public bool DoubleBuffer {
			get { return true; }
		}

		public bool Interruptable {
			get { return false; }
		}
		
		public int Width {
			get { return Math.Max (300, DockServices.DrawingService.CurrentDockWidth); }
		}
		
		public SummonModeRenderer ()
		{
			paint = true;
			TextUtility = new TextRenderer (DockWindow.Window);
			
			RegisterEvents ();
		}

		void RegisterEvents ()
		{
			DockPreferences.IconSizeChanged += HandleIconSizeChanged;
			DockServices.DoInteropService.Summoned += HandleSummoned;
			DockServices.DoInteropService.Vanished += HandleVanished;
			DockState.Instance.StateChanged += HandleStateChanged;
		}

		void UnregisterEvents ()
		{
			DockPreferences.IconSizeChanged -= HandleIconSizeChanged;
			DockServices.DoInteropService.Summoned -= HandleSummoned;
			DockServices.DoInteropService.Vanished -= HandleVanished;
			DockState.Instance.StateChanged -= HandleStateChanged;
		}

		void HandleStateChanged (object sender, EventArgs e)
		{
			if (PaintNeeded != null) {
				paint = true;
				PaintNeededArgs args;
				if (DateTime.UtcNow - DockState.Instance.CurrentPaneTime < RenderBuffer) 
					args = new PaintNeededArgs (RenderBuffer);
				else
					args = new PaintNeededArgs ();
				PaintNeeded (this, args);
			}
		}

		void HandleVanished(object sender, EventArgs e)
		{
			if (HideRequested != null)
				HideRequested (this, new EventArgs ());
		}

		void HandleSummoned(object sender, EventArgs e)
		{
			if (ShowRequested != null)
				ShowRequested (this, new EventArgs ());
			paint = true;
		}

		void HandleIconSizeChanged ()
		{
			if (LargeIconCache != null)
				LargeIconCache.Dispose ();
			LargeIconCache = null;
		}

		public void Clicked (Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
			Gdk.Point center = GetButtonCenter (ref dockArea);
			Gdk.Rectangle rect = new Gdk.Rectangle (center.X - IconSize / 2, center.Y - IconSize / 2, IconSize, IconSize);
			if (rect.Contains (cursor) && State [State.CurrentPane] is Item) {
				DockServices.ItemsService.AddItemToDock (State [State.CurrentPane]);
				DockServices.DoInteropService.RequestClickOff ();
			} else if (!dockArea.Contains (cursor)) {
				DockServices.DoInteropService.RequestClickOff ();
			}
		}

		public void Interrupt ()
		{
			Log.Error ("Docky has been interupted innapropriately.  Please report this bug.");
		}
		
		Gdk.Point GetButtonCenter (ref Gdk.Rectangle dockArea)
		{
			return new Gdk.Point (dockArea.X + IconSize / 2 + 5, dockArea.Y + dockArea.Height - (IconSize / 2 + 5));
		}
			
		public void Paint (Context cr, Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
			if (previous_area == dockArea && !paint && DateTime.UtcNow - DockState.Instance.CurrentPaneTime > RenderBuffer) {
				previous_area = dockArea;
				return;
			}
			previous_area = dockArea;
			
			paint = false;
			cr.AlphaFill ();
			
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
				if (DockPreferences.Orientation == DockOrientation.Top) {
					cr.SetSource (LargeIconCache.GetSurface (icon), 
					              left_x * (1 / zoom), 
					              (dockArea.Y * (1 / zoom)));
				} else {
					cr.SetSource (LargeIconCache.GetSurface (icon), 
					              left_x * (1 / zoom), 
					              ((dockArea.Y + dockArea.Height) - (DockPreferences.IconSize * 2 * zoom) - 5) * (1 / zoom));
				}
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
					RenderAddButton (cr, ref dockArea, ref cursor);
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
			text = Do.Interface.Util.FormatCommonSubstrings (text, State.GetPaneQuery (State.CurrentPane), DockPreferences.HighlightFormat);
			
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			
			double text_scale = (DockPreferences.IconSize / 64.0);
			int small_text_height = (int) (12 * text_scale);
			
			int big_text_height;
			if (8 < small_text_height) {
				big_text_height = (int) (20 * text_scale);
				TextUtility.RenderLayoutText (cr, text, base_x + TextOffset, 
				                              dockArea.Y + (int) (15 * text_scale), dockArea.Width - TextOffset - 50, 
				                              big_text_height, color, Pango.Alignment.Left, Pango.EllipsizeMode.End);
				
				TextUtility.RenderLayoutText (cr, GLib.Markup.EscapeText (State[State.CurrentPane].Description), 
				                              base_x + TextOffset, dockArea.Y + (int) (42 * text_scale), 
				                              dockArea.Width - TextOffset - 50, small_text_height, color, 
				                              Pango.Alignment.Left, Pango.EllipsizeMode.End);
				
				
			} else {
				big_text_height = (int) (35 * text_scale);
				TextUtility.RenderLayoutText (cr, text, base_x + TextOffset, 
				                              dockArea.Y + dockArea.Height / 2 - 3 * big_text_height / 5, dockArea.Width - TextOffset - 50, 
				                              big_text_height, color, Pango.Alignment.Left, Pango.EllipsizeMode.End);
			}
		}
		
		void RenderExplicitText (Context cr, ref Gdk.Rectangle dockArea)
		{
			int base_x = dockArea.X + 15;
			
			string text;
			Element current = State [State.CurrentPane];
			
			if (current == null)
				text = State.GetPaneQuery (State.CurrentPane);
			else if (current is ITextItem)
				text = (current as ITextItem).Text;
			else
				text = current.Name;

			if (string.IsNullOrEmpty (text))
			    return;

			text = GLib.Markup.EscapeText (text);
			
			double text_scale = (DockPreferences.IconSize / 64.0);
			int text_height = Math.Max (11, (int) (15 * text_scale));
				
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

			if (current == null)
				text = State.GetPaneQuery (State.CurrentPane);
			else if (current is ITextItem)
				text = (current as ITextItem).Text;
			else
				text = current.Name;

			if (string.IsNullOrEmpty (text))
				return;

			text = GLib.Markup.EscapeText (text);
			
			double text_scale = (DockPreferences.IconSize / 64.0);
			int text_height = Math.Max (11, (int) (15 * text_scale));
			
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
		
		void RenderAddButton (Context cr, ref Gdk.Rectangle dockArea, ref Gdk.Point cursor)
		{
			Gdk.Point buttonCenter = GetButtonCenter (ref dockArea);
			
			int x = buttonCenter.X - IconSize / 2;
			int y = buttonCenter.Y - IconSize / 2;
			
			cr.SetRoundedRectanglePath (x, y, IconSize, IconSize, IconSize / 2);
			cr.LineWidth = 2;
			// fixme
			cr.Color = new Cairo.Color (1, 1, 1);
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
			double slide_state = Math.Min (1, (DateTime.UtcNow - State.CurrentPaneTime).TotalMilliseconds / DockArea.BaseAnimationTime.TotalMilliseconds);
			
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

		public void Dispose ()
		{
			UnregisterEvents ();
			TextUtility.Dispose ();
			TextUtility = null;
		}
	}
}
