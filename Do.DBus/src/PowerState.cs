// PowerState.cs
// 
// Copyright (C) 2008 GNOME-Do
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

using NDesk.DBus;
using org.freedesktop.DBus;

namespace Do.DBus
{
	[Interface("org.freedesktop.PowerManagement")]
	public interface IPowerManagement
	{
		bool GetOnBattery ();
	}
	
	public class PowerState
	{
		public static bool OnBattery  ()
		{
			try {
				if (!Bus.Session.NameHasOwner ("org.freedesktop.PowerManagement")) return false;
				
				IPowerManagement power = 
					Bus.Session.GetObject<IPowerManagement> ("org.freedesktop.PowerManagement", 
					                                         new ObjectPath ("/org/freedesktop/PowerManagement"));
				
				return power.GetOnBattery ();
			} catch {
				return false;
			}
		}
	}
}
