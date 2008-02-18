/* DoObject.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * inner distribution.
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
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core
{
	public abstract class DoObject : IObject
	{
		const string kDefaultName = "No name";
		const string kDefaultDescription = "No description.";
		const string kDefaultIcon = "empty";
		
		static IRelevanceProvider relevanceProvider;

		int relevance;
		
		static DoObject ()
		{
			relevanceProvider = RelevanceProvider.GetProvider ();
		}

		public static List<Type> GetAllImplementedTypes (IObject o)
		{
			Type baseType;
			List<Type> types;
			
			baseType = o.GetType ();
			types = new List<Type> ();
			// Climb up the inheritance tree adding types.
			while (typeof (IObject).IsAssignableFrom (baseType)) {
				types.Add (baseType);
				baseType = baseType.BaseType;    
			}
			// Add all implemented interfaces
			foreach (Type interface_type in o.GetType ().GetInterfaces ()) {
				if (typeof (IObject).IsAssignableFrom (interface_type)) {
					types.Add (interface_type);
				}
			}
			return types;
		}

		public static bool IObjectTypeCheck (IObject o, Type[] types)
		{
			bool type_ok;

			type_ok = false;
			foreach (Type type in types) {
				if (type.IsAssignableFrom (o.GetType ())) {
					type_ok = true;
					break;
				}
			}
			return type_ok;
		}

		/// <summary>
		/// Returns the inner item if the static type of given
		/// item is an DoItem subtype. Returns the argument otherwise.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem"/> that may or may not be an DoItem subtype.
		/// </param>
		/// <returns>
		/// A <see cref="IItem"/> that is NOT an DoItem subtype (the inner IItem of an DoItem).
		/// </returns>
		public static IItem EnsureIItem (IItem item)
		{
			if (item is DoItem)
				item = (item as DoItem).Inner as IItem;
			return item;
		}

		/// <summary>
		/// Like EnsureItem but for arrays of IItems.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem[]"/> that may contain
		/// DoItem subtypes.
		/// </param>
		/// <returns>
		/// A <see cref="IItem[]"/> of inner IItems.
		/// </returns>
		public static IItem[] EnsureIItemArray (IItem[] items)
		{
			IItem[] inner_items;

			inner_items = items.Clone () as IItem[];
			for (int i = 0; i < items.Length; ++i) {
				if (items[i] is DoItem) {
					inner_items[i] = (items[i] as DoItem).Inner as IItem;
				}
			}
			return inner_items;
		}

		public static IItem[] EnsureDoItemArray (IItem[] items)
		{
			IItem[] do_items;

			do_items = items.Clone () as IItem[];
			for (int i = 0; i < items.Length; ++i) {
				if (!(items[i] is DoItem)) {
					do_items[i] = new DoItem (items[i]);
				}
			}
			return do_items;
		}
		
		protected int score;
		protected IObject inner;
		
		protected DoObject (IObject inner)
		{
			if (inner == null)
				throw new ArgumentNullException ("Inner IObject may not be null.");
			
			this.inner = inner;
			relevance = relevanceProvider.GetRelevance (this);
		}

		public virtual IObject Inner {
			get { return inner; }
			set { inner = value; }
		}
		
		public virtual string Name {
			get { return inner.Name ?? kDefaultName; }
		}
		
		public virtual string Description {
			get { return inner.Description ?? kDefaultDescription; }
		}
		
		public virtual string Icon {
			get { return inner.Icon ?? kDefaultIcon; }
		}
		
		public virtual string UID {
			get {
				return string.Format ("{0}{1}{2}", inner.GetType (), Name, Description);
			}
		}
		
		public override int GetHashCode ()
		{
			return UID.GetHashCode ();
		}
		
		public int Score {
			get { return score; }
			set { score = value; }
		}
		
		public int Relevance {
			get {
				return relevance;
			}
			set {
				if (value < Relevance)
					relevanceProvider.Decrease (this);
				else if (value > Relevance)
					relevanceProvider.Increase (this);
				relevance = relevanceProvider.GetRelevance (this);
			}
		}
		
		public int ScoreForAbbreviation (string ab)
		{
			float similarity;
			
			if (ab == "") {
				return 100;
			} else {
				similarity = Util.StringScoreForAbbreviation (Name, ab);
			}
			return (int) (100 * similarity);
		}
		
		public override bool Equals (object o)
		{
			IObject other = o as IObject;

			if (other == null) return false;
			return this.GetType () == other.GetType () &&
				Name == other.Name &&
				Description == other.Description;
		}

		public override string ToString ()
		{
			return UID;
		}
	}
}
