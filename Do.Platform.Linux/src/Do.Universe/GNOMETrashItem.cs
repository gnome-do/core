//  GNOMETrashItem.cs
//
//  GNOME Do is the legal property of its developers, whose names are too numerous
//  to list here.  Please refer to the COPYRIGHT file distributed with this
//  source distribution.
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Mono.Unix;

using Do.Platform;
using Do.Universe;

namespace Do.Universe.Linux {
	
	class GNOMETrashItem : Item, IFileItem, IOpenableItem
	{

		string path;
		public string Path {
			get {
				if (path != null) return path;
				
				return path = new [] {
					Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData),
					"Trash",
					"files",
				}.Aggregate (System.IO.Path.Combine);
			}
		}

		public override string Name  {
			get { return "Trash"; }
		}

		public override string Description  {
			get { return "Trash"; }
		}

		public string Uri {
			get { return "trash://"; }
		}

		public override string Icon 
		{
			get {
				if (Directory.Exists (Path) && Directory.GetFileSystemEntries (Path).Any ()) {
					return "user-trash-full";
				} else {
					return "user-trash";
				}
			}
		}

		public void Open ()
		{
			Services.Environment.OpenUrl ("trash://");
		}
	}
}
