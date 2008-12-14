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
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

using Gtk;
using Mono.Addins;
using Mono.Addins.Gui;
using Mono.Addins.Setup;

using Do;
using Do.Core;
using Do.Addins;
using Do.Platform;

namespace Do.UI
{
	[System.ComponentModel.Category("Do")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ManagePluginsPreferencesWidget : Bin, IConfigurable
	{

		const string PluginWikiPageFormat
			= "http://www.gnomedo.com/wiki/index.php?title={0}_Plugin";

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
			
			TargetEntry[] targets = {
				new TargetEntry ("text/uri-list", 0, 0), 
			};
			
			Gtk.Drag.DestSet (nview, DestDefaults.All, targets, Gdk.DragAction.Copy);
			nview.DragDataReceived += new DragDataReceivedHandler (OnDragDataReceived);
			
			scrollw.Add (nview);
			scrollw.ShowAll ();

			//foreach (string repo in PluginManager.RepositoryUrls.Keys) {
			//	show_combo.AppendText (repo);
			foreach (string repoName in PluginManager.RepositoryUrls.Keys) {
				if (PluginManager.RepositoryUrls [repoName].Any ())
					show_combo.AppendText (repoName);
			}
			show_combo.AppendText (PluginManager.AllPluginsRepository);
			show_combo.Active = 0;
		}
		
		protected void OnDragDataReceived (object sender, DragDataReceivedArgs args)
		{
			string data = System.Text.Encoding.UTF8.GetString (args.SelectionData.Data);
			data = data.TrimEnd ('\0'); //sometimes we get a null at the end, and it crashes us
			
			string [] uriList = Regex.Split (data, "\r\n");
			List<string> errors = new List<string> ();
			foreach (string uri in uriList) {
				string file;
				string path;
				
				try {
					file = uri.Remove (0, 7);
					if (!file.EndsWith (".dll")) {
						errors.Add (System.IO.Path.GetFileName (file));
						continue;
					}

					path = System.IO.Path.Combine (PluginManager.UserPluginsDirectory, System.IO.Path.GetFileName (file));
					File.Copy (file, path, true);
				} catch { }
			} 
			
			if (errors.Count > 0)
				new PluginErrorDialog (errors.ToArray ());
			
			SetupService setup = new SetupService (AddinManager.Registry);
			PluginManager.InstallLocalPlugins (setup);
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
			//string[] selected = nview.GetSelectedAddins ();
			//btn_configure.Sensitive = 
			//	selected.Any (id => PluginManager.ConfigurablesForAddin (id).Any ());	
			//btn_about.Sensitive = selected.Length > 0;
			btn_configure.Sensitive = false;
			btn_about.Sensitive = false;

			foreach (string id in nview.GetSelectedAddins ()) {
				if (PluginManager.ConfigurablesForAddin (id).Any ()) {
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

		protected void OnDragDataGet (object sender, DragDataGetArgs e)
		{
			Console.Error.WriteLine (e.SelectionData.ToString ());
		}
		
		protected virtual void OnBtnRefreshClicked (object sender, EventArgs e)
		{
			nview.Refresh ();
			UpdateButtonState ();
		}

		protected void OnBtnUpdateClicked (object sender, EventArgs e)
		{
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
					Services.Environment.OpenUrl (string.Format (PluginWikiPageFormat, name));
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

		protected virtual void OnScrollwDragDataReceived (object o, Gtk.DragDataReceivedArgs args)
		{
		}
	}
}
