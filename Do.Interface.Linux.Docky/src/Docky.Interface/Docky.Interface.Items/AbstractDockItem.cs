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

using Cairo;
using Gdk;
using Gtk;

using Do.Interface;
using Do.Platform;

using Docky.Utilities;

namespace Docky.Interface
{

	
	public abstract class AbstractDockItem : IDisposable, IEquatable<AbstractDockItem>
	{
		public event UpdateRequestHandler UpdateNeeded;
		
		Surface text_surface, resize_buffer;
		DockOrientation current_orientation;
		uint size_changed_timer;
		bool needs_attention;
		
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
		public virtual DateTime LastClick { get; protected set; }
		
		public int Position { get; set; }

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
		
		/// <value>
		/// Whether or not to draw an application present indicator
		/// </value>
		public virtual int WindowCount {
			get { return 0; }
		}
		
		public TimeSpan TimeSinceClick {
			get { return DockArea.RenderTime - LastClick; }
		}
		
		public TimeSpan TimeSinceAdd {
			get { return DockArea.RenderTime - DockAddItem; }
		}
		
		public AbstractDockItem ()
		{
			NeedsAttention = false;
			Description = "";
			AttentionRequestStartTime =  LastClick = new DateTime (0);
			
			DockPreferences.IconSizeChanged += OnIconSizeChanged;
			DockWindow.Window.StyleSet += HandleStyleSet; 
		}

		void HandleStyleSet(object o, StyleSetArgs args)
		{
			ResetSurfaces ();
		}

		protected abstract Pixbuf GetSurfacePixbuf (int size);

		/// <summary>
		/// Called whenever the icon receives a click event
		/// </summary>
		/// <param name="button">
		/// A <see cref="System.UInt32"/>
		/// </param>
		/// <param name="controller">
		/// A <see cref="IDoController"/>
		/// </param>
		public virtual void Clicked (uint button, ModifierType state, Gdk.Point position)
		{
			LastClick = DateTime.UtcNow;
		}
		
		public virtual void Scrolled (Gdk.ScrollDirection direction)
		{
		}

		Surface CopySurface (Surface source, int width, int height)
		{
			Surface sr = source.CreateSimilar (source.Content, width, height);
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
			switch (ScalingType) {
			case ScalingType.HighLow:
				if (targetSize == DockPreferences.IconSize) {
					actualSize = DockPreferences.IconSize;
					return (SecondaryIconSurface != null) ? SecondaryIconSurface 
						: SecondaryIconSurface = MakeIconSurface (similar, actualSize);
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
			return (IconSurface != null) ? IconSurface 
					: IconSurface = MakeIconSurface (similar, actualSize);
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
			Surface tmp_surface = similar.CreateSimilar (similar.Content, size, size);
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
				
				Surface new_surface = resize_buffer.CreateSimilar (resize_buffer.Content, 
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
			ResetIconSurface ();
			OnUpdateNeeded (new UpdateRequestArgs (this, UpdateRequestType.IconChanged));
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
