
using System;
using System.Collections.Generic;
using System.Linq;

using Cairo;
using Gdk;
using Gtk;

using Do.Interface;
using Do.Interface.CairoUtils;
using Docky.Utilities;

namespace Docky
{
	
	
	public class PoofWindow
	{
		TimeSpan run_length = new TimeSpan (0, 0, 0, 0, 300);
		
		Gdk.Pixbuf poof;
		Gtk.Window window;
		
		int size;
		int x, y;
		
		DateTime run_time;
		
		double AnimationState {
			get { 
				return Math.Max (0, Math.Min (1, (DateTime.UtcNow - run_time).TotalMilliseconds / run_length.TotalMilliseconds));
			}
		}
		
		public PoofWindow (int size)
		{
			this.size = size;
		}
		
		public void SetCenterPosition (Gdk.Point point)
		{
			x = point.X - (size / 2);
			y = point.Y - (size / 2);
		}
		
		public void Run ()
		{
			window = new Gtk.Window (Gtk.WindowType.Toplevel);
			poof = new Pixbuf (GetType ().Assembly, "poof.png");
			
			window.AppPaintable = true;
			window.Resizable = false;
			window.KeepAbove = true;
			window.CanFocus = false;
			window.TypeHint = WindowTypeHint.Splashscreen;
			window.SetCompositeColormap ();
			
			window.Realized += delegate { window.GdkWindow.SetBackPixmap (null, false); };
			
			window.SetSizeRequest (size, size);
			window.ExposeEvent += HandleExposeEvent;
			
			GLib.Timeout.Add (30, delegate {
				if (AnimationState == 1) {
					window.Hide ();
					window.Destroy ();
					poof.Dispose ();
					return false;
				} else {
					window.QueueDraw ();
					return true;
				}
			});
			
			window.Move (x, y);
			window.ShowAll ();
			run_time = DateTime.UtcNow;
		}

		void HandleExposeEvent (object o, ExposeEventArgs args)
		{
			using (Cairo.Context cr = CairoHelper.Create (window.GdkWindow)) {
				cr.Scale ((double) size / 128, (double) size / 128);
				cr.AlphaFill ();
				int offset;
				switch ((int) Math.Floor (5 * AnimationState)) {
				case 0:
					offset = 0;
					break;
				case 1:
					offset = 128;
					break;
				case 2:
					offset = 128 * 2;
					break;
				case 3:
					offset = 128 * 3;
					break;
				default:
					offset = 128 * 4;
					break;
				}
				
				CairoHelper.SetSourcePixbuf (cr, poof, 0, -(offset));
				cr.Paint ();
			}
		}
	}
}
