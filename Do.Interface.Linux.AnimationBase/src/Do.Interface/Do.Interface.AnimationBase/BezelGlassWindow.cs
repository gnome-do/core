// BezelGlassWindow.cs
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

using Gtk;
using Gdk;

using Cairo;
using Do.Interface;
using Do.Universe;

namespace Do.Interface.AnimationBase
{
	
	
	public class BezelGlassWindow : Gtk.Window
	{
		
		public BezelGlassWindow (BezelGlassResults results) : base(Gtk.WindowType.Toplevel)
		{
			Decorated = false;
			AppPaintable = true;
			KeepAbove = true;
			DoubleBuffered = false;
			
			TypeHint = WindowTypeHint.Splashscreen;
			Util.Appearance.SetColormap (this);
			
			Add (results);
			results.Show ();
			
			Realized += delegate { GdkWindow.SetBackPixmap (null, false); };
			
			StyleSet += delegate {
				if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			};
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			if (IsDrawable) {
				Cairo.Context cr = Gdk.CairoHelper.Create (GdkWindow);
				cr.Operator = Cairo.Operator.Source;
				cr.Paint ();
				(cr as IDisposable).Dispose ();
			}
			return base.OnExposeEvent (evnt);
		}
	}
}
