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
using System.Net;
using System.Threading;
using System.Collections.Generic;

using Gtk;
using Gdk;

namespace Do.Addins.UI
{
		
	public static class IconProvider
	{
	
		public static event EventHandler<IconUpdatedEventArgs> IconUpdated;
		public static readonly Pixbuf UnknownPixbuf;
		public const int DefaultIconSize = 80;
		
		const string AwaitingDownloadIcon = "application-x-executable";
		
		// Cache of loaded icons: key is "iconname_size".
		static Dictionary<string, Pixbuf> pixbufCache;
		
		// Icons downloaded from web: key is iconname, string is temp-file id.
		static Dictionary<string, int> downloadedIcons;  

		// Special flags for use in downloadedIcons.
		const int IconLoadInProgress = -1;
		const int IconNotYetRequested = -2;

		static IconTheme[] themes;

		static IconProvider ()
		{
			pixbufCache = new Dictionary<string, Pixbuf> ();
			downloadedIcons = new Dictionary<string, int> ();
						
			UnknownPixbuf = new Pixbuf (Colorspace.Rgb, true, 8, 1, 1);
			UnknownPixbuf.Fill (0x00000000);

			themes = new IconTheme[2];
			themes[0] = IconTheme.Default;
			
			IconTheme.Default.Changed += OnDefaultIconThemeChanged;
			
			if (!Directory.Exists (TemporaryIconsPath)) {
				try {
					Directory.CreateDirectory (TemporaryIconsPath);
				} catch (Exception e) {
					//Log.Error ("Could not create temporary icons directory {0}: {1}",
					//		TemporaryIconsPath, e.Message);
				}
			}
		}

		public static string TemporaryIconsPath
		{
			get {
				return Path.Combine (
					Path.GetTempPath (),
					Path.Combine ("gnome-do", "icons"));
			}
		}
		
		public static Pixbuf PixbufFromIconName (string name, int size)
		{			
			Pixbuf pixbuf;									
			string name_noext, iconKey;
			int fileId;
			IconTheme theme;
			
			if (string.IsNullOrEmpty (name)) return null;	

			/// Is the icon a http/https address? 
			/// Do this before checking cache, as the name can change in this method.
			if (name.StartsWith ("http://", StringComparison.OrdinalIgnoreCase) || 
			    name.StartsWith ("https://", StringComparison.OrdinalIgnoreCase)) {
				/// Strategy: icons are downloaded in a thread and saved as files in
				/// /tmp.  If the file has not been downloaded we start downloading it
				/// in a seperate thread, so we don't lock Do with slow network. Until
				/// the file is downloaded, we change the icon to a default icon.
				
				lock (downloadedIcons) {
					if (!downloadedIcons.TryGetValue (name, out fileId)) {						
						// Add this to the list of icons that are loading so we don't load
						// it again.
						downloadedIcons.Add (name, IconLoadInProgress);
						fileId = IconNotYetRequested;
					} 						
				}
								
				switch (fileId) {						
					case IconNotYetRequested:
						Thread loadHttpIconThread = 
							new Thread (new ParameterizedThreadStart (DownloadIconFromWeb));
						loadHttpIconThread.IsBackground = true;
						loadHttpIconThread.Start (name);
						name = AwaitingDownloadIcon;						
						break;					
					case IconLoadInProgress:
						name = AwaitingDownloadIcon;
						break;
					default:
						// The icon has been downloaded and is stored in the temp folder.
						name = Path.Combine (TemporaryIconsPath, fileId.ToString ());	
					break;
				}
			}
			
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
			}
			else {					
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
			if (pixbuf == null && themes[0].HasIcon ("empty")) {
				try {
					pixbuf = themes[0].LoadIcon ("empty", size, 0);
				} catch {
					pixbuf = null;					
				}
			}
			if (pixbuf == null) {
				pixbuf = UnknownPixbuf;
			}			
			// Cache icon pixbuf.
			if (pixbuf != null && pixbuf != UnknownPixbuf) {
				pixbufCache[iconKey] = pixbuf;				
			}
			
			return pixbuf;
		}
		
		/// <summary>
		/// Delete downloaded icons from the temp folder.
		/// </summary>
		public static void DeleteDownloadedIcons ()
		{
			lock (downloadedIcons) {
				foreach (int fileId in downloadedIcons.Values) {
					try {
						File.Delete (Path.Combine (TemporaryIconsPath, fileId.ToString ()));
					} catch { }
				}
				downloadedIcons.Clear ();				
			}
		} 
						
		static void DownloadIconFromWeb (object o)
		{
			WebRequest request;
			string name;
			byte[] buffer;
			int position, bytesRead;
			
			name = o as string;
			request = WebRequest.Create (name);
			try {
				int fileId;
				HttpWebResponse response;
				Stream stream;

				fileId = GetTempFileIdForDownloadedIcon ();
				response = request.GetResponse () as HttpWebResponse;			
				try {
					if (response.StatusCode != HttpStatusCode.OK || 
					   !response.ContentType.StartsWith ("image/")) {
						//Log.Error ("Could not download icon image {0}. Http-status: {1}. Content-type: {2}", 
						//         name, response.StatusDescription, response.ContentType);
						return;
					}
					stream = response.GetResponseStream ();
					buffer = new byte[response.ContentLength];
					position = 0;					
					do {
						bytesRead = stream.Read (buffer, position, buffer.Length - position);
						position += bytesRead;						
					} while (bytesRead > 0);
				} finally {
					response.Close ();
				}

				File.WriteAllBytes (
						Path.Combine (TemporaryIconsPath, fileId.ToString ()),
						buffer);
				lock (downloadedIcons) {
					downloadedIcons[name] = fileId;
				}
				Gtk.Application.Invoke (delegate {
					if (IconUpdated != null) {
						IconUpdated (null, new IconUpdatedEventArgs (name));
					}
				});
			} catch (Exception ex) {
				//Log.Error ("Could not download icon file \"{0}\": {1}", name, ex.Message);
			}
		}
							
		static int GetTempFileIdForDownloadedIcon ()
		{
			Random random;
			int fileId;
			string fileName;
			
			random = new Random ();
			do {
				fileId = random.Next ();
				fileName = Path.Combine (TemporaryIconsPath, fileId.ToString ());
			} while (File.Exists (fileName));
			return fileId;
		}
		
		static void OnDefaultIconThemeChanged (object sender, EventArgs args)
		{
			pixbufCache.Clear ();
		}
	}
	
	public class IconUpdatedEventArgs : EventArgs
	{
		string iconName;
		
		public IconUpdatedEventArgs (string iconName)
		{
			this.iconName = iconName;
		}

		public string IconName
		{ 
			get { return iconName; } 
		}		
	}
}
