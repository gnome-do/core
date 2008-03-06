/* IconProvider.cs
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
using System.IO;
using System.Collections.Generic;

using Gtk;
using Gdk;

namespace Do.UI
{
		
	public static class IconProvider
	{
	
		public static readonly Pixbuf UnknownPixbuf;
		public const int DefaultIconSize = 80;
		
		// Cache of loaded icons: key is "iconname_size".
		static Dictionary<string, Pixbuf> pixbufCache;

		static IconTheme  [] themes;

		static IconProvider ()
		{
			pixbufCache = new Dictionary<string, Pixbuf> ();
						
			UnknownPixbuf = new Pixbuf (Colorspace.Rgb, true, 8, 1, 1);
			UnknownPixbuf.Fill (0x00000000);

			themes = new IconTheme  [2];
			themes  [0] = IconTheme.Default;
			
			IconTheme.Default.Changed += OnDefaultIconThemeChanged;
		}

		public static Pixbuf PixbufFromIconName (string name, int size)
		{			
			Pixbuf pixbuf;									
			string name_noext, iconKey;
			IconTheme theme;
			
			if (string.IsNullOrEmpty (name)) return null;	

			// Is the icon name in cache?
			iconKey = string.Format ("{0}_{1}", name, size);
			if (pixbufCache.TryGetValue (iconKey, out pixbuf)) {				
				return pixbuf;
			}
			 
			// TODO: Use a GNOME ThumbnailFactory
			if (name.StartsWith ("/") ||
				name.StartsWith ("~/") || 
			    name.StartsWith ("file://", StringComparison.OrdinalIgnoreCase)) {
				try {
					pixbuf = new Pixbuf (name, size, size);
				} catch {
					// Could not load from file.
					pixbuf = null;
				}			
			} else {					
				if (name.Contains (".")) {
					name_noext = name.Remove (name.LastIndexOf ("."));
				}
				else {
					name_noext = name;
				}
				
				theme = IconTheme.Default;
				try	{
					if (theme.HasIcon (name)) {  
						pixbuf = theme.LoadIcon (name, size, 0);
					}
					else if (theme.HasIcon (name_noext)) { 
						pixbuf = theme.LoadIcon (name_noext, size, 0);
					}
					else if (name == "gnome-mime-text-plain" &&
							 theme.HasIcon ("gnome-mime-text")) { 
						pixbuf = theme.LoadIcon ("gnome-mime-text", size, 0);
					}
				} catch {
					pixbuf = null;
				}			

				// Try Tango theme if no icon was found.
				// This code duplication (loop unrolling) was necessary
				// becuase something funny was happening with the icon loading
				// when using themes stored in an array.
				if (pixbuf == null) {
					theme = new IconTheme ();
					theme.CustomTheme = "Tango";
					try	{
							if (theme.HasIcon (name)) {  
								pixbuf = theme.LoadIcon (name, size, 0);
							}
							else if (theme.HasIcon (name_noext)) { 
								pixbuf = theme.LoadIcon (name_noext, size, 0);
							}
							else if (name == "gnome-mime-text-plain" &&
											 theme.HasIcon ("gnome-mime-text")) { 
								pixbuf = theme.LoadIcon ("gnome-mime-text", size, 0);
							}
						} catch {
							pixbuf = null;
						}		
				}
			}
				
			theme = IconTheme.Default;
			if (pixbuf == null && name.StartsWith ("gnome-mime") &&
					themes [0].HasIcon ("gtk-file")) {
				try {
					pixbuf = themes [0].LoadIcon ("gtk-file", size, 0);
				} catch {
					pixbuf = null;					
				}
			}
			if (pixbuf == null && themes [0].HasIcon ("emblem-noread")) {
				try {
					pixbuf = themes [0].LoadIcon ("emblem-noread", size, 0);
				} catch {
					pixbuf = null;					
				}
			}
			if (pixbuf == null) {
				pixbuf = UnknownPixbuf;
			}			
			// Cache icon pixbuf.
			if (pixbuf != null && pixbuf != UnknownPixbuf) {
				pixbufCache [iconKey] = pixbuf;				
			}
			
			return pixbuf;
		}
							
		static void OnDefaultIconThemeChanged (object sender, EventArgs args)
		{
			pixbufCache.Clear ();
		}
	}
}
