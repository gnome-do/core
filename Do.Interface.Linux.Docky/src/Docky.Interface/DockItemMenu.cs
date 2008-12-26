// DockItemMenu.cs
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
using System.Linq;

using Cairo;
using Gdk;
using Gtk;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Platform;

using Docky.Utilities;

namespace Docky.Interface
{
	
	
	public class DockItemMenu : DockPopupMenu
	{
		class CustomSeparator : HSeparator
		{
			public CustomSeparator () : base ()
			{
				HeightRequest = 3;
			}
			
			protected override bool OnExposeEvent (Gdk.EventExpose evnt)
			{
				using (Context cr = CairoHelper.Create (GdkWindow)) {
					cr.Rectangle (evnt.Area.X, evnt.Area.Y + 1, evnt.Area.Width, 1);
					cr.Color = new Cairo.Color (.8, .8, .8, .7);
					cr.Fill ();
					
				}
				return true;
			}

		}
		
		public DockItemMenu() : base ()
		{
		}

		public override void PopUp (IEnumerable<MenuArgs> args, int x, int y)
		{
			foreach (Gtk.Widget child in Container.AllChildren) {
				Container.Remove (child);
				child.Dispose ();
			}
			
			foreach (MenuArgs arg in args) {
				if (arg is SeparatorMenuArgs) {
					Container.PackStart (new CustomSeparator ());
					continue;
				}
				
				HBox hbox = new HBox ();
				Label label = new Label ();
				if (arg.Sensitive)
					label.Markup = "<span color=\"#ffffff\"><b>" + arg.Description + "</b></span>";
				else
					label.Markup = "<span color=\"#888888\"><b>" + arg.Description + "</b></span>";
				label.ModifyFg (StateType.Normal, new Gdk.Color (byte.MaxValue, byte.MaxValue, byte.MaxValue));
				label.ModifyText (StateType.Normal, new Gdk.Color (byte.MaxValue, byte.MaxValue, byte.MaxValue));
				label.Xalign = 0f;
				label.Ellipsize = Pango.EllipsizeMode.End;
				label.Ypad = 0;
				
				Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName (arg.Icon, 16);
				Gtk.Image image = new Gtk.Image (pbuf);
				pbuf.Dispose ();
				hbox.PackStart (image, false, false, 0);
				hbox.PackStart (label, true, true, 2);
				
				Gtk.Button button = new Button (hbox);
				
				button.Pressed += arg.Handler;
				button.Pressed += OnButtonPressed;
				
				button.Relief = ReliefStyle.None;
				button.CanFocus = false;
				button.Sensitive = arg.Sensitive;
				button.BorderWidth = 0;
				
				button.ModifyBg (StateType.Prelight, new Gdk.Color ((byte) (byte.MaxValue * 0.15), 
				                                                    (byte) (byte.MaxValue * 0.15), 
				                                                    (byte) (byte.MaxValue * 0.15)));
				Container.PackStart (button, false, false, 0);
			}
			ShowAll ();
			
			base.PopUp (args, x, y);
		}
		
		void OnButtonPressed (object o, EventArgs args)
		{
			Hide ();
		}
	}
}
