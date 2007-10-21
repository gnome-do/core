/* IObject.cs
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

namespace Do.Universe
{
	/// <summary>
	/// This interface captures everything common to IItem, ICommand,
	/// and IItemSource interfaces.
	/// </summary>
	public interface IObject
	{
		/// <value>
		/// The human-readable name of the object.
		/// Example: The name of an application, like "Pidgin Internet Messenger."
		/// </value>
		string Name { get; }
		
		/// <value>
		/// The human-readable name of the object.
		/// Example: The URL of a bookmark or absolute path of a file.
		/// </value>
		string Description { get; }
		
		/// <summary>
		/// The object's icon. This can be either an absolute path to an image,
		/// an icon name that can be looked up with Gtk.IconTheme.Default.LoadIcon,
		/// or the path to a resource in the same assembly as the implementing class.
		/// In the case of a path to an assembly resource, use this path convention:
		/// 
		///    "Fully.Qualified.Class.Name:path_to_my_resource"
		/// 
		/// For example, if your class is JohnDoe.PidginItem and your assembly contains
		/// a resource called pidgin_icon.svg, use this string:
		/// 
		///    "JohnDoe.PidginItem:pidgin_icon.svg"
		/// </summary>
		string Icon { get; }
	}
}
