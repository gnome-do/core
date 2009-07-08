/* AbstractPackageManagerService.cs
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

using Mono.Addins;

using Do.Platform.ServiceStack;

namespace Do.Platform
{
	
	/// <summary>
	/// The package manager service is a service that runs and listens for events from a package backend. The idea is
	/// that when you install a package that has a corresponding Do plugin, Do will see this and ask you if you want to
	/// enable the corresponding plugin. This architecture allows us to write backends for whatever package manager is in
	/// use be it apt, rpm, packagekit, or whatever.
	/// </summary>
	public abstract class AbstractPackageManagerService : IService, IInitializedService
	{
		public const string PluginAvailableKey = "DontShowPluginAvailableDialog";
		public const bool PluginAvailableDefault = false;
		
		public virtual void Initialize ()
		{
			Preferences = Services.Preferences.Get<AbstractPackageManagerService> ();
		}
		
		IPreferences Preferences { get; set; }
		
		protected bool DontShowPluginAvailableDialog {
			get { return Preferences.Get (PluginAvailableKey, PluginAvailableDefault); }
			set { Preferences.Get (PluginAvailableKey, value); }
		}
		
		/// <summary>
		/// Tries to find a plugin that is appropriate for the packge given.
		/// i.e. if the package is a twitter prism app, it would give the Microblogging plugin
		/// </summary>
		/// <param name="package">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="Addin"/>
		/// </returns>
		protected Addin MaybePluginForPackage (string package)
		{
			package = package.ToLower ();
			
			foreach (Addin addin in Services.PluginManager.GetAddins ()) {
				if (addin.Name.ToLower ().Contains (package) || addin.Description.Description.Contains (package))
					return addin;
			}
			
			return null;
		}
	}
}
