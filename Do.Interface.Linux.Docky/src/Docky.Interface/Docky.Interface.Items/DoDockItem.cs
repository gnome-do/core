// DoDockItem.cs
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

using Mono.Unix;

using Do.Interface;
using Do.Platform;
using Do.Universe;

using Docky.Core;
using Docky.Interface.Menus;
using Docky.Interface.Painters;
using Docky.Utilities;

namespace Docky.Interface
{
	
	public class DoDockItem : AbstractDockItem, IRightClickable
	{
		const string DoIcon = "gnome-do";
		public const string EnableIcon = "gtk-apply";
		public const string DisableIcon = "gtk-remove";
		const string Text = "GNOME Do";

		HotSeatPainter hot_seat_painter;
		
		#region IDockItem implementation 
		
		public override ScalingType ScalingType {
			get {
				return ScalingType.HighLow;
			}
		}

		
		protected override Pixbuf GetSurfacePixbuf (int size)
		{
			return IconProvider.PixbufFromIconName (DoIcon, size);
		}

		
		public override void Clicked (uint button, ModifierType state, Gdk.Point position)
		{
			if (button == 1)
				Services.Windowing.SummonMainWindow ();
			if (button == 2)
				hot_seat_painter.Show ();
		}
		
		#endregion 
		
		public DoDockItem () : base ()
		{
			hot_seat_painter = new HotSeatPainter ();
			DockServices.PainterService.RegisterPainter (hot_seat_painter);
			
			SetText (Catalog.GetString (Text));
		}

		#region IDisposable implementation 
		
		#endregion 
		
		public override bool Equals (AbstractDockItem other)
		{
			return other is DoDockItem;
		}

		#region IRightClickable implementation 
		
		public event EventHandler RemoveClicked;
		
		public IEnumerable<AbstractMenuArgs> GetMenuItems ()
		{
			yield return new SeparatorMenuButtonArgs ();
			
			yield return new SimpleMenuButtonArgs (() => DockPreferences.AutoHide = !DockPreferences.AutoHide, 
			                                       Catalog.GetString ("Automatically Hide"), DockPreferences.AutoHide ? EnableIcon : DisableIcon).AsDark ();

			yield return new SimpleMenuButtonArgs (() => DockPreferences.ZoomEnabled = !DockPreferences.ZoomEnabled, 
			                                       Catalog.GetString ("Zoom Icons"), DockPreferences.ZoomEnabled ? EnableIcon : DisableIcon).AsDark ();
			
			if (Gdk.Screen.Default.NMonitors > 1)
				yield return new SimpleMenuButtonArgs (() => DockPreferences.Monitor++,
				                                       Catalog.GetString ("Switch Monitors"), "display").AsDark ();
			
			foreach (IRunnableItem item in Services.Application.MainMenuItems) {
				yield return new SeparatorMenuButtonArgs ();
				yield return new RunnableMenuButtonArgs (item);
			}
		}
		
		#endregion 
	}
}
