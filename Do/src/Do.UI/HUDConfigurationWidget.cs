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
		BezelDrawingArea bda;
		public HUDConfigurationWidget()
		{
			this.Build();
			bda = new BezelDrawingArea (HUDStyle.Classic, true);
			AppPaintable = true;
			Addins.Util.Appearance.SetColormap (this);
			this.preview_align.Add (bda);
			bda.Show ();
			
			SetupButtons ();
		}
		
		string [] option_list { get { return new string[] {"default", "hud", "classic"}; } }
		
		private void SetupButtons ()
		{
			title_combo.Active = Array.IndexOf<string> (option_list, bda.TitleRenderer);
			background_combo.Active = Array.IndexOf<string> (option_list, bda.WindowRenderer);
			outline_combo.Active = Array.IndexOf<string> (option_list, bda.PaneRenderer);
			radius_spin.Value = bda.WindowRadius;
			background_colorbutton.Color = bda.BackgroundColor;
		}
		
		public Gtk.Bin GetConfiguration ()
		{
			return this;
		}

		protected virtual void OnTitleComboChanged (object sender, System.EventArgs e)
		{
			bda.TitleRenderer = title_combo.ActiveText.ToLower ();
		}

		protected virtual void OnBackgroundComboChanged (object sender, System.EventArgs e)
		{
			bda.WindowRenderer = background_combo.ActiveText.ToLower ();
		}

		protected virtual void OnOutlineComboChanged (object sender, System.EventArgs e)
		{
			bda.PaneRenderer = outline_combo.ActiveText.ToLower ();
		}

		protected virtual void OnBackgroundColorbuttonColorSet (object sender, System.EventArgs e)
		{
			bda.BackgroundColor = background_colorbutton.Color;
		}

		protected virtual void OnRadiusSpinValueChanged (object sender, System.EventArgs e)
		{
			bda.WindowRadius = (int) radius_spin.Value;
		}

		protected virtual void OnClearBackgroundClicked (object sender, System.EventArgs e)
		{
			bda.ResetBackgroundStyle ();
			background_colorbutton.Color = bda.BackgroundColor;
		}

		protected virtual void OnClearRadiusClicked (object sender, System.EventArgs e)
		{
			bda.WindowRadius = -1;
			radius_spin.Value = bda.WindowRadius;
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
