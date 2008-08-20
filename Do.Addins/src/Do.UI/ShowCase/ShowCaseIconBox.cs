// ShowCaseIconBox.cs
// 
// Copyright (C) 2008 GNOME-Do
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

using Cairo;
using Gdk;
using Gtk;
using System;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	
	
	public class ShowCaseIconBox : Frame
	{
		Gtk.Image image;
		Label label;
		IObject display_object;
		bool focused;
		
		public bool Focused {
			get {
				return focused;
			}
			set {
				if (focused == value)
					return;
				
				focused = value;
				if (value) {
					FillColor = new Gdk.Color (0x12, 0x12, 0x12);
					FillAlpha = .8;
				} else {
					FillColor = new Gdk.Color (0x38, 0x38, 0x38);
					FillAlpha = .6;
				}
			}
		}
		
		public int Width {
			get {
				return width;
			}
		}
		
		public IObject DisplayObject {
			get {
				return display_object;
			}
			set {
				display_object = value;
				if (value == null) return;
				Gdk.Pixbuf pixbuf = IconProvider.PixbufFromIconName (value.Icon, 24);
				image.Pixbuf = pixbuf;
				pixbuf.Dispose ();
				label.Markup = "<b>"+value.Name+"</b>";
			}
		}
		
		public ShowCaseIconBox ()
		{
			Build ();
		}
		
		private void Build ()
		{
			HBox hbox = new HBox ();
			
			image = new Gtk.Image ();
			hbox.PackStart (image, false, false, 5);
			
			label = new Label ();
			label.ModifyFg (StateType.Normal, new Gdk.Color (0xff, 0xff, 0xff));
			label.Ellipsize = Pango.EllipsizeMode.End;
			hbox.PackStart (label, true, true, 0);
			
			FillColor = new Gdk.Color (0x38, 0x38, 0x38);
			DrawFill  = true;
			DrawFrame = false;
			
			Radius = 5;
			FillAlpha = FrameAlpha = .6;
			
			Add (hbox);
			ShowAll ();
		}
		
		public void Clear ()
		{
			label.Markup = string.Empty;
			image.Clear ();
			display_object = null;
		}
	}
}
