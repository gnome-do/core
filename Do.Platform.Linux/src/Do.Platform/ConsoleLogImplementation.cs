/* ConsoleLogImplementation.cs
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
using System.IO;
using System.Collections.Generic;

namespace Do.Platform {

	public class ConsoleLogImplementation: Common.AbstractLogImplementation
	{
		
		public override void Log (Log.Level type, string msg)
		{
			string stype  = Enum.GetName (typeof (Log.Level), type);
			string prompt = string.Format (Promptf, stype, Time);

			switch (type) {
			case Platform.Log.Level.Fatal:
				ConsoleCrayon.BackgroundColor = ConsoleColor.Red;
				ConsoleCrayon.ForegroundColor = ConsoleColor.White;
				break;
			case Platform.Log.Level.Error:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Red;
				break;
			case Platform.Log.Level.Warn:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Yellow;
				break;
			case Platform.Log.Level.Info:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Green;
				break;
			case Platform.Log.Level.Debug:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Blue;
				break;
			}
			Console.Write (prompt);
			ConsoleCrayon.ResetColor ();
			Console.Write (" ");
			Console.WriteLine (Platform.Log.AlignMessage (msg));
		}
	}
}
