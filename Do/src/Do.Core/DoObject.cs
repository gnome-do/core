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

namespace Do.Core {

	public abstract class DoObject : IObject, IComparable<IObject> {

		const string DefaultName = "No name";
		const string DefaultDescription = "No description.";
		const string DefaultIcon = "emblem-noread";
		
		static RelevanceProvider relevanceProvider;

		static DoObject ()
		{
			relevanceProvider = RelevanceProvider.GetProvider ();
		}

		public static bool IObjectTypeCheck (IObject o, Type [] types)
		{
			foreach (Type type in types) {
				if (type.IsAssignableFrom (o.GetType ()))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns the inner item if the static type of given item is an DoItem
		/// subtype. Returns the argument otherwise.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem"/> that may or may not be an DoItem subtype.
		/// </param>
		/// <returns>
		/// A <see cref="IItem"/> that is NOT an DoItem subtype (the inner IItem
		/// of an DoItem).
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
		/// A <see cref="IItem []"/> that may contain
		/// DoItem subtypes.
		/// </param>
		/// <returns>
		/// A <see cref="IItem []"/> of inner IItems.
		/// </returns>
		public static IItem [] EnsureIItemArray (IItem [] items)
		{
			IItem [] inner_items;

			inner_items = items.Clone () as IItem [];
			for (int i = 0; i < items.Length; ++i) {
				if (items [i] is DoItem) {
					inner_items [i] = (items [i] as DoItem).Inner as IItem;
				}
			}
			return inner_items;
		}

		public static IItem [] EnsureDoItemArray (IItem [] items)
		{
			IItem [] do_items;

			do_items = items.Clone () as IItem [];
			for (int i = 0; i < items.Length; ++i) {
				if (!(items [i] is DoItem)) {
					do_items [i] = new DoItem (items [i]);
				}
			}
			return do_items;
		}
		
		protected IObject inner;
		protected float relevance;
		
		protected DoObject (IObject inner)
		{
			if (inner == null)
				throw new ArgumentNullException ("Inner IObject may not be null.");

			this.inner = inner;
		}

		public virtual IObject Inner {
			get { return inner; }
			set { inner = value; }
		}
	
		public float Relevance {
			get { return relevance; }
			set { relevance = value; }
		}
		
		public virtual string Name {
			get {
				string name = null;
				try {
					name = inner.Name;
				} catch (Exception e) {
					LogError ("Name", e, "_");
				} finally {
					name = name ?? DefaultName;
				}
				return name;
			}
		}
		
		public virtual string Description {
			get {
				string description = null;
				try {
					description = inner.Description;
				} catch (Exception e) {
					LogError ("Description", e);
				} finally {
					description = description ?? DefaultDescription;
				}
				return description;
			}
		}
		
		public virtual string Icon {
			get {
				string icon = null;
				try {
					icon = inner.Icon;
				} catch (Exception e) {
					LogError ("Icon", e);
				} finally {
					icon = icon ?? DefaultIcon;
				}
				return icon;
			}
		}
		
		public virtual string UID {
			get {
				return string.Format ("{0}{1}{2}",
					inner.GetType (), Name, Description);
			}
		}
		
		public override int GetHashCode ()
		{
			return UID.GetHashCode ();
		}
	
		public override bool Equals (object o)
		{
			DoObject other = o as DoObject;

			if (other == null) return false;
			return other.UID == UID;
		}

		public override string ToString ()
		{
			return Name;
		}

		public void IncreaseRelevance (string match, DoObject other)
		{
			relevanceProvider.IncreaseRelevance (this, match, other);
		}

		public void DecreaseRelevance (string match, DoObject other)
		{
			relevanceProvider.DecreaseRelevance (this, match, other);
		}

		public void UpdateRelevance (string match, DoObject other)
		{
			relevance = relevanceProvider.GetRelevance (this, match, other);
		}

		public bool CanBeFirstResultForKeypress (char a)
		{
			return relevanceProvider.CanBeFirstResultForKeypress (this, a);
		}

		// Only compare with DoObjects.
		public int CompareTo (IObject other)
		{
			return (int) (1000000 * ((other as DoObject).Relevance - Relevance));
		}

		protected void LogError (string where, Exception e)
		{
			LogError (where, e, Name);
		}

		protected void LogError (string where, Exception e, string name)
		{
			Log.Error ("\"{0}\" ({1}) encountered an error in {2}:\n\t" +
				       "{3}: {4}",
				name, (inner != null ? inner.GetType () : GetType ()),
				where, e.GetType (), e.Message);
		}
	}
}
