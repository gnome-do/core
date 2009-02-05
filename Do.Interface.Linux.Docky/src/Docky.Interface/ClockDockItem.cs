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
using System.IO;
using System.Linq;

using Gdk;
using Cairo;
using Mono.Unix;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Platform;

namespace Docky.Interface
{
	
	
	public class ClockDockItem : BaseDockItem
	{
		int minute;
		
		public override ScalingType ScalingType {
			get {
				return ScalingType.HighLow;
			}
		}
		
		string ThemePath {
			get {
				if (Directory.Exists (System.IO.Path.Combine (Services.Paths.UserDataDirectory, "ClockTheme")))
					return System.IO.Path.Combine (Services.Paths.UserDataDirectory, "ClockTheme");
				if (Directory.Exists ("/usr/share/gnome-do/ClockTheme"))
					return "/usr/share/gnome-do/ClockTheme";
				if (Directory.Exists ("/usr/local/share/gnome-do/ClockTheme"))
					return "/usr/local/share/gnome-do/ClockTheme";
				return "";
			}
		}
		
		public ClockDockItem()
		{
			GLib.Timeout.Add (1000, ClockUpdateTimer);
		}
		
		bool ClockUpdateTimer ()
		{
			if (minute != DateTime.UtcNow.Minute) {
				SetText (DateTime.Now.ToLongDateString () + " " + DateTime.Now.ToShortTimeString ());
				RedrawIcon ();
				minute = DateTime.UtcNow.Minute;
			}
			return true;
		}
		
		protected override Pixbuf GetSurfacePixbuf (int size)
		{
			return null;
		}
		
		void RenderFileOntoContext (Context cr, string file, int size)
		{
			if (!File.Exists (file))
				return;
			
			Gdk.Pixbuf pbuf = Rsvg.Tool.PixbufFromFileAtSize (file, size, size);
			CairoHelper.SetSourcePixbuf (cr, pbuf, 0, 0);
			cr.Paint ();
			pbuf.Dispose ();
		}

		protected override Surface MakeIconSurface (Cairo.Surface similar, int size)
		{
			current_size = size;
			Surface tmp_surface = similar.CreateSimilar (similar.Content, size, size);
			
			using (Context cr = new Context (tmp_surface)) {
				cr.AlphaFill ();
				
				int center = size / 2;
				int radius = center;
				
				RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-drop-shadow.svg"), radius * 2);
				RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-face.svg"), radius * 2);
				RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-marks.svg"), radius * 2);
				RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-face-shadow.svg"), radius * 2);
				RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-glass.svg"), radius * 2);
				RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-frame.svg"), radius * 2);
				
				cr.Translate (center, center);
				cr.Color = new Cairo.Color (.15, .15, .15);
				
				cr.LineWidth = Math.Max (1, size / 48);
				cr.LineCap = LineCap.Round;
				double minuteRotation = 2 * Math.PI * (DateTime.Now.Minute / 60.0) + Math.PI;
				cr.Rotate (minuteRotation);
				cr.MoveTo (0, radius - radius * .35);
				cr.LineTo (0, 0 - radius * .15);
				cr.Stroke ();
				cr.Rotate (0 - minuteRotation);
				
				cr.Color = new Cairo.Color (0, 0, 0);
				double hourRotation = 2 * Math.PI * (DateTime.Now.Hour / 12.0) + 
					Math.PI + (Math.PI / 6) * DateTime.Now.Minute / 60.0;
				cr.Rotate (hourRotation);
				cr.MoveTo (0, radius - radius * .5);
				cr.LineTo (0, 0 - radius * .15);
				cr.Stroke ();
				cr.Rotate (0 - hourRotation);
				
				cr.Translate (0 - center, 0 - center);
			}
			
			return tmp_surface;
		}
	}
}
