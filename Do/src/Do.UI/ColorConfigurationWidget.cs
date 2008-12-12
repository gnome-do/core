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
using System.Linq;
using System.Collections.Generic;

using Do.Addins;
using Do.Universe;
using Do.Platform;
using Do.Interface.CairoUtils;

namespace Do.UI
{
	public partial class ColorConfigurationWidget : Gtk.Bin, IConfigurable
	{
		IList<string> Themes { get; set; }
		
		public ColorConfigurationWidget ()
		{
			Build ();
			AppPaintable = true;
			Themes = new List<string> ();
			Interface.Util.Appearance.SetColormap (this);
			
//			foreach (IRenderTheme theme in Core.PluginManager.GetThemes ()) {
//				theme_combo.AppendText (theme.Name);
//				Themes.Add (theme.Name);
//			}
		
//			theme_combo.AppendText ("MonoDock");
//			Themes.Add ("MonoDock");
			
			if (!Screen.IsComposited)
				theme_combo.Sensitive = false;
				
			// Setup theme combo
            theme_combo.Active = Math.Max (0, Themes.IndexOf (Do.Preferences.Theme));

//			BuildPreview ();
			
			pin_check.Active = Do.Preferences.AlwaysShowResults;
			Do.Preferences.ThemeChanged += OnThemeChanged;
		}
		
		private void OnThemeChanged (object sender, PreferencesChangedEventArgs e)
		{
//			BuildPreview ();
		}
		
		protected override void OnDestroyed ()
		{
			Do.Preferences.ThemeChanged -= OnThemeChanged;
			base.OnDestroyed ();
//			if (bda != null)
//				bda.Destroy ();
		}

		
		bool setup = false;
		
//		private void BuildPreview ()
//		{
//			if (bda != null) {
////				preview_align.Remove (bda);
//				preview_align.Remove (preview_align.Child);
//				bda.Destroy ();
//				bda = null;
//			}
//			
//			foreach (IRenderTheme theme in Core.PluginManager.GetThemes ()) {
//				if (theme.Name == Do.Preferences.Theme) {
//					bda = new BezelDrawingArea (null, theme, true);
//					break;
//				}
//			}
//			if (preview_align.Child != null)
//					preview_align.Remove (preview_align.Child);
//				
//			if (bda != null) {
//				this.preview_align.Add (bda);
//				bda.Show ();
//				
//				SetupButtons ();
//			} else {
//				this.preview_align.Add (new Gtk.Label ("No Preview Available"));
//				this.preview_align.Child.Show ();
//				DisableButtons ();
//			}
//		}
		
		private void DisableButtons ()
		{
			clear_background.Sensitive = false;
			background_colorbutton.Sensitive = false;
			shadow_check.Sensitive = false;
		}
		
		private void SetupButtons ()
		{
			setup = true;
//			clear_background.Sensitive = true;
//			background_colorbutton.Sensitive = shadow_check.Sensitive = true;
//			background_colorbutton.Color = bda.BackgroundColor.ConvertToGdk ();
//			background_colorbutton.Alpha = (ushort) (bda.BackgroundColor.A * ushort.MaxValue);
//			shadow_check.Active = BezelDrawingArea.DrawShadow;
//			animation_checkbutton.Active = BezelDrawingArea.Animated;
			Gtk.Application.Invoke (delegate { setup = false; });
		}
		
		public Gtk.Bin GetConfiguration ()
		{
			return this;
		}

		protected virtual void OnBackgroundColorbuttonColorSet (object sender, System.EventArgs e)
		{
//			if (setup) return;
//			string hex_string = string.Format ("{0}{1:X}", background_colorbutton.Color.ColorToHexString (), (byte) (background_colorbutton.Alpha >> 8));
//			BezelDrawingArea.BgColor = hex_string;
		}

		protected virtual void OnClearBackgroundClicked (object sender, System.EventArgs e)
		{
//			BezelDrawingArea.ResetBackgroundStyle ();
//			background_colorbutton.Color = bda.BackgroundColor.ConvertToGdk ();
//			background_colorbutton.Alpha = (ushort) (bda.BackgroundColor.A * ushort.MaxValue);
		}

		protected virtual void OnShadowCheckClicked (object sender, System.EventArgs e)
		{
//			if (setup) return;
//			BezelDrawingArea.DrawShadow = shadow_check.Active;
		}

		protected virtual void OnPinCheckClicked (object sender, System.EventArgs e)
		{
			Do.Preferences.AlwaysShowResults = pin_check.Active;
		}

		protected virtual void OnThemeComboChanged (object sender, System.EventArgs e)
		{
			Do.Preferences.Theme = Themes[theme_combo.Active];
		}

		protected virtual void OnAnimationCheckbuttonClicked (object sender, System.EventArgs e)
		{
//			BezelDrawingArea.Animated = animation_checkbutton.Active;
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
	
	}
}
