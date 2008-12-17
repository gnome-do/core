// PluginManager.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this
// source distribution.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Mono.Addins;
using Mono.Addins.Gui;
using Mono.Addins.Setup;

using Do;
using Do.UI;
using Do.Addins;
using Do.Platform;
using Do.Universe;
using Do.Interface;

namespace Do.Core {

	/// <summary>
	/// PluginManager serves as Do's primary interface to Mono.Addins.
	/// </summary>
	public static class PluginManager {

		public  const string AllPluginsRepository = "All Available Plugins";
		private const string DefaultPluginIcon = "folder_tar";

		static ICollection<DoAction> actions;
		static ICollection<DoItemSource> item_sources;

		static PluginManager ()
		{
			actions = new List<DoAction> ();
			item_sources = new List<DoItemSource> ();
		}

		private static IEnumerable<string> ExtensionPaths {
			get {
				return new [] {
					"/Do/ItemSource",
					"/Do/Action",
					"/Do/InterfaceWindow",
				};
			}
		}

		private static IDictionary<string, IEnumerable<string>> repository_urls;
		public static IDictionary<string, IEnumerable<string>> RepositoryUrls {
			get {
				if (repository_urls != null) return repository_urls;

				repository_urls = new Dictionary<string, IEnumerable<string>> ();      
				//repository_urls ["Official Plugins"] = new[] { OfficialRepo };
				//repository_urls ["Community Plugins"] = new[] { CommunityRepo };
				repository_urls ["Local Plugins"] = Paths.SystemPluginDirectories
					.Select (repo => "file://" + repo)
					.ToArray ();

				return repository_urls;
			}
		}

		/// <summary>
		/// Performs plugin system initialization. Should be called before this
		/// class or any Mono.Addins class is used. The ordering is very delicate.
		/// </summary>
		public static void Initialize ()
		{
			// Initialize Mono.Addins.
			AddinManager.Initialize (Paths.UserPluginsDirectory);

			// Register repositories.
			SetupService setup = new SetupService (AddinManager.Registry);
			foreach (IEnumerable<string> urls in RepositoryUrls.Values) {
				foreach (string url in urls) {
					if (!setup.Repositories.ContainsRepository (url)) {
						setup.Repositories.RegisterRepository (null, url, false);
					}
				}
			}

			// Initialize services before addins that may use them are loaded.
			Services.Initialize ();
			
			// Now allow loading of non-services.
			AddinManager.AddExtensionNodeHandler ("/Do/ItemSource", OnItemSourceChange);
			AddinManager.AddExtensionNodeHandler ("/Do/Action",  OnActionChange);

			InstallLocalPlugins (setup);
		}

		public static bool AddinIsFromRepository (Addin a, string name)
		{
			return name == AllPluginsRepository ||
				RepositoryUrls [name].Any (url => a.Description.Url.StartsWith (url));
		}

		public static bool AddinIsFromRepository (AddinRepositoryEntry e, string name)
		{
			return name == AllPluginsRepository ||
				RepositoryUrls [name].Any (url => e.RepositoryUrl.StartsWith (url));
		}

		/// <summary>
		/// Given an addin ID, returns an icon that may represent that addin.
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> containing an addin ID.
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> containing an icon name. Can be loaded
		/// via <see cref="Icons"/>.
		/// </returns>
		public static string IconForAddin (string id)
		{   
			string icon;

			// First look for an icon among ItemSources:
			icon = ObjectsForAddin<IItemSource> (id)
				.Select (source => DoObject.Wrap (source).Icon)
				.FirstOrDefault ();
			if (icon != null) return icon;

			// If no icon found among ItemSources, look for an icon among
			// Actions:		
			icon = ObjectsForAddin<IAction> (id)
				.Select (source => DoObject.Wrap (source).Icon)
				.FirstOrDefault ();
			if (icon != null) return icon;

			return DefaultPluginIcon;
		}

		/// <value>
		/// All loaded IItemSources.
		/// </value>
		internal static IEnumerable<DoItemSource> ItemSources {
			get { return item_sources; }
		}

		/// <value>
		/// All loaded IActions.
		/// </value>
		internal static IEnumerable<DoAction> Actions {
			get { return actions; }
		}

