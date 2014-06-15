//  %filename
// 
//   GNOME Do is the legal property of its developers. Please refer to the
//   COPYRIGHT file distributed with this source distribution.
// 
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//  
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//  
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;

using Gtk;
using Mono.Unix;
using Mono.Addins;

using Do.Platform;

namespace Do.Platform.Linux
{
		
	public partial class PluginAvailableDialog : Gtk.Dialog
	{		
		const string WhatIsDoUrl = "http://do.cooperteam.net/";
		
		Addin addin;
		
		public PluginAvailableDialog (string package, Addin addin)
		{
			this.Build();

			LinkButton wiki_btn, plugin_desc_btn;
			
			this.addin = addin;
			
			body_lbl.Text = string.Format (body_lbl.Text, package);
			
			wiki_btn = new LinkButton (WhatIsDoUrl, Catalog.GetString ("What is Do?"));
			wiki_btn.Xalign = 0F;
			link_vbox.Add (wiki_btn);
			
			// if the URL attribute is set, then we show a link to the plugin help
			if (!string.IsNullOrEmpty (addin.Description.Url)) {
				plugin_desc_btn = new LinkButton (addin.Description.Url,
					string.Format (Catalog.GetString ("What does the {0} plugin do?"), package));
				plugin_desc_btn.Xalign = 0F;
				link_vbox.Add (plugin_desc_btn);
			}
			
			ShowAll ();
		}
		
		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			Destroy ();
		}

		protected virtual void OnInstallBtnClicked (object sender, System.EventArgs e)
		{
			Services.PluginManager.Install (addin);
			Destroy ();
		}

		protected virtual void OnAskChkToggled (object sender, System.EventArgs e)
		{
			IPreferences prefs = Services.Preferences.Get<AbstractPackageManagerService> ();
			prefs.Set (AbstractPackageManagerService.PluginAvailableKey, ask_chk.Active);
		}		
	}
}
