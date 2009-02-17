// XdgAutostartService.cs
// 
// Copyright © 2009 GNOME Do project
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
using System.IO;
using System.Reflection;
using Do;
using Do.Platform;
using Gnome;

namespace Do.Platform.Linux
{
	
	
	public class XdgAutostartService : IAutostartService
	{
		const string AutostartKey = "Hidden";

		string AutostartDir {
			get {
				return System.IO.Path.Combine (
					Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "autostart");
		    }
		}
		
		string AutostartFileName {
		  get {
		      return System.IO.Path.Combine (AutostartDir, "gnome-do.desktop");
		    }
		}

		void WriteInitialAutostartFile (string fileName)
		{
			try {
				Stream s = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("gnome-do.desktop");
				using (StreamReader sr = new StreamReader (s)) {
					File.AppendAllText (fileName, sr.ReadToEnd ());
				}
			} catch (Exception e) {
				Log<XdgAutostartService>.Error ("Failed to write initial autostart file: {0}", e.Message);
			}
		}
		
		string AutostartUri {
			get {
				return Gnome.Vfs.Uri.GetUriFromLocalPath (AutostartFileName);
			}
		}
		
		DesktopItem AutostartFile {
			get {
				if (!File.Exists (AutostartFileName)) {
					WriteInitialAutostartFile (AutostartFileName);
				}
				return DesktopItem.NewFromUri (AutostartUri, DesktopItemLoadFlags.NoTranslations);
			}
		}
		
		public bool IsAutostartEnabled ()
		{
			DesktopItem autostart = AutostartFile;
			
			if (!autostart.Exists ()) {
				Log<XdgAutostartService>.Error ("Could not open autostart file {0}", AutostartUri);
			}
			
			if (autostart.AttrExists (GnomeAutostartKey)) {
				return !String.Equals(autostart.GetString (AutostartKey), "true", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}
		
		public void SetAutostart (bool enabled)
		{
			DesktopItem autostart = AutostartFile;
			
			autostart.SetBoolean (AutostartKey, !enabled);
			try {
				autostart.Save (null, true);
			} catch (Exception e) {
				Log<XdgAutostartService>.Error ("Failed to update autostart file: {0}", e.Message);
			}
		}
	}
}
