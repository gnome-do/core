// HotSeatPainter.cs
// 
// Copyright (C) 2009 GNOME Do
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
using System.Collections.Generic;

using Docky.Core;
using Docky.Interface;

namespace Docky.Interface.Painters
{
	
	
	public class HotSeatPainter : IDockPainter
	{
		
		
		#region IDockPainter implementation 
		
		public event EventHandler<PaintNeededArgs> PaintNeeded;
		public event EventHandler ShowRequested;
		public event EventHandler HideRequested;
		
		public void Paint (Cairo.Context cr, Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
		}
		
		public void Clicked (Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
		}
		
		public void Interupt ()
		{
		}
		
		public bool DoubleBuffer {
			get { return false; }
		}
		
		public bool Interuptable {
			get { return true; }
		}
		
		public int MinimumWidth {
			get { return 0; }
		}
		
		#endregion 
		

		
		public HotSeatPainter()
		{
		}

		public void Show ()
		{
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
		}
		
		#endregion 
		
	}
}
