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
using System.Collections.Generic;
using System.Linq;

using Gtk;
using GLib;
using Mono.Unix;

using Do.Platform;
using Do.Platform.Common;

namespace Do.UI
{
	public class KeybindingTreeView : TreeView
	{
		enum Column {
			Action = 0,
			BoundKeyString,
			DefaultKeybinding,
			Binding,
			NumColumns
		}
		
		public KeybindingTreeView ()
		{	
			Model = new ListStore (typeof (string), typeof (string), typeof (string), typeof (KeyBinding));
			
			CellRendererText actionCell = new CellRendererText ();
			actionCell.Width = 175;
			InsertColumn (-1, Catalog.GetString ("Action"), actionCell, "text", (int)Column.Action);
			
			CellRendererAccel bindingCell = new CellRendererAccel ();
			bindingCell.AccelMode = CellRendererAccelMode.Other;
			bindingCell.Editable = true;
			bindingCell.AccelEdited += new AccelEditedHandler (OnAccelEdited);
			bindingCell.AccelCleared += new AccelClearedHandler (OnAccelCleared);
			InsertColumn (-1, Catalog.GetString ("Shortcut"), bindingCell, "text", (int)Column.BoundKeyString);
						
			RowActivated += new RowActivatedHandler (OnRowActivated);
			ButtonPressEvent += new ButtonPressEventHandler (OnButtonPress);
			
			AddBindings ();
			Selection.SelectPath (TreePath.NewFirst ());
		}
		
		private void AddBindings ()
		{
			ListStore store = Model as ListStore;
			store.Clear ();

			string ks;

			foreach (KeyBinding binding in Services.Keybinder.Bindings) {
				ks = (string.IsNullOrEmpty (binding.KeyString)) ? Catalog.GetString ("Disabled") : binding.KeyString;
				store.AppendValues (binding.Description, ks, binding.DefaultKeyString, binding);
			}
		}
		
		[GLib.ConnectBefore]
		private void OnButtonPress (object o, ButtonPressEventArgs args)
		{
			TreePath path;
			if (!args.Event.Window.Equals (BinWindow))
				return;
				
			if (GetPathAtPos ((int) args.Event.X, (int) args.Event.Y,out path)) {
				GrabFocus ();
				SetCursor (path, GetColumn ((int) Column.BoundKeyString), true);
			}				
		}
		
		private void OnRowActivated (object o, RowActivatedArgs args)
		{
			GrabFocus ();
			SetCursor (args.Path, GetColumn ((int) Column.BoundKeyString), true);
		}

		private bool DoesBindingConflict (TreeModel model, TreePath path, TreeIter treeiter, string newKeyBinding)
		{
			string binding = model.GetValue (treeiter, (int)Column.BoundKeyString) as string;
			return binding == newKeyBinding;
		}
		
		private void OnAccelEdited (object o, AccelEditedArgs args)
		{
			TreeIter iter;
			ListStore store;
			
			store = Model as ListStore;
			store.GetIter (out iter, new TreePath (args.PathString));
						
			string realKey = Services.Keybinder.KeyEventToString (args.AccelKey, (uint)args.AccelMods);
			
			if (args.AccelKey == (uint)Gdk.Key.Super_L || args.AccelKey == (uint)Gdk.Key.Super_R) {
				//setting CellRenderAccelMode to "Other" ignores the Super key as a modifier
				//this prevents us from grabbing _only_ the Super key.
				return;
			}
			
			// Look for any other rows that have the same binding and then zero that binding out
			// PRECONDITION: There is at most one other row with the same binding.
			TreeIter conflictingBinding = TreeIter.Zero;
			Model.Foreach ((model, path, treeiter) => {
				if (DoesBindingConflict (model, path, treeiter, realKey))
					conflictingBinding = treeiter;
				return false;
			});

			if (!conflictingBinding.Equals (TreeIter.Zero)) {
				if (!SetNewBinding (conflictingBinding, Catalog.GetString ("Disabled"))) {
					// Nothing to do here; we can't set conflicting bindings
					Log<KeybindingTreeView>.Error ("Failed to unset conflicting keybinding");
					return;
				}
			}

			if (!SetNewBinding (iter, realKey)) {
				Log<KeybindingTreeView>.Debug ("Failed to bind key: {0}", realKey);
				Services.Notifications.Notify (Catalog.GetString ("Failed to bind keyboard shortcut"),
				                               Catalog.GetString ("This usually means that some other application has already " +
				                               	"grabbed the key combination"),
				                               "error");
				if (!conflictingBinding.Equals (TreeIter.Zero)) {
					// This has failed for some reason; reset the old binding
					SetNewBinding (conflictingBinding, realKey);
				}
			}
		}

		private bool SetNewBinding (TreeIter iter, string newKeyString)
		{
			var binding = Model.GetValue (iter, (int)Column.Binding) as KeyBinding;
			string keyString = newKeyString == Catalog.GetString ("Disabled") ? "" : newKeyString;
			if (!Services.Keybinder.SetKeyString (binding, keyString)) {
				// SetKeyString will return false with the binding unchanged if it cannot bind the new string
				// Do not update the display keystring in this case.
				return false;
			}
			Model.SetValue (iter, (int)Column.BoundKeyString, newKeyString);
			return true;
		}

		private void OnAccelCleared (object o, AccelClearedArgs args)
		{
			TreeIter iter;
			ListStore store;

			store = Model as ListStore;
			store.GetIter (out iter, new TreePath (args.PathString));

			string keyString;
			try {
				keyString = store.GetValue (iter, (int)Column.DefaultKeybinding).ToString ();
				keyString = (string.IsNullOrEmpty (keyString)) ? Catalog.GetString ("Disabled") : keyString;
			} catch (Exception) {
				keyString = Catalog.GetString ("Disabled");
			}

			SetNewBinding (iter, keyString);
		}
	}
}
