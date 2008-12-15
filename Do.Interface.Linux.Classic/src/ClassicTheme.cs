// ClassicTheme.cs
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
	public class AnimatedClassicWindow : AbstractAnimatedInterface
	{
		protected override IRenderTheme RenderTheme {
			get { return new ClassicTheme (); }
		}
	}
	
	public class ClassicTheme : IRenderTheme
	{
		
		public string Name {
			get {
				return "Classic";
			}
		}

		public string Description {
			get {
				return "It's the classic";
			}
		}
		
		public IBezelDefaults GetDefaults (BezelDrawingArea parent)
		{
			return new ClassicBezelDefaults ();
		}

		public IBezelOverlayRenderElement GetOverlay (BezelDrawingArea parent)
		{
			return new ClassicTextOverlayRenderer (parent);
		}

		public IBezelPaneRenderElement GetPane (BezelDrawingArea parent)
		{
			return new ClassicPaneOutlineRenderer (parent);
		}

		public IBezelTitleBarRenderElement GetTitleBar (BezelDrawingArea parent)
		{
			return new ClassicTopBar (parent);
		}

		public IBezelWindowRenderElement GetWindow (BezelDrawingArea parent)
		{
			return new ClassicBackgroundRenderer (parent);
		}

		
	}
}
