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

namespace Do.Universe
{

	public abstract class Element :
		IEquatable<Element>, IComparable<Element>, IComparable
	{
		const string UniqueIdFormat = "{0}: {1} ({2})";
		static readonly string DefaultName;
		static readonly string DefaultDescription;
		static readonly string DefaultIcon;

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

		public string UniqueId {
			get {
				// We have to initialize the UniqueId lazily, because it is not safe
				// to initialize in the constructor, before subclasses have initialized.
				if (object.Equals (uniqueId, DefaultName))
					uniqueId = string.Format (UniqueIdFormat, NameSafe, DescriptionSafe, GetType ());
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

		#region Safe alternatives
		
		public string NameSafe {
			get {
				try {
					return Name;
				} catch (Exception e) {
					Console.Error.WriteLine ("{0} encountered an error in Name: {1}", GetType (), e.Message);
					// Log.Debug (e.StackTrace);
				}
				return DefaultName;
			}
		}

		public string DescriptionSafe {
			get {
				try {
					return Description;
				} catch (Exception e) {
					Console.Error.WriteLine ("{0} \"{1}\" encountered an error in Description: {2}", GetType (), NameSafe, e.Message);
					// Log.Debug (e.StackTrace);
				}
				return DefaultDescription;
			}
		}

		public string IconSafe {
			get {
				try {
					return Icon;
				} catch (Exception e) {
					Console.Error.WriteLine ("{0} \"{1}\" encountered an error in Icon: {2}", GetType (), NameSafe, e.Message);
					// Log.Debug (e.StackTrace);
				}
				return DefaultIcon;
			}
		}

		#endregion
		
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

		public int CompareTo (object o)
		{
			return o is Element ? CompareTo (o as Element) : 0;
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
