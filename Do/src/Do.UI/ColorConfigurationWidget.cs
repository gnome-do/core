// ColorConfigurationWidget.cs
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

using Mono.Unix;

using Do.Universe;
using Do.Platform;
using Do.Platform.Linux;
using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Interface.AnimationBase;

namespace Do.UI
{
	public partial class ColorConfigurationWidget : Gtk.Bin, IConfigurable
	{
		List<string> themes;

		//TODO Make this an automatic property once mono 1.9 support is dropped
		List<string> Themes { 
			get { return themes; }
			set { themes = value; }
		}

		bool setup = false;
		
		public ColorConfigurationWidget ()
		{
			Build ();
			AppPaintable = true;
			Themes = new List<string> ();
			Interface.Util.Appearance.SetColormap (this);
			
			foreach (InterfaceDescription theme in InterfaceManager.GetInterfaceDescriptions ()) {
				theme_combo.AppendText (theme.Name);
				Themes.Add (theme.Name);
			}

			SetupButtons ();
			if (!Screen.IsComposited) {
				composite_warning_widget.Visible = true;
				theme_combo.Sensitive = false;
				animation_check.State = shadow_check.State = Gtk.StateType.Insensitive;
			}
				
			// Setup theme combo
			theme_combo.Active = Math.Max (0, Themes.IndexOf (Do.Preferences.Theme));
			pin_check.Active = Do.Preferences.AlwaysShowResults;
		}
		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
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
			ushort alpha;
			background_colorbutton.Color = ReadColor (BezelDrawingArea.BgColor, out alpha);
			background_colorbutton.Alpha = alpha;
			clear_background.Sensitive = true;
			background_colorbutton.Sensitive = shadow_check.Sensitive = true;
			shadow_check.Active = BezelDrawingArea.DrawShadow;
			animation_check.Active = BezelDrawingArea.Animated;
			Gtk.Application.Invoke (delegate { setup = false; });
		}
		
		private Gdk.Color ReadColor (string colorString, out ushort alpha)
		{
			Gdk.Color prefsColor;
			byte r,g,b;
			uint converted;
			
			try {
				converted = uint.Parse (colorString, System.Globalization.NumberStyles.HexNumber);
		
				alpha = (ushort) ((converted & 255) << 8);
				b = (byte) ((converted >> 8) & 255);
				g = (byte) ((converted >> 16) & 255);
				r = (byte) ((converted >> 24) & 255);
				prefsColor = new Gdk.Color (r,g,b);
			} catch (Exception e) {
				prefsColor = new Gdk.Color (0,0,0);
				alpha = ushort.MaxValue;
				if (colorString.ToLower () != "default") {
					Log<ColorConfigurationWidget>.Error ("Error setting color: {0}", e.Message);
					Log<ColorConfigurationWidget>.Debug (e.StackTrace);
				}
			}
			
			return prefsColor;
		}
			                                                  
		
		public Gtk.Bin GetConfiguration ()
		{
			return this;
		}

		protected virtual void OnBackgroundColorbuttonColorSet (object sender, System.EventArgs e)
		{
			if (setup) return;
			string hex_string = string.Format ("{0}{1:X}", background_colorbutton.Color.ColorToHexString (), (byte) (background_colorbutton.Alpha >> 8));
			BezelDrawingArea.BgColor = hex_string;
		}

		protected virtual void OnClearBackgroundClicked (object sender, System.EventArgs e)
		{
			BezelDrawingArea.ResetBackgroundStyle ();
			background_colorbutton.Color = new Gdk.Color (0, 0, 0);
			background_colorbutton.Alpha = ushort.MaxValue;
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

		protected virtual void OnAnimationCheckbuttonClicked (object sender, System.EventArgs e)
		{
			BezelDrawingArea.Animated = animation_check.Active;
		}

		protected virtual void OnCompositeWarningInfoBtnClicked (object sender, System.EventArgs e)
		{
		}
		
		public new string Name {
			get { return Catalog.GetString ("Appearance"); }
		}

		public string Description {
			get { return ""; }
		}
		
		public string Icon {
			get { return ""; }
		}
	
	}
}
