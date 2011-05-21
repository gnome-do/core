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

			if (!Screen.IsComposited) {
				composite_warning_widget.Visible = true;
				theme_combo.Sensitive = false;
			}
				
			// Setup theme combo
			theme_combo.Active = Math.Max (0, Themes.IndexOf (Do.Preferences.Theme));
			pin_check.Active = Do.Preferences.AlwaysShowResults;
			
			theme_configuration_container.ShowAll ();
		}
		
		public Gtk.Bin GetConfiguration ()
		{
			return this;
		}

		protected virtual void OnPinCheckClicked (object sender, System.EventArgs e)
		{
			Do.Preferences.AlwaysShowResults = pin_check.Active;
		}

		protected virtual void OnThemeComboChanged (object sender, System.EventArgs e)
		{
			Do.Preferences.Theme = Themes[theme_combo.Active];
			GLib.Idle.Add (() => {
				SetupConfigurationWidget ();
				return false;
			});
		}
		
		void SetupConfigurationWidget ()
		{
			if (theme_configuration_container.Child != null)
					theme_configuration_container.Remove (theme_configuration_container.Child);
			
			if (Do.Controller.Window is IConfigurable) {
				IConfigurable window = Do.Controller.Window as IConfigurable;
				Gtk.Bin bin = window.GetConfiguration ();
				theme_configuration_container.Add (bin);
			}
			theme_configuration_container.ShowAll ();
		}

		protected virtual void OnCompositeWarningInfoBtnClicked (object sender, System.EventArgs e)
		{
		}
		
		// This must be an explicit interface method to disambiguate between
		// Widget.Name and IConfigurable.Name
		string IConfigurable.Name {
			get { return Catalog.GetString ("Appearance"); }
		}

		public string Description {
			get { return ""; }
		}
		
		public string Icon {
			get { return ""; }
		}

		public override void Dispose ()
		{
			if (theme_configuration_container.Child != null)
				theme_configuration_container.Child.Dispose ();
			base.Dispose ();
		}
	}
}
