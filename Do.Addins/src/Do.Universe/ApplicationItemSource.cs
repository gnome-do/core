// GCApplicationItemSource.cs created with MonoDevelop
// User: dave at 1:13 AM 8/17/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.IO;

namespace Do.Universe
{
	public class ApplicationItemSource : IItemSource
	{
		
		public static readonly string[] DesktopFilesDirectories = {
			"/usr/share/applications",
			"/usr/share/applications/kde",
			"/usr/local/share/applications",
		};
		
		private List<IItem> apps;

		static ApplicationItemSource ()
		{
			Gnome.Vfs.Vfs.Initialize ();
		}
		
		public ApplicationItemSource ()
		{
			apps = new List<IItem> ();			
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
			ApplicationItem app;
			
			if (!Directory.Exists (desktop_files_dir)) return;
			foreach (string filename in Directory.GetFiles (desktop_files_dir)) {
				if (!filename.EndsWith (".desktop")) continue;
				
				try {
					app = new ApplicationItem (filename);
				} catch {
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
