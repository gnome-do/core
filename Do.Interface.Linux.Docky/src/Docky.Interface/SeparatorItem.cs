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
using Do.Interface;
using Do.Interface.CairoUtils;

using Docky.Utilities;

namespace Docky.Interface
{
	
	
	public class SeparatorItem : IDockItem
	{
		
		Surface sr;
		#region IDockItem implementation 
		
		public Surface GetIconSurface (Surface buffer)
		{
			if (sr == null) {
				sr = buffer.CreateSimilar (buffer.Content, Width, DockPreferences.IconSize);
				Context cr = new Context (sr);
				cr.AlphaFill ();
				
				for (int i=1; i*6+2 < Height; i++) {
					cr.Rectangle (Width/2-1, i*6, 4, 2);
				}
				
				cr.Color = new Cairo.Color (1, 1, 1, .3);
				cr.Fill ();
				
				(cr as IDisposable).Dispose ();
			}
			return sr;
		}
		
		public Surface GetTextSurface (Surface similar)
		{
			return null;
		}
		
		public Gdk.Pixbuf GetDragPixbuf ()
		{
			return null;
		}
		
		public string Description {
			get {
				return "";
			}
		}
		
		public int Width {
			get {
				return (int) (DockPreferences.IconSize * .3);
			}
		}
		
		public int Height {
			get {
				return DockPreferences.IconSize;
			}
		}
		
		public bool IsAcceptingDrops {
			get { return false; }
		}
		
		public int WindowCount { get { return 0; } }
		
		public bool Scalable { get { return false; } }
		
		public DateTime LastClick { get; set; }
		public DateTime DockAddItem { get; set; }
		
		#endregion 
		
		public SeparatorItem ()
		{
			DockPreferences.IconSizeChanged += delegate {
				if (sr != null)
					sr.Destroy ();
				sr = null;
			};
		}
		
		public bool ReceiveItem (string item)
		{
			return false;
		}
		
		public void Clicked (uint button)
		{
		}
		
		public void SetIconRegion (Gdk.Rectangle region)
		{
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			if (sr != null)
				sr.Destroy ();
		}
		
		#endregion 
		
		public bool Equals (IDockItem other) 
		{
			return GetHashCode ().Equals (other.GetHashCode ());
		}
	}
}
