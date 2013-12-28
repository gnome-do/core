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

using Do.Interface.CairoUtils;

namespace Do.Interface
{

	public delegate void DoEventKeyDelegate (Gdk.EventKey key);
	public delegate void NullEventHandler ();
	public delegate void SearchStartedEventHandler (bool upstream_search);
	public delegate void SearchFinishedEventHandler (object controller, SearchFinishState state);
	
	public static class Util
	{
		public static string FormatCommonSubstrings (string main, string other, string format)
		{
			int pos, len, match_pos, last_main_cut;
			string lower_main, result;
			string skipped, matched, remainder;
			bool matchedTermination;

			result = "";
			match_pos = last_main_cut = 0;
			lower_main = main.ToLower ();
			other = other.ToLower ();

			for (pos = 0; pos < other.Length; ++pos) {
				matchedTermination = false;
				for (len = 1; len <= other.Length - pos; ++len) {
					int tmp_match_pos = lower_main.IndexOf (other.Substring (pos, len));
					if (tmp_match_pos < 0) {
						len--;
						matchedTermination = false;
						break;
					} else {
						matchedTermination = true;
						match_pos = tmp_match_pos;
					}
				}
				if (matchedTermination) {
					len--;
				}
				if (0 < len) {
					 //Theres a match starting at match_pos with positive length
					skipped = main.Substring (last_main_cut, match_pos - last_main_cut);
					matched = main.Substring (match_pos, len);
					if ( skipped.Length + matched.Length < main.Length) {
						remainder = FormatCommonSubstrings ( main.Substring (match_pos + len), other.Substring (pos + len), format);
					}
					else {
						remainder = "";
					}
					result = string.Format ("{0}{1}{2}", skipped, string.Format(format, matched), remainder);
					break;
				}
			}
			if (result == "") {
				// no matches
				result = main;
			}
			return result;
		}
		
		public static class Appearance
		{

			public static string MarkupSafeString (string s)
			{
				if (s == null) throw new ArgumentNullException ("s");
				
				return GLib.Markup.EscapeText (s);
			}
			
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
					sat = 0;
				} else {
					sat = (delta / max) * 100;
					
					if (red == max)   hue = (green - blue) / delta;
					if (green == max) hue = 2 + (blue - red) / delta;
					if (blue == max)  hue = 4 + (red - green) / delta;
					
					hue *= 60;
					if (hue < 0) hue += 360;
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
				cairo.SetSource (shadow);
				cairo.Fill ();
			}

			public static void DrawShadow (Cairo.Context cr, double x, double y, double width, 
			                                  double height, double radius, ShadowParameters shadowParams)
			{
				using (Surface sr = cr.CreateSimilarToTarget ((int)width + (int)(2*shadowParams.shadowRadius) + (int)x, 
															  (int)height + (int)(2*shadowParams.shadowRadius) + (int)y))
				{
					using (Context cairo = new Context (sr))
					{
						radius++;
						y++;
						height--;
						Cairo.Gradient shadow;
						/* Top Left */
						using (shadow = new Cairo.RadialGradient (x+radius, y+radius, radius,
																  x+radius, y+radius, radius+shadowParams.shadowRadius))
						{
							cairo.Rectangle (x-shadowParams.shadowRadius, y-shadowParams.shadowRadius,
						                 	radius+shadowParams.shadowRadius, radius+shadowParams.shadowRadius);
							FillShadowPattern (cairo, shadow, shadowParams);
						}
						/* Top */
						using (shadow = new Cairo.LinearGradient (0.0, y, 0.0, y-shadowParams.shadowRadius))
						{
							cairo.Rectangle (x+radius, y-shadowParams.shadowRadius,
							                 width-radius*2, shadowParams.shadowRadius);
							FillShadowPattern (cairo, shadow, shadowParams);
						}					
						/* Top Right */
						using (shadow = new Cairo.RadialGradient (width+x-radius, y+radius, radius,
																  width+x-radius, y+radius, radius+shadowParams.shadowRadius))
						{
							cairo.Rectangle (width+x-radius, y-shadowParams.shadowRadius,
						                 	radius+shadowParams.shadowRadius, radius+shadowParams.shadowRadius);
							FillShadowPattern (cairo, shadow, shadowParams);
						}
					
						/* Right */
						using (shadow = new Cairo.LinearGradient (width+x, 0.0,
																  width+x+shadowParams.shadowRadius, 0.0))
						{
							cairo.Rectangle (width+x, y+radius, shadowParams.shadowRadius, height-radius*2);
							FillShadowPattern (cairo, shadow, shadowParams);
						}
					
						/* Bottom Right */
						using (shadow = new Cairo.RadialGradient (width+x-radius, height+y-radius, radius,
																  width+x-radius, height+y-radius, radius+shadowParams.shadowRadius))
						{
							cairo.Rectangle (width+x-radius, height+y-radius,
							                radius+shadowParams.shadowRadius, radius+shadowParams.shadowRadius);
							FillShadowPattern (cairo, shadow, shadowParams);
						}
						
						/* Bottom */
						using (shadow = new Cairo.LinearGradient (0.0, height+y, 
																  0.0, height+y+shadowParams.shadowRadius))
						{
							cairo.Rectangle (x+radius, height+y,
							                 width-radius*2, shadowParams.shadowRadius);
							FillShadowPattern (cairo, shadow, shadowParams);
						}
						
						/* Bottom Left */
						using (shadow = new Cairo.RadialGradient (x+radius, height+y-radius, radius, 
																  x+radius, height+y-radius, radius+shadowParams.shadowRadius))
						{
							cairo.Rectangle (x-shadowParams.shadowRadius, height+y-radius,
							                 radius+shadowParams.shadowRadius, radius+shadowParams.shadowRadius);
							FillShadowPattern (cairo, shadow, shadowParams);
						}
						
						/* Left */
						using (shadow = new Cairo.LinearGradient (x, 0.0, 
																  x-shadowParams.shadowRadius, 0.0))
						{
							cairo.Rectangle (x-shadowParams.shadowRadius, y+radius, 
							                 radius+shadowParams.shadowRadius, height-radius*2);
							FillShadowPattern (cairo, shadow, shadowParams);
						}
						
						y--;
						height++;
						/* Clear inner rectangle */
						GetFrame (cairo, x, y, width, height, radius);
						cairo.Operator = Cairo.Operator.Clear;
						cairo.Fill();
						
						cr.SetSource (sr);
						cr.Paint ();
					}
				}
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
