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
using Gtk;
using Mono.Unix;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Platform;

using Docky.Utilities;
using Docky.Interface.Menus;

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
			get { return "Trash"; }
		}

		protected override string Icon {
			get {
				if (Directory.Exists (Trash) && (Directory.GetFiles (Trash).Any () || Directory.GetDirectories (Trash).Any ()))
					return TrashFullIcon;
				return TrashEmptyIcon;
			}
		}
		
		public TrashDockItem()
		{
			if (!Directory.Exists (Trash))
				Directory.CreateDirectory (Trash);

			SetText (Catalog.GetString ("Trash"));
			
			fsw = new FileSystemWatcher (Trash);
			fsw.IncludeSubdirectories = false;
			fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;

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

		public override bool ReceiveItem (string item)
		{
			if (item.StartsWith ("file://"))
				item = item.Substring ("file://".Length);
			
			// if the file doesn't exist for whatever reason, we bail
			if (!File.Exists (item) && !Directory.Exists (item))
				return false;
			
			Console.WriteLine (item);
			try {
				Services.Environment.Execute (string.Format ("gvfs-trash \"{0}\"", item));
			} catch (Exception e) { 
				Log.Error (e.Message);
				Log.Error ("Could not move {0} to trash", item); 
				return false;
			}
			
			RedrawIcon ();
			return true;
		}
		
		public override void Clicked (uint button, ModifierType state, Cairo.PointD position)
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
			MessageDialog md = new MessageDialog (null, 
												  DialogFlags.Modal,
												  MessageType.Warning, 
												  ButtonsType.OkCancel,
												  Catalog.GetString ("Empty all of the items from the trash?"));
			
			ResponseType result = (ResponseType) md.Run ();
			md.Destroy ();

			if (result == ResponseType.Cancel)
				return;

			// fixme, this breaks the fsw
			if (!Directory.Exists (Trash)) return;
			
			try {
				Directory.Delete (Trash, true);
				Directory.CreateDirectory (Trash);
			} catch { }
				
			// we have now changed the inode and need to get the fsw to reflect this...
			fsw.Path = "/tmp";
			fsw.Path = Trash;
			
			RedrawIcon ();
		}

		#region IRightClickable implementation 
		
		public event EventHandler RemoveClicked;
		
		public IEnumerable<AbstractMenuArgs> GetMenuItems ()
		{
			yield return new SeparatorMenuButtonArgs ();
			
			yield return new SimpleMenuButtonArgs (
					() => Services.Environment.OpenUrl ("trash://"),
					Catalog.GetString ("Open Trash"), TrashFullIcon);
			yield return new SimpleMenuButtonArgs (EmptyTrash,
					Catalog.GetString ("Empty Trash"), Gtk.Stock.Delete);
		}
		
		#endregion 
		
	}
}
