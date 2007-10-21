/* IItem.cs
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
using System.Collections.Generic;

namespace Do.Universe
{
	/// <summary>
	/// The base interface that all classes to be used as items
	/// (objects on which commands operate) must implement, either directly
	/// or indirectly.
	/// </summary>
	public interface IItem : IObject
	{
	}
	
	/// <summary>
	/// An IItem with a meaningful text representation.
	/// Example: Something that would make sense if copied
	/// to the clipboard. 
	/// </summary>
	public interface ITextItem : IItem
	{
		/// <summary>
		/// A string of text representing this IItem.
		/// </summary>
		string Text { get; }
	}
	
	/// <value>
	/// An IItem with a URL.
	/// </value>
	public interface IURLItem : IItem
	{
		/// <value>
		/// The URL represented by this IItem.
		/// </value>
		string URL { get; }
	}
	
	/// <value>
	/// An IItem with a URI.
	/// </value>
	public interface IURIItem : IItem
	{
		/// <value>
		/// The URI represented by this IItem.
		/// </value>
		string URI { get; }
	}

	/// <summary>
	/// An IItem that is considered "runnable" by users.
	/// </summary>
	public interface IRunnableItem : IItem
	{
		/// <summary>
		/// When called, this method should have a meaningful
		/// "run" effect for the user.
		/// </summary>
		void Run ();
	}

	/// <summary>
	/// An IItem that is considered "openable" by users.
	/// </summary>
	public interface IOpenableItem : IItem
	{
		/// <summary>
		/// When called, this method should have a meaningful
		/// "open" effect for the user.
		/// </summary>
		void Open ();
	}
	
	/// <summary>
	/// An IItem representing a file.
	/// </summary>
	public interface IFileItem : IURIItem
	{
		/// <summary>
		/// The mime-type of the file. This is used to determine
		/// which applications can open which files.
		/// </summary>
		string MimeType { get; }
	}
}
