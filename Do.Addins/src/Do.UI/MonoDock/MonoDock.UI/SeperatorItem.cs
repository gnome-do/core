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
using Do.Addins.CairoUtils;

namespace MonoDock.UI
{
	
	
	public class SeperatorItem : IDockItem
	{
		
		Surface sr;
		#region IDockItem implementation 
		
		public Surface GetIconSurface ()
		{
			if (sr == null) {
				sr = new ImageSurface (Cairo.Format.Argb32, 20, 64);
				Context cr = new Context (sr);
				cr.AlphaFill ();
				
				cr.MoveTo (10, 0);
				cr.LineTo (10, 64);
				LinearGradient lg = new LinearGradient (0, 0, 0, 64);
				lg.AddColorStop (0, new Cairo.Color (1, 1, 1, 0));
				lg.AddColorStop (.5, new Cairo.Color (1, 1, 1, .8));
				lg.AddColorStop (1, new Cairo.Color (1, 1, 1, 0));
				cr.Pattern = lg;
				
				cr.Stroke ();
				
				lg.Destroy ();
				(cr as IDisposable).Dispose ();
			}
			return sr;
		}
		
		public Surface GetTextSurface ()
		{
			return null;
		}
		
		public string Description {
			get {
				return string.Empty;
			}
		}
		
		public int Width {
			get {
				return 20;
			}
		}
		
		public int Height {
			get {
				return 64;
			}
		}
		
		public bool Scalable { get { return false; } }
		
		public DateTime LastClick { get; set; }
		public DateTime DockAddItem { get; set; }
		
		#endregion 
		

		
		public SeperatorItem()
		{
		}
		
		public void Clicked ()
		{
			
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			return;
		}
		
		#endregion 
		
		public bool Equals (IDockItem other) 
		{
			return GetHashCode ().Equals (other.GetHashCode ());
		}
	}
}
