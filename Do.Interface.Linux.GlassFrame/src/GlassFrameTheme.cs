// GlassFrameTheme.cs
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
using Do.Interface.AnimationBase;

namespace Do.Interface
{
	public class GlassFrameWindow : AbstractAnimatedInterface
	{
		protected override IRenderTheme RenderTheme {
			get { return new GlassFrameTheme (); }
		}
	}
	
	public class GlassFrameTheme : IRenderTheme
	{
		
		
		#region IRenderTheme implementation 
		
		public IBezelDefaults GetDefaults (BezelDrawingArea parent)
		{
			return new GlassFrameDefaults ();
		}
		
		public IBezelOverlayRenderElement GetOverlay (BezelDrawingArea parent)
		{
			return new GlassFrameTextOverlayRenderer (parent);
		}
		
		public IBezelPaneRenderElement GetPane (BezelDrawingArea parent)
		{
			return new GlassFramePaneOutlineRenderer (parent);
		}
		
		public IBezelTitleBarRenderElement GetTitleBar (BezelDrawingArea parent)
		{
			return new GlassFrameTopBar (parent);
		}
		
		public IBezelWindowRenderElement GetWindow (BezelDrawingArea parent)
		{
			return new GlassFrameBackgroundRenderer (parent);
		}
		
		public string Name {
			get {
				return "KGlass";
			}
		}
		
		public string Description {
			get {
				return "Playing nice with the K boys";
			}
		}
		
		#endregion 
		

	}
}
