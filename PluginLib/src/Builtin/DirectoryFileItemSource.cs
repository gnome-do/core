// RecentFileItemSource.cs created with MonoDevelop
// User: dave at 2:18 PM 9/13/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.IO;

using Do.PluginLib;

namespace Do.PluginLib.Builtin
{
	
	public class DirectoryFileItemSource : IItemSource
	{
		
		List<IItem> items;
		int levels;
		string path;

		static DirectoryFileItemSource ()
		{
			Gnome.Vfs.Vfs.Initialize ();
		}

		public DirectoryFileItemSource (string path, int levels)
		{
			string home;
			
			home = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			path = path.Replace ("~", home);
			
			this.path = path;
			this.levels = levels;
			this.items = new List<IItem> ();
			UpdateItems ();
		}
		
		public string Name {
			get { return path; }
		}
		
		public string Description {
			get { return string.Format("Finds items in directory '{0}'", path); }
		}
		
		public string Icon {
			get { return "folder"; }
		}
		
		public ICollection<IItem> Items {
			get { return items; }
		}
		
		public bool UpdateItems ()
		{
			Readitems (path, levels);
			return true;
		}
		
		protected virtual void Readitems (string dir, int levels)
		{
			string[] files;
			string[] directories;
			string path;
			FileItem item;
			
			if (levels == 0) return;
			try {
				files = Directory.GetFiles (dir);
				directories = Directory.GetDirectories (dir);
			} catch (FileNotFoundException) {
				return;
			}
			foreach (string file in files) {
				// No hidden items or special directories.
				if (file.StartsWith (".")) continue;
				
				path = Path.Combine (dir, file);
				try {
					item = FileItem.Create (path);
				} catch (FileNotFoundException) {
					continue;
				}
				items.Add (item);
			}
			foreach (string directory in directories) {
				if (Path.GetFileName (directory).StartsWith (".")) continue;
				item = FileItem.Create (directory);
				items.Add (item);
				Readitems (directory, levels - 1);
			}
		}
	}
}
