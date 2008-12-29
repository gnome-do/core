// TrashDockItem.cs
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
using System.IO;
using System.Linq;

using Gdk;

using Docky.Utilities;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Platform;

namespace Docky.Interface
{
	
	
	public class TrashDockItem :  AbstractDockItem
	{
		
		string Trash {
			get { 
				return System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "Trash/files/");
			}
		}
		
		public override bool IsAcceptingDrops {
			get { return true; }
		}
		
		public TrashDockItem()
		{
		}
		
		protected override Pixbuf GetSurfacePixbuf ()
		{
			if (Directory.GetFiles (Trash).Any ())
				return IconProvider.PixbufFromIconName ("gnome-stock-trash-full", DockPreferences.FullIconSize);
			return IconProvider.PixbufFromIconName ("gnome-stock-trash", DockPreferences.FullIconSize);
		}

		#region IDockDragAwareItem implementation 
		
		public override bool ReceiveItem (string item)
		{
			bool trashHadFiles = Directory.GetFiles (Trash).Any ();
			
			if (item.StartsWith ("file://"))
				item = item.Substring ("file://".Length);
			
			if (!System.IO.File.Exists (item) && !System.IO.Directory.Exists (item))
				return false;
			
			try {
				System.IO.File.Move (item, Path.Combine (Trash, Path.GetFileName (item)));
			} catch (Exception e) { 
				Log.Error (e.Message);
				Log.Error ("Could not move {0} to trash", item); 
				return false;
			}
			
			if (!trashHadFiles)
				RedrawIcon ();
			return true;
		}
		
		#endregion 
		
		
		public override string Description {
			get {
				return "Trash";
			}
		}
		
		public override void Clicked (uint button)
		{
			if (button == 1) {
				Services.Environment.OpenUrl ("trash://");
				LastClick = DateTime.UtcNow;
			}
		}

	}
}
