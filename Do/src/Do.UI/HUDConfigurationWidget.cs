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
		public HUDConfigurationWidget ()
		{
			this.Build();
			AppPaintable = true;
			Addins.Util.Appearance.SetColormap (this);
			BuildPreview ();
			
			Do.Preferences.PreferenceChanged += delegate (object o, PreferenceChangedEventArgs args) {
				if (args.Key == "Theme")
					BuildPreview ();
			};
		}
		
		~HUDConfigurationWidget ()
		{
			bda.Dispose ();
		}
		
		string [] option_list { get { return new string[] {"default", "hud", "classic"}; } }
		bool setup = false;
		
		private void BuildPreview ()
		{
			if (bda != null) {
				preview_align.Remove (bda);
				bda.Destroy ();
				bda = null;
			}
			
			switch (Do.Preferences.Theme) {
				case "ShowCase":
					bda = new BezelDrawingArea (HUDStyle.Classic, true);
					break;
				case "HUD":
					bda = new BezelDrawingArea (HUDStyle.HUD, true);
					break;
			}
			if (bda != null) {
				this.preview_align.Add (bda);
				bda.Show ();
				
				SetupButtons ();
			} else {
				DisableButtons ();
			}
		}
		
		private void DisableButtons ()
		{
			
		}
		
		private void SetupButtons ()
		{
			setup = true;
			title_combo.Active = Array.IndexOf<string> (option_list, BezelDrawingArea.TitleRenderer);
			background_combo.Active = Array.IndexOf<string> (option_list, BezelDrawingArea.WindowRenderer);
			outline_combo.Active = Array.IndexOf<string> (option_list, BezelDrawingArea.PaneRenderer);
			radius_spin.Value = bda.WindowRadius;
			background_colorbutton.Color = Addins.Util.Appearance.ConvertToGdk (bda.BackgroundColor);
			Gtk.Application.Invoke (delegate { setup = false; });
		}
		
		public Gtk.Bin GetConfiguration ()
		{
			return this;
		}

		protected virtual void OnTitleComboChanged (object sender, System.EventArgs e)
		{
			if (setup) return;
			BezelDrawingArea.TitleRenderer = title_combo.ActiveText.ToLower ();
		}

		protected virtual void OnBackgroundComboChanged (object sender, System.EventArgs e)
		{
			if (setup) return;
			BezelDrawingArea.WindowRenderer = background_combo.ActiveText.ToLower ();
			SetupButtons ();
		}

		protected virtual void OnOutlineComboChanged (object sender, System.EventArgs e)
		{
			if (setup) return;
			BezelDrawingArea.PaneRenderer = outline_combo.ActiveText.ToLower ();
		}

		protected virtual void OnBackgroundColorbuttonColorSet (object sender, System.EventArgs e)
		{
			if (setup) return;
			BezelDrawingArea.BgColor = Addins.Util.Appearance.ColorToHexString (background_colorbutton.Color);
		}

		protected virtual void OnRadiusSpinValueChanged (object sender, System.EventArgs e)
		{
			if (setup) return;
			BezelDrawingArea.RoundingRadius = (int) radius_spin.Value;
		}

		protected virtual void OnClearBackgroundClicked (object sender, System.EventArgs e)
		{
			BezelDrawingArea.ResetBackgroundStyle ();
			background_colorbutton.Color = Addins.Util.Appearance.ConvertToGdk (bda.BackgroundColor);
		}

		protected virtual void OnClearRadiusClicked (object sender, System.EventArgs e)
		{
			setup = true;
			BezelDrawingArea.RoundingRadius = -1;
			radius_spin.Value = bda.WindowRadius;
			Gtk.Application.Invoke (delegate { setup = false; });
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
