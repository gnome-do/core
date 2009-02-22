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

using Docky.Core;

namespace Docky.Interface
{
	
	
	internal class PainterService : IPainterService
	{
		ICollection<IDockPainter> painters;
		DockArea parent;
		
		#region IPainterService implementation

		public void RegisterPainter (IDockPainter painter)
		{
			painters.Add (painter);
			painter.ShowRequested += HandleShowRequested;
			painter.HideRequested += HandleHideRequested;
		}

		void HandleHideRequested(object sender, EventArgs e)
		{
			IDockPainter painter = sender as IDockPainter;
			if (painter == null)
				return;
			
			parent.RequestHidePainter (painter);
		}

		void HandleShowRequested(object sender, EventArgs e)
		{
			IDockPainter painter = sender as IDockPainter;
			if (painter == null)
				return;
			
			bool shown = parent.RequestShowPainter (painter);
			if (!shown)
				painter.Interrupt ();
		}
		
		#endregion 
		
		internal PainterService (DockArea parent)
		{
			this.parent = parent;
			painters = new List<IDockPainter> ();
		}

		public void BuildPainters ()
		{
			RegisterPainter (new Painters.SummonModeRenderer ());
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			DockServices.UnregisterService (this);
			
			foreach (IDockPainter painter in painters)
				painter.Dispose ();
			
			parent = null;
		}
		
		#endregion 
		
	}
}
