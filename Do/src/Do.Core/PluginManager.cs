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
using Do.UI;

namespace Do.Core {

    /// <summary>
    /// PluginManager serves as Do's primary interface to Mono.Addins.
    /// </summary>
    public static class PluginManager {

		public static string AllPluginsRepository {
			get {
				return "All Available Plugins";
			}
		}
		
        private const string DefaultPluginIcon = "folder_tar";

        private static string[] ExtensionPaths {
            get {
                return new string[] {
                    "/Do/ItemSource",
                    "/Do/Action",
					"/Do/RenderProvider",
                };
            }
        }
        
        private static Dictionary<string, List<string>> repository_urls;
        public static IDictionary<string, List<string>> RepositoryUrls {
            get {
            	if (null == repository_urls) {
            		repository_urls = new Dictionary<string, List<string>> ();      
            		
            		repository_urls ["Official Plugins"] = new List<string> ();
            		repository_urls ["Official Plugins"].Add 
            			("http://do.davebsd.com/repo/" + Version +"/official");
            				
            		repository_urls ["Community Plugins"] = new List<string> ();
            		repository_urls ["Community Plugins"].Add
            			("http://do.davebsd.com/repo/" + Version +"/community");
            		
            		repository_urls ["Local Plugins"] = new List<string> ();
            		foreach (string repo in Paths.SystemPlugins) {
            			if (Directory.Exists (repo)) {
							repository_urls ["Local Plugins"].Add ("file://" + repo);
						}
					}
            	}
            	
            	return repository_urls;;
            }
        }

		private static string Version {
			get {
				System.Reflection.AssemblyName name;
				
				name = typeof (PluginManager).Assembly.GetName ();
				return String.Format ("{0}.{1}.{2}",
					name.Version.Major, name.Version.Minor, name.Version.Build);
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
			AddinManager.AddExtensionNodeHandler ("/Do/RenderProvider", OnIRenderThemeChange);

            // Register repositories.
            SetupService setup = new SetupService (AddinManager.Registry);
			foreach (List<string> urls in RepositoryUrls.Values) {
				foreach (string url in urls) {
					if (!setup.Repositories.ContainsRepository (url)) {
						setup.Repositories.RegisterRepository (null, url, false);
					}
				}
			}
            InstallLocalPlugins (setup);
        }
        
        public static bool AddinIsFromRepository (Addin a, string name)
		{
			List<string> urls;
			if (name == AllPluginsRepository) return true;
			RepositoryUrls.TryGetValue (name, out urls);
			foreach (string url in urls) {
				if (a.Description.Url.StartsWith (url))
					return true;
			}
			return false;
		}
		
		public static bool AddinIsFromRepository (AddinRepositoryEntry e,
												  string name)
		{
			List<string> urls;
			if (name == AllPluginsRepository) return true;
			RepositoryUrls.TryGetValue (name, out urls);
			foreach (string url in urls) {
				if (e.RepositoryUrl.StartsWith (url))
					return true;
			}
			return false;
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
				try {
					if (null != obj.Icon)
						return obj.Icon;
				} catch { }
            }
            // If no icon found among ItemSources, look for an icon among
            // Actions:		
            foreach (IAction obj in ObjectsForAddin<IAction> (id)) {
				try {
					if (null != obj.Icon)
						return obj.Icon;
				} catch { }
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
        internal static ICollection<DoAction> GetActions () 
		{
            List<DoAction> actions;

            actions = new List<DoAction> ();
            foreach (IAction action in
                AddinManager.GetExtensionObjects ("/Do/Action")) {
                actions.Add (new DoAction (action));
            }
            return actions;
        }
		
		/// <summary>
		/// Finds all UI themes
		/// </summary>
		/// <returns>
		/// A <see cref="ICollection`1"/> of IRenderTheme instances from plugins
		/// </returns>
		static List<IRenderTheme> themes;
		internal static ICollection<IRenderTheme> GetThemes () 
		{
			if (themes == null) {
				themes = new List<IRenderTheme> ();
				foreach (IRenderTheme theme in
				         AddinManager.GetExtensionObjects ("/Do/RenderProvider")) {
					themes.Add (theme);
				}
			}
			return themes;
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
            installer = graphical ? new DoAddinInstaller () as IAddinInstaller
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
                    AddinManager.Registry, string.Empty, updates.ToArray ());
            }
            return updates.Count > 0;
        }
        
        /// <summary>
        /// Checks if there are any updates available for download/installatition
        /// </summary>
        /// <returns>
        /// A <see cref="System.Boolean"/> representing whether or not there
        /// are any updates available for install
        /// </returns>
        public static bool UpdatesAvailable ()
        {
        	SetupService setup;
            setup = new SetupService (AddinManager.Registry);
            	setup.Repositories.UpdateAllRepositories (
                	new ConsoleProgressStatus (true));
            foreach (AddinRepositoryEntry rep in
                setup.Repositories.GetAvailableAddins ()) {
                Addin installed;

                installed = AddinManager.Registry.GetAddin (Addin.GetIdName (
                    rep.Addin.Id));
                if (null == installed) continue;
                if (Addin.CompareVersions (installed.Version, rep.Addin.Version) > 0) {
                    return true;
                }
            }
            return false;
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
            // Create mpack (addin packages) out of dlls.
            foreach (string file in 
                Directory.GetFiles (Paths.UserPlugins, "*.dll")) {
                string path;

                path = Path.Combine (Paths.UserPlugins, file);
                setup.BuildPackage (new ConsoleProgressStatus (false),
                    Paths.UserPlugins, new string [] { path });
            }
			
			//Delete dlls.  If we do it earlier why might delete dll's brought in as
			//dependancies.  Doing it now has the same effect without breakage.
			foreach (string file in 
                Directory.GetFiles (Paths.UserPlugins, "*.dll")) {
                string path;

                path = Path.Combine (Paths.UserPlugins, file);
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
					// plugin is to be used only for inspection here.
					IObject plugin = node.GetInstance () as IObject;
					// Wrap in a DoObject for safety.
                    IObject o = new DoObject (plugin);
					if (plugin is Pluggable)
						(plugin as Pluggable).NotifyLoad ();					
                    Log.Info ("Loaded \"{0}\".", o.Name);
                } catch (Exception e) {
                    Log.Error ("Encountered error loading \"{0}\": {0}",
                        e.Message);
					Log.Debug (e.StackTrace);
                }
            } else {
                try {
					IObject plugin = node.GetInstance() as IObject;
                    IObject o = new DoObject (plugin);
					if (plugin is Pluggable)
						(plugin as Pluggable).NotifyUnload ();
                    Log.Info ("Unloaded \"{0}\".", o.Name);
                } catch (Exception e) {
                    Log.Error ("Encountered error unloading plugin: {0}",
                        e.Message);
					Log.Debug (e.StackTrace);
                }
            }	
        }
		
		internal static void OnIRenderThemeChange (object s, ExtensionNodeEventArgs args)
		{
			TypeExtensionNode node;
			themes = null; //reset our cached list of themes;
			
			node = args.ExtensionNode as TypeExtensionNode;
			if (args.Change == ExtensionChange.Add) {
				try {
					IRenderTheme plugin = node.GetInstance () as IRenderTheme;
					Log.Info ("Loaded UI Plugin \"{0}\" Successfully", plugin.Name);
				} catch (Exception e) {
					Log.Error ("Encounted error loading \"{0}\": {0}",
					           e.Message);
					Log.Debug (e.StackTrace);
				}
			} else {
				
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