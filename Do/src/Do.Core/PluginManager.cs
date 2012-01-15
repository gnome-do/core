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
using System.Xml;
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
	public class PluginManager
	{
		const string DefaultPluginIcon = "folder_tar";
		
		static IEnumerable<string> ExtensionPaths = new [] { "/Do/ItemSource", "/Do/Action", "/Do/DynamicItemSource" };

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
			// we need to save these before initializing mono.addins or else ones that have been update in the
			// plugins directory will be lost.
			IEnumerable<string> savedPlugins = PluginsEnabledBeforeLoad ();
			
			// Initialize Mono.Addins.
			AddinManager.Initialize (Paths.UserPluginsDirectory);	
			
			// reload any enabled plugins that got disabled on init
			if (CorePreferences.PeekDebug)
				AddinManager.Registry.Rebuild (null);
			else
				AddinManager.Registry.Update (null);
			EnableDisabledPlugins (savedPlugins);
			
			// Initialize services before addins that may use them are loaded.
			Services.Initialize ();
			InterfaceManager.Initialize ();
			
			// Now allow loading of non-services.
			foreach (string path in ExtensionPaths)
				AddinManager.AddExtensionNodeHandler (path, OnPluginChanged);
		}
		
		/// <summary>
		/// Refresh the addin registry in case any new plugins have shown up
		/// and also make upgrades.
		/// </summary>
		public static void RefreshPlugins ()
		{
			IEnumerable<string> savedPlugins = PluginsEnabledBeforeLoad ();
			RefreshPlugins (savedPlugins);
		}
		
		/// <summary>
		/// This is a workaround for a Mono.Addins bug where updated addins will get
		/// disabled on update. We save the currently enabled addins, update, then
		/// reenable them with the Id of the new version. It's a bit hackish but lluis
		/// said it's a reasonable approach until that bug is fixed 
	 	/// https://bugzilla.novell.com/show_bug.cgi?id=490302
		/// </summary>
		static void RefreshPlugins (IEnumerable<string> savedPlugins)
		{
			AddinManager.Registry.Update (null);
			EnableDisabledPlugins (savedPlugins);
		}
		
		public static void InstallLocalPlugins ()
		{	
			IEnumerable<string> saved, manual;
			
			manual = Directory.GetFiles (Paths.UserAddinInstallationDirectory, "*.dll")
				.Select (s => Path.GetFileName (s))
				.ForEach (dll => Log.Debug ("Installing {0}", dll));
			
			AddinManager.Registry.Rebuild (null);
			saved = AddinManager.Registry.GetAddins ()
				.Where (addin => manual.Contains (Path.GetFileName (addin.AddinFile)))
				.Select (addin => addin.Id);
				
			EnableDisabledPlugins (saved);
			manual.ForEach (dll => File.Delete (dll));
		}
		
		public static void Enable (Addin addin)
		{
			SetAddinEnabled (addin, true);
		}
		
		public static void Enable (string id)
		{
			Enable (AddinManager.Registry.GetAddin (id));
		}
		
		public static void Disable (Addin addin)
		{
			SetAddinEnabled (addin, false);
		}
		
		public static void Disable (string id)
		{
			Disable (AddinManager.Registry.GetAddin (id));
		}
		
		static void SetAddinEnabled (Addin addin, bool enabled)
		{
			if (addin != null)
				addin.Enabled = enabled;
		}
		
		public static IEnumerable<Addin> GetAddins ()
		{
			return AddinManager.Registry.GetAddins ();
		}
		
		public static bool PluginClassifiesAs (AddinRepositoryEntry entry, string className)
		{
			AddinClassifier classifier = Classifiers.FirstOrDefault (c => c.Name == className);
			return classifier == null ? false : classifier.IsMatch (entry);
		}

		public static bool PluginClassifiesAs (Addin addin, string className)
		{
			AddinClassifier classifier = Classifiers.FirstOrDefault (c => c.Name == className);
			return classifier == null ? false : classifier.IsMatch (addin);
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

		/// <summary>
		/// All loaded DynamicItemSources.
		/// </summary>
		public static IEnumerable<DynamicItemSource> DynamicItemSources {
			get { return AddinManager.GetExtensionObjects ("/Do/DynamicItemSource").OfType<DynamicItemSource> (); }
		}

		/// <value>
		/// All loaded Actions.
		/// </value>
		public static IEnumerable<Act> Actions {
			get { return AddinManager.GetExtensionObjects ("/Do/Action").OfType<Act> (); }
		}
		
		/// <summary>
		/// Returns a list of the plugins that were enabled before Mono.Addins was initialised.
		/// this is read from config.xml.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerable"/> of strings containing the versionless plugin id of all
		/// enabled plugins.
		/// </returns>
		static IEnumerable<string> PluginsEnabledBeforeLoad ()
		{
			XmlTextReader reader;
			List<string> plugins;
			
			if (!Directory.Exists (Paths.UserAddinInstallationDirectory))
				return Enumerable.Empty<string> ();
				
			plugins = new List<string> ();
		
			try {
				// set up the reader by loading file, telling it that whitespace doesn't matter, and the DTD is irrelevant
				using (reader = new XmlTextReader (Paths.UserPluginsDirectory.Combine ("addin-db-001", "config.xml"))) {
					reader.XmlResolver = null;
					reader.WhitespaceHandling = WhitespaceHandling.None;
					reader.MoveToContent ();
					
					if (string.IsNullOrEmpty (reader.Name))
						return Enumerable.Empty<string> ();
					
					while (reader.Read ()) {
						string id;
						if (reader.NodeType != XmlNodeType.Element || !reader.HasAttributes)
							continue;
						
						reader.MoveToAttribute ("id");
						id = AddinIdWithoutVersion (reader.Value);
						
						if (string.IsNullOrEmpty (id))
							continue;
							
						reader.MoveToAttribute ("enabled");
						
						if (Boolean.Parse (reader.Value))
							plugins.Add (id);
					}
				}
			} catch (FileNotFoundException e) {
				Log.Debug ("Could not find locate Mono.Addins config.xml: {0}", e.Message);
			} catch (XmlException e) {
				Log.Error ("Error while parsing Mono.Addins config.xml: {0}", e.Message);
				Log.Debug (e.StackTrace);
			}
			
			return plugins;	
		}
		
		static void EnableDisabledPlugins (IEnumerable<string> savedPlugins)
		{
			AddinManager.Registry.GetAddins ()
				.Where (addin => !addin.Enabled && savedPlugins.Any (name => addin.Id.StartsWith (name)))
				.ForEach (addin => Enable (addin));
		}

		static void OnPluginChanged (object sender, ExtensionNodeEventArgs args)
		{
			TypeExtensionNode node = args.ExtensionNode as TypeExtensionNode;

			switch (args.Change) {
				case ExtensionChange.Add:
					try {
						object plugin = node.GetInstance ();
						Log<PluginManager>.Debug ("Loaded \"{0}\" from plugin.", plugin.GetType ().Name);
					} catch (Exception e) {
						Log<PluginManager>.Error ("Encountered error loading plugin: {0} \"{1}\"",
							e.GetType ().Name, e.Message);
						Log<PluginManager>.Debug (e.StackTrace);
					}
					break;
				case ExtensionChange.Remove:
					try {
						object plugin = node.GetInstance ();
						Log<PluginManager>.Debug ("Unloaded \"{0}\".", plugin.GetType ().Name);
					} catch (Exception e) {
						Log<PluginManager>.Error ("Encountered error unloading plugin: {0} \"{1}\"",
							e.GetType ().Name, e.Message);
						Log<PluginManager>.Debug (e.StackTrace);
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
		static IEnumerable<T> ObjectsForAddin<T> (string id) where T : class
		{
			// TODO try using AddinManager.GetExtensionPoints (Type)
			foreach (string path in ExtensionPaths) {
				foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (path)) {
					object instance;

					try {
						instance = node.GetInstance ();
					} catch (Exception e) {
						Log<PluginManager>.Error ("ObjectsForAddin encountered an error: {0} \"{1}\"",
								e.GetType ().Name, e.Message);
						Log<PluginManager>.Debug (e.StackTrace);
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
		
		static string AddinIdWithoutVersion (string id)
		{
			return id.Substring (0, id.IndexOf (','));
		}
	}
}
