/* ManagePluginsPreferencesWidget.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
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

using Gtk;
using Mono.Addins;
using Mono.Addins.Gui;

using Do;

namespace Do.UI
{
	public partial class ManagePluginsPreferencesWidget : Gtk.Bin
	{
		PluginNodeView nview;
		
		public ManagePluginsPreferencesWidget()
		{
			Build ();
			
			nview = new PluginNodeView ();
			nview.PluginToggled += OnPluginToggle;
			
			scrollw.Add (nview);
			scrollw.ShowAll ();
			//SetupService setup = new SetupService (AddinManager.Registry);
		}

		protected virtual void OnBtnManagePluginsClicked (object sender, System.EventArgs e)
		{			
			Window addins = AddinManagerWindow.Show (Do.Controller.PreferencesWindow);
			addins.DeleteEvent += delegate {
				Log.Info ("Completely refreshing universe...");
				Do.UniverseManager.Reload ();
				Log.Info ("Universe completely refreshed!");
			};
		}
		
		private void OnPluginToggle (Addin addin, bool enabled)
		{
			addin.Enabled = enabled;
		}
	}
}
