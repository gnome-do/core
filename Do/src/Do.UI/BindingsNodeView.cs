/* BindingsNodeView.cs
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

using Gtk;
using GConf;

namespace Do.UI
{
	public class BindingsNodeView : NodeView
	{
		enum Column {
			Action = 0,
			Binding,
			NumColumns,
		}
		
		bool accelIncomplete = false;
		bool accelComplete = false;
		string mode, binding;
		
		public BindingsNodeView() :
			base ()
		{
			CellRendererText cell;
			
			this.RulesHint = true;
			this.HeadersVisible = true;
			
			this.Model = new ListStore (typeof (string), typeof (string));
			
			cell = new CellRendererText ();
			cell.Editable = false;
			AppendColumn ("Action", cell, "text", Column.Action);
			
			cell = new CellRendererText ();
			cell.Editable = false;
			//cell.Edited += OnBindingEdited;
			AppendColumn ("Binding", cell, "text", Column.Binding);
			
			Selection.Changed += new EventHandler (OnKeysTreeViewSelectionChange);
			
			AddBindings ();
		}
		
		private void AddBindings ()
		{
			ListStore store;
			
			store = Model as ListStore;
			store.Clear ();
			
			store.AppendValues ("Summon", Do.Preferences.SummonKeyBinding);
		}
		
		void OnKeysTreeViewSelectionChange (object sender, EventArgs e)
		{
			TreeSelection sel = sender as TreeSelection;
			TreeModel model;
			TreeIter iter;
			ListStore store;
			
			accelComplete = false;
			
			if (sel.GetSelected (out model, out iter)) {
				store = Model as ListStore;
				//TreePath path = model.GetPath (iter);
				store.GetIter (out iter, model.GetPath (iter));
				KeyPressEvent += new KeyPressEventHandler (OnAccelEntryKeyPress);
				//this.KeyReleaseEvent += new KeyReleaseEventHandler (OnAccelEntryKeyRelease);
				store.SetValue (iter, (int)Column.Binding, binding);
			}
		}
		
		private void OnBindingEdited (object o, Gtk.EditedArgs args)
		{
			TreeIter iter;
			ListStore store;
			
			store = Model as ListStore;
			store.GetIter (out iter, new Gtk.TreePath (args.Path));
			
			this.KeyPressEvent += new KeyPressEventHandler (OnAccelEntryKeyPress);
			//this.KeyReleaseEvent += new KeyReleaseEventHandler (OnAccelEntryKeyRelease);
			
			store.SetValue (iter, (int)Column.Binding, binding);
		}
		
		[GLib.ConnectBefore]
		void OnAccelEntryKeyPress (object sender, KeyPressEventArgs e)
		{
			Gdk.ModifierType mod = e.Event.State;
			Gdk.Key key = e.Event.Key;
			string accel;
			
			e.RetVal = true;
			
			if (accelComplete) {
				accelIncomplete = false;
				accelComplete = false;
				mode = null;
				
				if (key.Equals (Gdk.Key.BackSpace))
					return;
			}
			
			accelComplete = false;
			if ((accel = KeyBindingManager.AccelFromKey (key, mod)) != null) {
				binding = KeyBindingManager.Binding (mode, accel);
				accelIncomplete = false;
				if (mode != null)
					accelComplete = true;
				else
					mode = accel;
			} else {
				//accel = mode != null ? mode + "|" : String.Empty;
				accelIncomplete = true;
				
				if ((mod & Gdk.ModifierType.ControlMask) != 0)
					accel += "<Control>";
				if ((mod & Gdk.ModifierType.Mod1Mask) != 0 ||
				    (key.Equals (Gdk.Key.Meta_L) || key.Equals (Gdk.Key.Meta_R)))
					accel += "<Alt>";
				if ((mod & Gdk.ModifierType.ShiftMask) != 0)
					accel += "<Shift>";
				if ((mod & Gdk.ModifierType.SuperMask) != 0)
					accel += "<Super>";
				
				if (key.Equals (Gdk.Key.Control_L) || key.Equals (Gdk.Key.Control_R))
					accel += "<Control>";
				else if (key.Equals (Gdk.Key.Alt_L) || key.Equals (Gdk.Key.Alt_R))
					accel += "<Alt>";
				else if (key.Equals (Gdk.Key.Shift_L) || key.Equals (Gdk.Key.Shift_R))
					accel += "<Shift>";
				else if (key.Equals (Gdk.Key.Super_L) || key.Equals (Gdk.Key.Super_R))
					accel += "<Super>";
				
				Console.Error.WriteLine (accel);
				binding = accel;
			}
		}
		/*
		void OnAccelEntryKeyRelease (object sender, KeyReleaseEventArgs e)
		{
			if (accelIncomplete)
				binding = mode != null ? mode : String.Empty;
		}
		*/
	}
}
