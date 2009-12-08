/* SafeElement.cs
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

namespace Do.Universe.Safe
{
	
	public class SafeElement : Element
	{

		public static void LogSafeError (Element who, Exception what, string where)
		{
			LogSafeError (who, what, where, who.Safe.Name);
		}

		public static void LogSafeError (Element who, Exception what, string where, string name)
		{
			Console.Error.WriteLine ("{0} \"{1}\" encountered an error in {2}: {3}.",
					who.GetType (), name, where, what.ToString ());
		}

		public Element Element { protected get; set; }

		public SafeElement () : this (null)
		{
		}
		
		public SafeElement (Element element)
		{
			Element = element;
		}
		
		public override string Name {
			get {
				try {
					return Element.Name ?? Element.DefaultName;
				} catch (Exception e) {
					LogSafeError (Element, e, "Name", Element.DefaultName);
				}
				return Element.DefaultName;
			}
		}

		public override string Description {
			get {
				try {
					return Element.Description ?? Element.DefaultDescription;
				} catch (Exception e) {
					LogSafeError (Element, e, "Description");
				}
				return Element.DefaultDescription;
			}
		}

		public override string Icon {
			get {
				try {
					return Element.Icon ?? Element.DefaultIcon;
				} catch (Exception e) {
					LogSafeError (Element, e, "Icon");
				}
				return Element.DefaultIcon;
			}
		}
	}
}
