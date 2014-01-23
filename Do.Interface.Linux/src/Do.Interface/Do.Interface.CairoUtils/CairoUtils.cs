// CairoUtils.cs
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

using Cairo;
using Gdk;

using Do.Interface;
using Do.Universe;

namespace Do.Interface.CairoUtils
{
	
	public static class CairoUtils
	{
		/// <summary>
		/// Sets a rounded rectangle path of the context
		/// </summary>
		/// <param name="cr">
		/// A <see cref="Context"/>
		/// </param>
		/// <param name="x">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="y">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="width">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="height">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="radius">
		/// A <see cref="System.Double"/>
		/// </param>
		public static void SetRoundedRectanglePath (this Context cr, double x, double y, 
		                                            double width, double height, double radius)
		{
			cr.MoveTo (x+radius, y);
			cr.Arc (x+width-radius, y+radius, radius, Math.PI*1.5, Math.PI*2);
			cr.Arc (x+width-radius, y+height-radius, radius, 0, Math.PI*.5);
			cr.Arc (x+radius, y+height-radius, radius, Math.PI*.5, Math.PI);
			cr.Arc (x+radius, y+radius, radius, Math.PI, Math.PI*1.5);
		}
		
		
		/// <summary>
		/// Set a rounded rectangle path from a Gdk.Rectangle.  If stroke is set to true, the path will be
		/// adjusted to allow for stroking with a single line width.
		/// </summary>
		/// <param name="cr">
		/// A <see cref="Context"/>
		/// </param>
		/// <param name="region">
		/// A <see cref="Gdk.Rectangle"/>
		/// </param>
		/// <param name="radius">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="stroke">
		/// A <see cref="System.Boolean"/>
		/// </param>
		public static void SetRoundedRectanglePath (this Context cr, Gdk.Rectangle region, double radius, bool stroke)
		{
			if (stroke)
				SetRoundedRectanglePath (cr, (double)region.X+.5, (double)region.Y+.5, (double)region.Width-1,
				                         (double)region.Height-1, radius);
			else
				SetRoundedRectanglePath (cr, region.X, region.Y, region.Width, region.Height, radius);
		}
		
		public static void AlphaFill (this Context cr)
		{
			cr.Save ();
			cr.SetSourceRGBA (0, 0, 0, 0);
			cr.Operator = Operator.Source;
			cr.Paint ();
			cr.Restore ();
		}
		
		/// <summary>
		/// Convert a Gdk.Color to Cairo.Colo
		/// </summary>
		/// <param name="color">
		/// A <see cref="Gdk.Color"/>
		/// </param>
		/// <param name="alpha">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="Cairo.Color"/>
		/// </returns>
		public static Cairo.Color ConvertToCairo (this Gdk.Color color, double alpha)
		{
			return new Cairo.Color ((double) color.Red/ushort.MaxValue,
			                        (double) color.Green/ushort.MaxValue,
			                        (double) color.Blue/ushort.MaxValue,
			                        alpha);
		}
		
		/// <summary>
		/// Convert a Cairo.Color to a Gdk.Color
		/// </summary>
		/// <param name="color">
		/// A <see cref="Cairo.Color"/>
		/// </param>
		/// <returns>
		/// A <see cref="Gdk.Color"/>
		/// </returns>
		public static Gdk.Color ConvertToGdk (this Cairo.Color color)
		{
			return new Gdk.Color (Convert.ToByte (color.R*byte.MaxValue),
			                      Convert.ToByte (color.G*byte.MaxValue),
			                      Convert.ToByte (color.B*byte.MaxValue));
		}
		
