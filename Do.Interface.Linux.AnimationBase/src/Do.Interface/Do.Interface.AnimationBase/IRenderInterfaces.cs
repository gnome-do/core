// IRenderInterfaces.cs
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
// GNU General Public License for more details.bz
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;

using Cairo;
using Gdk;

namespace Do.Interface.AnimationBase
{
	/// <summary>
	/// Provides a Window background rendering element.  Classes of this type are responsible for 
	/// rendering the background of the interface.  Complex themes with slow to render backgrounds should
	/// consider buffering.
	/// </summary>
	public interface IBezelWindowRenderElement
	{
		/// <value>
		/// Retrieves that background color.  This color will be used as the base color for a variety of colors
		/// </value>
		Cairo.Color BackgroundColor {get;}
		
		/// <summary>
		/// Render the element.  The renderer should respect the rectangle given to it.
		/// </summary>
		/// <param name="cr">
		/// A <see cref="Context"/>
		/// </param>
		/// <param name="drawing_area">
		/// A <see cref="Gdk.Rectangle"/> that represents the current render area.  This is NOT the damage
		/// area but rather the area that the background should cover.  The edges of Do's window are defined
		/// by this rect.  The X,Y coordinates represent the top left of the backgrounds location.
		/// </param>
		void RenderItem (Context cr, Gdk.Rectangle drawing_area);
		
		/// <summary>
		/// Returns what was clicked on.
		/// </summary>
		/// <param name="drawing_area">
		/// A <see cref="Gdk.Rectangle"/>
		/// </param>
		/// <param name="point">
		/// A <see cref="Gdk.Point"/>
		/// </param>
		/// <returns>
		/// A <see cref="PointLocation"/>
		/// </returns>
		PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point);
	}
	
	/// <summary>
	/// Renders a titlebar like element.  This render can optionally do nothing.
	/// </summary>
	public interface IBezelTitleBarRenderElement
	{
		/// <value>
		/// Will be used by BezelDrawingArea to calculate an additional offset at the top of the rendering area.
		/// </value>
		int Height {get;}
		
		/// <summary>
		/// Renders the element in the given region
		/// </summary>
		/// <param name="cr">
		/// A <see cref="Context"/>
		/// </param>
		/// <param name="drawing_area">
		/// A <see cref="Gdk.Rectangle"/>
		/// </param>
		void RenderItem (Context cr, Gdk.Rectangle drawing_area);
		PointLocation GetPointLocation (Gdk.Rectangle drawing_area, Gdk.Point point);
	}

	/// <summary>
	/// An element primarily used
	/// </summary>
	public interface IBezelOverlayRenderElement
	{
		/// <summary>
		/// Render the text mode overlay.  Generally this will not render over a titlebar.
		/// </summary>
		/// <param name="cr">
		/// A <see cref="Context"/>
		/// </param>
		/// <param name="drawing_area">
		/// A <see cref="Gdk.Rectangle"/>
		/// </param>
		/// <param name="overlay">
		/// A <see cref="System.Double"/>
		/// </param>
		void RenderItem (Context cr, Gdk.Rectangle drawing_area, double overlay);
	}
	
	/// <summary>
	/// Interface to render Panes
	/// </summary>
	public interface IBezelPaneRenderElement
	{
		/// <value>
		/// Width of a pane
		/// </value>
		int Width    { get; }
		
		/// <value>
		/// Height of a pane
		/// </value>
		int Height   { get; }
		
		/// <value>
		/// Icon size of a pane
		/// </value>
		int IconSize { get; }
		
		/// <value>
		/// true if the icon and text are stacked, false if the text is designed to be to the right of the icon
		/// </value>
		bool StackIconText { get; }
		
		void RenderItem (Context cr, Gdk.Rectangle render_area, bool focused);
	}
}
