// NetworkService.cs
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
using System.Reflection;

#if USE_DBUS_SHARP
using DBus;
#else
using NDesk.DBus;
#endif
using org.freedesktop.DBus;

using Do.Platform.ServiceStack;
using Do.Platform.Linux.DBus;

namespace Do.Platform.Linux
{
	
	public class NetworkService : INetworkService
	{
		const string NetworkManagerName = "org.freedesktop.NetworkManager";
		const string NetworkManagerPath = "/org/freedesktop/NetworkManager";
		
		[Interface(NetworkManagerName)]
		interface INetworkManager : org.freedesktop.DBus.Properties
		{
			event StateChangedHandler StateChanged;
		}
		
		delegate void StateChangedHandler (uint state);
		
		INetworkManager network;
		
		public event EventHandler<NetworkStateChangedEventArgs> StateChanged;
		
		public NetworkService ()
		{
			this.IsConnected = true;
			try {
				BusG.Init ();
				if (Bus.System.NameHasOwner (NetworkManagerName)) {
					network = Bus.System.GetObject<INetworkManager> (NetworkManagerName, new ObjectPath (NetworkManagerPath));
					network.StateChanged += OnStateChanged;
					SetConnected ();
				}
			} catch (Exception e) {
				// if something bad happened, log the error and assume we are connected
				Log<NetworkService>.Error ("Could not initialize Network Manager dbus: {0}", e.Message);
				Log<NetworkService>.Debug (e.StackTrace);
			}
		}

		void OnStateChanged (uint state)
		{
			NetworkState newState = (NetworkState) Enum.ToObject (typeof (NetworkState), state);
			SetConnected ();
			if (StateChanged != null) {
				StateChanged (this, new NetworkStateChangedEventArgs (newState));
			}
		}
		
		void SetConnected ()
		{
			if (this.State == NetworkState.Connected)
				IsConnected = true;
			else
				IsConnected = false;
		}
		
		NetworkState State {
			get	{ 
				try {
					return (NetworkState) Enum.ToObject (typeof (NetworkState), network.Get (NetworkManagerName, "State"));
				} catch (Exception) {
					return NetworkState.Unknown;
				}
			}
		}
		
		#region INetworkService

		public bool IsConnected { get; private set; }

		#endregion
	}
}
