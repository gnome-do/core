// IRenderTheme.cs
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

using Do.Addins;

namespace Do.UI
{
	
	
	public interface IRenderTheme
	{
		/// <value>
		/// Theme Name
		/// </value>
		string Name { get; }
		
		/// <value>
		/// Theme description
		/// </value>
		string Description { get; }
		
		/// <value>
		/// Values that are not specific to any one renderer
		/// </value>
		IBezelDefaults Defaults { get; }
		
		/// <value>
		/// Text mode overlay renderer
		/// </value>
		IBezelOverlayRenderElement Overlay { get; }
		
		/// <value>
		/// Pane background renderer
		/// </value>
		IBezelPaneRenderElement Pane { get; }
		
		/// <value>
		/// Text mode renderer
		/// </value>
		IBezelTitleBarRenderElement TitleBar { get; }
		
		/// <value>
		/// Window background renderer
		/// </value>
		IBezelWindowRenderElement Window { get; }
	}
}
