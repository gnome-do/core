// SeparatorMenuButtonArgs.cs
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
using Gtk;

namespace Docky.Interface.Menus
{
	public class SeparatorMenuButtonArgs : WidgetMenuArgs
	{
		class CustomSeparator : HSeparator
		{
			public CustomSeparator () : base ()
			{
				HeightRequest = 2;
			}
			
			protected override bool OnExposeEvent (Gdk.EventExpose evnt)
			{
				using (Context cr = CairoHelper.Create (GdkWindow)) {
					cr.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, 1);
					cr.Color = new Cairo.Color (0, 0, 0, .2);
					cr.Fill ();
					
					cr.Rectangle (evnt.Area.X, evnt.Area.Y + 1, evnt.Area.Width, 1);
					cr.Color = new Cairo.Color (1, 1, 1, .1);
					cr.Fill ();
				}
				return true;
			}

		}
		
		public SeparatorMenuButtonArgs () : base (new CustomSeparator ())
		{
		}
		
	}
}
