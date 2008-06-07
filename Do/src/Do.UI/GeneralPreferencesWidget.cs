/* GeneralPreferencesWidget.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;

using Gtk;
using Gdk;
using Gnome;

using Do;

namespace Do.UI
{
    public partial class GeneralPreferencesWidget : Bin, Addins.IConfigurable
    {
    	static readonly string AutostartPath =
    		Paths.Combine(Paths.UserHome, ".config/autostart/gnome-do.desktop");
    		
		new public string Name {
			get { return "General"; }
		}
		
        public string Description {
        	get { return ""; }
        }
        
        public string Icon {
        	get { return ""; }
        }
        
        public string[] Themes {
        	get {
        		return new string[] {
        			"Classic",
        			"Glass Frame",
        			"Mini",
        		};
        	}
        }
		
        public GeneralPreferencesWidget ()
        {
        	int themeI;
        	
            Build ();
            
			// Setup theme combo
            themeI = Array.IndexOf (Themes, Do.Preferences.Theme);
            themeI = themeI >= 0 ? themeI : 0;
            theme_combo.Active = themeI;

			// Setup checkboxes
        	hide_check.Active = Do.Preferences.QuietStart;
        	login_check.Active = AutostartEnabled;
        }
        
        public Bin GetConfiguration ()
        {
        	return this;
        }
        
        protected bool AutostartEnabled {
        	get {
        		try {
					using (DesktopItem item = DesktopItem.NewFromFile (
						AutostartPath, DesktopItemLoadFlags.OnlyIfExists)) {
						return item != null &&
							item.GetBoolean ("X-GNOME-Autostart-enabled");
					}
				} catch (Exception e) {
					Log.Error("Failed to get AutostartEnabled: {0}", e.Message);
				}
				return false;
			}
			set {
				try {
					using (DesktopItem item = DesktopItem.NewFromFile (
						AutostartPath, DesktopItemLoadFlags.OnlyIfExists)) {
						if (item != null)
							item.SetBoolean("X-GNOME-Autostart-enabled", value);
					}
				} catch (Exception e) {
					Log.Error("Failed to set AutostartEnabled: {0}", e.Message);
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

        protected virtual void OnThemeComboChanged (object sender, EventArgs e)
        {
        	Do.Preferences.Theme = theme_combo.ActiveText;
        }
    }
}