		/// <summary>
		/// Finds all UI themes
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerable`1"/> of IRenderTheme instances from plugins
		/// </returns>
		internal static IEnumerable<IDoWindow> GetThemes () 
		{
			return InterfaceManager.Interfaces;
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
			IProgressStatus status = new ConsoleProgressStatus (false);
			// GetFilePaths is like Directory.GetFiles but returned files have directory prefixed.
			Func<string, string, IEnumerable<string>> GetFilePaths = (dir, pattern) =>
				Directory.GetFiles (dir, pattern).Select (f => Path.Combine (dir, f));
			
			// Create mpack (addin packages) out of dlls.
			GetFilePaths (Paths.UserPluginsDirectory, "*.dll")
				.ForEach (path => setup.BuildPackage (status, Paths.UserPluginsDirectory, new[] { path }))
				// We delete the dlls after creating mpacks so we don't delete any dlls prematurely.
				.ForEach (File.Delete);

			// Install each mpack file, deleting each file when finished installing it.
			foreach (string path in GetFilePaths (Paths.UserPluginsDirectory, "*.mpack")) {
				setup.Install (status, new[] { path });
				File.Delete (path);
			}
		}
		
		static void OnActionChange (object s, ExtensionNodeEventArgs args)
		{
			ActionChange (args.ExtensionNode as TypeExtensionNode, args.Change);
		}
		
		static void ActionChange (TypeExtensionNode node, ExtensionChange change)
		{
			switch (change) {
			case ExtensionChange.Add:
				try {
					DoAction action = DoAction.Wrap (node.GetInstance () as IAction) as DoAction;
					if (actions.Contains (action)) {
						Log.Warn ("\"{0}\" action was loaded twice. Ignoring.", action.Name);
					} else {
						actions.Add (action);
						Log.Info ("Loaded \"{0}\" action.", action.Name);
					}
				} catch (Exception e) {
					Log.Error ("Encountered error loading action: {0} \"{1}\"",
							e.GetType ().Name, e.Message);
					Log.Debug (e.StackTrace);
				}
				break;
			case ExtensionChange.Remove:
				try {
					DoAction action = DoAction.Wrap (node.GetInstance () as IAction) as DoAction;
					Log.Info ("Unloaded \"{0}\" action.", action.Name);
					actions.Remove (action);
				} catch (Exception e) {
					Log.Error ("Encountered error unloading action: {0} \"{1}\"",
							e.GetType ().Name, e.Message);
					Log.Debug (e.StackTrace);
				}
				break;
			}	
		}
		
		static void OnItemSourceChange (object s, ExtensionNodeEventArgs args)
		{
			ItemSourceChange (args.ExtensionNode as TypeExtensionNode, args.Change);
		}
		
		static void ItemSourceChange (TypeExtensionNode node, ExtensionChange change)
		{
			switch (change) {
			case ExtensionChange.Add:
				try {
					DoItemSource source = DoItemSource.Wrap (node.GetInstance () as IItemSource) as DoItemSource;
					if (item_sources.Contains (source)) {
						Log.Warn ("\"{0}\" item source was loaded twice. Ignoring.", source.Name);
					} else {
						item_sources.Add (source);
						Log.Info ("Loaded \"{0}\" item source.", source.Name);
					}
				} catch (Exception e) {
					Log.Error ("Encountered error loading item source: {0} \"{1}\"",
							e.GetType ().Name, e.Message);
					Log.Debug (e.StackTrace);
				}
				break;
			case ExtensionChange.Remove:
				try {
					DoItemSource source = DoItemSource.Wrap (node.GetInstance () as IItemSource) as DoItemSource;
					Log.Info ("Unloaded \"{0}\".", source.Name);
					item_sources.Remove (source);
				} catch (Exception e) {
					Log.Error ("Encountered error unloading item source: {0} \"{1}\"",
							e.GetType ().Name, e.Message);
					Log.Debug (e.StackTrace);
				}
				break;
			}	
		}

		/// <summary>
		/// Get all objects conforming to type T provided by a given addin.
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> containing an addin id.
		/// </param>
		/// <returns>
		/// A <see cref="IEnumerable`1"/> of instances of type T.
		/// </returns>
		private static IEnumerable<T> ObjectsForAddin<T> (string id) where T : class
		{
			foreach (string path in ExtensionPaths) {
				foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (path)) {
					object instance;

					try {
						instance = node.GetInstance ();
					} catch (Exception e) {
						Log.Error ("ObjectsForAddin encountered an error: {0} \"{1}\"",
								e.GetType ().Name, e.Message);
						Log.Debug (e.StackTrace);
						continue;
					}

					if (!(instance is T))
						continue; // Does not conform to required type.
					if (Addin.GetIdName (id) != Addin.GetIdName (node.Addin.Id))
						continue; // Instances not from same addin. Version mismatch?
					yield return instance as T;
				}
			}
		}

		/// <summary>
		/// Get all IConfigurable instances loaded from plugins.
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> containing an addin id.
		/// </param>
		/// <returns>
		/// A <see cref="IEnumerable`1"/> of <see cref="IConfigurable"/>
		/// provided by the addin for that id.
		/// </returns>
		internal static IEnumerable<IConfigurable> ConfigurablesForAddin (string id)
		{
			return ObjectsForAddin<IConfigurable> (id)
				.Select (con => new DoObject (con) as IConfigurable);
		}
	}
}
