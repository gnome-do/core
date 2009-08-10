// AbstractSystemService.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
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

using Do.Platform.ServiceStack;

namespace Do.Platform
{
	
	public abstract class AbstractSystemService : IService
	{		
		public event EventHandler OnBatteryChanged;
		
		protected void OnOnBatteryChanged ()
		{
			if (OnBatteryChanged != null)
				OnBatteryChanged (this, EventArgs.Empty);
		}
		
		public virtual bool GetOnBatteryPower ()
		{
			return false;
		}

		public abstract void EnsureSingleApplicationInstance ();
		
		public virtual bool IsAutoStartEnabled ()
		{
			return false;
		}
		public virtual void SetAutoStartEnabled (bool enabled)
		{
		}
		
		public abstract void SetProcessName (string name);
	}
}
