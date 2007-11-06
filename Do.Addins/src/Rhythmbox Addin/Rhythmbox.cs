//  Rhythmbox.cs
//
//  GNOME Do is the legal property of its developers, whose names are too numerous
//  to list here.  Please refer to the COPYRIGHT file distributed with this
//  source distribution.
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;

namespace RhythmboxAddin
{
	
	
	public class Rhythmbox
	{
		
		public static void StartIfNeccessary ()
		{
			if (!InstanceIsRunning)
			{
				Process.Start ("rhythmbox-client", "--no-present");
				System.Threading.Thread.Sleep (2 * 1000);
			}
		}
		
		public static bool InstanceIsRunning
		{
			get {
				Process pidof;
				
				try {
					// Use pidof command to look for Rhythmbox process. Exit
					// status is 0 if at least one matching process is found.
					// If there's any error, just assume some it's running.
					pidof = Process.Start ("pidof", "rhythmbox");
					pidof.WaitForExit ();
					return pidof.ExitCode == 0;
				} catch {
					return true;
				}
			}
		}
		
		public static void Client (string command)
		{
			Client (command, false);
		}
		
		public static void Client (string command, bool wait)
		{
			Process client;
			try {
				client = Process.Start ("rhythmbox-client", command);
				if (wait)
					client.WaitForExit ();
			} catch {
			}
		}
	}
}
