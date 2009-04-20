//  
//  Copyright (C) 2009 GNOME Do
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Linq;

using Gtk;

using Docky.Utilities;

namespace Docky.Interface
{
	
	
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DockyConfigurationWidget : Gtk.Bin
	{

		public DockyConfigurationWidget()
		{
			this.Build();
			
			zoom_scale.Adjustment.PageSize = .1;
			zoom_scale.Adjustment.SetBounds (1.1, 4, .1, 1, 1);
			zoom_scale.Value = DockPreferences.ZoomPercent;
			
			advanced_indicators_checkbutton.Active = DockPreferences.IndicateMultipleWindows;
			autohide_checkbutton.Active = DockPreferences.AutoHide;
			window_overlap_checkbutton.Active = DockPreferences.AllowOverlap;
			zoom_checkbutton.Active = DockPreferences.ZoomEnabled;
			
			orientation_combobox.AppendText (DockOrientation.Bottom.ToString ());
			orientation_combobox.AppendText (DockOrientation.Top.ToString ());
			orientation_combobox.Active = DockPreferences.Orientation == DockOrientation.Bottom ? 0 : 1;
		}
		
		protected virtual void OnZoomScaleFormatValue (object o, Gtk.FormatValueArgs args)
		{
			args.RetVal = string.Format ("{0}%", Math.Round (args.Value * 100));
		}

		protected virtual void OnZoomScaleValueChanged (object sender, System.EventArgs e)
		{
			if (!(sender is HScale)) return;
			
			HScale scale = sender as HScale;
			DockPreferences.ZoomPercent = scale.Value;
		}

		protected virtual void OnAdvancedIndicatorsCheckbuttonToggled (object sender, System.EventArgs e)
		{
			DockPreferences.IndicateMultipleWindows = advanced_indicators_checkbutton.Active;
		}

		protected virtual void OnZoomCheckbuttonToggled (object sender, System.EventArgs e)
		{
			DockPreferences.ZoomEnabled = zoom_checkbutton.Active;
		}

		protected virtual void OnWindowOverlapCheckbuttonToggled (object sender, System.EventArgs e)
		{
			DockPreferences.AllowOverlap = window_overlap_checkbutton.Active;
		}

		protected virtual void OnAutohideCheckbuttonToggled (object sender, System.EventArgs e)
		{
			DockPreferences.AutoHide = autohide_checkbutton.Active;
		}

		protected virtual void OnOrientationComboboxChanged (object sender, System.EventArgs e)
		{
			DockPreferences.Orientation = (DockOrientation) orientation_combobox.Active;
		}
	}
}
