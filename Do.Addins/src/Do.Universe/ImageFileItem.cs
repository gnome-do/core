/* ImageFileItem.cs
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
	/// FileItem subclass represnting an image file.
	/// </summary>
	public class ImageFileItem : FileItem
	{
		/*
		 * This was not being called because no other class references this
		 * class. I am guessing this will be called when FileItem subclasses
		 * are loaded from plugins. For now this code is moved to FileItem.cs
		 * 
		 * TODO: fix this.
		
		static string[] imageExtentions = { "jpg", "jpeg", "png", "gif" };
		
		static ImageFileItem ()
		{
			foreach (string ext in imageExtentions) {
				FileItem.RegisterExtensionForFileItemType (ext, typeof (ImageFileItem));
			}
		}
		*/
		
		public ImageFileItem (string uri)
			: base (uri)
		{	
		}
		
		/// <value>
		/// ImageFileItems use their image files as their icons!
		/// </value>
		public override string Icon
		{
			get { return URI; }
		}
	}
}
