// DoDockItem.cs
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
	
	
	public class DoDockItem : IDockItem
	{
		
		Surface icon_surface;
		#region IDockItem implementation 
		
		public Surface GetIconSurface (Surface sr)
		{
			if (icon_surface == null) {
				icon_surface = sr.CreateSimilar (sr.Content, DockPreferences.FullIconSize, DockPreferences.FullIconSize);
				Context cr = new Context (icon_surface);
				
				Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName ("gnome-do", DockPreferences.FullIconSize);
				
				Gdk.CairoHelper.SetSourcePixbuf (cr, pbuf, 0, 0);
				cr.Paint ();
				
				pbuf.Dispose ();
				(cr as IDisposable).Dispose ();
			}
			return icon_surface;
		}
		
		public Surface GetTextSurface (Surface similar)
		{
			return null;
		}
		
		public void Clicked (uint button, IDoController controller)
		{
			Services.Windowing.Summon ();
		}
		
		public void SetIconRegion (Gdk.Rectangle region)
		{
		}
		
		public string Description {
			get {
				return "Summon GNOME Do";
			}
		}
		
		public int Width {
			get {
				return DockPreferences.IconSize;
			}
		}
		
		public int Height {
			get {
				return DockPreferences.IconSize;
			}
		}
		
		public bool Scalable {
			get {
				return true;
			}
		}
		
		public bool DrawIndicator {
			get {
				return false;
			}
		}
		
		public DateTime LastClick {
			get;
			private set;
		}
		
		public DateTime DockAddItem {
			get;
			set;
		}
		
		#endregion 
		

		
		public DoDockItem()
		{
			LastClick = DateTime.UtcNow - new TimeSpan (0, 10, 0);
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			throw new System.NotImplementedException();
		}
		
		#endregion 
		
		public bool Equals (IDockItem other)
		{
			return other is DoDockItem;
		}
	}
}