		/// <summary>
		/// Adjust the brightness of a Color
		/// </summary>
		/// <param name="color">
		/// A <see cref="Cairo.Color"/>
		/// </param>
		/// <param name="brightness">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="Cairo.Color"/>
		/// </returns>
		public static Cairo.Color ShadeColor (this Cairo.Color color, double brightness)
		{
			Gdk.Color gdk_color = ConvertToGdk (color);
			
			byte r, g, b; 
			double h, s, v;
			
			r = (byte) ((gdk_color.Red)   >> 8);
			g = (byte) ((gdk_color.Green) >> 8);
			b = (byte) ((gdk_color.Blue)  >> 8);
			
			Util.Appearance.RGBToHSV (r, g, b, out h, out s, out v);
			v = Math.Min (100, v * brightness);
			Util.Appearance.HSVToRGB (h, s, v, out r, out g, out b);
			
			return new Cairo.Color ((double) r/byte.MaxValue,
			                        (double) g/byte.MaxValue,
			                        (double) b/byte.MaxValue,
			                        color.A);
		}
		
		/// <summary>
		/// Adjust the saturation of a color
		/// </summary>
		/// <param name="color">
		/// A <see cref="Cairo.Color"/>
		/// </param>
		/// <param name="saturation">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="Cairo.Color"/>
		/// </returns>
		public static Cairo.Color SaturateColor (this Cairo.Color color, double saturation)
		{
			Gdk.Color gdk_color = ConvertToGdk (color);
			
			byte r, g, b; 
			double h, s, v;
			
			r = (byte) ((gdk_color.Red)   >> 8);
			g = (byte) ((gdk_color.Green) >> 8);
			b = (byte) ((gdk_color.Blue)  >> 8);
			
			Util.Appearance.RGBToHSV (r, g, b, out h, out s, out v);
			s *= saturation;
			Util.Appearance.HSVToRGB (h, s, v, out r, out g, out b);
			
			return new Cairo.Color ((double) r/byte.MaxValue,
			                        (double) g/byte.MaxValue,
			                        (double) b/byte.MaxValue,
			                        color.A);
		}
		
		/// <summary>
		/// Adjust the Hue of a color
		/// </summary>
		/// <param name="color">
		/// A <see cref="Cairo.Color"/>
		/// </param>
		/// <param name="hue">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="Cairo.Color"/>
		/// </returns>
		public static Cairo.Color SetHue (this Cairo.Color color, double hue)
		{
			if (hue <= 0 || hue > 360)
				return color;
			
			Gdk.Color gdk_color = ConvertToGdk (color);
			
			byte r, g, b; 
			double h, s, v;
			
			r = (byte) ((gdk_color.Red)   >> 8);
			g = (byte) ((gdk_color.Green) >> 8);
			b = (byte) ((gdk_color.Blue)  >> 8);
			
			Util.Appearance.RGBToHSV (r, g, b, out h, out s, out v);
			h = hue;
			Util.Appearance.HSVToRGB (h, s, v, out r, out g, out b);
			
			return new Cairo.Color ((double) r/byte.MaxValue,
			                        (double) g/byte.MaxValue,
			                        (double) b/byte.MaxValue,
			                        color.A);
		}
		
		/// <summary>
		/// Colorize grey colors to allow for better theme matching.  Colors with saturation will
		/// have nothing done to them
		/// </summary>
		/// <param name="color">
		/// A <see cref="Cairo.Color"/>
		/// </param>
		/// <param name="hue">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="Cairo.Color"/>
		/// </returns>
		public static Cairo.Color ColorizeColor (this Cairo.Color color, Cairo.Color reference_color)
		{
			//Color has no saturation to it, we need to give it some
			if (color.B == color.G && color.B == color.R) {
				Gdk.Color gdk_color0 = ConvertToGdk (color);
				Gdk.Color gdk_color1 = ConvertToGdk (reference_color);
				byte r0, g0, b0, r1, g1, b1; 
				double h0, s0, v0, h1, s1, v1;
				
				r0 = (byte) ((gdk_color0.Red)   >> 8);
				g0 = (byte) ((gdk_color0.Green) >> 8);
				b0 = (byte) ((gdk_color0.Blue)  >> 8);
				r1 = (byte) ((gdk_color1.Red)   >> 8);
				g1 = (byte) ((gdk_color1.Green) >> 8);
				b1 = (byte) ((gdk_color1.Blue)  >> 8);
				
				Util.Appearance.RGBToHSV (r0, g0, b0, out h0, out s0, out v0);
				Util.Appearance.RGBToHSV (r1, g1, b1, out h1, out s1, out v1);
				h0 = h1;
				s0 = (s0+s1)/2;
				Util.Appearance.HSVToRGB (h0, s0, v0, out r0, out g0, out b0);
				
				return new Cairo.Color ((double) r0/byte.MaxValue,
				                        (double) g0/byte.MaxValue,
				                        (double) b0/byte.MaxValue,
				                        color.A);
			} else { //color is already saturated in some manner, do nothing
				return color;
			}
		}
		
