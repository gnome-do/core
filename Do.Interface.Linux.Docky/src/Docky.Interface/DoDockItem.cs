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

using Cairo;
using Gdk;
using Gtk;

using Do.Interface;
using Do.Platform;

using Docky.Utilities;

namespace Docky.Interface
{
	
	public class DoDockItem : AbstractDockItem, IRightClickable
	{
		const string DoIcon = "gnome-do";
		
		#region IDockItem implementation 
		
		protected override Pixbuf GetSurfacePixbuf ()
		{
			return IconProvider.PixbufFromIconName (DoIcon, DockPreferences.FullIconSize);
		}

		
		public override void Clicked (uint button)
		{
			if (button == 1)
				Services.Windowing.SummonMainWindow ();
		}
		
		public override string Description {
			get {
				return Mono.Unix.Catalog.GetString ("Summon GNOME Do");
			}
		}
		
		#endregion 
		
		public DoDockItem () : base ()
		{
		}

		#region IDisposable implementation 
		
		#endregion 
		
		public override bool Equals (IDockItem other)
		{
			return other is DoDockItem;
		}

		#region IRightClickable implementation 
		
		public event EventHandler RemoveClicked;
		
		public IEnumerable<AbstractMenuButtonArgs> GetMenuItems ()
		{
			if (DockPreferences.AutoHide)
				yield return new SimpleMenuButtonArgs (() => DockPreferences.AutoHide = false, "Disable Autohide", "gtk-delete");
			else
				yield return new SimpleMenuButtonArgs (() => DockPreferences.AutoHide = true, "Enable Autohide", "gtk-ok");
			
			if (DockPreferences.ZoomEnabled)
				yield return new SimpleMenuButtonArgs (() => DockPreferences.ZoomEnabled = false, "Disable Zoom", "gtk-delete");
			else
				yield return new SimpleMenuButtonArgs (() => DockPreferences.ZoomEnabled = true, "Enable Zoom", "gtk-ok");
		}
		
		#endregion 
		
	}
}
