/* PluginUpdateNodeView.cs
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
using System.Threading;
using System.Collections.Generic;

using Gtk;
using Mono.Addins;
using Mono.Addins.Setup;

using Do.Core;
using Do.Platform.Linux;
using Do.Universe;
using Do.Interface;

namespace Do.UI
{
	public class PluginUpdateNodeView : NodeView
	{
		enum Column {
			Enabled = 0,
			Description,
			Id,
			NumColumns,
		}

		const int IconSize = 26;
		const int WrapWidth = 305;
		const string DescriptionFormat = "<b>{0}</b> <small>v{2}</small>\n<small>{1}</small>";
			
		string [] addinIds;
		SetupService setup;
		
		public PluginUpdateNodeView (AddinRegistry reg, string [] addinIds) :
			base ()
		{
			CellRenderer cell;
			
			setup = new SetupService (reg);			
			this.addinIds = addinIds;

			RulesHint = true;
			HeadersVisible = false;
			Model = new ListStore (typeof (string), typeof (string));

			cell = new CellRendererPixbuf ();				
			cell.SetFixedSize (IconSize + 8, IconSize + 8);
			AppendColumn ("Icon", cell, new TreeCellDataFunc (IconDataFunc));

			cell = new Gtk.CellRendererText ();
			(cell as CellRendererText).WrapWidth = WrapWidth;
			(cell as CellRendererText).WrapMode = Pango.WrapMode.Word;
			AppendColumn ("Plugin", cell, "markup", Column.Description);

			Refresh ();
		}
		
		protected virtual void IconDataFunc (TreeViewColumn column,
				CellRenderer cell,
				TreeModel model,
				TreeIter iter)
		{
			CellRendererPixbuf renderer;

			renderer = cell as CellRendererPixbuf;
			renderer.Pixbuf = IconProvider.PixbufFromIconName ("package-x-generic", IconSize);
		}

		void Refresh () 
		{
			ListStore store;

			store = Model as ListStore;
			store.Clear ();
			foreach (string addin in addinIds) {
				string name = Addin.GetIdName (addin);
				string version = Addin.GetIdVersion (addin);
				AddinRepositoryEntry[] ares = setup.Repositories.GetAvailableAddin (name, version);
				if (ares.Length == 0) return;
				AddinHeader header = ares[0].Addin;
				store.AppendValues (addin,
					String.Format (DescriptionFormat, header.Name, header.Description, header.Version));
			}
		}
	}
}
