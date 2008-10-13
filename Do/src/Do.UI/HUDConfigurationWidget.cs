// HUDConfigurationWidget.cs
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

using Do.Addins;
using Do.Universe;

namespace Do.UI
{


	
	
	public partial class HUDConfigurationWidget : Gtk.Bin, IConfigurable
	{
		
		public HUDConfigurationWidget()
		{
			this.Build();
			
			BezelDrawingArea bda = new BezelDrawingArea (HUDStyle.Classic, true);
			AppPaintable = true;
			Addins.Util.Appearance.SetColormap (this);
			this.preview_align.Add (bda);
			bda.Show ();
		}
		
		public Gtk.Bin GetConfiguration ()
		{
			return this;
		}
		
		public string Description {
			get {
				return "HUD Configuration";
			}
		}
		
		public string Name {
			get {
				return "HUD Configuration";
			}
		}
		
		public string Icon {
			get {
				return "";
			}
		}
	}
}
