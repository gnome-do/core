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
			cell.Editable = true;
			cell.Edited += OnBindingEdited;
			AppendColumn ("Binding", cell, "text", Column.Binding);
			
			//Selection.Changed += OnSelectionChanged;
			
			AddBindings ();
		}
		
		private void AddBindings ()
		{
			ListStore store;
			
			store = Model as ListStore;
			store.Clear ();
			
			store.AppendValues ("Summon", Do.Preferences.SummonKeyBinding);
		}
		
		private void artistNameCell_Edited (object o, Gtk.EditedArgs args)
		{
			TreeIter iter;
			ListStore store;
			
			store = Model as ListStore;
			store.GetIter (out iter, new Gtk.TreePath (args.Path));
		 
			string binding = musicListStore.GetValue (iter, Column.Binding) as string;
			binding = args.NewText;
		}
	}
}
