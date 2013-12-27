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
using System.Reflection;

using Gtk;
using Gdk;

namespace Do.Platform.Linux
{
	public static class IconProvider
	{
		const string MissingIconIcon = "emblem-noread";
		public const int DefaultIconSize = 80;
		
		static IconProvider ()
		{
		}
		
		static Pixbuf UnknownPixbuf () 
		{
			Pixbuf pb = new Pixbuf (Colorspace.Rgb, true, 8, 1, 1);
			pb.Fill (0x00000000);
			return pb;
		}
		
		static bool IconIsEmbeddedResource (string name)
		{
			return 0 < name.IndexOf ("@");
		}
		
		static bool IconIsFile (string name)
		{
			return name.StartsWith ("/") ||
				   name.StartsWith ("~/") || 
				   name.StartsWith ("file://", StringComparison.OrdinalIgnoreCase);
		}
		
		static Pixbuf IconFromEmbeddedResource (string name, int size)
		{
			Pixbuf pixbuf = null;
			string resource, assemblyName;
			
			resource = name.Substring (0, name.IndexOf ("@"));
			assemblyName = name.Substring (resource.Length + 1);
			try {
				foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ()) {
					if (asm.FullName != assemblyName) continue;
					pixbuf = new Pixbuf (asm, resource, size, size);
					break;
				}
			} catch (Exception e) {
				Log.Error ("Failed to load icon resource {0} from assembly {1}: {2}",
				     resource, assemblyName, e.Message); 
				Log.Debug (e.StackTrace);
				pixbuf = null;
			}
			return pixbuf;
		}
		
		static Pixbuf IconFromFile (string name, int size)
		{
			Pixbuf pixbuf;

			string home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			name = name.Replace ("~", home);
			try	{
				pixbuf = new Pixbuf (name, size, size);
			} catch {
				pixbuf = null;
			}
			return pixbuf;
		}
		
		static Pixbuf IconFromTheme (string name, int size, IconTheme theme)
		{
			Pixbuf pixbuf = null;
			string name_noext;
			
			// We may have to remove the extension.
			if (name.Contains (".")) {
				name_noext = name.Remove (name.LastIndexOf ("."));
			} else {
				name_noext = name;
			}
			try	{
				if (theme.HasIcon (name)) {  
					pixbuf = theme.LoadIcon (name, size, 0);
				} else if (theme.HasIcon (name_noext)) { 
					pixbuf = theme.LoadIcon (name_noext, size, 0);
				} else if (name == "gnome-mime-text-plain" && theme.HasIcon ("gnome-mime-text")) { 
					pixbuf = theme.LoadIcon ("gnome-mime-text", size, 0);
				}
			} catch {
				pixbuf = null;
			}
		
			return pixbuf;
		}
		
		static Pixbuf GenericFileIcon (int size)
		{
			Pixbuf pixbuf = null;
			if (IconTheme.Default.HasIcon ("gtk-file")) {
				try {
					pixbuf = IconTheme.Default.LoadIcon ("gtk-file", size, 0);
				} catch {
					pixbuf = null;					
				}
			}
			return pixbuf;
		}
		
		public static Pixbuf PixbufFromIconName (string name, int size)
		{
			Pixbuf pixbuf;
			PixbufFromIconName (name, size, out pixbuf);
			return pixbuf;
		}
		
		public static bool PixbufFromIconName (string name, int size, out Pixbuf pixbuf)		
		{
			if (null == name) throw new ArgumentNullException ("name");
			
			// The icon can be loaded from a loaded assembly if the icon has
			// the format: "resource@assemblyname".
			if (IconIsEmbeddedResource (name)) {
				pixbuf = IconFromEmbeddedResource (name, size);
				if (pixbuf != null) return true;
			} 

			if (IconIsFile (name)) {
				pixbuf = IconFromFile (name, size);
				if (pixbuf != null) return true;
			}

			// Try to load icon from defaul theme.
			pixbuf = IconFromTheme (name, size, IconTheme.Default);
			if (pixbuf != null) return true;

			// Try to load a generic file icon.
			if (name.StartsWith ("gnome-mime")) {
				pixbuf = GenericFileIcon (size);
				if (pixbuf != null) return true;
			}
			
			// After this point, we assume that the caller's icon cannot be found, so
			// we attempt to provide a suitable alternative. We return false to
			// indicate that an alternative icon selection was made.
			
			// Try to load a pretty "no icon found" icon.
			if (name != MissingIconIcon) {
				pixbuf = PixbufFromIconName (MissingIconIcon, size);
				if (pixbuf != null) return false;
			}
			
			// If all else fails, use the UnknownPixbuf.
			pixbuf = UnknownPixbuf ();
			return false;
		}
	}
}
