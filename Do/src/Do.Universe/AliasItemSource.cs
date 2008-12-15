/* AliasItemSource.cs
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Mono.Unix;

using Do;
using Do.Platform;

namespace Do.Universe {	
	
	class AliasItem : DoProxyItem	{
		public AliasItem (string alias, IItem item) :
			base (alias, item)
		{
		}
	}

	[Serializable]
	class AliasRecord {
		
		public readonly string UID, Alias;
		
		public AliasRecord (string uid, string alias)
		{
			UID = uid;
			Alias = alias;
		}
	}
	
	public class AliasItemSource : IItemSource {
		
		static List<AliasRecord> aliases;
		
		static string AliasFile {
			get {
				return Path.Combine (Services.Paths.UserDataDirectory, typeof (AliasItemSource).FullName);
			}
		}
		
		static AliasItemSource ()
		{
			Deserialize ();
		}
		
		static void Deserialize ()
		{
			aliases = null;
			try {
				using (Stream s = File.OpenRead (AliasFile)) {
					BinaryFormatter f = new BinaryFormatter ();
					aliases = f.Deserialize (s) as List<AliasRecord>;
				}
			} catch (FileNotFoundException) {
			} catch (Exception e) {
				Log.Error (e.Message);
				Log.Debug (e.StackTrace);
			} finally {
				if (aliases == null)
					aliases = new List<AliasRecord> ();
			}
		}
		
		static void Serialize ()
		{
			try {
				using (Stream s = File.OpenWrite (AliasFile)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (s, aliases);
				}
			} catch (Exception e) {
				Log.Error (e.Message);
			}
		}
		
		public static IItem Alias (IItem item, string alias)
		{
			AliasItem aliasItem;
			
			if (!ItemHasAlias (item, alias)) {
				string uid = Services.Core.GetUID (item);
				aliases.Add (new AliasRecord (uid, alias));
			}
			
			aliasItem = new AliasItem (alias, item);
			Do.UniverseManager.AddItems (new IItem [] { aliasItem });

			Serialize ();
			return aliasItem;
		}
		
		public static void Unalias (IItem item)
		{
			int i = IndexOfAlias (item);
			if (i != -1)
				aliases.RemoveAt (i);
			
			Serialize ();
		}
		
		public static bool ItemHasAlias (IItem item)
		{
			return IndexOfAlias (item) != -1;
		}
		
		public static bool ItemHasAlias (IItem item, string alias)
		{
			int i = IndexOfAlias (item);
			return i != -1 && aliases [i].Alias == alias;
		}
		
		static int IndexOfAlias (IItem item)
		{
			int i = 0;
			string uid = Services.Core.GetUID (item);
			foreach (AliasRecord alias in aliases) {
				if (alias.UID == uid)
					return i;
				i++;
			}
			return -1;
		}
		
		public string Name {
			get {
				return Catalog.GetString ("Alias items");
			}
		}

		public string Description {
			get {
				return Catalog.GetString ("Aliased items from Do's universe.");
			}
		}

		public string Icon {
			get {
				return "emblem-symbolic-link";
			}
		}

		public IEnumerable<Type> SupportedItemTypes {
			get {
				yield return typeof (AliasItem);
			}
		}

		public IEnumerable<IItem> Items {
			get {
				List<IItem> items;
				
				items = new List<IItem> ();
				foreach (AliasRecord alias in aliases) {
					IObject item;
					
					Do.UniverseManager.TryGetObjectForUID (alias.UID, out item);
					if (null != item && item is IItem) {
						items.Add (new AliasItem (alias.Alias, item as IItem));
					}
				}
				return items;
			}
		}
		
		public IEnumerable<IItem> ChildrenOfItem (IItem item)
		{
			return null;
		}

		public void UpdateItems ()
		{
		}
	}
}
