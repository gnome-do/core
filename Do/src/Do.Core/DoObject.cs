// DoObject.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this
// inner distribution.
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
using System.Linq;
using System.Collections.Generic;

using Mono.Unix;

using Do.Addins;
using Do.Universe;
using Do.Platform;

namespace Do.Core {

	/// <summary>
	/// The root of our wrapper heirarchy (DoObject, DoItem, DoItemSource,
	/// DoAction).
	/// </summary>
	public class DoObject : IObject, IConfigurable,
		IEquatable<DoObject>, IComparable<IObject>, IComparable<DoObject> {

		const string UIDFormat = "{0}: {1} ({2})";
		static readonly string DefaultName;
		static readonly string DefaultDescription;
		static readonly string DefaultIcon;

		static DoObject ()
		{
			DefaultName = Catalog.GetString ("No name");
			DefaultDescription = Catalog.GetString ("No description.");
			DefaultIcon = "emblem-noread";
		}

		/// <summary>
		/// Ensures that the dynamic type of <paramref name="o"/> is
		/// <see cref="DoObject"/>.
		/// </summary>
		/// <param name="o">
		/// An <see cref="IObject"/>.
		/// </param>
		/// <returns>
		/// An <see cref="IObject"/> whose dynamic type is
		/// <see cref="DoObject"/>.
		/// </returns>
		public static IObject Wrap (IObject o)
		{
			return o is DoObject ? o : new DoObject (o);
		}

		public static IObject Unwrap (IObject o)
		{
			while (o is DoObject)
				o = (IObject) (o as DoObject).Inner;
			return o;
		}

		/// <value>
		/// A unique identifier for this <see cref="IObject"/>.
		/// </value>
		public string UID { get; private set; }
		
		/// <value>
		/// This <see cref="IObject"/>'s relevance for the most recent search.
		/// </value>
		public float Relevance { get; set; }
		
		/// <value>
		/// The inner <see cref="IObject"/> wrapped by this instance.
		/// </value>
		protected IObject Inner { get; set; }
		
		public DoObject (IObject inner)
		{
			if (inner == null)
				throw new ArgumentNullException ("inner", "Inner IObject may not be null.");
			
			Inner = inner;

			// In case Name or Description throws when constructing the UID, set it to a default so
			// something appears in the log message.
			UID = DefaultName;
			UID = string.Format (UIDFormat, Name, Description, Inner.GetType ());
		}

		public bool PassesTypeFilter (IEnumerable<Type> types)
		{
			return !types.Any () || Inner.IsAssignableToAny (types);
		}

		//// <value>
		/// Safe wrapper for inner <see cref="IObject"/>'s Name property.
		/// </value>
		public virtual string Name {
			get {
				string name = null;
				try {
					name = Inner.Name;
				} catch (Exception e) {
					LogError ("Name", e);
				} finally {
					name = name ?? DefaultName;
				}
				return name;
			}
		}

		//// <value>
		/// Safe wrapper for inner <see cref="IObject"/>'s Description property.
		/// </value>
		public virtual string Description {
			get {
				string description = null;
				try {
					description = Inner.Description;
				} catch (Exception e) {
					LogError ("Description", e);
				} finally {
					description = description ?? DefaultDescription;
				}
				return description;
			}
		}

		//// <value>
		/// Safe wrapper for inner <see cref="IObject"/>'s Icon property.
		/// </value>
		public virtual string Icon {
			get {
				string icon = null;
				try {
					icon = Inner.Icon;
				} catch (Exception e) {
					LogError ("Icon", e);
				} finally {
					icon = icon ?? DefaultIcon;
				}
				return icon;
			}
		}

		/// <summary>
		/// Safe wrapper for inner <see cref="IObject"/>'s
		/// <see cref="IConfigurable"/>.GetConfiguration method.
		/// Returns null if inner <see cref="IObject"/> is not <see cref="IConfigurable"/>,
		/// or an exception is thrown in the inner GetConfiguration call.
		/// </summary>
		/// <returns>
		/// A <see cref="Gtk.Bin"/> containing configuration widgets to be associated with
		/// this IObject.
		/// </returns>
		public Gtk.Bin GetConfiguration ()
		{
			Gtk.Bin config = null;
			try {
				if (Inner is IConfigurable)
					config = (Inner as IConfigurable).GetConfiguration ();
			} catch (Exception e) {
				LogError ("GetConfiguration", e);
			}
			return config;
		}
		
		public override int GetHashCode ()
		{
			return UID.GetHashCode ();
		}
	
		public override bool Equals (object o)
		{
			if (o is DoObject)
				return Equals (o as DoObject);
			return false;
		}

		public bool Equals (DoObject o)
		{
			return o != null && o.UID == UID;
		}

		public override string ToString ()
		{
			return UID;
		}

		public int CompareTo (IObject other)
		{
			return CompareTo (Wrap (other) as DoObject);
		}

		public int CompareTo (DoObject other)
		{
			return (int) (1000000 * (other.Relevance - Relevance));
		}

		protected void LogError (string where, Exception e)
		{
			Log.Error ("{0} encountered an error in {1}: {2} \"{3}\".",
				UID, where, e.GetType ().Name, e.Message);
			Log.Debug (e.StackTrace);
		}
	}
}
