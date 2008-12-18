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

using Do.Interface.CairoUtils;

namespace Do.Interface.AnimationBase
{
	public class BezelColors
	{
		Dictionary<string, Cairo.Color> Colors = new Dictionary<string,Cairo.Color> ();

		public Cairo.Color Background {
			get {
				return Colors["background"];
			}
		}			
			
		public Cairo.Color BackgroundLight {
			get {
				return Colors["background_lt"];
			}
		}
		
		public Cairo.Color BackgroundDark {
			get {
				return Colors["background_dk"];
			}
		}
		
		public Cairo.Color TitleBarBase {
			get {
				return Colors["titlebar_step3"];
			}
		}
		
		public Cairo.Color TitleBarGlossLight {
			get {
				return Colors["titlebar_step1"];
			}
		}
		
		public Cairo.Color TitleBarGlossDark {
			get {
				return Colors["titlebar_step2"];
			}
		}
		
		public Cairo.Color FocusedLine {
			get {
				return Colors["focused_line"];
			}
		}
		
		public Cairo.Color UnfocusedLine {
			get {
				return Colors["unfocused_line"];
			}
		}
		
		public Cairo.Color FocusedText {
			get {
				return Colors["focused_text"];
			}
		}
		
		public Cairo.Color UnfocusedText {
			get {
				return Colors["unfocused_text"];
			}
		}
		
		public BezelColors (Cairo.Color bgColor)
		{
			RebuildColors (bgColor);
		}
		
		public void RebuildColors (Cairo.Color bgColor)
		{
			Colors = new Dictionary<string,Cairo.Color> ();
			Colors["background"]     = bgColor;
			Colors["focused_line"]   = new Cairo.Color (1.0, 1.0, 1.0, 0.3);
			Colors["unfocused_line"] = new Cairo.Color (1.0, 1.0, 1.0, 0.2);
			Colors["focused_text"]   = new Cairo.Color (0.0, 0.0, 0.0, 0.85);
			Colors["unfocused_text"] = new Cairo.Color (0.3, 0.3, 0.3, 0.7);
			Colors["titlebar_step1"] = Colors["background"].ShadeColor (3);
			Colors["titlebar_step2"] = Colors["titlebar_step1"].ShadeColor (.72);
			Colors["titlebar_step3"] = Colors["titlebar_step1"].ShadeColor (.60);
			Colors["background_dk"]  = Colors["background"].ShadeColor (.9);
			Colors["background_lt"]  = Colors["background"].ShadeColor (1.15);
		}
	}
}
