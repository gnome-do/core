// RecentFileItemSource.cs created with MonoDevelop
// User: dave at 2:18 PM 9/13/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using Gnome.Vfs;

using Do.PluginLib;

namespace Do.PluginLib.Builtin
{
	
	public class DirectoryFileItemSource : IItemSource
	{
		
		List<IItem> files;
		int levels;
		string path;
		
		public DirectoryFileItemSource (string path, int levels)
		{
			string home;
			
			home = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			path = path.Replace ("~", home);
			
			this.path = path;
			this.levels = levels;
			this.files = new List<IItem> ();
			UpdateItems ();
		}
		
		public string Name {
			get { return string.Format("Files in directory '{0}'", path); }
		}
		
		public string Icon {
			get { return "folder"; }
		}
		
		public ICollection<IItem> Items {
			get { return files; }
		}
		
		public bool UpdateItems ()
		{
			ReadFiles (path, levels);
			return true;
		}
		
		protected virtual void ReadFiles (string path, int levels)
		{
			FileInfo[] directoryEntries;
			FileItem item;
			string item_path;
			
			if (levels == 0) {
				return;
			}
			
			directoryEntries = Directory.GetEntries (path);
			foreach (FileInfo file in directoryEntries) {
				// No hidden files or special directories.
				if (file.Name.StartsWith (".")) continue;
				
				item_path = System.IO.Path.Combine (path, file.Name);
				item = new FileItem (file.Name, item_path);
				files.Add (item);
				if (file.Type == FileType.Directory) {
					ReadFiles (item_path, levels-1);
				}
			}
		}
	}
}
