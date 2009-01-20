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
using Do.Universe;

using Docky.Interface.Menus;
using Docky.Utilities;

namespace Docky.Interface
{
	
	public class DoDockItem : BaseDockItem, IRightClickable
	{
		const string DoIcon = "gnome-do";
		const string EnableIcon = "gtk-apply";
		const string DisableIcon = "gtk-remove";
		
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
		
		public override bool Equals (BaseDockItem other)
		{
			return other is DoDockItem;
		}

		#region IRightClickable implementation 
		
		public event EventHandler RemoveClicked;
		
		public IEnumerable<AbstractMenuButtonArgs> GetMenuItems ()
		{
			yield return new SimpleMenuButtonArgs (() => DockPreferences.AutoHide = !DockPreferences.AutoHide, 
			                                       "Automatically Hide", DockPreferences.AutoHide ? EnableIcon : DisableIcon);

			if (!DockPreferences.AutoHide)
				yield return new SimpleMenuButtonArgs (() => DockPreferences.AllowOverlap = !DockPreferences.AllowOverlap,
				                                       "Allow Window Overlap", DockPreferences.AllowOverlap ? EnableIcon : DisableIcon);
			
			yield return new SimpleMenuButtonArgs (() => DockPreferences.ZoomEnabled = !DockPreferences.ZoomEnabled, 
			                                       "Zoom Icons", DockPreferences.ZoomEnabled ? EnableIcon : DisableIcon);
			
			yield return new SimpleMenuButtonArgs (() => DockPreferences.ShowTrash = !DockPreferences.ShowTrash, 
			                                       "Show Trash", DockPreferences.ShowTrash ? EnableIcon : DisableIcon);
			
			yield return new SimpleMenuButtonArgs (() => DockPreferences.IndicateMultipleWindows = !DockPreferences.IndicateMultipleWindows, 
			                                       "Advanced Indicators", DockPreferences.IndicateMultipleWindows ? EnableIcon : DisableIcon);

			if (Gdk.Screen.Default.NMonitors > 1)
				yield return new SimpleMenuButtonArgs (() => DockPreferences.Monitor++,
				                                       "Switch Monitors", "display");
			
			yield return new SeparatorMenuButtonArgs ();
			
			foreach (IRunnableItem item in Services.Application.MainMenuItems) {
				yield return new RunnableMenuButtonArgs (item);
			}
		}
		
		#endregion 
		
	}
}
