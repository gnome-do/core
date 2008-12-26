// IDockItem.cs
// 
// Copyright (C) 2008 GNOME Do
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

using Cairo;
using Gdk;

using Do.Interface;
using Do.Universe;

namespace Docky.Interface
{
	
	
	public interface IDockItem : IEquatable<IDockItem>, IDisposable
	{
		string Description { get; }
		int Width { get; }
		
		/// <value>
		/// the height of the icon
		/// </value>
		int Height { get; }
		
		//// <value>
		/// Determines if the icon will be scaled when displayed.  Non-scalable icons should be used only
		/// in situations where it makes no logical sense.
		/// </value>
		bool Scalable { get; }
		
		/// <value>
		/// If the "active applications" indicator should be drawn under the icon
		/// </value>
//		bool DrawIndicator { get; }
		
		int WindowCount { get; }
		
		/// <value>
		/// The last time the item was clicked
		/// </value>
		DateTime LastClick { get; }
		
		/// <value>
		/// The time at which the Dock Item was added to the dock
		/// </value>
		DateTime DockAddItem { get; set; }
		
		/// <summary>
		/// Returns the surface of the icon
		/// </summary>
		/// <param name="similar">
		/// A <see cref="Surface"/> similar surface used to create the icons surface.  This is for peformance reasons
		/// </param>
		/// <returns>
		/// A <see cref="Surface"/> that MUST be either Height * DockPreferences.IconQuality x Width * DockPreferences.IconQuality if scalable, 
		/// or Height x Width if not scalable
		/// </returns>
		Surface GetIconSurface (Surface similar);
		
		/// <summary>
		/// Returns a surface that contains the hover text pre-rendered
		/// </summary>
		/// <param name="similar">
		/// A <see cref="Surface"/>
		/// </param>
		/// <returns>
		/// A <see cref="Surface"/>
		/// </returns>
		Surface GetTextSurface (Surface similar);
		
		/// <summary>
		/// Returns a pixbuf suitable for using as a gtk dnd icon
		/// </summary>
		Pixbuf GetDragPixbuf ();
		
		/// <summary>
		/// Called every time the icon is clicked on
		/// </summary>
		/// <param name="button">
		/// A <see cref="System.UInt32"/>
		/// </param>
		void Clicked (uint button);
		
		/// <summary>
		/// Whenever the icons base position is updated, this method will be called.  This is so icons can set their child
		/// windows minimize target.  Or any other reason they might need it.  Root X,Y coordinates are given, so there
		/// is no need to translate.
		/// </summary>
		/// <param name="region">
		/// A <see cref="Gdk.Rectangle"/>
		/// </param>
		void SetIconRegion (Gdk.Rectangle region);
	}
	
	public interface IDockAppItem
	{
		event UpdateRequestHandler UpdateNeeded;
		
		bool NeedsAttention { get; }
		
		DateTime AttentionRequestStartTime { get; }
	}
}
