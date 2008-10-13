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
using System.Collections.Generic;

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
			
			public static void SetColormap (Gtk.Widget widget)
			{
				Gdk.Colormap  colormap;
				
				colormap = widget.Screen.RgbaColormap;
				if (colormap == null) {
					colormap = widget.Screen.RgbColormap;
					Console.Error.WriteLine ("No alpha support.");
				}
				
				widget.Colormap = colormap;
				colormap.Dispose ();
			}				
			
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
//					val = 0;
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
				v = Math.Min (100, v * brightness);
				Console.WriteLine (v);
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
			
			public static string ColorToHexString (Gdk.Color gdk_color)
			{
				byte r, g, b;
				r = (byte) ((gdk_color.Red)   >> 8);
				g = (byte) ((gdk_color.Green) >> 8);
				b = (byte) ((gdk_color.Blue)  >> 8);
				
				return string.Format ("{0:X}{1:X}{2:X}", r, g, b);
			}

			static void GetFrame (Cairo.Context cairo, double x, double y, double width, double height, double radius)
			{
				if (radius == 0)
				{
					cairo.MoveTo (x, y);
					cairo.Rectangle (x, y, width, height);
				} else {
					cairo.MoveTo (x+radius, y);
					cairo.Arc (x+width-radius, y+radius, radius, (Math.PI*1.5), (Math.PI*2));
					cairo.Arc (x+width-radius, y+height-radius, radius, 0, (Math.PI*0.5));
					cairo.Arc (x+radius, y+height-radius, radius, (Math.PI*0.5), Math.PI);
					cairo.Arc (x+radius, y+radius, radius, Math.PI, (Math.PI*1.5));
				}
			}

			static void GetShadowPattern (Cairo.Gradient shadow, ShadowParameters shadowParams)
			{
				double denLog = Math.Log(1.0f/shadowParams.shadowRadius);
				
				shadow.AddColorStop (0.0, new Cairo.Color (0, 0, 0, shadowParams.shadowAlpha));

				for (int i=2; i<=shadowParams.shadowRadius; i++)
				{
					double step = i/shadowParams.shadowRadius;
					shadow.AddColorStop (step, new Cairo.Color (0, 0, 0, shadowParams.shadowAlpha*(Math.Log(step)/denLog)));
				}
			}
			
			static void FillShadowPattern (Cairo.Context cairo, Cairo.Gradient shadow, ShadowParameters shadowParams)
			{
				GetShadowPattern (shadow, shadowParams);
				cairo.Pattern = shadow;
				cairo.Fill ();
			}

			public static void DrawShadow (Cairo.Context cr, double x, double y, double width, 
			                                  double height, double radius, ShadowParameters shadowParams)
			{
				Surface sr = cr.Target.CreateSimilar (cr.Target.Content, (int)width + (int)(2*shadowParams.shadowRadius) + (int)x, 
				                                      (int)height + (int)(2*shadowParams.shadowRadius) + (int)y);
				Context cairo = new Context (sr);
				
				Cairo.Gradient shadow;
				/* Top Left */
				shadow = new Cairo.RadialGradient (x+radius, y+radius, radius,
				                                   x+radius, y+radius, radius+shadowParams.shadowRadius);
				cairo.Rectangle (x-shadowParams.shadowRadius, y-shadowParams.shadowRadius,
				                 radius+shadowParams.shadowRadius, radius+shadowParams.shadowRadius);
				FillShadowPattern (cairo, shadow, shadowParams);
				shadow.Destroy ();
				
				/* Top */
				shadow = new Cairo.LinearGradient (0.0, y,
				                                   0.0, y-shadowParams.shadowRadius);
				cairo.Rectangle (x+radius, y-shadowParams.shadowRadius,
				                 width-radius*2, shadowParams.shadowRadius);
				FillShadowPattern (cairo, shadow, shadowParams);
				shadow.Destroy ();
				
				/* Top Right */
				shadow = new Cairo.RadialGradient (width+x-radius, y+radius, radius,
				                                   width+x-radius, y+radius, radius+shadowParams.shadowRadius);
				cairo.Rectangle (width+x-radius, y-shadowParams.shadowRadius,
				                 radius+shadowParams.shadowRadius, radius+shadowParams.shadowRadius);
				FillShadowPattern (cairo, shadow, shadowParams);
				shadow.Destroy ();
				
				/* Right */
				shadow = new Cairo.LinearGradient (width+x, 0.0,
				                                   width+x+shadowParams.shadowRadius, 0.0);
				cairo.Rectangle (width+x, y+radius, shadowParams.shadowRadius, height-radius*2);
				FillShadowPattern (cairo, shadow, shadowParams);
				shadow.Destroy ();
				
				/* Bottom Right */
				shadow = new Cairo.RadialGradient (width+x-radius, height+y-radius, radius,
				                                   width+x-radius, height+y-radius, radius+shadowParams.shadowRadius);
				cairo.Rectangle (width+x-radius, height+y-radius,
				                radius+shadowParams.shadowRadius, radius+shadowParams.shadowRadius);
				FillShadowPattern (cairo, shadow, shadowParams);
				shadow.Destroy ();
				
				/* Bottom */
				shadow = new Cairo.LinearGradient (0.0, height+y, 
				                                   0.0, height+y+shadowParams.shadowRadius);
				cairo.Rectangle (x+radius, height+y,
				                 width-radius*2, shadowParams.shadowRadius);
				FillShadowPattern (cairo, shadow, shadowParams);
				shadow.Destroy ();
				
				/* Bottom Left */
				shadow = new Cairo.RadialGradient (x+radius, height+y-radius, radius, 
				                                   x+radius, height+y-radius, radius+shadowParams.shadowRadius);
				cairo.Rectangle (x-shadowParams.shadowRadius, height+y-radius,
				                 radius+shadowParams.shadowRadius, radius+shadowParams.shadowRadius);
				FillShadowPattern (cairo, shadow, shadowParams);
				shadow.Destroy ();
				
				/* Left */
				shadow = new Cairo.LinearGradient (x, 0.0, 
				                                   x-shadowParams.shadowRadius, 0.0);
				cairo.Rectangle (x-shadowParams.shadowRadius, y+radius, 
				                 radius+shadowParams.shadowRadius, height-radius*2);
				FillShadowPattern (cairo, shadow, shadowParams);
				shadow.Destroy ();
				
				/* Clear inner rectangle */
				GetFrame (cairo, x, y, width, height, radius);
				cairo.Operator = Cairo.Operator.Clear;
				cairo.Fill();
				
				cr.SetSource (sr);
				cr.Paint ();
				
				(cairo as IDisposable).Dispose ();
				(sr as IDisposable).Dispose ();
			}
		}
		
		public class ShadowParameters
		{
			public double shadowAlpha = 0.325;
			public double shadowRadius = 15f;
			
			public ShadowParameters (double shadowAlpha, double shadowRadius)
			{
				this.shadowAlpha = shadowAlpha;
				this.shadowRadius = shadowRadius;
			}
		}
	}
}
