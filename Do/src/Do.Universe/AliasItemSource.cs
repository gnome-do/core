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

namespace Do.Universe
{
	
	class AliasItem : ProxyItem	{
		public AliasItem (Item item, string alias) : base (item, alias)
		{
		}
	}

	[Serializable]
	class AliasRecord {
		
		public readonly string UniqueId, Alias;
		
		public AliasRecord (string uniqueId, string alias)
		{
			UniqueId = uniqueId;
			Alias = alias;
		}
	}
	
	public class AliasItemSource : ItemSource {
		
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
		
		public static Item Alias (Item item, string alias)
		{
			AliasItem aliasItem;
			
			if (!ItemHasAlias (item, alias)) {
				aliases.Add (new AliasRecord (item.UniqueId, alias));
			}
			
			aliasItem = new AliasItem (item, alias);
			Do.UniverseManager.AddItems (new Item [] { aliasItem });

			Serialize ();
			return aliasItem;
		}
		
		public static void Unalias (Item item)
		{
			int i = IndexOfAlias (item);
			if (i != -1)
				aliases.RemoveAt (i);
			
			Serialize ();
		}
		
		public static bool ItemHasAlias (Item item)
		{
			return IndexOfAlias (item) != -1;
		}
		
		public static bool ItemHasAlias (Item item, string alias)
		{
			int i = IndexOfAlias (item);
			return i != -1 && aliases [i].Alias == alias;
		}
		
		static int IndexOfAlias (Item item)
		{
			int i = 0;
			foreach (AliasRecord alias in aliases) {
				if (alias.UniqueId == item.UniqueId)
					return i;
				i++;
			}
			return -1;
		}
		
		protected override string Name {
			get {
				return Catalog.GetString ("Alias items");
			}
		}

		protected override string Description {
			get {
				return Catalog.GetString ("Aliased items from Do's universe.");
			}
		}

		protected override string Icon {
			get {
				return "emblem-symbolic-link";
			}
		}

		public override IEnumerable<Type> SupportedItemTypes {
			get { yield return typeof (AliasItem); }
		}

		protected override IEnumerable<Item> Items {
			get {
				List<Item> items;
				
				items = new List<Item> ();
				foreach (AliasRecord aliasRecord in aliases) {
					Element item;
					
					Do.UniverseManager.TryGetElementForUniqueId (aliasRecord.UniqueId, out item);
					if (null != item && item is Item) {
						items.Add (new AliasItem (item as Item, aliasRecord.Alias));
					}
				}
				return items;
			}
		}
		
	}
}
