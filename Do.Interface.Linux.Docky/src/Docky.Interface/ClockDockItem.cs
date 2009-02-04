
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

using Docky.Utilities;

namespace Docky.Interface
{
	
	
	public class ClockDockItem : BaseDockItem
	{
		Surface sr, clock_backdrop;
		DateTime last_draw;
		int minute;
		
		public override ScalingType ScalingType {
			get {
				return ScalingType.Upscaled;
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
			last_draw = new DateTime (0);
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

		public override Surface GetIconSurface (Cairo.Surface similar)
		{
			if (sr == null)
				sr = similar.CreateSimilar (similar.Content, 
				                            DockPreferences.IconSize, 
				                            DockPreferences.IconSize);
			
			if ((DateTime.UtcNow - last_draw).TotalMinutes >= 1) {
				using (Context cr = new Context (sr)) {
					cr.AlphaFill ();
					
					int center = DockPreferences.IconSize / 2;
					int radius = center;
					
					if (clock_backdrop == null) {
						clock_backdrop = similar.CreateSimilar (similar.Content,
						                                        DockPreferences.IconSize,
						                                        DockPreferences.IconSize);
						using (Cairo.Context cr2 = new Cairo.Context (clock_backdrop)) {
							RenderFileOntoContext (cr2, System.IO.Path.Combine (ThemePath, "clock-drop-shadow.svg"), radius * 2);
							RenderFileOntoContext (cr2, System.IO.Path.Combine (ThemePath, "clock-face.svg"), radius * 2);
							RenderFileOntoContext (cr2, System.IO.Path.Combine (ThemePath, "clock-marks.svg"), radius * 2);
							RenderFileOntoContext (cr2, System.IO.Path.Combine (ThemePath, "clock-face-shadow.svg"), radius * 2);
							RenderFileOntoContext (cr2, System.IO.Path.Combine (ThemePath, "clock-glass.svg"), radius * 2);
							RenderFileOntoContext (cr2, System.IO.Path.Combine (ThemePath, "clock-frame.svg"), radius * 2);
						}
					}
					
					clock_backdrop.Show (cr, 0, 0);
				
					cr.Translate (center, center);
					cr.Color = new Cairo.Color (.15, .15, .15);
					
					cr.LineCap = LineCap.Round;
					double minuteRotation = 2 * Math.PI * (DateTime.Now.Minute / 60.0) + Math.PI;
					cr.Rotate (minuteRotation);
					cr.MoveTo (0, radius - radius * .35);
					cr.LineTo (0, 0 - radius * .15);
					cr.Stroke ();
					cr.Rotate (0 - minuteRotation);
					
					cr.Color = new Cairo.Color (0, 0, 0);
					double hourRotation = 2 * Math.PI * (DateTime.Now.Hour / 12.0) + Math.PI + (Math.PI / 6) * DateTime.Now.Minute / 60.0;
					cr.Rotate (hourRotation);
					cr.MoveTo (0, radius - radius * .5);
					cr.LineTo (0, 0 - radius * .15);
					cr.Stroke ();
					cr.Rotate (0 - hourRotation);
					
					cr.Translate (0 - center, 0 - center);
				}
			}
			
			return sr;
		}
	}
}
