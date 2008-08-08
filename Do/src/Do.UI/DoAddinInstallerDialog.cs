/* DoAddinInstallerDialog.cs
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
using System.Collections;
using Mono.Addins.Setup;
using Mono.Addins.Description;
using Mono.Unix;
using Do.Core;
using Mono.Addins.Gui;
using Mono.Addins;
using Mono;

namespace Do.UI
{
	public partial class DoAddinInstallerDialog : Gtk.Dialog, IProgressStatus
	{
		PackageCollection entries = new PackageCollection ();
		PluginUpdateNodeView plugins_view;
		
		string[] addin_ids;
		string err_message;
		bool addins_not_found;
		SetupService setup;
		
		public DoAddinInstallerDialog (AddinRegistry reg, string message, string[] addinIds)
		{
			addin_ids = addinIds;
			
			Build();
			
			plugins_view = new PluginUpdateNodeView (reg, addin_ids);
			plugin_scroll.Add (plugins_view);
			plugin_scroll.ShowAll ();
			
			setup = new SetupService (reg);			

			if (!CheckAddins (true))
				UpdateRepos ();
		}
		
		bool CheckAddins (bool updating)
		{
			entries.Clear ();
			bool addinsNotFound = false;
			foreach (string id in addin_ids) {
				string name = Addin.GetIdName (id);
				string version = Addin.GetIdVersion (id);
				AddinRepositoryEntry[] ares = setup.Repositories.GetAvailableAddin (name, version);
				if (ares.Length == 0) {
					addinsNotFound = true;
				} else {
					entries.Add (Package.FromRepository (ares[0]));
				}
			}
			
			DependencyCollection unresolved;
			PackageCollection toUninstall;
			
			if (!setup.ResolveDependencies (this, entries, out toUninstall, out unresolved)) {
				/* Change this to some way of notifying that there are missing deps
				foreach (Dependency dep in unresolved) {
					txt += "<span foreground='red'><b>" + dep.Name + "</b> (not found)</span>\n";
				}
				*/
				addinsNotFound = true;
			}
			return !addinsNotFound;
		}
		
		void UpdateRepos ()
		{
			progress_bar.Show ();
			setup.Repositories.UpdateAllRepositories (this);
			progress_bar.Hide ();
			addins_not_found = CheckAddins (false);
			if (err_message != null) {
				err_message = null;
			}
		}
		
		public int LogLevel {
			get {
				return 1;
			}
		}

		public bool IsCanceled {
			get {
				return false;
			}
		}

		public bool AddinsNotFound {
			get {
				return addins_not_found;
			}
		}

		public string ErrMessage {
			get {
				return err_message;
			}
		}

		public void SetMessage (string msg)
		{
			progress_bar.Text = msg;
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
			       
		public void SetProgress (double progress)
		{
			progress_bar.Fraction = progress;
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}

		public void ReportError (string message, System.Exception exception)
		{
			err_message = message;
		}
		
		public void Log (string msg)
		{
		}

		public void ReportWarning (string message)
		{
		}

		public void Cancel ()
		{
			Respond (Gtk.ResponseType.Cancel);
		}

		protected virtual void OnButtonOKClick (object sender, System.EventArgs e)
		{
			if (addins_not_found) {
				err_message = Catalog.GetString ("Some of the required add-ins were not found");
				Respond (Gtk.ResponseType.Ok);
			}
			else {
				progress_bar.Visible = true;
				err_message = null;
				progress_bar.Show ();
				progress_bar.Fraction = 0;
				progress_bar.Text = "";
				bool res = setup.Install (this, entries);
				if (!res) {
					button_cancel.Sensitive = button_ok.Sensitive = false;
					if (err_message == null)
						Catalog.GetString ("Installation failed");
				}
			}
			Respond (Gtk.ResponseType.Ok);
		}
	}
}
