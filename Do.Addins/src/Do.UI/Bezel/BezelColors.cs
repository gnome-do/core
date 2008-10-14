// BezelColors.cs
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
using System.Collections.Generic;

using Gdk;
using Gtk;
using Cairo;

using Do.Addins;

namespace Do.UI
{
	//FIXME!!!
	public class BezelColors
	{
		public static Dictionary<string, Cairo.Color> Colors = new Dictionary<string,Cairo.Color> ();

		public static void InitColors (HUDStyle style, Cairo.Color bgColor)
		{
			
			Colors = new Dictionary<string,Cairo.Color> ();
			Colors["background"]     = bgColor;
			Colors["focused_line"]   = new Cairo.Color (1.0, 1.0, 1.0, 0.3);
			Colors["unfocused_line"] = new Cairo.Color (1.0, 1.0, 1.0, 0.2);
			Colors["focused_text"]   = new Cairo.Color (0.0, 0.0, 0.0, 0.85);
			Colors["unfocused_text"] = new Cairo.Color (0.3, 0.3, 0.3, 0.7);
			Colors["titlebar_step1"] = Util.Appearance.ShadeColor (Colors["background"], 3);
			Colors["titlebar_step2"] = Util.Appearance.ShadeColor (Colors["titlebar_step1"], .72);
			Colors["titlebar_step3"] = Util.Appearance.ShadeColor (Colors["titlebar_step1"], .60);
			Colors["background_dk"]  = Util.Appearance.ShadeColor (Colors["background"], .9);
			Colors["background_lt"]  = Util.Appearance.ShadeColor (Colors["background"], 1.15);
			switch (style) {
			case HUDStyle.HUD:
				Colors["focused_box"]    = new Cairo.Color (0.3, 0.3, 0.3, 0.6);
				Colors["unfocused_box"]  = new Cairo.Color (0.0, 0.0, 0.0, 0.2);
				Colors["outline"]        = new Cairo.Color (.35, .35, .35);
				break;
			case HUDStyle.Classic:
				Colors["focused_box"]    = new Cairo.Color (1.0, 1.0, 1.0, 0.4);
				Colors["unfocused_box"]  = new Cairo.Color (1.0, 1.0, 1.0, 0.1);
				Colors["outline"] = Colors["background"];
				break;
			}
		}
	}
}
