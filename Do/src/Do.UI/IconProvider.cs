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
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;

using Gtk;
using Gdk;

namespace Do.UI
{
		
	public static class IconProvider
	{
	
		public static event EventHandler<IconUpdatedEventArgs> IconUpdated;
		public static readonly Pixbuf UnknownPixbuf;
		public const int DefaultIconSize = 80;
		
		const string notYetDownloadedIcon = "gnome-mime-text-plain";
		
		// save icons from web to /tmp/GNOME do/IconsFromWeb/ 
		static readonly string downloadedIconsPath = string.Format ("{1}{0}GNOME Do{0}IconsFromWeb{0}",
								                          Path.DirectorySeparatorChar, 
		                                                  Path.GetTempPath());
		
		// cache of loaded icons: key is iconname_size.
		static Dictionary<string, Pixbuf> pixbufCache;
		
		// list icons from downloaded from web: key is iconname, string is temp-filename-id
		static Dictionary<string, int> downloadedIcons;  
		
		static IconProvider ()
		{
			pixbufCache = new Dictionary<string, Pixbuf> ();
			downloadedIcons = new Dictionary<string, int> ();
						
			UnknownPixbuf = new Pixbuf (Colorspace.Rgb, true, 8, 1, 1);
			UnknownPixbuf.Fill (0x00000000);
			
			Gtk.IconTheme.Default.Changed += OnDefaultIconThemeChanged;
			
			System.IO.Directory.CreateDirectory (downloadedIconsPath);
		}
		
