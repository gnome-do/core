// PainterService.cs
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

using Do.Platform;

using Docky.Core;
using Docky.Interface;

namespace Docky.Core.Default
{
	
	
	public class PainterService : IPainterService
	{
		
		
		ICollection<IDockPainter> painters;
		
		#region IPainterService implementation
		
		public event EventHandler PainterHideRequest;
		public event EventHandler PainterShowRequest;
		
		public void RegisterPainter (IDockPainter painter)
		{
			painters.Add (painter);
			painter.ShowRequested += HandleShowRequested;
			painter.HideRequested += HandleHideRequested;
		}

		void HandleHideRequested(object sender, EventArgs e)
		{
			IDockPainter painter = sender as IDockPainter;
			if (painter == null || PainterHideRequest == null)
				return;
			
			PainterHideRequest (sender, e);
		}

		void HandleShowRequested(object sender, EventArgs e)
		{
			IDockPainter painter = sender as IDockPainter;
			if (painter == null || PainterShowRequest == null)
				return;
			
			PainterShowRequest (sender, e);
		}
		
		#endregion 
		
		public PainterService ()
		{
			painters = new List<IDockPainter> ();
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			DockServices.UnregisterService (this);
			
			foreach (IDockPainter painter in painters) {
				painter.ShowRequested -= HandleShowRequested;
				painter.HideRequested -= HandleHideRequested;
				painter.Dispose ();
			}
			painters.Clear ();
		}
		
		#endregion
	}
}
