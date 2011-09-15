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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using Gnome;
using Mono.Unix;

using Do.Universe;
using Do.Platform;

namespace Do.Universe.Linux {

	internal class ApplicationItem : Item, IApplicationItem {

		const string DefaultApplicationIcon = "applications-other";

		static IDictionary<string, ApplicationItem> Instances { get; set; }

		static ApplicationItem ()
		{
			Instances = new Dictionary<string, ApplicationItem> ();
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
					DesktopItem item = null;
					try {
						item = DesktopItem.NewFromFile (path, 0);
						appItem = new ApplicationItem (item);
					} catch (Exception e) {
						appItem = null;
						try { item.Dispose (); } catch { }
						Log.Error ("Could not load desktop item: {0}", e.Message);
						Log.Debug (e.StackTrace);
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
					if (item.IsAppropriateForCurrentDesktop) {
						if (!bestMatch.IsAppropriateForCurrentDesktop || item.Exec.Length < bestMatch.Exec.Length)
							bestMatch = item;
					}
				}
			}
			
			return bestMatch;
		}

		protected DesktopItem item;
		string name, description, icon;
		IEnumerable<string> categories;

		/// <summary>
		/// Create an application item from a desktop file.
		/// </summary>
		/// <param name="desktopFile">
		/// A <see cref="System.String"/> containing the absolute path of
		/// a desktop (.desktop) file.
		/// </param>
		protected ApplicationItem (DesktopItem item)
		{
			this.item = item;
			if (item.Exists ()) {
				name = item.GetLocalestring ("Name");
				description = item.GetLocalestring ("Comment");
				icon = item.GetString ("Icon") ?? DefaultApplicationIcon;
				
				if (item.AttrExists ("Categories"))
					categories = item.GetString ("Categories").Split (';');
				else
					categories = Enumerable.Empty<string> ();
			} else {
				name = Path.GetFileName (item.Location);
				description =
					Catalog.GetString ("This application could not be indexed.");
				icon = DefaultApplicationIcon;
				categories = Enumerable.Empty<string> ();
			}
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
				return item.AttrExists ("NoDisplay") && item.GetBoolean ("NoDisplay");
			}
		}
		
		public string Exec {
			get { return item.GetString ("Exec"); }
		}
		
		protected string Location {
			get { return item.Location; }
		}

		public bool Hidden {
			get { return item.GetBoolean ("NoDisplay"); }
		}
		
		public bool IsUserCustomItem {
			get { return item.Location.StartsWith ("file:///home"); }
		}

		public bool IsAppropriateForCurrentDesktop {
			get {
				string onlyShowIn = item.GetString ("OnlyShowIn");
				string notShowIn = item.GetString ("NotShowIn");
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

				// It doesn't make sense for a DE to appear in both OnlyShowIn and
				// NotShowIn.  We choose to prefer OnlyShowIn in this case as it makes
				// the following checks easier.
				if (onlyShowIn != null) {
					foreach (string environment in onlyShowIn.Split (';')) {
						if (desktopSession.Equals (environment, StringComparison.InvariantCultureIgnoreCase)) {
							return true;
						}
					}
					// There's an OnlyShowIn attribute, and the current environment doesn't match.
					return false;
				}

				if (notShowIn != null) {
					foreach (string environment in notShowIn.Split (';')) {
						if (desktopSession.Equals (environment, StringComparison.InvariantCultureIgnoreCase)) {
							return false;
						}
					}
				}

				return true;
			}
		}
		
		/// <summary>
		/// Executes the application.
		/// </summary>
		public void Run ()
		{
			item.Launch (null, DesktopItemLaunchFlags.OnlyOne);
		}

		public void LaunchWithFiles (IEnumerable<IFileItem> files)
		{
			string [] uris = files.Select (file => file.Uri).ToArray ();
			GLib.List glist = new GLib.List (uris as object[], typeof (string), false, true);
			item.Launch (glist, DesktopItemLaunchFlags.OnlyOne);
		}
	}
}