		public static Pixbuf PixbufFromIconName (string name, int size)
		{			
			Pixbuf pixbuf;									
			string name_noext, name_cachekey;
			IconTheme iconTheme;
			
			if (name == null || name.Length == 0) return null;	
			
			/// Is the icon a http/https address? 
			/// Do this before checking cache, as the name can change in this method.
 
			if (name.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
			    name.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
				
				/// Strategy: icons are downloaded in a thread and saved as files in /tmp.
				/// If the file has been downloaded we start downloading it in a seperate thread,
				/// so we don't lock Do with slow network. Until the file is downloaded, we 
				/// change the icon to whatever's in iconNotYetDownloaded.
 				  
				int tempFileId;
				
				lock (downloadedIcons) {
					if (!downloadedIcons.TryGetValue (name, out tempFileId)) {						
						//add this to the list of icons that are loading so we don't load it again
						downloadedIcons.Add (name, -1);
						tempFileId = -2;
					} 						
				}
								
				switch (tempFileId) {						
					
					case -2:  // means that loading has not been requested before
						Thread loadHttpIconThread = 
							new Thread (new ParameterizedThreadStart (DownloadIconFromWeb));
								
						loadHttpIconThread.IsBackground = true;
						loadHttpIconThread.Start (name);
						name = notYetDownloadedIcon;						
						break;					
						
					case -1:  // means that the icon is currenly being loaded in a thread.						
						name = notYetDownloadedIcon;
						break;
						
					default:
						// the icon has been downloaded and is stored in the temp folder
						name = downloadedIconsPath + tempFileId;	
					break;
				}
			}
			
			// Is the icon name in cache?
			name_cachekey = name + "_" + size;
			
			if (pixbufCache.TryGetValue (name_cachekey, out pixbuf)) {				
				return pixbuf;
			}
			 
			iconTheme = Gtk.IconTheme.Default;
			
			// TODO: Use a GNOME ThumbnailFactory
			if (name.StartsWith ("/") || name.StartsWith ("~/") || 
			    name.StartsWith ("file://", StringComparison.OrdinalIgnoreCase)) {
				try {
					pixbuf = new Pixbuf (name, size, size);
				} catch {
					//could not load from file.
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
				
				try	{
					if (iconTheme.HasIcon (name)) {  
						pixbuf = iconTheme.LoadIcon (name, size, 0);
					}
					else if (iconTheme.HasIcon (name_noext)) { 
						pixbuf = iconTheme.LoadIcon (name_noext, size, 0);
					}
					else if (name == "gnome-mime-text-plain" &&
								     iconTheme.HasIcon ("gnome-mime-text")) { 
						pixbuf = iconTheme.LoadIcon ("gnome-mime-text", size, 0);
					}
				} catch {
					pixbuf = null;
				}			
				
				if (pixbuf == null) {
					// If the icon couldn't be found in the default icon theme, search Tango.
					IconTheme tango;

					tango = new IconTheme ();
					tango.CustomTheme = "Tango";
					try 
					{
						if (tango.HasIcon (name)) { 
							pixbuf = tango.LoadIcon (name, size, 0);
						}
						else if (tango.HasIcon (name_noext)) { 
							pixbuf = tango.LoadIcon (name_noext, size, 0);
						}
						else if (name == "gnome-mime-text-plain" && 
							      	tango.HasIcon ("gnome-mime-text")) { 
							pixbuf = tango.LoadIcon ("gnome-mime-text", size, 0);
						}
					} catch {
						pixbuf = null;
					}
				}
			}
			
			if (pixbuf == null && iconTheme.HasIcon ("empty")) {
				try 
				{
					pixbuf = iconTheme.LoadIcon ("empty", size, 0);
				} catch {
					pixbuf = null;					
				}
			}
			
			if (pixbuf == null) {
				pixbuf = UnknownPixbuf;
			}			
			
			// Cache icon pixbuf
			if (pixbuf != null && pixbuf != UnknownPixbuf) {
				pixbufCache[name_cachekey] = pixbuf;				
			}
			
			return pixbuf;
		}
		
		/// <summary>
		/// Delete downloaded icons from the temp folder.
		/// </summary>
		public static void DeleteDownloadedIcons ()
		{
			int fileId;
			
			lock (downloadedIcons) {
				foreach (string name in downloadedIcons.Keys) {
					fileId = downloadedIcons[name];
					try {
						File.Delete (downloadedIconsPath + fileId);
					} catch {}
				}
				downloadedIcons.Clear();				
			}
		} 
						
		static void DownloadIconFromWeb (object o)
		{
			string name = (string)o; //this is also the url

			WebRequest webReq = WebRequest.Create (name);
			
			try {
				byte[] buff;
				int pos, bytesRead;
				HttpWebResponse webResp;

				webResp = (HttpWebResponse) webReq.GetResponse ();			
				try {
					if (webResp.StatusCode != HttpStatusCode.OK || 
					   !webResp.ContentType.StartsWith("image/")) {
						Log.Warn ("Could not download image {0}. Http-status: {1}. Content-type: {2}", 
						         name, webResp.StatusDescription, webResp.ContentType);
						return;
					}
					
					Stream respStream = webResp.GetResponseStream ();
					buff = new byte[webResp.ContentLength];
					pos = 0;					
					do {
						bytesRead = respStream.Read (buff, pos, buff.Length - pos);
						pos += bytesRead;						
					} while (bytesRead > 0);
				} finally {
					webResp.Close ();
				}
							
				// write to temp-file
				int tempFileId = GetTempFileIdForDownloadedIcon ();
				FileStream fstr = new FileStream (downloadedIconsPath + tempFileId, FileMode.Create);
				fstr.Write (buff, 0, buff.Length);
				fstr.Close ();	

				lock(downloadedIcons) {
					downloadedIcons[name] = tempFileId;
				}

				Gtk.Application.Invoke (delegate {
					if (IconUpdated != null) {
						IconUpdated (null, new IconUpdatedEventArgs (name));
					}
				});
				
			} catch (Exception ex) {
				Log.Warn ("Could not download \"{0}\" to temp path. {1}", name, ex.ToString ());
			}
		}
							
		static readonly Random random = new Random ();
		
		static int GetTempFileIdForDownloadedIcon ()
		{
			int fileId;
			string filename;
			
			while (true) {
				fileId = random.Next();				
				filename = downloadedIconsPath + fileId;
				
				try	{
					FileStream fstr = new FileStream (filename, FileMode.CreateNew);
					fstr.Dispose ();
					break;
				} catch {}
			}
			return fileId;
		}
		
		static void OnDefaultIconThemeChanged (object sender, EventArgs args)
		{
			pixbufCache.Clear ();
		}
		
	}
	
	public class IconUpdatedEventArgs : System.EventArgs
	{
		string iconName;
		
		public IconUpdatedEventArgs (string iconName)
		{
			this.iconName = iconName;
		}

		public string IconName { 
			get { return iconName; } 
		}		
	}
}