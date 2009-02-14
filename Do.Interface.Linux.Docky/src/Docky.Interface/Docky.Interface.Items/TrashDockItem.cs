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
using Mono.Unix;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Platform;

using Docky.Utilities;

namespace Docky.Interface
{
	
	
	public class TrashDockItem :  AbstractDockletItem, IRightClickable
	{
		const string TrashEmptyIcon = "gnome-stock-trash";
		const string TrashFullIcon = "gnome-stock-trash-full";

		FileSystemWatcher fsw;
		
		string Trash {
			get { 
				return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "Trash/files/");
			}
		}
		
		public override bool IsAcceptingDrops {
			get { return true; }
		}
		
		public override string Name {
			get {
				return "Trash";
			}
		}

		
		public TrashDockItem()
		{
			if (!Directory.Exists (Trash))
				Directory.CreateDirectory (Trash);

			SetText (Catalog.GetString ("Trash"));
			
			fsw = new FileSystemWatcher (Trash);
			fsw.IncludeSubdirectories = true;
			fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
				| NotifyFilters.FileName | NotifyFilters.DirectoryName;

			fsw.Changed += HandleChanged;
			fsw.Created += HandleChanged;
			fsw.Deleted += HandleChanged;
			fsw.EnableRaisingEvents = true;
		}

		void HandleChanged(object sender, FileSystemEventArgs e)
		{
			Gtk.Application.Invoke (delegate {
				RedrawIcon ();
			});
		}
		
		protected override Pixbuf GetSurfacePixbuf (int size)
		{
			if (Directory.Exists (Trash) && Directory.GetFiles (Trash).Any ())
				return IconProvider.PixbufFromIconName (TrashFullIcon, size);
			return IconProvider.PixbufFromIconName (TrashEmptyIcon, size);
		}

		public override bool ReceiveItem (string item)
		{
			if (item.StartsWith ("file://"))
				item = item.Substring ("file://".Length);
			
			// if the file doesn't exist for whatever reason, we bail
			if (!File.Exists (item) && !Directory.Exists (item))
				return false;
			
			try {
				File.Move (item, Path.Combine (Trash, Path.GetFileName (item)));
			} catch (Exception e) { 
				Log.Error (e.Message);
				Log.Error ("Could not move {0} to trash", item); 
				return false;
			}
			
			RedrawIcon ();
			return true;
		}
		
		public override void Clicked (uint button, ModifierType state, Gdk.Point position)
		{
			if (button == 1) {
				Services.Environment.OpenUrl ("trash://");
				AnimationType = ClickAnimationType.Bounce;
			} else {
				AnimationType = ClickAnimationType.None;
			}
			
			base.Clicked (button, state, position);
		}

		void EmptyTrash ()
		{
			// fixme, this breaks the fsw
			if (!Directory.Exists (Trash)) return;
			
			Directory.Delete (Trash, true);
			Directory.CreateDirectory (Trash);
			
			fsw.Path = "/tmp";
			fsw.Path = Trash;
			
			RedrawIcon ();
		}

		#region IRightClickable implementation 
		
		public event EventHandler RemoveClicked;
		
		public IEnumerable<Menus.AbstractMenuButtonArgs> GetMenuItems ()
		{
			yield return new Docky.Interface.Menus.SimpleMenuButtonArgs (() => Services.Environment.OpenUrl ("trash://"),
			                                                             Catalog.GetString ("Open Trash"), TrashFullIcon);
			yield return new Docky.Interface.Menus.SimpleMenuButtonArgs (EmptyTrash, Catalog.GetString ("Empty Tash"), Gtk.Stock.Delete);
		}
		
		#endregion 
		
	}
}
