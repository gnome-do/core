/* PluginConfigurationWindow.cs
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
using Mono.Addins;

using Do.Core;
using Do.Addins;

namespace Do.UI
{
    public partial class PluginConfigurationWindow : Gtk.Window
    {
        public PluginConfigurationWindow (string id) : 
            base(Gtk.WindowType.Toplevel)
        {
            Addin addin;
            ICollection<IConfigurable> configs;

            Build ();

            addin = AddinManager.Registry.GetAddin (id);
            configs = PluginManager.ConfigurablesForAddin (id);
            Title = string.Format ("{0} Configuration", addin.Name);
            notebook.RemovePage (0);
            notebook.ShowTabs = configs.Count > 1;

            foreach (IConfigurable configurable in configs) {
                Bin config;

                config = configurable.GetConfiguration ();
                notebook.AppendPage (config, new Label (configurable.Name));
                config.ShowAll ();
            }
        }

        protected virtual void OnBtnCloseClicked (object sender, EventArgs e)
        {
            Hide ();
        }
    }
}
