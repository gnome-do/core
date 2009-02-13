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
		const int BorderSize = 10;
		
		Surface icon_surface;
		
		public AbstractIntegratedPainter ()
		{
			Gdk.Pixbuf icon = GetIcon (DockPreferences.FullIconSize);
		}
		
		protected abstract Gdk.Pixbuf GetIcon (int size);
		
		protected abstract void PaintArea (Cairo.Context context, Gdk.Rectangle paintableArea);
		
		protected abstract void ReceiveClick (Gdk.Rectangle paintArea, Gdk.Point cursor);
		
		#region IDockPainter implementation 
		
		public event EventHandler HideRequested;
		
		public event EventHandler<PaintNeededArgs> PaintNeeded;
		
		public event EventHandler ShowRequested;
		
		public void Clicked (Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
			ReceiveClick (GetPaintArea (dockArea), cursor);
		}
		
		public virtual bool DoubleBuffer {
			get { return false; }
		}
		
		public virtual bool Interuptable {
			get { return true; }
		}
		
		public virtual void Interupt ()
		{
		}
		
		public void Paint (Cairo.Context cr, Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
			PaintIcon (cr, dockArea);
			
			Gdk.Rectangle paintArea = GetPaintArea (dockArea);
			
			cr.Rectangle (paintArea.X, paintArea.Y, paintArea.Width, paintArea.Height);
			cr.Clip ();
			
			PaintArea (cr, paintArea);
			
			cr.ResetClip ();
		}
		
		#endregion 
		
		Gdk.Rectangle GetPaintArea (Gdk.Rectangle dockArea)
		{
			Gdk.Rectangle paintArea = dockArea;
			paintArea.X += DockPreferences.FullIconSize + 2 * BorderSize;
			paintArea.Width -= DockPreferences.FullIconSize + 2 * BorderSize;
			
			return paintArea;
		}
		
		void PaintIcon (Cairo.Context cr, Gdk.Rectangle dockArea)
		{
			if (icon_surface == null)
				icon_surface = CreateIconSurface (cr.Target);
			
			icon_surface.Show (cr, dockArea.X + BorderSize, dockArea.Y + 5);
		}
		
		protected virtual Surface CreateIconSurface (Surface similar)
		{
			Surface surface = similar.CreateSimilar (similar.Content,
			                                           DockPreferences.FullIconSize,
			                                           DockPreferences.FullIconSize);
			Gdk.Pixbuf pbuf = GetIcon (DockPreferences.FullIconSize);
			
			Context context = new Context (surface);
			CairoHelper.SetSourcePixbuf (context, 
			                             pbuf, 
			                             (DockPreferences.FullIconSize - pbuf.Width) / 2,
			                             (DockPreferences.FullIconSize - pbuf.Height) / 2);
			context.Paint ();
			
			pbuf.Dispose ();
			((IDisposable)context).Dispose ();
			
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
