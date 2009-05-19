// AbstractDockItem.cs
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
using System.Threading;

using Cairo;
using Gdk;
using Gtk;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Interface.Wink;
using Do.Platform;

using Docky.Utilities;

namespace Docky.Interface
{

	
	public abstract class AbstractDockItem : IDisposable, IEquatable<AbstractDockItem>
	{
		public event UpdateRequestHandler UpdateNeeded;
		
		Surface text_surface, resize_buffer;
		DockOrientation current_orientation;
		uint size_changed_timer, redraw_timer;
		bool needs_attention;
		
		bool time_since_click_overdue;
		
		Cairo.Color? average_color;
		
		protected int current_size;

		protected virtual Surface IconSurface { get; set; }
		
		Surface SecondaryIconSurface { get; set; }
		
		public string Description { get; private set; }
		
		public bool Disposed { get; private set; }
		
		/// <value>
		/// The currently requested animation type
		/// </value>
		public virtual ClickAnimationType AnimationType { get; protected set; }

		/// <value>
		/// The time at which the NeedsAttention flag was set true
		/// </value>
		public virtual DateTime AttentionRequestStartTime { get; protected set; }

		/// <value>
		/// When this item was added to the Dock
		/// </value>
		public virtual DateTime DockAddItem { get; set; }

		/// <value>
		/// The last time this icon was "clicked" that required an animation
		/// </value>
		public virtual DateTime LastClick { get; private set; }
		
		public int Position { get; set; }
		
		public virtual bool ContainsFocusedWindow {
			get { return false; }
		}

		/// <value>
		/// Determines if drop actions will be passed on to the icon
		/// </value>
		public virtual bool IsAcceptingDrops { 
			get { return false; } 
		}
		
		/// <value>
		/// Determines the type of indicator drawn under the item
		/// </value>
		public virtual bool NeedsAttention { 
			get {
				return needs_attention;
			}
			protected set {
				if (value == needs_attention)
					return;
				needs_attention = value;
				if (needs_attention)
					AttentionRequestStartTime = DateTime.UtcNow;
			}
		}
		
		/// <value>
		/// The Widget of the icon.
		/// </value>
		public virtual int Width {
			get { return DockPreferences.IconSize; }
		}
		
		/// <value>
		/// The Height of the icon.
		/// </value>
		public virtual int Height {
			get { return DockPreferences.IconSize; }
		}
		
		public virtual ScalingType ScalingType {
			get { return ScalingType.Downscaled; }
		}
		
		protected virtual string Icon {
			get { return "default"; }
		}
		
		/// <value>
		/// Whether or not to draw an application present indicator
		/// </value>
		public virtual int WindowCount {
			get { return 0; }
		}
		
		public TimeSpan TimeSinceClick {
			get { 
				if (time_since_click_overdue)
					return new TimeSpan (1, 0, 0);
				
				TimeSpan result = DockArea.RenderTime - LastClick;
				
				if (result.TotalMilliseconds > 1000)
					time_since_click_overdue = true;
				
				return result;
			}
		}
		
		public TimeSpan TimeSinceAdd {
			get { return DockArea.RenderTime - DockAddItem; }
		}
		
		public AbstractDockItem ()
		{
			NeedsAttention = false;
			Description = "";
			AttentionRequestStartTime = LastClick = new DateTime (0);
			
			DockPreferences.IconSizeChanged += OnIconSizeChanged;
			DockWindow.Window.StyleSet += HandleStyleSet; 
		}

		void HandleStyleSet(object o, StyleSetArgs args)
		{
			ResetSurfaces ();
		}
		
		public Cairo.Color AverageColor ()
		{
			if (IconSurface == null)
				return new Cairo.Color (1, 1, 1, 1);
			
			if (average_color.HasValue)
				return average_color.Value;
				
			ImageSurface sr = new ImageSurface (Format.ARGB32, current_size, current_size);
			using (Context cr = new Context (sr)) {
				cr.Operator = Operator.Source;
				IconSurface.Show (cr, 0, 0);
			}
			
			sr.Flush ();
			
			byte [] data;
			try {
				data = sr.Data;
			} catch {
				return new Cairo.Color (1, 1, 1, 1);
			}
			byte r, g, b;
			
			double rTotal = 0;
			double gTotal = 0;
			double bTotal = 0;
			
			for (int i=0; i < data.Length - 3; i += 4) {
				b = data [i + 0];
				g = data [i + 1];
				r = data [i + 2];
				
				byte max = Math.Max (r, Math.Max (g, b));
				byte min = Math.Min (r, Math.Min (g, b));
				double delta = max - min;
				
				double sat;
				if (delta == 0) {
					sat = 0;
				} else {
					sat = delta / max;
				}
				double score = .2 + .8 * sat;
				
				rTotal += r * score;
				gTotal += g * score;
				bTotal += b * score;
			}
			double pixelCount = current_size * current_size * byte.MaxValue;
			
			r = (byte) (byte.MaxValue * (rTotal / pixelCount));
			g = (byte) (byte.MaxValue * (gTotal / pixelCount));
			b = (byte) (byte.MaxValue * (bTotal / pixelCount));
			
			double h, s, v;
			Do.Interface.Util.Appearance.RGBToHSV (r, g, b, out h, out s, out v);
			v = 100;
			s = Math.Min (100, s * 1.3);
			Do.Interface.Util.Appearance.HSVToRGB (h, s, v, out r, out g, out b);
			
			Cairo.Color color = new Cairo.Color ((double) r / byte.MaxValue, (double) g / byte.MaxValue, (double) b / byte.MaxValue);
			
			sr.Destroy ();
			
			average_color = color;
			return average_color.Value;
		}

