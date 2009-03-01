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

namespace Docky.Interface.Menus
{
	
	
	public class DockItemMenu : DockPopupMenu
	{
		
		public DockItemMenu() : base ()
		{
		}
		
		public override void PopUp (IEnumerable<AbstractMenuArgs> args, int x, int y)
		{
			foreach (Gtk.Widget child in Container.AllChildren) {
				Container.Remove (child);
				child.Dispose ();
			}
			
			foreach (AbstractMenuArgs arg in args) {
				Container.PackStart (arg.Widget, false, false, 0);
				arg.Activated += OnButtonClicked;
				arg.Widget.ShowAll ();
			}
			ShowAll ();
			
			base.PopUp (args, x, y);
		}
		
		void OnButtonClicked (object o, EventArgs args)
		{
			foreach (Gtk.Widget widget in Container.AllChildren) {
				if (!(widget is Gtk.Button))
					continue;
				(widget as Gtk.Button).Activated -= OnButtonClicked;
			}
			Hide ();
		}
	}
}
