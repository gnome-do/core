/* KeybindingTreeView.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using Mono.Unix;
using Gtk;

namespace Do.UI
{
	public class KeybindingTreeView : TreeView
	{
		enum Column {
			Action = 0,
			Binding,
			NumColumns
		}
		
		public KeybindingTreeView ()
		{	
			Model = new ListStore (typeof (string), typeof (string));
			
			CellRendererText actionCell = new CellRendererText ();
			actionCell.Width = 150;
			InsertColumn (-1, Catalog.GetString ("Action"), actionCell, "text",
				(int) Column.Action);
			
			CellRendererAccel bindingCell = new CellRendererAccel ();
			bindingCell.Editable = true;
			bindingCell.AccelEdited += new AccelEditedHandler (OnAccelEdited);
			bindingCell.AccelCleared += new AccelClearedHandler (OnAccelCleared);
			InsertColumn (-1, Catalog.GetString ("Shortcut"), bindingCell, "text",
				(int) Column.Binding);
			
			RowActivated += new RowActivatedHandler (OnRowActivated);
			ButtonPressEvent += new ButtonPressEventHandler (OnButtonPress);
			
			AddBindings ();
			Selection.SelectPath (TreePath.NewFirst ());
		}
		
		private void AddBindings ()
		{
			ListStore store = Model as ListStore;
			store.Clear ();
			
			store.AppendValues ("Summon",
				Do.Preferences.SummonKeyBinding);
		}
		
		[GLib.ConnectBefore]
		private void OnButtonPress (object o, ButtonPressEventArgs args)
		{
			TreePath path;
			if (!args.Event.Window.Equals (BinWindow))
				return;
				
			if (GetPathAtPos ((int)args.Event.X,(int)args.Event.Y,out path)) {
				GrabFocus ();
				SetCursor (path, GetColumn ((int)Column.Binding), true);
			}				
		}
		
		private void OnRowActivated (object o, RowActivatedArgs args)
		{
			GrabFocus ();
			SetCursor (args.Path, GetColumn ((int)Column.Binding), true);
		}
		
		private void OnAccelEdited (object o, AccelEditedArgs args)
		{
			TreeIter iter;
			ListStore store;
			
			store = Model as ListStore;
			store.GetIter (out iter, new TreePath (args.PathString));
			
			string realKey = Gtk.Accelerator.Name (args.AccelKey, args.AccelMods);
			
			store.SetValue (iter, (int)Column.Binding, realKey);
			SaveBindings ();
		}
		
		private void OnAccelCleared (object o, AccelClearedArgs args)
		{
			TreeIter iter;
			ListStore store;
			
			store = Model as ListStore;
			store.GetIter (out iter, new TreePath (args.PathString));
			store.SetValue (iter, (int)Column.Binding, Catalog.GetString ("DISABLED"));
		}
		
		private void SaveBindings ()
		{
			Model.Foreach (SaveBindingsForeachFunc);
		}
		
		private bool SaveBindingsForeachFunc (TreeModel model, TreePath path, TreeIter iter)
		{
			string action, binding;
			action = model.GetValue (iter, (int)Column.Action) as string;
			
			switch (action.ToLower ()) {
			case "summon":
				binding = model.GetValue (iter, (int)Column.Binding) as string;
				Do.Preferences.SummonKeyBinding = binding;
				break;
			}
			return false;
		}
	}
}
