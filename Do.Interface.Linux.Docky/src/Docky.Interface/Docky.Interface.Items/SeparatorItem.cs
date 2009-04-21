// SeperatorItem.cs
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
using Do.Interface.CairoUtils;

using Docky.Utilities;

namespace Docky.Interface
{
	public class SeparatorItem : AbstractDockItem
	{
		Surface sr;
		#region IDockItem implementation 
		
		public override int Width {
			get { return (int) (DockPreferences.IconSize * .3); }
		}
		
		public override ScalingType ScalingType {
			get {
				return ScalingType.None;
			}
		}

		
		#endregion 
		
		public SeparatorItem ()
		{
			AnimationType = ClickAnimationType.None;
			DockPreferences.IconSizeChanged += HandleIconSizeChanged;
		}

		void HandleIconSizeChanged ()
		{
			if (sr != null)
				sr.Destroy ();
			sr = null;
		}
		
		public override Surface GetIconSurface (Surface buffer, int targetSize, out int actualSize)
		{
			actualSize = DockPreferences.IconSize;
			if (sr == null) {
				sr = buffer.CreateSimilar (buffer.Content, Width, Height);
				Context cr = new Context (sr);
				cr.AlphaFill ();

				cr.LineWidth = 1;
				
				cr.MoveTo (Width / 2 - .5, 0);
				cr.LineTo (Width / 2 - .5, Height);
				RadialGradient rg = new RadialGradient (Width / 2, Height / 2, 0, Width / 2, Height / 2, Height / 2);
				rg.AddColorStop (0, new Cairo.Color (1, 1, 1, .3));
				rg.AddColorStop (0.3, new Cairo.Color (1, 1, 1, .3));
				rg.AddColorStop (1, new Cairo.Color (1, 1, 1, 0));
				cr.Pattern = rg;
				cr.Stroke ();
				
				rg.Destroy ();
				
				cr.MoveTo (Width / 2 + .5, 0);
				cr.LineTo (Width / 2 + .5, Height);
				rg = new RadialGradient (Width / 2, Height / 2, 0, Width / 2, Height / 2, Height / 2);
				rg.AddColorStop (0, new Cairo.Color (1, 1, 1, .15));
				rg.AddColorStop (0.3, new Cairo.Color (1, 1, 1, .15));
				rg.AddColorStop (1, new Cairo.Color (1, 1, 1, 0));
				cr.Pattern = rg;
				cr.Stroke ();
				
				rg.Destroy ();
				
				(cr as IDisposable).Dispose ();
			}
			return sr;
		}
		
		#region IDisposable implementation 
		
		public override void Dispose ()
		{
			DockPreferences.IconSizeChanged -= HandleIconSizeChanged;
			if (sr != null) {
				sr.Destroy ();
				sr = null;
			}
		}
		
		#endregion 
		
		public override bool Equals (AbstractDockItem other) 
		{
			if (other == null)
				return false;
			return object.ReferenceEquals (this, other);
		}
	}
}
