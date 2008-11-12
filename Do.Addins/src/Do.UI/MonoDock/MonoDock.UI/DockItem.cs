// DockItem.cs
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

using Do.UI;

namespace MonoDock.UI
{
	
	
	public class DockItem
	{
#region Static Area
		static int icon_size = 64;
		static double icon_quality = 2;
		
		public static int IconSize {
			get { return icon_size; }
			set { icon_size = value; }
		}
		
		public static double IconQuality {
			get { return icon_quality; }
			set { icon_quality = value; }
		}
#endregion
		
		public string Icon { get; private set; }
		public string Description { get; private set; }
		
		Gdk.Pixbuf pixbuf;
		public Gdk.Pixbuf Pixbuf {
			get {
				return pixbuf ?? pixbuf = IconProvider.PixbufFromIconName (Icon, (int) (IconSize*IconQuality));
			}
		}
		
		public DockItem(string icon, string description)
		{
			this.Icon = icon;
			this.Description = description;
		}
	}
}
