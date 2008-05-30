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

    /// <summary>
    /// PluginManager serves as Do's primary interface to Mono.Addins.
    /// </summary>
    public static class PluginManager {

        private const string DefaultPluginIcon = "folder_tar";
        private const string HttpRepo = "http://do.davebsd.com/repository/dev";

        private static string[] ExtensionPaths {
            get {
                return new string[] {
                    "/Do/ItemSource",
                    "/Do/Action",
                };
            }
        }

        /// <summary>
        /// Performs plugin system initialization. Should be called before this
        /// class or any Mono.Addins class is used.
        /// </summary>
        internal static void Initialize ()
        {
            // Initialize Mono.Addins.
            AddinManager.Initialize (Paths.UserPlugins);
            AddinManager.AddExtensionNodeHandler ("/Do/ItemSource", OnIObjectChange);
            AddinManager.AddExtensionNodeHandler ("/Do/Action",  OnIObjectChange);

            // Check that HttpRepo is registered in the HttpRepository
            SetupService setup = new SetupService (AddinManager.Registry);
            if (!setup.Repositories.ContainsRepository (HttpRepo)) {
                setup.Repositories.RegisterRepository (null, HttpRepo, true);
            }
            InstallLocalPlugins (setup);
        }

        /// <summary>
        /// Given an addin ID, returns an icon that may represent that addin.
        /// </summary>
        /// <param name="id">
        /// A <see cref="System.String"/> containing an addin ID.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> containing an icon name. Can be loaded
        /// via IconProvider.
        /// </returns>
        public static string IconForAddin (string id)
        {
            // First look for an icon among ItemSources:
            foreach (IItemSource obj in ObjectsForAddin<IItemSource> (id)) {
                return obj.Icon;
            }
            // If no icon found among ItemSources, look for an icon among
            // Actions:		
            foreach (IAction obj in ObjectsForAddin<IAction> (id)) {
                return obj.Icon;
            }
            return DefaultPluginIcon;
        }

        /// <summary>
        /// Finds all plugged-in item sources.
        /// </summary>
        /// <returns>
        /// A <see cref="ICollection`1"/> of DoItemSource instances loaded from
        /// plugins.
        /// </returns>
        internal static ICollection<DoItemSource> GetItemSources () {
            List<DoItemSource> sources;

            sources = new List<DoItemSource> ();
            foreach (IItemSource source in
                AddinManager.GetExtensionObjects ("/Do/ItemSource")) {
                sources.Add (new DoItemSource (source));
            }
            return sources;
        }

        /// <summary>
        /// Finds all plugged-in actions.
        /// </summary>
        /// <returns>
        /// A <see cref="ICollection`1"/> of DoAction instances loaded from
        /// plugins.
        /// </returns>
        internal static ICollection<DoAction> GetActions () {
            List<DoAction> actions;

            actions = new List<DoAction> ();
            foreach (IAction action in
                AddinManager.GetExtensionObjects ("/Do/Action")) {
                actions.Add (new DoAction (action));
            }
            return actions;
        }

        /// <summary>
        /// Install all available plugin updates, either
        /// graphically or non-graphically.
        /// </summary>
        /// <param name="graphical">
        /// A <see cref="System.Boolean"/> to determine whether installer should
        /// be graphical (true iff graphical installer should be used).
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/> indicating whether any updates were
        /// performed.
        /// </returns>
        internal static bool InstallAvailableUpdates (bool graphical)
        {
            SetupService setup;
            List<string> updates;
            IAddinInstaller installer;

            updates = new List<string> ();
            setup = new SetupService (AddinManager.Registry);
            installer = graphical ? new AddinInstaller () as IAddinInstaller
                : new ConsoleAddinInstaller () as IAddinInstaller ;

            setup.Repositories.UpdateAllRepositories (
                new ConsoleProgressStatus (true));
            foreach (AddinRepositoryEntry rep in
                setup.Repositories.GetAvailableAddins ()) {
                Addin installed;

                installed = AddinManager.Registry.GetAddin (Addin.GetIdName (
                    rep.Addin.Id));
                if (null == installed) continue;
                if (Addin.CompareVersions (installed.Version, rep.Addin.Version) > 0) {
                    updates.Add (rep.Addin.Id);
                }
            }
            if (updates.Count > 0) {
                installer.InstallAddins (
                    AddinManager.Registry, "", updates.ToArray ());
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
        internal static void InstallLocalPlugins (SetupService setup)
        {
            // Create mpack (addin packages) out of dlls. Delete each dll
            // when finished creating package.
            foreach (string file in 
                Directory.GetFiles (Paths.UserPlugins, "*.dll")) {
                string path;

                path = Path.Combine (Paths.UserPlugins, file);
                setup.BuildPackage (new ConsoleProgressStatus (false),
                    Paths.UserPlugins, new string [] { path });
                File.Delete (path);
            }
            // Install each mpack file, deleting each file when finished
            // installing it.
            foreach (string file in 
                    Directory.GetFiles (Paths.UserPlugins, "*.mpack")) {
                string path;

                path = Path.Combine (Paths.UserPlugins, file);
                setup.Install (new ConsoleProgressStatus (false),
                    new string [] { path });
                File.Delete (path);
            }
        }

        internal static void OnIObjectChange (object s,
                                              ExtensionNodeEventArgs args)
        {
            TypeExtensionNode node;

            node = args.ExtensionNode as TypeExtensionNode;
            if (args.Change.Equals (ExtensionChange.Add)) {
                try {
                    IObject o = new DoObject (node.GetInstance () as IObject);
                    Log.Info ("Loaded \"{0}\".", o.Name);
                } catch (Exception e) {
                    Log.Info ("Encountered error loading \"{0}\": {0}",
                        e.Message);
                }
            } else {
                try {
                    IObject o = new DoObject (node.GetInstance () as IObject);
                    Log.Info ("Unloaded \"{0}\".", o.Name);
                } catch {
                }
            }	
        }

		/// <summary>
		/// Get all objects conforming to type T provided by a given addin.
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> containing an addin id.
		/// </param>
		/// <returns>
		/// A <see cref="ICollection`1"/> of instances of type T.
		/// </returns>
        private static ICollection<T> ObjectsForAddin<T> (string id)
        {
            List<T> obs;

            obs = new List<T> ();
            foreach (string path in ExtensionPaths) {
                foreach (TypeExtensionNode n in
                    AddinManager.GetExtensionNodes (path)) {
                    object instance;
                    bool addinMatch, typeMatch;

                    try {
                        instance = n.GetInstance ();
                    } catch {
                        continue;
                    }
                    addinMatch =
                        Addin.GetIdName (id) == Addin.GetIdName (n.Addin.Id);
                    typeMatch =
                        typeof (T).IsAssignableFrom (instance.GetType ());
                    if (addinMatch && typeMatch) {
                        obs.Add ((T) instance);
                    }
                }
            }
            return obs;
        }

		/// <summary>
		/// Get all IConfigurable instances loaded from plugins.
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> containing an addin id.
		/// </param>
		/// <returns>
		/// A <see cref="ICollection`1"/> of <see cref="IConfigurable"/>
		/// provided by the addin for that id.
		/// </returns>
        internal static ICollection<IConfigurable> ConfigurablesForAddin (string id)
        {
            List<IConfigurable> cons;

            cons = new List<IConfigurable> ();	
            foreach (IConfigurable con in ObjectsForAddin<IConfigurable> (id))
                cons.Add (new DoObject (con));
            return cons;
        }
    }
}
