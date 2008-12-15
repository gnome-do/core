/* FileLogImplementation.cs
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

using Do.Platform.ServiceStack;

namespace Do.Platform.Common
{
	
	public class FileLogService : AbstractLogService, IInitializedService 
	{

		TextWriter Writer { get; set; }
			
		public void Initialize ()
		{
			DateTime now = DateTime.Now;
			string logName = string.Format ("log-{0}-{1}", now.ToShortDateString (), now.ToShortTimeString ());
			string logPath = Path.Combine (Services.Paths.TemporaryDirectory, logName);
			Writer = new StreamWriter (logPath, true);
		}
		
		public override void Log (LogLevel level, string msg)
		{
			Writer.WriteLine (FormatLogMessage (level, msg));
		}
		
	}
}
