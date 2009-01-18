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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Gtk;
using Mono.Unix;
using Mono.Addins;
using Mono.Addins.Gui;
using Mono.Addins.Setup;

using Do;
using Do.Core;
using Do.Core.Addins;
using Do.Interface;
using Do.Platform;
using Do.Platform.Linux;

namespace Do.UI
{
	[System.ComponentModel.Category("Do")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ManagePluginsPreferencesWidget : Bin, IConfigurable
	{

		const string PluginWikiPageFormat =
			"http://do.davebsd.com/wiki/index.php?title={0}_Plugin";

		PluginNodeView nview;

		new public string Name {
			get { return Catalog.GetString ("Plugins"); }
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

			foreach (AddinClassifier cfier in PluginManager.Classifiers) {
				show_combo.AppendText (cfier.Name);
			}
			show_combo.Active = 0;

			Services.Application.RunOnMainThread (() =>
				search_entry.GrabFocus ()
			);
		}
		
		protected void OnDragDataReceived (object sender, DragDataReceivedArgs args)
		{
			string data = Encoding.UTF8.GetString (args.SelectionData.Data);
			// Sometimes we get a null at the end, and it crashes us.
			data = data.TrimEnd ('\0');
			
			string [] uriList = Regex.Split (data, "\r\n");
			List<string> errors = new List<string> ();
			foreach (string uri in uriList) {
				string file;
				string path;
				
				try {
					file = uri.Remove (0, 7);
					string fileName = System.IO.Path.GetFileName (file);
					if (!file.EndsWith (".dll")) {
						errors.Add (fileName);
						continue;
					}

					path = Paths.UserPluginsDirectory.Combine (fileName);
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

		private void OnPluginSelected (object sender, PluginSelectionEventArgs e)
		{
			UpdateButtonState ();
		}

		protected void UpdateButtonState ()
		{
			btn_configure.Sensitive = nview.GetSelectedAddins ()
				.SelectMany (id => PluginManager.ConfigurablesForAddin (id))
				.Any ();
			btn_about.Sensitive = nview.GetSelectedAddins ().Any ();
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

		void OnDragDataGet (object sender, DragDataGetArgs e)
		{
			Console.Error.WriteLine (e.SelectionData.ToString ());
		}
		
		void OnBtnRefreshClicked (object sender, EventArgs e)
		{
			nview.Refresh ();
			UpdateButtonState ();
		}

		void OnBtnUpdateClicked (object sender, EventArgs e)
		{
			nview.Refresh ();
		}

		void OnBtnConfigurePluginClicked (object sender, EventArgs e)
		{
			Window win;
			string[] ids;

			ids = nview.GetSelectedAddins ();
			if (ids.Length == 0) return;

			win = new PluginConfigurationWindow (ids [0]);
			win.Modal = true;
			win.ShowAll ();
		}

		void OnBtnAboutClicked (object sender, EventArgs e)
		{
			foreach (string id in nview.GetSelectedAddins ()) {
				try {
					string name = Addin.GetIdName (id).Split ('.')[1];
					Services.Environment.OpenUrl (string.Format (PluginWikiPageFormat, name));
				} catch { }
			}
		}

		void OnShowComboChanged (object sender, EventArgs e)
		{
			nview.ShowCategory = show_combo.ActiveText;
			nview.Filter = search_entry.Text = "";
		}

		void OnSearchEntryChanged (object sender, EventArgs e)
		{
			nview.Filter = search_entry.Text;
		}

		void OnScrollwDragDataReceived (object o, DragDataReceivedArgs e)
		{
		}
	}
}
