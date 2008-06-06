// MiniIconBox.cs
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

using System;

using Gtk;
using Gdk;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	public class MiniIconBox : IconBox
	{
		protected HBox hbox;
		
		public MiniIconBox(int iconBoxSize) : base (iconBoxSize)
		{
			focused_transparency = 0.25f;
			unfocused_transparency = 0.0f;
			
			drawGradient = true;
		}
		
		public int Width
		{
			get {
				return icon_size * 3 + 4 + 12;
			}
		}
		
		protected override void Build ()
		{
			Alignment label_align;

			caption = "";
			pixbuf = empty_pixbuf;

			hbox = new HBox (false, 4);
			hbox.BorderWidth = 6;
			Add (hbox);
			hbox.Show ();

			empty_pixbuf = new Pixbuf (Colorspace.Rgb, true, 8, icon_size, icon_size);
			empty_pixbuf.Fill (uint.MinValue);

			image = new Gtk.Image ();
			hbox.PackStart (image, false, false, 0);
			image.Show ();

			label = new Label ();
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.ModifyFg (StateType.Normal, Style.White);
			label_align = new Alignment (1.0F, 0.5F, 0, 0);
			label_align.SetPadding (0, 0, 0, 0);
			label_align.Add (label);
			hbox.PackStart (label_align, true, true, 0);
			label.Show ();
			label_align.Show ();

			image.SetSizeRequest (icon_size, icon_size);
			label.SetSizeRequest ((int) (icon_size * 2), -1);

			DrawFill = true;
			FrameColor = FillColor = new Color (byte.MaxValue, byte.MaxValue, byte.MaxValue);

			Realized += OnRealized;
			UpdateFocus ();
		}
		
		protected override Cairo.LinearGradient GetGradient ()
		{
			double r, g, b;
			
			Cairo.LinearGradient gloss = base.GetGradient ();
			r = (double) fillColor.Red / ushort.MaxValue;
			g = (double) fillColor.Green / ushort.MaxValue;
			b = (double) fillColor.Blue / ushort.MaxValue;
			
			gloss.AddColorStop (0,   new Cairo.Color (r, g, b, 0));
			gloss.AddColorStop (.4, new Cairo.Color (r, g, b, 0));
			gloss.AddColorStop (1,   new Cairo.Color (r, g, b, fillAlpha));
			
			return gloss;
		}
	}
}
