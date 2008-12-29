/* Util.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Text;
using System.Runtime.InteropServices;

using Mono.Unix.Native;

namespace Do
{

	public static class Util
	{
		
		[DllImport ("libc")] // Linux
		private static extern int prctl (int option, byte [] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
		
		private static int prctl (int option, byte [] arg2)
		{
			return prctl (option, arg2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
		}

		private static int prctl (int option, string arg2)
		{
			return prctl (option, Encoding.ASCII.GetBytes (arg2 + "\0"));
		}

		[DllImport ("libc")] // BSD
		private static extern void setproctitle (byte [] fmt, byte [] name);

		private static void setproctitle (string fmt, string name)
		{
			setproctitle (Encoding.ASCII.GetBytes (fmt + "\0"), Encoding.ASCII.GetBytes (name + "\0"));
		}

		private static void setproctitle (string name)
		{
			setproctitle ("%s", name);
		}

		public static void SetProcessName (string name)
		{
			try {
				if (prctl (15 /* PR_SET_NAME */, name) != 0) {
					throw new ApplicationException ("Error setting process name: " + Stdlib.GetLastError ());
				}
			} catch (EntryPointNotFoundException) {
				setproctitle (name);
			}
		}
	}
}
