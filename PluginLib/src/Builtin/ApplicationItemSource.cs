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
		
		public static readonly string[] DesktopFilesDirectories = {
			"/usr/share/applications",
			"/usr/share/applications/kde",
			"/usr/local/share/applications",
		};
		
		private List<IItem> apps;
		
		public ApplicationItemSource ()
		{
			apps = new List<IItem> ();
			
			Vfs.Initialize();
			
			UpdateItems ();
		}
		
		public string Name {
			get { return "Applications"; }
		}
		
		public string Description {
			get { return "Finds applications in many default locations."; }
		}
		
		public string Icon {
			get { return "gtk-run"; }
		}
		
		private void LoadDesktopFiles (string desktop_files_dir)
		{
			Gnome.Vfs.FileInfo[] directoryEntries;
			ApplicationItem app;
			string desktopFile;
			
			try {
				directoryEntries = Gnome.Vfs.Directory.GetEntries (desktop_files_dir);
			} catch {
				return;
			}
			foreach (Gnome.Vfs.FileInfo file in directoryEntries) {
				desktopFile = Path.Combine (desktop_files_dir, file.Name);
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
			apps.Clear ();
			foreach (string dir in DesktopFilesDirectories) {
				LoadDesktopFiles (dir);
			}
			return true;
		}
		
		public ICollection<IItem> Items {
			get { return apps; }
		}
		
	}
}
