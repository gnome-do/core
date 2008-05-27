// PluginNodeView.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using System.Collections.Generic;

using Gtk;
using Mono.Addins;
using Mono.Addins.Setup;

namespace Do
{
    public class PluginNodeView : NodeView
    {
        const string DescriptionFormat = "<b>{0} <small>v{2}</small></b>\n<small>{1}</small>";

        enum Column {
            Enabled = 0,
            Description,
            Id,
            NumColumns,
        }

        public PluginNodeView () :
            base ()
        {

            CellRenderer cell;

            Model = new ListStore (typeof (bool),
                typeof (string),
                typeof (string));

            cell = new CellRendererToggle ();
            (cell as CellRendererToggle).Activatable = true;
            (cell as CellRendererToggle).Toggled += OnPluginToggle;
            AppendColumn ("Enable", cell, "active", Column.Enabled);

            cell = new Gtk.CellRendererText ();
            (cell as CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
            AppendColumn ("Plugin", cell, "markup", Column.Description);
			
			Refresh ();
		}
		
		public void Refresh () {
            ListStore store;
            SetupService setup;
            Dictionary<string, string> seenAddins;
			
			store = Model as ListStore;
            setup = new SetupService (AddinManager.Registry);
            seenAddins = new Dictionary<string, string> ();
			
			setup.Repositories.UpdateAllRepositories (new ConsoleProgressStatus (true));
			store.Clear ();
			
            // Add addins from online repositories.
            foreach (AddinRepositoryEntry e in setup.Repositories.GetAvailableAddins ()) {
                store.AppendValues (AddinManager.Registry.IsAddinEnabled (e.Addin.Id),
                    Description (e),
                    e.Addin.Id);
                seenAddins [e.Addin.Id] = e.Addin.Id;
            }
            // Add other (non-online) addins.
            foreach (Addin a in AddinManager.Registry.GetAddins ()) {
                if (seenAddins.ContainsKey (a.Id)) continue;
                store.AppendValues (a.Enabled,
                    Description (a),
                    a.Id);
                seenAddins [a.Id] = a.Id;
            }
		}
		
		string Description (string name, string desc, string version)
		{
			return string.Format (DescriptionFormat, name, desc, version);
		}
		string Description (Addin a)
		{
			return Description (a.Name, a.Description.ToString (), a.Version);
		}
		
		string Description (AddinRepositoryEntry a)
		{
			return Description (a.Addin);
		}
		
		string Description (AddinHeader a)
		{
			return Description (a.Name, a.Description, a.Version);
		}

        protected void OnPluginToggle (object sender, ToggledArgs args)
        {
            string addinId;
            bool enabled;
            TreeIter iter;
            ListStore store;

            store = Model as ListStore;
            if (!store.GetIter (out iter, new TreePath (args.Path)))
                return;

            addinId = (string) store.GetValue (iter, (int)Column.Id);
            enabled = (bool) store.GetValue (iter, (int)Column.Enabled);

            if (null != PluginToggled) {
                PluginToggled (addinId, !enabled);
            }
            store.SetValue (iter, 0,
                AddinManager.Registry.IsAddinEnabled (addinId));
        }

        public event PluginToggledDelegate PluginToggled;
        public delegate void PluginToggledDelegate (string addinId, bool enabled);
    }
}
