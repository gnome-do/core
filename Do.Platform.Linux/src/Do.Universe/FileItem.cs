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
using IO = System.IO;
using System.Collections.Generic;
using SpecialFolder = System.Environment.SpecialFolder;

using Gnome;
using Mono.Unix;

using Do.Platform;
using Do.Universe;
using Do.Platform.Linux;

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
			// Initialize SpecialFolderIcons by expanding paths in
			// SpecialFolderIconsXDG.
			//
			// If an icon already exists in SpecialFolderIcons for a given path, we
			// don't overwrite it. This way SpecialFolderIconsXDG defines an ordering
			// for which icons take precedent; for example, XDG_DOWNLOAD_DIR and
			// XDG_DESKTOP_DIR are often the same folder, so we use the icon for
			// whichever one comes first in SpecialFolderIconsXDG.
			SpecialFolderIcons = new Dictionary<string, string> ();
			foreach (KeyValuePair<string, string> kv in SpecialFolderIconsXDG) {
				string path = Services.Environment.MaybePathForXdgVariable (kv.Key);
				if (path != null && !SpecialFolderIcons.ContainsKey (path)) {
					SpecialFolderIcons [path] = kv.Value;
				}
			}

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
			get {
				GLib.File file = GLib.FileFactory.NewFromCommandlineArg(Path);
				var info = file.QueryInfo ("standard::content-type", GLib.FileQueryInfoFlags.None, null);
				return info.ContentType;
			}
		}

		public override string Icon {
			get {
				// Icon is memoized.
				if (null != icon) return icon;

				// See if the Path is a special folder with a special icon.
				icon = MaybeGetSpecialFolderIconForPath (Path);
				if (icon != null) return icon;

				string large_thumb = Desktop.ThumbnailPathForUri (Uri, DesktopThumbnailSize.Large);
				string normal_thumb = Desktop.ThumbnailPathForUri (Uri, DesktopThumbnailSize.Normal);

				// Generating the thumbnail ourself is too slow for large files.
				// Suggestion: generate thumbnails asynchronously. Banshee's
				// notion of job queues may be useful.
				if (IO.File.Exists (large_thumb)) {
					icon = large_thumb;	
				} else if (IO.File.Exists (normal_thumb)) {
					icon = normal_thumb;	
				} else {
					try {
						if (MimeType == "x-directory/normal" || MimeType == "inode/directory") {
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
