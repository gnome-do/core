/* ${FileName}
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

using Do.Core;
using Do.DBusLib;

namespace Do
{
	
	public class Do {
		
		static Commander commander;
	
		public static void Main (string[] args) {
			
			Log.Initialize ();

			Gtk.Application.Init ();
						
			commander = DBusRegistrar.GetCommanderInstance () as Commander;
			if (commander != null) {
				commander.Show ();
				System.Environment.Exit (0);
			}
			
			Util.Initialize ();
			
			commander = DBusRegistrar.RegisterCommander (new DefaultCommander ()) as Commander;
			commander.Show ();
			
			Gtk.Application.Run ();
		}	
		
		public static Commander Commander {
			get { return commander; }
		}
	}
}
