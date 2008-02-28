/* BaseDoWindow.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Mono.Unix;

using Do.Universe;
using Gdk;
using Gtk;

namespace Do.Addins
{
	
	
	public class BaseDoGtkWindow : Gtk.Window
	{
		public new event DoEventKeyDelegate KeyPressEvent;
		
		public BaseDoGtkWindow() : base (Gtk.WindowType.Toplevel)
		{
		}
		
		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Tab || (evnt.Key == Gdk.Key.Up && resultsWindow.SelectedIndex <= 0))
				resultsWindow.Hide ();
			
			
			
			KeyPressEvent (evnt);
			
			return base.OnKeyPressEvent (evnt);
		}
		
		private void RGBToHSV (ref byte r, ref byte g, ref byte b)
		{
			// Ported from Murrine Engine.
			double red, green, blue;
			double hue = 0, lum, sat;
			double max, min;
			double delta;
			
			red = (double) r;
			green = (double) g;
			blue = (double) b;
			
			max = Math.Max (red, Math.Max (blue, green));
			min = Math.Min (red, Math.Min (blue, green));
			delta = max - min;
			lum = max / 255.0 * 100.0;
			
			if (Math.Abs (delta) < 0.0001) {
				lum = 0;
				sat = 0;
			} else {
				sat = (delta / max) * 100;
				
				if (red == max)   hue = (green - blue) / delta;
				if (green == max) hue = 2 + (blue - red) / delta;
				if (blue == max)  hue = 4 + (red - green) / delta;
				
				hue *= 60;
				if (hue <= 0) hue += 360;
			}
			r = (byte) hue;
			g = (byte) sat;
			b = (byte) lum;
		}
		
		private void HSVToRGB (ref byte hue, ref byte sat, ref byte val)
		{
			double h, s, v;
			double r = 0, g = 0, b = 0;

			h = (double) hue;
			s = (double) sat / 100;
			v = (double) val / 100;

			if (s == 0) {
				r = v;
				g = v;
				b = v;
			} else {
				int secNum;
				double fracSec;
				double p, q, t;
				
				secNum = (int) Math.Floor(h / 60);
				fracSec = h/60 - secNum;

				p = v * (1 - s);
				q = v * (1 - s*fracSec);
				t = v * (1 - s*(1 - fracSec));

				switch (secNum) {
					case 0:
						r = v;
						g = t;
						b = p;
						break;
					case 1:
						r = q;
						g = v;
						b = p;
						break;
					case 2:
						r = p;
						g = v;
						b = t;
						break;
					case 3:
						r = p;
						g = q;
						b = v;
						break;
					case 4:
						r = t;
						g = p;
						b = v;
						break;
					case 5:
						r = v;
						g = p;
						b = q;
						break;
				}
			}
			hue = Convert.ToByte(r*255);
			sat = Convert.ToByte(g*255);
			val = Convert.ToByte(b*255);
		}
	}
}
