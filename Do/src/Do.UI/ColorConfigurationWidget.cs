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
using System.Collections.Generic;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	public partial class ColorConfigurationWidget : Gtk.Bin, IConfigurable
	{
		BezelDrawingArea bda;
		
		List<string> themes = new List<string> (new string[] {
			"Glass Frame",
			"Mini",
		});
		
		public ColorConfigurationWidget ()
		{
			int themeI;
			Build();
			AppPaintable = true;
			Addins.Util.Appearance.SetColormap (this);
			
			foreach (IRenderTheme theme in Core.PluginManager.GetThemes ()) {
				theme_combo.AppendText (theme.Name);
				themes.Add (theme.Name);
			}
			
			if (!Screen.IsComposited)
				theme_combo.Sensitive = false;
				
			// Setup theme combo
            themeI = Array.IndexOf (Themes, Do.Preferences.Theme);
            themeI = themeI >= 0 ? themeI : 0;
            theme_combo.Active = themeI;            

			BuildPreview ();
			
			pin_check.Active = Do.Preferences.AlwaysShowResults;
			Do.Preferences.PreferenceChanged += OnPrefsChanged;
		}
		
		private void OnPrefsChanged (object o, PreferenceChangedEventArgs args) {
			if (args.Key == "Theme")
					BuildPreview ();
		}
		
		protected override void OnDestroyed ()
		{
			Do.Preferences.PreferenceChanged -= OnPrefsChanged;
			base.OnDestroyed ();
			if (bda != null)
				bda.Destroy ();
		}

		
		bool setup = false;
		
		private void BuildPreview ()
		{
			if (bda != null) {
//				preview_align.Remove (bda);
				preview_align.Remove (preview_align.Child);
				bda.Destroy ();
				bda = null;
			}
			
			foreach (IRenderTheme theme in Core.PluginManager.GetThemes ()) {
				if (theme.Name == Do.Preferences.Theme) {
					bda = new BezelDrawingArea (theme, true);
					break;
				}
			}
			if (preview_align.Child != null)
					preview_align.Remove (preview_align.Child);
				
			if (bda != null) {
				this.preview_align.Add (bda);
				bda.Show ();
				
				SetupButtons ();
			} else {
				this.preview_align.Add (new Gtk.Label ("No Preview Available"));
				this.preview_align.Child.Show ();
				DisableButtons ();
			}
		}
		
		private void DisableButtons ()
		{
			clear_background.Sensitive = false;
			background_colorbutton.Sensitive = false;
			shadow_check.Sensitive = false;
		}
		
		private void SetupButtons ()
		{
			setup = true;
			clear_background.Sensitive = true;
			background_colorbutton.Sensitive = shadow_check.Sensitive = true;
			background_colorbutton.Color = Addins.CairoUtils.ConvertToGdk (bda.BackgroundColor);
			shadow_check.Active = BezelDrawingArea.DrawShadow;
			Gtk.Application.Invoke (delegate { setup = false; });
		}
		
		public Gtk.Bin GetConfiguration ()
		{
			return this;
		}

		protected virtual void OnBackgroundColorbuttonColorSet (object sender, System.EventArgs e)
		{
			if (setup) return;
			BezelDrawingArea.BgColor = Addins.CairoUtils.ColorToHexString (background_colorbutton.Color);
		}

		protected virtual void OnClearBackgroundClicked (object sender, System.EventArgs e)
		{
			BezelDrawingArea.ResetBackgroundStyle ();
			background_colorbutton.Color = Addins.CairoUtils.ConvertToGdk (bda.BackgroundColor);
		}

		protected virtual void OnShadowCheckClicked (object sender, System.EventArgs e)
		{
			if (setup) return;
			BezelDrawingArea.DrawShadow = shadow_check.Active;
		}

		protected virtual void OnPinCheckClicked (object sender, System.EventArgs e)
		{
			Do.Preferences.AlwaysShowResults = pin_check.Active;
		}

		protected virtual void OnThemeComboChanged (object sender, System.EventArgs e)
		{
			Do.Preferences.Theme = Themes[theme_combo.Active];
		}

		public string Description {
			get {
				return "Color Configuration";
			}
		}
		
		public new string Name {
			get {
				return "Appearance";
			}
		}
		
		public string Icon {
			get {
				return "";
			}
		}
		
		public string[] Themes {
        	get {
				return themes.ToArray ();
        	}
        }
	}
}
