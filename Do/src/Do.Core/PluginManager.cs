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

using Mono.Unix;
using Mono.Addins;
using Mono.Addins.Gui;
using Mono.Addins.Setup;

using Do.UI;
using Do.Platform;
using Do.Platform.Linux;
using Do.Universe;
using Do.Interface;
using Do.Core.Addins;

namespace Do.Core
{

	/// <summary>
	/// PluginManager serves as Do's primary interface to Mono.Addins.
	/// </summary>
	internal static class PluginManager
	{
		const string DefaultPluginIcon = "folder_tar";
		
		static IEnumerable<string> ExtensionPaths =
			new [] { "/Do/ItemSource", "/Do/Action", };

		public static readonly IEnumerable<AddinClassifier> Classifiers =
			new AddinClassifier [] {
				new OfficialAddinClassifier (),
				new CommunityAddinClassifier (),
				new GreedyAddinClassifier (),
			};

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
			foreach (string path in Paths.SystemPluginDirectories) {
				if (!Directory.Exists (path)) continue;
				string url = "file://" + path;
				if (!setup.Repositories.ContainsRepository (url)) {
					setup.Repositories.RegisterRepository (null, url, false);
				}
			}

			// Initialize services before addins that may use them are loaded.
			Services.Initialize ();
			InterfaceManager.Initialize ();
			
			// Now allow loading of non-services.
			foreach (string path in ExtensionPaths)
				AddinManager.AddExtensionNodeHandler (path, OnPluginChanged);

			InstallLocalPlugins (setup);
		}

		public static bool PluginClassifiesAs (AddinRepositoryEntry entry, string className)
		{
			AddinClassifier classifier = Classifiers.FirstOrDefault (c => c.Name == className);
			return classifier == null ? false : classifier.IsMatch (entry);
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
			icon = ObjectsForAddin<ItemSource> (id)
				.Select (source => source.Safe.Icon)
				.FirstOrDefault ();
			if (icon != null) return icon;

			// If no icon found among ItemSources, look for an icon among
			// Actions:		
			icon = ObjectsForAddin<Act> (id)
				.Select (source => source.Safe.Icon)
				.FirstOrDefault ();
			if (icon != null) return icon;

			return DefaultPluginIcon;
		}

		/// <value>
		/// All loaded ItemSources.
		/// </value>
		public static IEnumerable<ItemSource> ItemSources {
			get { return AddinManager.GetExtensionObjects ("/Do/ItemSource").OfType<ItemSource> (); }
		}

		/// <value>
		/// All loaded Actions.
		/// </value>
		public static IEnumerable<Act> Actions {
			get { return AddinManager.GetExtensionObjects ("/Do/Action").OfType<Act> (); }
		}

		/// <summary>
		/// Finds all UI themes
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerable`1"/> of IRenderTheme instances from plugins
		/// </returns>
		public static IEnumerable<IDoWindow> GetThemes () 
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
		public static void InstallLocalPlugins (SetupService setup)
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
		
		static void OnPluginChanged (object sender, ExtensionNodeEventArgs args)
		{
			TypeExtensionNode node = args.ExtensionNode as TypeExtensionNode;

			switch (args.Change) {
			case ExtensionChange.Add:
				try {
					object plugin = node.GetInstance ();
					Log.Debug ("Loaded \"{0}\" from plugin.", plugin.GetType ().Name);
				} catch (Exception e) {
					Log.Error ("Encountered error loading plugin: {0} \"{1}\"",
							e.GetType ().Name, e.Message);
					Log.Debug (e.StackTrace);
				}
				break;
			case ExtensionChange.Remove:
				try {
					object plugin = node.GetInstance ();
					Log.Debug ("Unloaded \"{0}\".", plugin.GetType ().Name);
				} catch (Exception e) {
					Log.Error ("Encountered error unloading plugin: {0} \"{1}\"",
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
			// TODO try using AddinManager.GetExtensionPoints (Type)
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
		public static IEnumerable<IConfigurable> ConfigurablesForAddin (string id)
		{
			return ObjectsForAddin<IConfigurable> (id);
		}
	}
}
