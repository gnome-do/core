// CustomDockItems.cs
// 
// Copyright (C) 2008 GNOME Do
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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Do.UI;
using Do.Addins;
using Do.Universe;
using Do.Platform;

namespace MonoDock.UI
{
	public static class CustomDockItems
	{
		static Dictionary<string, IDockItem> items;
		
		static string DesktopFilesPath {
			get {
				return Paths.Combine (Paths.UserData, "dock_desktop_files");
			}
		}
		
		public static IEnumerable<IDockItem> DockItems {
			get { return items.Values; }
		}
		
		static CustomDockItems()
		{
			items = new Dictionary<string, IDockItem> ();
			
			if (!File.Exists (DesktopFilesPath))
				return;
			
			string[] filenames;
			try {
				using (Stream s = File.OpenRead (DesktopFilesPath)) {
					BinaryFormatter f = new BinaryFormatter ();
					filenames = f.Deserialize (s) as string[];
				}
			} catch (Exception e) {
				filenames = new string[0];
			}
			
			foreach (string s in filenames) {
				if (!File.Exists (s))
					continue;
				IDockItem di = CreatDockItem (s);
				if (di != null)
					items[s] = di;
			}
		}
		
		public static void AddApplication (string desktopFile)
		{
			try {
				IDockItem di = CreatDockItem (desktopFile);
				di.DockAddItem = DateTime.UtcNow;
				if (di != null)
					items[desktopFile] = di;
			} catch {
				Console.Error.WriteLine ("Failed loading {0}", desktopFile);
			}
			Serialize ();
		}
		
		public static void AddFile (string file)
		{
			try {
				IDockItem di = CreateDockFile (file);
				di.DockAddItem = DateTime.UtcNow;
				if (di != null)
					items[file] = di;
			} catch {
				Console.Error.WriteLine ("Failed loading {0}", file);
			}
			Serialize ();
		}
		
		public static void RemoveItem (IDockItem item)
		{
			string s = null;
			foreach (KeyValuePair<string, IDockItem> kvp in items) {
				if (item.Equals (kvp.Value))
					s = kvp.Key;
			}
			if (!string.IsNullOrEmpty (s))
				items.Remove (s);
			Serialize ();
		}
		
		static IDockItem CreatDockItem (string desktopFile)
		{
			IApplicationItem appItem;
			try {
				appItem = UniverseFactory.NewApplicationItem (desktopFile);
			} catch {
				return null;
			}
			return new DockItem (appItem);
		}
		
		static IDockItem CreateDockFile (string file)
		{
			IFileItem fileItem;
			try {
				fileItem = UniverseFactory.NewFileItem (file);
			} catch {
				return null;
			}
			return new DockItem (fileItem);
		}
		
		static void Serialize ()
		{
			try {
				using (Stream s = File.OpenWrite (DesktopFilesPath)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (s, items.Keys.ToArray ());
				}
			} catch (Exception e) {
			}
		}
	}
}
