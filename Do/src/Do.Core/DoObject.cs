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
using System.Linq;
using System.Collections.Generic;

using Do.Addins;
using Do.Universe;

namespace Do.Core {

	public class DoObject : IObject, IConfigurable, IComparable<IObject> {

		const string DefaultName = "No name";
		const string DefaultDescription = "No description.";
		const string DefaultIcon = "emblem-noread";

		protected IObject inner;
		protected float relevance;
		protected string uid;

		public static IObject Wrap (IObject o)
		{
			return o is DoObject ? o : new DoObject (o);
		}

		public static T Unwrap<T> (T o) where T : class, IObject
		{
			while (o is DoObject)
				o = (o as DoObject).Inner as T;
			return o;
		}

		public DoObject (IObject inner)
		{
			if (inner == null)
				throw new ArgumentNullException ("inner","Inner IObject may not be null.");
			this.inner = inner;
			
			uid = string.Format ("{0}{1}{2}", inner.GetType (), Name, Description);
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
		
		public Gtk.Bin GetConfiguration ()
		{
			Gtk.Bin config = null;
			try {
				if (Inner is IConfigurable)
					config = (Inner as IConfigurable).GetConfiguration ();
			} catch {
			}
			return config;
		}
		
		public virtual string UID {
			get {
				return uid;
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
			Type t = inner != null ? inner.GetType () : GetType ();
			Log.Error ("\"{0}\" ({1}) encountered an error in {2}: {3}",
				name, t, where, e.Message);
		}
	}
}
