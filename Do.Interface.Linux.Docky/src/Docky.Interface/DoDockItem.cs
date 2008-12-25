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
	
	
	public class DoDockItem : AbstractDockItem
	{
		const string DoIcon = "gnome-do";
		
		Surface icon_surface;
		#region IDockItem implementation 
		
		public override Surface GetIconSurface (Surface sr)
		{
			if (icon_surface == null) {
				icon_surface = sr.CreateSimilar (sr.Content, DockPreferences.FullIconSize, DockPreferences.FullIconSize);
				Context cr = new Context (icon_surface);
				
				Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName (DoIcon, DockPreferences.FullIconSize);
				
				Gdk.CairoHelper.SetSourcePixbuf (cr, pbuf, 0, 0);
				cr.Paint ();
				
				pbuf.Dispose ();
				(cr as IDisposable).Dispose ();
			}
			return icon_surface;
		}
		
		public override void Clicked (uint button)
		{
			if (button == 1)
				Services.Windowing.SummonMainWindow ();
		}
		
		public override string Description {
			get {
				return Mono.Unix.Catalog.GetString ("Summon GNOME Do");
			}
		}
		
		#endregion 
		
		public DoDockItem () : base ()
		{
		}
		
		protected override void OnIconSizeChanged ()
		{
			if (icon_surface != null) {
				icon_surface.Destroy ();
				icon_surface = null;
			}
			
			base.OnIconSizeChanged ();
		}

		#region IDisposable implementation 
		
		public override void Dispose ()
		{
			if (icon_surface != null) {
				icon_surface.Destroy ();
				icon_surface = null;
			}
			
			base.Dispose ();
		}
		
		#endregion 
		
		public override bool Equals (IDockItem other)
		{
			return other is DoDockItem;
		}
	}
}
