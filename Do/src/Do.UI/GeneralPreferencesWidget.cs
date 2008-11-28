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
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Do;
using Do.Platform;

using Gtk;

namespace Do.UI
{
    [System.ComponentModel.Category("Do")]
    [System.ComponentModel.ToolboxItem(true)]
    public partial class GeneralPreferencesWidget : Bin, Addins.IConfigurable
    {
      const string AutostartAttribute = "X-GNOME-Autostart-enabled";
	
	string AutostartDir {
	  get {
	      return Paths.Combine (Paths.UserHome, ".config/autostart");
	    }
	}
	
	string AutostartFile {
	  get {
	      return Paths.Combine (AutostartDir, "gnome-do.desktop");
	    }
	}
	
	new public string Name {
	  get { return "General"; }
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
	  hide_check.Active = CorePreferences.QuietStart;
	  login_check.Active = AutostartEnabled;
	  notification_check.Active = Platform.StatusIcon.VisibilityPreference;
	}
        
	public Bin GetConfiguration ()
	{
	  return this;
	}
        
	/// <value>
	/// This property sacrifies much efficiency to eschew the gnomedesktop
	/// dependency and to work more reliably.
	/// </value>
	protected bool AutostartEnabled {
	  get {
	      try {
		  return File.Exists (AutostartFile) && !File.ReadAllText (AutostartFile).Contains (AutostartAttribute + "=false");
		} catch (Exception e) {
		  Log.Error ("Failed to get autostart: {0}", e.Message);
		}
		
		return false;
	    }
	    set {
	      try {
		    if (File.Exists (AutostartFile))
		      File.Delete (AutostartFile);
		    if (value) {
		      Directory.CreateDirectory (AutostartDir);
			Stream s = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("gnome-do.desktop");
			using (StreamReader sr = new StreamReader (s))
			  File.AppendAllText (AutostartFile, sr.ReadToEnd ());
		    }
		} catch (Exception e) {
		  Log.Error ("Failed to set autostart: {0}", e.Message);
		}
	    }
	}
	
	protected virtual void OnLoginCheckClicked (object sender, EventArgs e)
	{
	  AutostartEnabled = login_check.Active;
	}
	
	protected virtual void OnHideCheckClicked (object sender, EventArgs e)
	{
	  CorePreferences.QuietStart = hide_check.Active;
	}
	
	protected virtual void OnNotificationCheckClicked (object sender, System.EventArgs e)
	{
	  Platform.StatusIcon.VisibilityPreference = notification_check.Active;
        	
	  if (notification_check.Active)
	    Platform.StatusIcon.Show ();
	  else
	    Platform.StatusIcon.Hide ();
	}
    }
}