		protected virtual Pixbuf GetSurfacePixbuf (int size)
		{
			if (Icon == null)
				return null;
			
			Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName (Icon, size);
			if (pbuf.Height != size && pbuf.Width != size) {
				double scale = (double)DockPreferences.FullIconSize / Math.Max (pbuf.Width, pbuf.Height);
				Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), InterpType.Bilinear);
				pbuf.Dispose ();
				pbuf = temp;
			}
			
			return pbuf;
		}

		/// <summary>
		/// Called whenever the icon receives a click event
		/// </summary>
		/// <param name="button">
		/// A <see cref="System.UInt32"/>
		/// </param>
		/// <param name="controller">
		/// A <see cref="IDoController"/>
		/// </param>
		public virtual void Clicked (uint button, ModifierType state, PointD position)
		{
			SetLastClick ();
		}
		
		protected void SetLastClick ()
		{
			LastClick = DateTime.UtcNow;
			time_since_click_overdue = false;
		}
		
		public virtual void Scrolled (Gdk.ScrollDirection direction)
		{
		}

		Surface CopySurface (Surface source, int width, int height)
		{
			Surface sr = source.CreateSimilar (Cairo.Content.ColorAlpha, width, height);
			using (Context cr = new Context (sr)) {
				source.Show (cr, 0, 0);
			}
			return sr;
		}

		public virtual Pixbuf GetDragPixbuf ()
		{
			return null;
		}

		public virtual Surface GetIconSurface (Surface similar, int targetSize, out int actualSize)
		{
			Surface sr;
			do {
				switch (ScalingType) {
				case ScalingType.HighLow:
					if (targetSize == DockPreferences.IconSize) {
						actualSize = DockPreferences.IconSize;
						if (SecondaryIconSurface == null) {
							SecondaryIconSurface = MakeIconSurface (similar, actualSize);
							current_size = actualSize;
						}
						sr = SecondaryIconSurface;
						continue;
					}
					actualSize = DockPreferences.FullIconSize;
					break;
				case ScalingType.Downscaled:
					actualSize = DockPreferences.FullIconSize;
					break;
				case ScalingType.Upscaled:
				case ScalingType.None:
				default:
					actualSize = DockPreferences.IconSize;
					break;
				}
				if (IconSurface == null) {
					IconSurface = MakeIconSurface (similar, actualSize);
					current_size = actualSize;
				}
				sr = IconSurface;
			} while (false);
			
			return sr;
		}

		/// <summary>
		/// Gets a surface that is useful for display by the Dock based on the Description
		/// </summary>
		/// <param name="similar">
		/// A <see cref="Surface"/>
		/// </param>
		/// <returns>
		/// A <see cref="Surface"/>
		/// </returns>
		public virtual Surface GetTextSurface (Surface similar)
		{
			if (string.IsNullOrEmpty (Description))
				return null;
			
			if (text_surface == null || DockPreferences.Orientation != current_orientation) {
				if (text_surface != null)
					text_surface.Destroy ();
				
				current_orientation = DockPreferences.Orientation;
				text_surface = Util.GetBorderedTextSurface (GLib.Markup.EscapeText (Description), 
				                                            DockPreferences.TextWidth, 
				                                            similar, 
				                                            current_orientation);
			}
			return text_surface;
		}

		protected virtual Surface MakeIconSurface (Surface similar, int size)
		{
			current_size = size;
			Surface tmp_surface = similar.CreateSimilar (Cairo.Content.ColorAlpha, size, size);
			Context cr = new Context (tmp_surface);
			
			Gdk.Pixbuf pbuf = GetSurfacePixbuf (size);
			if (pbuf != null) {
				if (pbuf.Width != size || pbuf.Height != size) {
					double scale = (double)size / Math.Max (pbuf.Width, pbuf.Height);
					Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), Gdk.InterpType.Bilinear);
					pbuf.Dispose ();
					pbuf = temp;
				}
			
				Gdk.CairoHelper.SetSourcePixbuf (cr, 
				                                 pbuf, 
				                                 (size - pbuf.Width) / 2,
				                                 (size - pbuf.Height) / 2);
				cr.Paint ();
			
				pbuf.Dispose ();
			}
			(cr as IDisposable).Dispose ();
			
			return tmp_surface;
		}

		void OnIconSizeChanged ()
		{
			if (size_changed_timer > 0)
				GLib.Source.Remove (size_changed_timer);
			
			if (ScalingType == ScalingType.HighLow) {
				ResetSurfaces ();
			} else if (IconSurface != null) {
				if (resize_buffer == null)
					resize_buffer = CopySurface (IconSurface, current_size, current_size);
				
				Surface new_surface = resize_buffer.CreateSimilar (Cairo.Content.ColorAlpha, 
				                                                  DockPreferences.FullIconSize, 
				                                                  DockPreferences.FullIconSize);
				using (Context cr = new Context (new_surface)) {
					double scale;
					if (ScalingType == ScalingType.Downscaled)
						scale = (double) DockPreferences.FullIconSize / (double) current_size;
					else
						scale = (double) DockPreferences.IconSize / (double) current_size;
					cr.Scale (scale, scale);
					resize_buffer.Show (cr, 0, 0);
				}
				IconSurface.Destroy ();
				IconSurface = new_surface;
			}
			
			size_changed_timer = GLib.Timeout.Add (150, delegate {
				ResetSurfaces ();
				size_changed_timer = 0;
				return false;
			});
		}

		protected void OnUpdateNeeded (UpdateRequestArgs args) 
		{
			if (UpdateNeeded != null)
				UpdateNeeded (this, args);
		}

		public virtual bool ReceiveItem (string item) 
		{
			return false;
		}

		protected void RedrawIcon ()
		{
			if (size_changed_timer > 0 || redraw_timer > 0)
				return;
			if (IconSurface != null) {
				redraw_timer = GLib.Idle.Add (delegate {
					Surface similar = IconSurface;
					Surface second = SecondaryIconSurface;
					IconSurface = null;
					SecondaryIconSurface = null;
					
					if (similar != null && ((similar.Status & Cairo.Status.SurfaceFinished) != Cairo.Status.SurfaceFinished)) {
						switch (ScalingType) {
						case ScalingType.HighLow:
							IconSurface = MakeIconSurface (similar, DockPreferences.FullIconSize);
							SecondaryIconSurface = MakeIconSurface (similar, DockPreferences.IconSize);
							break;
						case ScalingType.Downscaled:
							IconSurface = MakeIconSurface (similar, DockPreferences.FullIconSize);
							break;
						case ScalingType.Upscaled:
						case ScalingType.None:
						default:
							IconSurface = MakeIconSurface (similar, DockPreferences.IconSize);
							break;
						}
					}
					
					if (similar != null)
						similar.Destroy ();
					if (second != null)
						second.Destroy ();
					
					OnUpdateNeeded (new UpdateRequestArgs (this, UpdateRequestType.IconChanged));
					redraw_timer = 0;
					return false;
				});
			} else {
				ResetIconSurface ();
				OnUpdateNeeded (new UpdateRequestArgs (this, UpdateRequestType.IconChanged));
			}
		}
		
		void ResetBufferSurface ()
		{
			if (resize_buffer != null) {
				resize_buffer.Destroy ();
				resize_buffer = null;
			}
		}

		void ResetIconSurface ()
		{
			if (IconSurface != null) {
				IconSurface.Destroy ();
				IconSurface = null;
			}
			
			if (SecondaryIconSurface != null) {
				SecondaryIconSurface.Destroy ();
				SecondaryIconSurface = null;
			}
		}

		protected virtual void ResetSurfaces ()
		{
			ResetTextSurface ();
			ResetBufferSurface ();
			ResetIconSurface ();
			average_color = null;
		}

		void ResetTextSurface ()
		{
			if (text_surface != null) {
				text_surface.Destroy ();
				text_surface = null;
			}
		}
		
		public virtual void HotSeatRequested ()
		{
		}

		/// <summary>
		/// Called whenever an icon get repositioned, so it can update its child applications icon regions
		/// </summary>
		public virtual void SetIconRegion (Gdk.Rectangle region)
		{
		}

		protected void SetText (string text)
		{
			Description = text;
			ResetTextSurface ();
		}

		#region IDisposable implementation 
		
		public virtual void Dispose ()
		{
			Disposed = true;
			DockPreferences.IconSizeChanged -= OnIconSizeChanged;
			ResetSurfaces ();
		}
		
		#endregion 
		
		public virtual bool Equals (AbstractDockItem other)
		{
			return other == this;
		}
	}
}
