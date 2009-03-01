// AbstractMenuButtonArgs.cs
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
using Mono.Unix;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Platform;

namespace Docky.Interface.Menus
{
	public abstract class AbstractMenuButtonArgs : AbstractMenuArgs
	{
		Gtk.Widget widget;
		
		public override Gtk.Widget Widget { 
			get {
				if (widget == null)
					widget = BuildWidget ();
				return widget;
			}
		}
		
		protected virtual string Description { get; set; }
		
		protected virtual string Icon { get; set; }
		
		public AbstractMenuButtonArgs ()
		{
			
		}
		
		public AbstractMenuButtonArgs (string description, string icon)
		{
			Description = GLib.Markup.EscapeText (Catalog.GetString (description));
			Icon = icon;
		}
		
		Gtk.Button BuildWidget ()
		{
			HBox hbox = new HBox ();
			Label label = new Label ();
			label.Markup = "<span color=\"#ffffff\"><b>" + Description + "</b></span>";
			label.ModifyFg (StateType.Normal, new Gdk.Color (byte.MaxValue, byte.MaxValue, byte.MaxValue));
			label.ModifyText (StateType.Normal, new Gdk.Color (byte.MaxValue, byte.MaxValue, byte.MaxValue));
			label.Xalign = 0f;
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.Ypad = 0;
			
			Gtk.Image image;
			using (Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName (Icon, 16)) {
				image = new Gtk.Image (pbuf);
			}
				
			hbox.PackStart (image, false, false, 0);
			hbox.PackStart (label, true, true, 2);
			
			Gtk.Button button = new Gtk.Button (hbox);
			
			button.Relief = ReliefStyle.None;
			button.CanFocus = false;
			button.BorderWidth = 0;
			
			button.ModifyBg (StateType.Prelight, new Gdk.Color ((byte) (byte.MaxValue * 0.25), 
			                                                    (byte) (byte.MaxValue * 0.25), 
			                                                    (byte) (byte.MaxValue * 0.25)));
			
			button.Clicked += (sender, e) => Action ();
			button.Clicked += (sender, e) => base.OnActivated ();
			button.ShowAll ();
			
			return button;
		}
		
		public abstract void Action ();
		
		public override void Dispose ()
		{
			Widget.Destroy ();
			base.Dispose ();
		}

	}
}