		/// <summary>
		/// Set a color to use a maximum value
		/// </summary>
		/// <param name="gdk_color">
		/// A <see cref="Gdk.Color"/>
		/// </param>
		/// <param name="max_value">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="Gdk.Color"/>
		/// </returns>
		public static Gdk.Color SetMaximumValue (this Gdk.Color gdk_color, double max_value)
		{
			byte r, g, b; 
			double h, s, v;
			
			r = (byte) ((gdk_color.Red)   >> 8);
			g = (byte) ((gdk_color.Green) >> 8);
			b = (byte) ((gdk_color.Blue)  >> 8);
			
			Util.Appearance.RGBToHSV (r, g, b, out h, out s, out v);
			v = Math.Min (v, max_value);
			Util.Appearance.HSVToRGB (h, s, v, out r, out g, out b);
			
			return new Gdk.Color (r, g, b);
		}
		
		/// <summary>
		/// Set a color to use a maximum value
		/// </summary>
		/// <param name="gdk_color">
		/// A <see cref="Gdk.Color"/>
		/// </param>
		/// <param name="max_value">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="Gdk.Color"/>
		/// </returns>
		public static Gdk.Color SetMinimumValue (this Gdk.Color gdk_color, double min_value)
		{
			byte r, g, b; 
			double h, s, v;
			
			r = (byte) ((gdk_color.Red)   >> 8);
			g = (byte) ((gdk_color.Green) >> 8);
			b = (byte) ((gdk_color.Blue)  >> 8);
			
			Util.Appearance.RGBToHSV (r, g, b, out h, out s, out v);
			v = Math.Max (v, min_value);
			Util.Appearance.HSVToRGB (h, s, v, out r, out g, out b);
			
			return new Gdk.Color (r, g, b);
		}
		
		/// <summary>
		/// Convert a Gdk.color to a hex string
		/// </summary>
		/// <param name="gdk_color">
		/// A <see cref="Gdk.Color"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string ColorToHexString (this Gdk.Color gdk_color)
		{
			byte r, g, b;
			r = (byte) ((gdk_color.Red)   >> 8);
			g = (byte) ((gdk_color.Green) >> 8);
			b = (byte) ((gdk_color.Blue)  >> 8);
			
			return string.Format ("{0:X2}{1:X2}{2:X2}", r, g, b);
		}

		public static void SetSourceRGBA(this Cairo.Context cr, Cairo.Color color)
		{
			cr.SetSourceRGBA (color.R, color.G, color.B, color.A);
		}

		public static void SetSourceRGB(this Cairo.Context cr, Gdk.Color color)
		{
			cr.SetSourceRGBA (color.ConvertToCairo(1));
		}

		/// <summary>
		/// Helper to create a surface similar to the current target of a context
		/// </summary>
		/// <returns>A <see cref="Cairo.Surface"/>. Caller owns this surface, and must dispose it.</returns>
		/// <param name="cr">Context</param>
		/// <param name="width">Width for new surface</param>
		/// <param name="height">Height for new surface</param>
		public static Surface CreateSimilarToTarget(this Cairo.Context cr, int width, int height)
		{
#if USING_OLD_CAIRO
			return cr.GetTarget().CreateSimilar (cr.GetTarget().Content, width, height);
#else
			using (var targetSurface = cr.GetTarget()) {
				return targetSurface.CreateSimilar (targetSurface.Content, width, height);
			}
#endif
		}
	}
}
