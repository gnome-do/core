// GeneralPreferencesWidget.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
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

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using Gtk;
using Mono.Unix;

using Do;
using Do.Platform;
// TODO once IServices have their own preferences, we can
// let TrayIconService configure itself, remove this, and drop
// our reference to Do.Platform.Linux.
using Do.Platform.Linux;

namespace Do.UI
{
    [System.ComponentModel.Category("Do")]
    [System.ComponentModel.ToolboxItem(true)]
    public partial class GeneralPreferencesWidget : Bin, IConfigurable
    {
		// This must be an explicit interface method to disambiguate between
		// Widget.Name and IConfigurable.Name
		string IConfigurable.Name {
			get { return Catalog.GetString ("General"); }
		}

		public string Description {
			get { return ""; }
		}

		public string Icon {
		  get { return ""; }
		}
		
		public GeneralPreferencesWidget ()
		{
		  Build ();
		  
		  // Setup checkboxes
		  hide_check.Active = Do.Preferences.QuietStart;
		  login_check.Active = AutostartEnabled;
		  notification_check.Active = TrayIconService.Visible;
		}
	        
		public Bin GetConfiguration ()
		{
		  return this;
		}
	        
		protected bool AutostartEnabled {
	            get {
				return Services.System.IsAutoStartEnabled ();
			}
			
			set {
				try {
					Services.System.SetAutoStartEnabled (value);
				} catch (Exception e) {
					Log<GeneralPreferencesWidget>.Error ("Failed to set autostart: {0}", e.Message);
				}
			}
		}
		
		protected virtual void OnLoginCheckClicked (object sender, EventArgs e)
		{
		  AutostartEnabled = login_check.Active;
		}
		
		protected virtual void OnHideCheckClicked (object sender, EventArgs e)
		{
		  Do.Preferences.QuietStart = hide_check.Active;
		}
		
		protected virtual void OnNotificationCheckClicked (object sender, System.EventArgs e)
		{
		  TrayIconService.Visible = notification_check.Active;
		}
    }
}
