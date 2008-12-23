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
	
	
	public class DockItemMenu : Gtk.Window
	{
		const int TailHeight = 15;
		new const int BorderWidth = 2;
		const int Radius = 10;
		
		VBox vbox;
		
		public DockItemMenu() : base (Gtk.WindowType.Popup)
		{
			Decorated = false;
			KeepAbove = true;
			AppPaintable = true;
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			Resizable = false;
			Modal = true;
			TypeHint = WindowTypeHint.PopupMenu;
			
			WidthRequest = 225;
			
			this.SetCompositeColormap ();
			
			AddEvents ((int) EventMask.PointerMotionMask | 
			           (int) EventMask.LeaveNotifyMask |
			           (int) EventMask.ButtonPressMask | 
			           (int) EventMask.ButtonReleaseMask |
			           (int) EventMask.FocusChangeMask);
			
			vbox = new VBox ();
			VBox space = new VBox ();
			space.Add (vbox);
			space.Add (new Label (" "));
			vbox.BorderWidth = 10;
			Add (space);
			space.ShowAll ();
		}

		
		public void PopUp (IEnumerable<MenuArgs> args, int x, int y)
		{
			foreach (Gtk.Widget child in vbox.AllChildren) {
				vbox.Remove (child);
				child.Dispose ();
			}
			
			foreach (MenuArgs arg in args) {
				if (arg is SeparatorMenuArgs) {
					vbox.PackStart (new HSeparator ());
					continue;
				}
				Label label = new Label ();
				if (arg.Sensitive)
					label.Markup = "<span color=\"#ffffff\"><b>" + arg.Description + "</b></span>";
				else
					label.Markup = "<span color=\"#888888\"><b>" + arg.Description + "</b></span>";
				label.ModifyFg (StateType.Normal, new Gdk.Color (byte.MaxValue, byte.MaxValue, byte.MaxValue));
				label.ModifyText (StateType.Normal, new Gdk.Color (byte.MaxValue, byte.MaxValue, byte.MaxValue));
				label.Ellipsize = Pango.EllipsizeMode.End;
				label.Ypad = 0;
				
				Gtk.Button button = new Button (label);
				
				button.Pressed += arg.Handler;
				button.Pressed += OnButtonPressed;
				
				button.Relief = ReliefStyle.None;
				button.CanFocus = false;
				button.Sensitive = arg.Sensitive;
				button.BorderWidth = 0;
				
				button.ModifyBg (StateType.Prelight, new Gdk.Color ((byte) (byte.MaxValue * 0.15), 
				                                                    (byte) (byte.MaxValue * 0.15), 
				                                                    (byte) (byte.MaxValue * 0.15)));
				vbox.PackStart (button, false, false, 0);
			}
			ShowAll ();
			Gtk.Requisition req = SizeRequest ();
			Move (x - req.Width / 2, y - req.Height);
			
			Do.Interface.Windowing.PresentWindow (this);
		}
		
		void OnButtonPressed (object o, EventArgs args)
		{
			Hide ();
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
				cr.AlphaFill ();
				DrawBackground (cr);
			}
			
			return base.OnExposeEvent (evnt);
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			Gdk.Rectangle rect;
			GetSize (out rect.Width, out rect.Height);
			
			if (evnt.X < 0 || evnt.Y < 0 || evnt.X > rect.Width || evnt.Y > rect.Height)
				Hide ();
			return base.OnButtonReleaseEvent (evnt);
		}

		void DrawBackground (Context cr)
		{
			Gdk.Rectangle rect;
			GetSize (out rect.Width, out rect.Height);
			
			cr.MoveTo (BorderWidth + Radius, BorderWidth);
			cr.Arc (rect.Width - BorderWidth - Radius, BorderWidth + Radius, Radius, Math.PI * 1.5, Math.PI * 2);
			cr.Arc (rect.Width - BorderWidth - Radius, rect.Height - BorderWidth - Radius - TailHeight, Radius, 0, Math.PI * 0.5);
			
			cr.LineTo (rect.Width / 2 + 20 - BorderWidth - Radius, rect.Height - BorderWidth - TailHeight);
			cr.LineTo (rect.Width / 2 + 10 - BorderWidth - Radius, rect.Height - BorderWidth);
			cr.LineTo (rect.Width / 2 - 0 - BorderWidth - Radius, rect.Height - BorderWidth - TailHeight);
			
			cr.Arc (BorderWidth + Radius, rect.Height - BorderWidth - Radius - TailHeight, Radius, Math.PI * 0.5, Math.PI);
			cr.Arc (BorderWidth + Radius, BorderWidth + Radius, Radius, Math.PI, Math.PI * 1.5);
			
			cr.Color = new Cairo.Color (0, 0, 0, .9);
			cr.FillPreserve ();
			
			cr.Color = new Cairo.Color (.2, .2, .2, .8);
			cr.Stroke ();
		}
	}
}
