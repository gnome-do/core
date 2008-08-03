// TextFrame.cs
//
//  Copyright (C) 2008 Jason Smith
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using Gtk;
using Gdk;

namespace Do.UI
{
	
	
	public class TextFrame : Frame
	{
		protected Label label;
		protected string labelText;
		protected bool isFocused;
		
		protected double focusedTransparency = 0.4;
		protected double unfocusedTransparency = 0.1;
		
		public string LabelText
		{
			get { return labelText; }
			set { 
				labelText = value;
				label.Markup = string.Format("<b>{0}</b>", value);
			}
		}
		
		public TextFrame ()
		{
			Build ();
			this.LabelText = string.Empty;
			drawGradient = true;
		}
		
		protected virtual void Build ()
		{
			label = new Label ();
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.ModifyFg (StateType.Normal, Style.White);
			
			Add (label);
			label.Show ();
		}
		
		public bool IsFocused
		{
			get { return isFocused; }
			set {
				isFocused = value;
				UpdateFocus ();
			}
		}
		
		protected override Cairo.LinearGradient GetGradient ()
		{
			double r, g, b;
			
			r = (double) frameColor.Red / ushort.MaxValue;
			g = (double) frameColor.Green / ushort.MaxValue;
			b = (double) frameColor.Blue / ushort.MaxValue;
			
			Cairo.LinearGradient grad = new Cairo.LinearGradient (0, 0, x+width, 0);
			grad.AddColorStop (.1,  new Cairo.Color (r, g, b, 0));
			grad.AddColorStop (.35, new Cairo.Color (r, g, b, fillAlpha));
			grad.AddColorStop (.65, new Cairo.Color (r, g, b, fillAlpha));
			grad.AddColorStop (.9,  new Cairo.Color (r, g, b, 0));
			
			return grad;
		}

		
		protected virtual void UpdateFocus ()
		{
			FrameAlpha = FillAlpha = (isFocused ? focusedTransparency : unfocusedTransparency);
		}
	}
}
