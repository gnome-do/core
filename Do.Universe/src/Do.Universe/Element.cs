/* Element.cs
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
using System.Linq;
using System.Collections.Generic;

using Mono.Unix;

using Do.Universe.Safe;

namespace Do.Universe
{

	public abstract class Element :
		IEquatable<Element>, IComparable<Element>
	{
		const string UniqueIdFormat = "{0}: {1} ({2})";

		public static readonly string DefaultName;
		public static readonly string DefaultDescription;
		public static readonly string DefaultIcon;

		static Element ()
		{
			DefaultName = Catalog.GetString ("No name");
			DefaultDescription = Catalog.GetString ("No description.");
			DefaultIcon = "emblem-noread";
		}

		string uniqueId;

		protected Element ()
		{
			uniqueId = DefaultName;
		}
		
		static SafeElement safe_element = new SafeElement ();
		
		public SafeElement Safe {
			get {
				safe_element.Element = this;
				return safe_element;
			}
		}

		public SafeElement RetainSafe ()
		{
			return new SafeElement (this);
		}

		public string UniqueId {
			get {
				// We have to initialize the UniqueId lazily, because it is not safe
				// to initialize in the constructor, before subclasses have initialized.
				if (object.Equals (uniqueId, DefaultName)) { 
					SafeElement safe = Safe;
					uniqueId = string.Format (UniqueIdFormat, safe.Name, safe.Description, GetType ());
				}
				return uniqueId;
			}
		}
		
		public float Relevance { get; set; }
		
		/// <value>
		/// The human-readable name of the element.
		/// Example: The name of an application, like "Pidgin Internet Messenger."
		/// </value>
		public abstract string Name { get; }
		
		/// <value>
		/// The human-readable description of the element.
		/// Example: The URL of a bookmark or absolute path of a file.
		/// </value>
		public abstract string Description { get; }
		
		public abstract string Icon { get; }
		
		public override int GetHashCode ()
		{
			return UniqueId.GetHashCode ();
		}
	
		public override bool Equals (object o)
		{
			return o is Element && Equals (o as Element);
		}

		public bool Equals (Element e)
		{
			return e != null && e.UniqueId == UniqueId;
		}

		public override string ToString ()
		{
			return UniqueId;
		}

		public int CompareTo (Element e)
		{
			return (int) (1000000 * (e.Relevance - Relevance));
		}

		public bool PassesTypeFilter (IEnumerable<Type> types)
		{
			return !types.Any () || types.Any (type => type.IsInstanceOfType (this));
		}
		
	}
}
