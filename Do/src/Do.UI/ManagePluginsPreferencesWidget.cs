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
using System.Collections.Generic;

using Gtk;
using Mono.Addins;
using Mono.Addins.Gui;
using Mono.Addins.Setup;

using Do;
using Do.Core;
using Do.Addins;

namespace Do.UI
{
	[System.ComponentModel.Category("Do")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ManagePluginsPreferencesWidget : Bin, IConfigurable
	{
		PluginNodeView nview;

		new public string Name {
			get { return "Plugins"; }
		}

		public string Description {
			get { return ""; }
		}

		public string Icon {
			get { return ""; }
		}

		public ManagePluginsPreferencesWidget()
		{
			Build ();

			search_entry.GrabFocus ();
			nview = new PluginNodeView ();
			nview.PluginToggled += OnPluginToggled;
			nview.PluginSelected += OnPluginSelected;

			scrollw.Add (nview);
			scrollw.ShowAll ();

			foreach (string repoName in PluginManager.RepositoryUrls.Keys) {
				show_combo.AppendText (repoName);
			}
			show_combo.AppendText (PluginManager.AllPluginsRepository);
			show_combo.Active = 0;
		}

		public Bin GetConfiguration ()
		{
			return this;
		}

		private void OnPluginSelected (object sender,
									   PluginSelectionEventArgs args)
		{
			UpdateButtonState ();
		}

		protected void UpdateButtonState ()
		{
			btn_configure.Sensitive = false;
			btn_about.Sensitive = false;

			foreach (string id in nview.GetSelectedAddins ()) {
				if (PluginManager.ConfigurablesForAddin (id).Count > 0) {
					btn_configure.Sensitive = true;
					break;
				}
			}
			btn_about.Sensitive = nview.GetSelectedAddins ().Length > 0;
		}

		private void OnPluginToggled (string id, bool enabled)
		{	
			// If the addin isn't found, install it.
			if (null == AddinManager.Registry.GetAddin (id)) {
				IAddinInstaller installer;

				installer = new ConsoleAddinInstaller ();
				//installer = new Mono.Addins.Gui.AddinInstaller ();
				try {
					installer.InstallAddins (AddinManager.Registry,
							string.Format ("Installing \"{0}\" addin...", id),
							new string[] { id });
				} catch (InstallException) {
					return;
				}
			}

			// Now enable or disable the plugin.
			if (enabled) {
				AddinManager.Registry.EnableAddin (id);
			} else {
				AddinManager.Registry.DisableAddin (id);
			}

			UpdateButtonState ();
		}

		protected virtual void OnBtnRefreshClicked (object sender, EventArgs e)
		{
			nview.Refresh ();
			UpdateButtonState ();
		}

		protected void OnBtnUpdateClicked (object sender, EventArgs e)
		{
			if (PluginManager.InstallAvailableUpdates (true))
				nview.Refresh (false);
		}

		protected void OnBtnConfigurePluginClicked (object sender, EventArgs e)
		{
			Window win;
			string[] ids;

			ids = nview.GetSelectedAddins ();
			if (ids.Length == 0) return;

			win = new PluginConfigurationWindow (ids [0]);
			win.Modal = true;
			win.ShowAll ();
		}

		protected virtual void OnBtnAboutClicked (object sender, EventArgs e)
		{
			foreach (string id in nview.GetSelectedAddins ()) {
				try {
					string name = Addin.GetIdName (id).Split ('.')[1];
					Util.Environment.Open (
							"http://wiki.ubuntu.com/GnomeDo/Plugins/" + name);
				} catch { }
			}
		}

		protected virtual void OnShowComboChanged (object sender, EventArgs e)
		{
			nview.ShowRepository = show_combo.ActiveText;
			nview.Filter = search_entry.Text = "";
		}

		protected virtual void OnSearchEntryChanged (object sender, EventArgs e)
		{
			nview.Filter = search_entry.Text;
		}
	}
}
