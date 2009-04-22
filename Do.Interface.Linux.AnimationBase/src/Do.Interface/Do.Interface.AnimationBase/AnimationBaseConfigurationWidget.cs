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
using Gtk;
using Cairo;

using Do.Interface.CairoUtils;

namespace Do.Interface.AnimationBase
{
	
	
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AnimationBaseConfigurationWidget : Gtk.Bin
	{
		bool setup = false;
		BezelDrawingArea drawing_area;
		
		public AnimationBaseConfigurationWidget (BezelDrawingArea drawingArea)
		{
			Build ();
			
			drawing_area = drawingArea;
			SetupButtons ();
		}
		
		void SetupButtons ()
		{
			setup = true;
			
			shadow_check.Active = BezelDrawingArea.DrawShadow;
			animation_check.Active = BezelDrawingArea.Animated;
			
			background_colorbutton.Color = drawing_area.BackgroundColor.ConvertToGdk ();
			background_colorbutton.Alpha = (ushort) (drawing_area.BackgroundColor.A * ushort.MaxValue);
			
			Gtk.Application.Invoke (delegate { setup = false; });
		}

		protected virtual void OnShadowCheckToggled (object sender, System.EventArgs e)
		{
			if (setup) return;
			BezelDrawingArea.DrawShadow = shadow_check.Active;
		}

		protected virtual void OnAnimationCheckToggled (object sender, System.EventArgs e)
		{
			if (setup) return;
			BezelDrawingArea.Animated = animation_check.Active;
		}

		protected virtual void OnBackgroundColorbuttonColorSet (object sender, System.EventArgs e)
		{
			if (setup) return;
			string hex_string = string.Format ("{0}{1:X}", background_colorbutton.Color.ColorToHexString (), (byte) (background_colorbutton.Alpha >> 8));
			BezelDrawingArea.BgColor = hex_string;
		}

		protected virtual void OnClearBackgroundClicked (object sender, System.EventArgs e)
		{
			BezelDrawingArea.ResetBackgroundStyle ();
			background_colorbutton.Color = drawing_area.BackgroundColor.ConvertToGdk ();
			background_colorbutton.Alpha = (ushort) (drawing_area.BackgroundColor.A * ushort.MaxValue);
		}
	}
}
