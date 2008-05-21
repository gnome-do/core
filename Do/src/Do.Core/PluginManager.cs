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
using Mono.Addins.Setup;

using Do;
using Do.Universe;

namespace Do.Core {
	
	public class PluginManager {
		
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
		
		/// <summary>
        /// Installs plugins that are located in the <see
        /// cref="Paths.PluginsInstall"/> directory.  This will build addins
        /// (mpack files) and install them.
		/// </summary>
		/// <param name="setup">
		/// A <see cref="SetupService"/>
		/// </param>
		public void InstallLocalPlugins (SetupService setup)
		{
			// Load local items into repo
			if (!Directory.Exists (Paths.PluginInstall)) return;
			
            // Create mpack (addin packages) out of dlls. Delete each dll
            // when finished creating package.
			foreach (string file in 
                Directory.GetFiles (Paths.PluginInstall, "*.dll")) {
				string path = Path.Combine (Paths.PluginInstall, file);
				setup.BuildPackage (new ConsoleProgressStatus (false),
                    Paths.PluginInstall, new string [] { path });
				File.Delete (path);
			}

            // Install each mpack file, deleting each file when finished
            // installing it.
			foreach (string file in 
                Directory.GetFiles (Paths.PluginInstall, "*.mpack")) {
				string path = Path.Combine (Paths.PluginInstall, file);
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
		
	}
}
