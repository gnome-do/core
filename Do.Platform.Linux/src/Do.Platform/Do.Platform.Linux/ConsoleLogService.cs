// ConsoleLogService.cs
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
using System.IO;
using System.Collections.Generic;

using Do.Platform;

namespace Do.Platform.Linux {

	public class ConsoleLogService: Common.AbstractLogService
	{
		
		public override void Log (LogLevel level, string message)
		{
			switch (level) {
			case LogLevel.Fatal:
				ConsoleCrayon.BackgroundColor = ConsoleColor.Red;
				ConsoleCrayon.ForegroundColor = ConsoleColor.White;
				break;
			case LogLevel.Error:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Red;
				break;
			case LogLevel.Warn:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Yellow;
				break;
			case LogLevel.Info:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Blue;
				break;
			case LogLevel.Debug:
				ConsoleCrayon.ForegroundColor = ConsoleColor.Green;
				break;
			}
			Console.Write (FormatLogPrompt (level));
			ConsoleCrayon.ResetColor ();
			
			Console.Write (" ");
			Console.WriteLine (message);
		}
		
	}
}
