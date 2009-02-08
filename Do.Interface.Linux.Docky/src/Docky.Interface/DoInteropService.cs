// DoInteropService.cs
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

using Do.Interface;

using Docky.Core;

namespace Docky.Interface
{
	
	
	public class DoInteropService : IDoInteropService
	{
		
		IDoController controller;

		public void SignalSummon ()
		{
			if (Summoned != null)
				Summoned (this, EventArgs.Empty);
		}

		public void SignalVanish ()
		{
			if (Vanished != null)
				Vanished (this, EventArgs.Empty);
		}

		public void SignalReset ()
		{
			if (Reset != null)
				Reset (this, EventArgs.Empty);
		}

		public void SignalResultsGrow ()
		{
			if (ResultsGrow != null)
				ResultsGrow (this, EventArgs.Empty);
		}

		public void SignalResultsShrink ()
		{
			if (ResultsShrink != null)
				ResultsShrink (this, EventArgs.Empty);
		}
		
		#region IDoInteropService implementation 
		
		public event EventHandler Summoned;
		
		public event EventHandler Vanished;

		public event EventHandler Reset;
		
		public event EventHandler ResultsGrow;
		
		public event EventHandler ResultsShrink;

		public void RequestClickOff ()
		{
			controller.ButtonPressOffWindow ();
		}
		
		#endregion 
		
		public DoInteropService (IDoController controller)
		{
			this.controller = controller;
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			DockServices.UnregisterService (this);
			controller = null;
		}
		
		#endregion 
		
	}
}
