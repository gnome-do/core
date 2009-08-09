// FileItem.cs
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
using System.Linq;
using IO = System.IO;
using System.Collections.Generic;
using SpecialFolder = System.Environment.SpecialFolder;

using Gnome;
using Mono.Unix;

using Do.Platform;
using Do.Universe;

namespace Do.Universe.Linux {

	/// <summary>
	/// FileItem is an item describing a file. FileItem subclasses
	/// can be created and registered with FileItem for instantiation
	/// in the factory method FileItem.Create.
	/// </summary>
	internal class FileItem : Item, IFileItem, IOpenableItem {

		// A map from absolute paths to icon names.
		static readonly Dictionary<string, string> SpecialFolderIcons;

		// A map from XDG user-dir names to icons names.
		static readonly Dictionary<string, string> SpecialFolderIconsXDG
			= new Dictionary<string, string> () {
				{ "XDG_DESKTOP_DIR", "desktop" },
				{ "XDG_DOWNLOAD_DIR", "folder-downloads" },
				{ "XDG_TEMPLATES_DIR", "folder-templates" },
				{ "XDG_PUBLICSHARE_DIR", "folder-publicshare" },
				{ "XDG_DOCUMENTS_DIR", "folder-documents" },
				{ "XDG_MUSIC_DIR", "folder-music" },
				{ "XDG_PICTURES_DIR", "folder-pictures" },
				{ "XDG_VIDEOS_DIR", "folder-videos" },
			};

		static string MaybeGetSpecialFolderIconForPath (string path)
		{
			return SpecialFolderIcons.ContainsKey (path)
				? SpecialFolderIcons [path]
				: null;
		}

		static FileItem ()
		{
			Gnome.Vfs.Vfs.Initialize ();

			// Initialize SpecialFolderIcons by expaning paths in
			// SpecialFolderIconsXDG. If an icon already exists in SpecialFolderIcons
			// for a given path, we don't overwrite it. This way SpecialFolderIconsXDG
			// defines an ordering for which icons take precedent.
			SpecialFolderIcons = new Dictionary<string, string> ();
			foreach (KeyValuePair<string, string> kv in SpecialFolderIconsXDG) {
				string path = MaybeReadXdgUserDir (kv.Key);
				if (path != null && !SpecialFolderIcons.ContainsKey (path)) {
					SpecialFolderIcons [path] = kv.Value;
				}
			}

		}

		static string MaybeReadXdgUserDir (string key)
		{
			string home_dir, config_dir, env_path, user_dirs_path;

			home_dir = Environment.GetFolderPath (SpecialFolder.Personal);
			config_dir = Environment.GetFolderPath (SpecialFolder.ApplicationData);

			env_path = Environment.GetEnvironmentVariable (key);
			if (!String.IsNullOrEmpty (env_path)) {
				return env_path;
			}

			user_dirs_path = IO.Path.Combine (config_dir, "user-dirs.dirs");
			if (!IO.File.Exists (user_dirs_path)) {
				return null;
			}

			try {
				using (IO.StreamReader reader = new IO.StreamReader (user_dirs_path)) {
					string line;
					while ((line = reader.ReadLine ()) != null) {
						line = line.Trim ();
						int delim_index = line.IndexOf ('=');
						if (delim_index > 8 && line.Substring (0, delim_index) == key) {
							string path = line.Substring (delim_index + 1).Trim ('"');
							bool relative = false;

							if (path.StartsWith ("$HOME/")) {
								relative = true;
								path = path.Substring (6);
							} else if (path.StartsWith ("~")) {
								relative = true;
								path = path.Substring (1);
							} else if (!path.StartsWith ("/")) {
								relative = true;
							}
							return relative ? IO.Path.Combine (home_dir, path) : path;
						}
					}
				}
			} catch (IO.FileNotFoundException) {
			}
			return null;
		}


		/// <summary>
		/// Abbreviates an absolute path by replacing $HOME with ~.
		/// </summary>
		/// <param name="uri">
		/// A <see cref="System.String"/> containing a path.
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> containing the abbreviated path.
		/// </returns>
		public static string DisplayPath (string path)
		{
			if (null == path) throw new ArgumentNullException ();

			return path.Replace (Environment.GetFolderPath (SpecialFolder.Personal), "~");
		}

		string name, description, icon;
		
		/// <summary>
		/// Create a new FileItem for a given file.
		/// </summary>
		/// <param name="path">
		/// A <see cref="System.String"/> containing an absolute path to a file.
		/// </param>
		public FileItem (string path)
		{	
			if (null == path) throw new ArgumentNullException ("path");

			Path = path;
			name = IO.Path.GetFileName (Path);
			// Showing only "~" looks too abbreviated.
			description = DisplayPath (Path) == "~"
				? Path
				: DisplayPath (Path);
		}

		public string Path { get; private set; }
		public override string Name { get { return name; } }
		public override string Description { get { return description; } }

		public string Uri {
			get {
				return System.Uri.EscapeUriString ("file://" + Path);
			}
		}

		public string MimeType {
			get { return Gnome.Vfs.Global.GetMimeType (Path); }
		}

		public override string Icon {
			get {
				// Icon is memoized.
				if (null != icon) return icon;

				// See if the Path is a special folder with a special icon.
				icon = MaybeGetSpecialFolderIconForPath (Path);
				if (icon != null) return icon;

				string large_thumb = Thumbnail.PathForUri (Uri, ThumbnailSize.Large);
				string normal_thumb = Thumbnail.PathForUri (Uri, ThumbnailSize.Normal);
				// Generating the thumbnail ourself is too slow for large files.
				// Suggestion: generate thumbnails asynchronously. Banshee's
				// notion of job queues may be useful.
				if (IO.File.Exists (large_thumb)) {
					icon = large_thumb;	
				} else if (IO.File.Exists (normal_thumb)) {
					icon = normal_thumb;	
				} else {
					try {
						if (MimeType == "x-directory/normal") {
							icon = "folder";
						} else if (MimeType.StartsWith ("image")) {
							icon = "gnome-mime-image";
						} else {
							icon = MimeType.Replace ('/', '-');
							icon = string.Format ("gnome-mime-{0}", icon);
						}
					} catch (NullReferenceException) {
						icon = "gtk-file";
					}
				}
				return icon;
			}
		}

		public virtual void Open ()
		{
			Services.Environment.OpenPath (Path);
		}

	}
}
