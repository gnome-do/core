/* PluginManager.cs
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
using System.IO;
using System.Collections.Generic;

using Mono.Addins;
using Mono.Addins.Gui;
using Mono.Addins.Setup;

using Do;
using Do.Addins;
using Do.Universe;

namespace Do.Core {
	
	public class PluginManager {
		
		const string DefaultPluginIcon = "folder_tar";
		const string HttpRepo = "http://do.davebsd.com/repository/dev";

		List<DoItemSource> sources;
		List<DoAction> actions;

		public PluginManager()
		{
			sources = new List<DoItemSource> ();
			actions = new List<DoAction> ();
		}

		public ICollection<DoItemSource> ItemSources {
			get { return sources; }
		}

		public ICollection<DoAction> Actions {
			get { return actions; }
		}

		internal void Initialize ()
		{
			// Initialize the registry
			AddinManager.Initialize (Paths.UserPlugins);

			AddinManager.AddExtensionNodeHandler ("/Do/ItemSource",
				OnItemSourceChange);
			AddinManager.AddExtensionNodeHandler ("/Do/Action",
				OnActionChange);

			// Check that HttpRepo is registered in the HttpRepository
			SetupService setup = new SetupService (AddinManager.Registry);
			if (!setup.Repositories.ContainsRepository (HttpRepo)) {
				setup.Repositories.RegisterRepository (null, HttpRepo, true);
			}
		
			InstallLocalPlugins (setup);
		}
		
		internal bool InstallAvailableUpdates (bool graphical)
		{
			SetupService setup;
            List<string> updates;
            IAddinInstaller installer;

            updates = new List<string> ();
            setup = new SetupService (AddinManager.Registry);
            installer = graphical ? new AddinInstaller () as IAddinInstaller
                : new ConsoleAddinInstaller () as IAddinInstaller ;

            setup.Repositories.UpdateAllRepositories (new ConsoleProgressStatus (true));
            foreach (AddinRepositoryEntry rep in setup.Repositories.GetAvailableAddins ()) {
                Addin installed;

                installed = AddinManager.Registry.GetAddin (Addin.GetIdName (rep.Addin.Id));
                if (null == installed) continue;
                if (Addin.CompareVersions (installed.Version, rep.Addin.Version) > 0) {
                    updates.Add (rep.Addin.Id);
				}
            }
            if (updates.Count > 0) {
                installer.InstallAddins (AddinManager.Registry, "", updates.ToArray ());
			}
			return updates.Count > 0;
		}
		
		/// <summary>
        /// Installs plugins that are located in the <see
        /// cref="Paths.UserPlugins"/> directory.  This will build addins
        /// (mpack files) and install them.
		/// </summary>
		/// <param name="setup">
		/// A <see cref="SetupService"/>
		/// </param>
        public void InstallLocalPlugins (SetupService setup)
        {
            // Load local items into repo
            if (!Directory.Exists (Paths.UserPlugins)) return;

            // Create mpack (addin packages) out of dlls. Delete each dll
            // when finished creating package.
            foreach (string file in 
                    Directory.GetFiles (Paths.UserPlugins, "*.dll")) {
                string path = Path.Combine (Paths.UserPlugins, file);
                setup.BuildPackage (new ConsoleProgressStatus (false),
                        Paths.UserPlugins, new string [] { path });
                File.Delete (path);
            }

            // Install each mpack file, deleting each file when finished
            // installing it.
            foreach (string file in 
                    Directory.GetFiles (Paths.UserPlugins, "*.mpack")) {
                string path = Path.Combine (Paths.UserPlugins, file);
                //Log.Info ("Installing local plugin {0}...", path);
                setup.Install (new ConsoleProgressStatus (false),
                        new string [] { path });
                File.Delete (path);
            }
        }

		/// <summary>
		/// Called when a node is added or removed
		/// </summary>
		/// <param name="s">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="ExtensionNodeEventArgs"/>
		/// </param>
		public void OnItemSourceChange (object s, ExtensionNodeEventArgs args)
		{
			DoItemSource source;
			TypeExtensionNode node;

			node = args.ExtensionNode as TypeExtensionNode;
			if (args.Change == ExtensionChange.Add) {
				try {
					source = new DoItemSource (node.CreateInstance () as IItemSource);
					sources.Add (source);
					Log.Info ("Successfully loaded \"{0}\" item source.",
						source.Name);
				} catch (Exception e) {
					Log.Error ("Failed to load item source: {0}", e.Message);
				}
			} else {
				try {
					source = new DoItemSource (node.GetInstance () as IItemSource);
					sources.Remove (source);
					Log.Info ("Successfully unloaded \"{0}\" item source.",
						source.Name);
				} catch (Exception e) {
					Log.Error ("Failed to unload item source: {0}", e.Message);
				}
			}
		}
		
		public void OnActionChange (object s, ExtensionNodeEventArgs args)
		{
			DoAction action;
			TypeExtensionNode node;

			node = args.ExtensionNode as TypeExtensionNode;
			if (args.Change == ExtensionChange.Add) {
				try {
					action = new DoAction (node.CreateInstance () as IAction);
					actions.Add (action);
					Log.Info ("Successfully loaded \"{0}\" action.", action.Name);
				} catch (Exception e) {
					Log.Error ("Action failed to load: {0}.", e.Message);
				}
			} else {
				try {
					action = new DoAction (node.GetInstance () as IAction);
					actions.Remove (action);
					Log.Info ("Successfully unloaded \"{0}\" action.", action.Name);
				} catch (Exception e) {
					Log.Error ("Action failed to unload: {0}", e.Message);
				}
			}	
		}
		
		/// <summary>
		/// Given an Addin ID, returns an icon that may represent that addin.
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> containing an addin ID.
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> containing an icon name. Can be loaded
		/// via IconProvider.
		/// </returns>
		public string IconForAddin (string id)
		{
			Addin a;
			
			a = AddinManager.Registry.GetAddin (id);
			if (null == a) return DefaultPluginIcon;
			
			// First look for an icon among ItemSources:
			foreach (TypeExtensionNode n in AddinManager.GetExtensionNodes ("/Do/ItemSource")) {
				if (Addin.GetIdName (id) == Addin.GetIdName (n.Addin.Id)) {
					try {
						return (n.GetInstance () as IItemSource).Icon;
					} catch { }
				}
			}
			// If no icon found among ItemSources, look for an icon among Actions:
			foreach (TypeExtensionNode n in AddinManager.GetExtensionNodes ("/Do/Action")) {
				if (Addin.GetIdName (id) == Addin.GetIdName (n.Addin.Id)) {
					try {
						return (n.GetInstance () as IAction).Icon;
					} catch { }
				}
			}
			return DefaultPluginIcon;
		}
		
		public DoObject[] ConfigurablesForAddin (string id)
		{
			Addin a;
			List<DoObject> configs;

			configs = new List<DoObject> ();
			a = AddinManager.Registry.GetAddin (id);
			if (null == a) return new DoObject [0];

			foreach (TypeExtensionNode n in AddinManager.GetExtensionNodes ("/Do/ItemSource")) {
				if (Addin.GetIdName (id) == Addin.GetIdName (n.Addin.Id)) {
					try {
						if (n.GetInstance () is IConfigurable) {
							IItemSource inner = n.GetInstance () as IItemSource;
							DoItemSource source = new DoItemSource (inner);
							configs.Add (source);
						}
					} catch { }
				}
			}
			foreach (TypeExtensionNode n in AddinManager.GetExtensionNodes ("/Do/Action")) {
				if (Addin.GetIdName (id) == Addin.GetIdName (n.Addin.Id)) {
					try {
						if (n.GetInstance () is IConfigurable) {
							IAction inner = n.GetInstance () as IAction;
							DoAction action = new DoAction (inner);
							configs.Add (action);
						}
					} catch { }
				}
			}
			return configs.ToArray ();
		}
		
	}
}
