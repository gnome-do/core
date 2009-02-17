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

using Cairo;
using Gdk;
using Gtk;

using Do.Interface;
using Do.Universe;
using Do.Platform;

using Docky.Utilities;

namespace Docky.Interface
{
	
	
	public class ActionDockItem : AbstractDockItem
	{
		Act action;
		Do.Universe.Item target_item;
		
		string Icon { 
			get { return action.Icon; } 
		}
		
		public ActionDockItem(Act action, Do.Universe.Item targetItem)
		{
			this.action = action;
			this.target_item = targetItem;
			SetText (action.Name);
		}
		
		protected override Pixbuf GetSurfacePixbuf (int size)
		{
			Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName (Icon, size);
			if (pbuf.Height != size && pbuf.Width != size) {
				double scale = (double)DockPreferences.FullIconSize / Math.Max (pbuf.Width, pbuf.Height);
				Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), InterpType.Bilinear);
				pbuf.Dispose ();
				pbuf = temp;
			}
			
			return pbuf;
		}
		
		public override void Clicked (uint button, ModifierType state, Gdk.Point position)
		{
			if (button == 1) {
				Services.Core.PerformActionOnItem (action, target_item);
			}
			
			base.Clicked (button, state, position);
		}

	}
}
