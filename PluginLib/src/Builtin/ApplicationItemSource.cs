// GCApplicationItemSource.cs created with MonoDevelop
// User: dave at 1:13 AM 8/17/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.IO;
using Gnome.Vfs;

using Do.PluginLib;

namespace Do.PluginLib.Builtin
{
	public class ApplicationItemSource : IItemSource
	{
		
		public static readonly string DesktopFilesDirectory = "/usr/share/applications";
		
		private List<IItem> apps;
		
		public ApplicationItemSource ()
		{
			apps = new List<IItem> ();
			
			Vfs.Initialize();
				
			LoadDesktopFiles ();
		}
		
		public string Name {
			get { return "Applications"; }
		}
		
		public string Description {
			get { return "Finds applications in /usr/share/applications"; }
		}
		
		public string Icon {
			get { return "gtk-run"; }
		}
		
		private void LoadDesktopFiles ()
		{
			Gnome.Vfs.FileInfo[] directoryEntries;
			ApplicationItem app;
			string desktopFile;
			
			directoryEntries = Gnome.Vfs.Directory.GetEntries (DesktopFilesDirectory);
			foreach (Gnome.Vfs.FileInfo file in directoryEntries) {
				desktopFile = Path.Combine (DesktopFilesDirectory, file.Name);
				try {
					app = new ApplicationItem (desktopFile);
				} catch (ApplicationDetailMissingException) {
					continue;
				}
				apps.Add(app);
			}
			
		}
		
		public bool UpdateItems ()
		{
			return false;
		}
		
		public ICollection<IItem> Items {
			get { return apps; }
		}
		
	}
}
