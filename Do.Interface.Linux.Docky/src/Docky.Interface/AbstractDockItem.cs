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

	
	public abstract class AbstractDockItem : IDockItem
	{
		Surface text_surface;
		#region IDockItem implementation 
		
		public abstract Surface GetIconSurface (Surface sr);
		
		public virtual Surface GetTextSurface (Surface similar)
		{
			if (text_surface == null)
				text_surface = Util.GetBorderedTextSurface (Description, DockPreferences.TextWidth, similar);
			return text_surface;
		}
		
		public virtual void Clicked (uint button, IDoController controller)
		{
		}
		
		public virtual void SetIconRegion (Gdk.Rectangle region)
		{
		}
		
		public abstract string Description {
			get;		
		}
		
		public virtual int Width {
			get {
				return DockPreferences.IconSize;
			}
		}
		
		public virtual int Height {
			get {
				return DockPreferences.IconSize;
			}
		}
		
		public abstract bool Scalable {
			get;
		}
		
		public abstract bool DrawIndicator {
			get;
		}
		
		public virtual DateTime LastClick {
			get;
			protected set;
		}
		
		public virtual DateTime DockAddItem {
			get;
			set;
		}
		
		#endregion 
		

		
		public AbstractDockItem ()
		{
			LastClick = DateTime.UtcNow - new TimeSpan (0, 10, 0);
			
			DockPreferences.IconSizeChanged += OnIconSizeChanged;
		}
		
		protected virtual void OnIconSizeChanged ()
		{
			if (text_surface != null) {
				text_surface.Destroy ();
				text_surface = null;
			}
		}

		#region IDisposable implementation 
		
		public virtual void Dispose ()
		{
			if (text_surface != null) {
				text_surface.Destroy ();
				text_surface = null;
			}
		}
		
		#endregion 
		
		public virtual bool Equals (IDockItem other)
		{
			return other == this;
		}
		
	}
}
