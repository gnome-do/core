// ApplicationItem.cs
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
//

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Mono.Unix;

using Do.Universe;
using Do.Platform;
using GLib;

namespace Do.Universe.Linux {

	internal class ApplicationItem : Item, IApplicationItem {

		const string DefaultApplicationIcon = "applications-other";

		static IDictionary<string, ApplicationItem> Instances { get; set; }

		static ApplicationItem ()
		{
			Instances = new Dictionary<string, ApplicationItem> ();

			
			// Populate desktop environment flag, for ShouldShow
			string desktopSession = Environment.GetEnvironmentVariable ("XDG_CURRENT_DESKTOP");
			if (desktopSession == null) {
				// Legacy fallbacks:
				// If KDE_FULL_SESSION is true, assume kde.
				// Else, assume GNOME
				if (Environment.GetEnvironmentVariable ("KDE_FULL_SESSION") == "true") {
					desktopSession = "KDE";
				} else {
					desktopSession = "GNOME";
				}
			}
			DesktopAppInfo.DesktopEnv = desktopSession;
		}

		public static ApplicationItem MaybeCreateFromDesktopItem (string path)
		{
			string key = path;
			ApplicationItem appItem;

			if (path == null) throw new ArgumentNullException ("path");

			lock (Instances)
			{
				if (Instances.ContainsKey (key)) {
						appItem = Instances [key];
				} else {
					DesktopAppInfo item = null;
					try {
						item = DesktopAppInfo.NewFromFilename(path);
						appItem = new ApplicationItem (item);
					} catch (Exception e) {
						appItem = null;
						try { item.Dispose (); } catch { }
						Do.Platform.Log.Error ("Could not load desktop item: {0}", e.Message);
						Do.Platform.Log.Debug (e.StackTrace);
					}

					if (appItem != null)
						Instances [key] = appItem;
				}
			}
			return appItem;
		}
		
		public static ApplicationItem MaybeCreateFromCmd (string cmd)
		{
			if (string.IsNullOrEmpty (cmd))
				return null;
			
			List<ApplicationItem> appItems = new List<ApplicationItem> ();
			
			cmd = Regex.Escape (cmd);
			Regex regex = new Regex (string .Format ("(^| ){0}( |)", cmd));
			foreach (ApplicationItem item in Instances.Values) {
				string path = item.Location;
				try {
					if (path.StartsWith ("file://"))
						path = path.Substring ("file://".Length); 
				
					path = Path.GetFileName (path);
				} catch { continue; }
				
				try {
					if (!string.IsNullOrEmpty (path) && !string.IsNullOrEmpty (item.Exec) &&
					    (regex.IsMatch (path) || regex.IsMatch (item.Exec))) {
						appItems.Add (item);
					}
				} catch {
					// it failed, probably a null somewhere, we dont care really
				}
			}
			
			ApplicationItem bestMatch = null;
			
			foreach (ApplicationItem item in appItems) {
				if (bestMatch == null) {
					bestMatch = item;
					continue;
				}
				if (!item.Hidden) {
					if (bestMatch.Hidden) {
						bestMatch = item;
						continue;
					}
					if (item.ShouldShow) {
						if (!bestMatch.ShouldShow || item.Exec.Length < bestMatch.Exec.Length)
							bestMatch = item;
					}
				}
			}
			
			return bestMatch;
		}

		protected GLib.DesktopAppInfo item;
		string name, description, icon;
		IEnumerable<string> categories;

		/// <summary>
		/// Create an application item from a desktop file.
		/// </summary>
		/// <param name="desktopFile">
		/// A <see cref="System.String"/> containing the absolute path of
		/// a desktop (.desktop) file.
		/// </param>
		protected ApplicationItem (GLib.DesktopAppInfo item)
		{
			this.item = item;

			name = item.Name;
			description = item.Description;
			icon = item.Icon.ToString() ?? DefaultApplicationIcon;

			// TODO: Populate categories once GIO# exposes them
			categories = Enumerable.Empty<string> ();
		}
		
		public override string Name {
			get { return name; }
		}

		public override string Description {
			get { return description; }
		}
		
		public override string Icon {
			get { return icon; }
		}
		
		public IEnumerable<string> Categories {
			get { return categories; }
		}

		public bool NoDisplay {
			get {
				return !item.ShouldShow;
			}
		}
		
		public string Exec {
			get { return item.Commandline; }
		}
		
		protected string Location {
			get { return item.Executable; }
		}

		public bool Hidden {
			get { return item.IsHidden; }
		}
		
		public bool IsUserCustomItem {
			get { return item.Executable.StartsWith ("file:///home"); }
		}

		public bool ShouldShow {
			get {
				return item.ShouldShow;
			}
		}
		
		/// <summary>
		/// Executes the application.
		/// </summary>
		public void Run ()
		{
			item.Launch (null, null);
		}

		public void LaunchWithFiles (IEnumerable<IFileItem> files)
		{
			string [] uris = files.Select (file => file.Uri).ToArray ();
			GLib.List glist = new GLib.List (uris as object[], typeof (string), false, true);
			item.Launch (glist, null);
		}
	}
}
