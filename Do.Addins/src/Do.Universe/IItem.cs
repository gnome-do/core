/* ${FileName}
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
	public interface IItem : IObject
	{
	}
	
	public interface ITextItem : IItem
	{
		string Text { get; }
	}
	
	public interface IURLItem : IItem
	{
		string URL { get; }
	}
	
	public interface IURIItem : IItem
	{
		string URI { get; }
	}

	public interface IRunnableItem : IItem
	{
		void Run ();
	}

	public interface IOpenableItem : IItem
	{
		void Open ();
	}
	
	public interface IFileItem : IURIItem
	{
		string MimeType { get; }
	}
}
