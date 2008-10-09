/* Util.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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

using Gdk;
using Cairo;

using Do.Universe;

namespace Do.Addins
{
	public delegate IPreferences GetPreferencesDelegate (string id);
	public delegate void EnvironmentOpenDelegate (string item);
	public delegate void PopupMainMenuAtPositionDelegate (int x, int y);
	public delegate Gdk.Pixbuf PixbufFromIconNameDelegate (string icon_name, int size);
	public delegate string StringTransformationDelegate (string old);
	public delegate string FormatCommonSubstringsDelegate (string main, string highlight, string format);
	public delegate void PresentWindowDelegate (Gtk.Window window);
	public delegate void DoEventKeyDelegate (Gdk.EventKey key);
	public delegate void NullEventHandler ();
	public delegate void SearchStartedEventHandler (bool upstream_search);
	public delegate void SearchFinishedEventHandler (object controller, SearchFinishState state);
	
	/// <summary>
	/// Useful functionality for plugins. See <see cref="Do.Util"/>.
	/// </summary>
	public static class Util
	{
		public static GetPreferencesDelegate GetPreferences;
		public static FormatCommonSubstringsDelegate FormatCommonSubstrings;
		
		public static class Environment
		{
			public static EnvironmentOpenDelegate Open;
		}
		
		public static class Appearance
		{
			public static PresentWindowDelegate PresentWindow;
			public static PixbufFromIconNameDelegate PixbufFromIconName;
			public static StringTransformationDelegate MarkupSafeString;
			public static PopupMainMenuAtPositionDelegate PopupMainMenuAtPosition;
			
			public static void RGBToHSV (byte r, byte g, byte b, 
			                             out double hue, out double sat, out double val)
			{
				// Ported from Murrine Engine.
				double red, green, blue;
				double max, min;
				double delta;
				
				red = (double) r;
				green = (double) g;
				blue = (double) b;
				
				hue = 0;
				
				max = Math.Max (red, Math.Max (blue, green));
				min = Math.Min (red, Math.Min (blue, green));
				delta = max - min;
				val = max / 255.0 * 100.0;
				
				if (Math.Abs (delta) < 0.0001) {
					val = 0;
					sat = 0;
				} else {
					sat = (delta / max) * 100;
					
					if (red == max)   hue = (green - blue) / delta;
					if (green == max) hue = 2 + (blue - red) / delta;
					if (blue == max)  hue = 4 + (red - green) / delta;
					
					hue *= 60;
					if (hue <= 0) hue += 360;
				}
			}
			
			public static void HSVToRGB (double hue, double sat, double val,
			                             out byte red, out byte green, out byte blue)
			{
				double h, s, v;
				double r = 0, g = 0, b = 0;

				h = hue;
				s = sat / 100;
				v = val / 100;

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
				red   = Convert.ToByte(r*255);
				green = Convert.ToByte(g*255);
				blue  = Convert.ToByte(b*255);
			}
			
			public static Cairo.Color ConvertToCairo (Gdk.Color color, double alpha)
			{
				return new Cairo.Color ((double) color.Red/ushort.MaxValue,
				                        (double) color.Green/ushort.MaxValue,
				                        (double) color.Blue/ushort.MaxValue,
				                        alpha);
			}
			
			public static Gdk.Color ConvertToGdk (Cairo.Color color)
			{
				return new Gdk.Color (Convert.ToByte (color.R*byte.MaxValue),
				                      Convert.ToByte (color.G*byte.MaxValue),
				                      Convert.ToByte (color.B*byte.MaxValue));
			}
			
			public static Cairo.Color ShadeColor (Cairo.Color color, double brightness)
			{
				Gdk.Color gdk_color = ConvertToGdk (color);
				
				byte r, g, b; 
				double h, s, v;
				
				r = (byte) ((gdk_color.Red)   >> 8);
				g = (byte) ((gdk_color.Green) >> 8);
				b = (byte) ((gdk_color.Blue)  >> 8);
				
				RGBToHSV (r, g, b, out h, out s, out v);
				v *= brightness;
				HSVToRGB (h, s, v, out r, out g, out b);
				
				return new Cairo.Color ((double) r/byte.MaxValue,
				                        (double) g/byte.MaxValue,
				                        (double) b/byte.MaxValue,
				                        color.A);
			}
			
			public static Cairo.Color SaturateColor (Cairo.Color color, double saturation)
			{
				Gdk.Color gdk_color = ConvertToGdk (color);
				
				byte r, g, b; 
				double h, s, v;
				
				r = (byte) ((gdk_color.Red)   >> 8);
				g = (byte) ((gdk_color.Green) >> 8);
				b = (byte) ((gdk_color.Blue)  >> 8);
				
				RGBToHSV (r, g, b, out h, out s, out v);
				s *= saturation;
				HSVToRGB (h, s, v, out r, out g, out b);
				
				return new Cairo.Color ((double) r/byte.MaxValue,
				                        (double) g/byte.MaxValue,
				                        (double) b/byte.MaxValue,
				                        color.A);
			}
			
			public static Cairo.Color SetHue (Cairo.Color color, double hue)
			{
				if (hue <= 0 || hue > 360)
					return color;
				
				Gdk.Color gdk_color = ConvertToGdk (color);
				
				byte r, g, b; 
				double h, s, v;
				
				r = (byte) ((gdk_color.Red)   >> 8);
				g = (byte) ((gdk_color.Green) >> 8);
				b = (byte) ((gdk_color.Blue)  >> 8);
				
				RGBToHSV (r, g, b, out h, out s, out v);
				h = hue;
				HSVToRGB (h, s, v, out r, out g, out b);
				
				return new Cairo.Color ((double) r/byte.MaxValue,
				                        (double) g/byte.MaxValue,
				                        (double) b/byte.MaxValue,
				                        color.A);
			}
			
			public static Gdk.Color SetMaximumValue (Gdk.Color gdk_color, double max_value)
			{
				byte r, g, b; 
				double h, s, v;
				
				r = (byte) ((gdk_color.Red)   >> 8);
				g = (byte) ((gdk_color.Green) >> 8);
				b = (byte) ((gdk_color.Blue)  >> 8);
				
				RGBToHSV (r, g, b, out h, out s, out v);
				v = Math.Min (v, max_value);
				HSVToRGB (h, s, v, out r, out g, out b);
				
				return new Gdk.Color (r, g, b);
			}
		}
	}
}
