//  
//  Copyright (C) 2009 GNOME Do
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Linq;

using Gdk;
using Cairo;

using Do.Interface;

using Docky.Core;
using Docky.Interface;
using Docky.Utilities;

namespace Docky.Interface.Painters
{
	
	
	public abstract class AbstractIntegratedPainter : IDockPainter
	{
		protected const int BorderSize = 10;
		int buffer_height = 0;
		
		Surface icon_surface, buffer;
		AbstractDockItem dock_item;
		
		protected abstract int Width { get; }
		
		protected virtual bool NeedsRepaint {
			get { return false; }
		}
		
		protected abstract void PaintArea (Cairo.Context context, Gdk.Rectangle paintableArea);
		
		protected virtual bool ReceiveClick (Gdk.Rectangle paintArea, Gdk.Point cursor)
		{
			return true;
		}
		
		#region IDockPainter implementation 
		
		public event EventHandler HideRequested;
		
		public event EventHandler<PaintNeededArgs> PaintNeeded;
		
		public event EventHandler ShowRequested;
		
		public void Clicked (Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
			if (!dockArea.Contains (cursor) || ReceiveClick (dockArea, new Gdk.Point (cursor.X - dockArea.Left, cursor.Y - dockArea.Top)))
				OnHideRequested ();
		}
		
		public virtual bool DoubleBuffer {
			get { return false; }
		}
		
		public virtual bool Interruptable {
			get { return true; }
		}
		
		public int MinimumWidth {
			get {
				return Width + DockPreferences.FullIconSize + 2 * BorderSize;
			}
		}
		
		public AbstractIntegratedPainter (AbstractDockItem dockItem)
		{
			dock_item = dockItem;
			DockPreferences.IconSizeChanged +=HandleIconSizeChanged; 
			dockItem.UpdateNeeded +=HandleUpdateNeeded; 
		}

		void HandleUpdateNeeded(object sender, UpdateRequestArgs args)
		{
			if (icon_surface != null)
				icon_surface.Destroy ();
			icon_surface = null;
			
			OnPaintNeeded (new PaintNeededArgs ());
		}

		void HandleIconSizeChanged()
		{
			if (icon_surface != null)
				icon_surface.Destroy ();
			icon_surface = null;
		}
		
		public virtual void Interrupt ()
		{
			OnHideRequested ();
		}
		
		public void Paint (Cairo.Context cr, Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
			if (buffer == null || buffer_height != dockArea.Height || NeedsRepaint) {
				if (buffer != null)
					buffer.Destroy ();
				
				buffer = cr.Target.CreateSimilar (cr.Target.Content, Width, dockArea.Height);
				using (Cairo.Context context = new Cairo.Context (buffer)) {
					PaintArea (context, new Gdk.Rectangle (0, 0, Width, dockArea.Height));
				}
				buffer_height = dockArea.Height;
			}
			
			PaintIcon (cr, dockArea);
			
			cr.Rectangle (dockArea.X, dockArea.Y, dockArea.Width, dockArea.Height);
			cr.Clip ();
			
			int x = dockArea.X + DockPreferences.FullIconSize + 2 * BorderSize;
			x = x + (dockArea.Width - MinimumWidth) / 2;
			buffer.Show (cr, x, dockArea.Y);
			cr.ResetClip ();
		}
		
		#endregion 
		
		void PaintIcon (Cairo.Context cr, Gdk.Rectangle dockArea)
		{
			if (icon_surface == null)
				icon_surface = CreateIconSurface (cr.Target);
			
			switch (DockPreferences.Orientation) {
			case DockOrientation.Top:
				icon_surface.Show (cr, dockArea.X + BorderSize, dockArea.Y + 5);
				break;
			case DockOrientation.Bottom:
				icon_surface.Show (cr, dockArea.X + BorderSize, dockArea.Y + dockArea.Height - 5 - DockPreferences.FullIconSize);
				break;
			}
		}
		
		protected virtual Surface CreateIconSurface (Surface similar)
		{
			Surface surface = similar.CreateSimilar (similar.Content,
			                                           DockPreferences.FullIconSize,
			                                           DockPreferences.FullIconSize);
			
			int actual_size;
			Surface icon_surface = dock_item.GetIconSurface (similar, DockPreferences.FullIconSize, out actual_size);
			
			using (Context context = new Context (surface)) {
				int offset = (DockPreferences.FullIconSize - actual_size) / 2;
				icon_surface.Show (context, offset, offset);
			}
			
			return surface;
		}
		
		protected void OnPaintNeeded (PaintNeededArgs args)
		{
			if (PaintNeeded != null)
				PaintNeeded (this, args);
		}
		
		protected void OnShowRequested ()
		{
			if (ShowRequested != null)
				ShowRequested (this, EventArgs.Empty);
		}
		
		protected void OnHideRequested ()
		{
			if (HideRequested != null)
				HideRequested (this, EventArgs.Empty);
		}
		
		#region IDisposable implementation 
		
		public virtual void Dispose ()
		{
		}
		
		#endregion 
	}
}
