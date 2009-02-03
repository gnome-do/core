// AbstractLogService.cs
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
using System.Collections.Generic;

using Do.Platform;

namespace Do.Platform.Common {

	public abstract class AbstractLogService : ILogService {
		
		/// <value>
		/// A string to make printing the current time simpler
		/// </value>
		protected const string TimeFormat = "{0:00}:{1:00}:{2:00}.{3:000}";
		
		/// <value>
		/// A consistent way of printing [Time LogLevel]
		/// </value>
		protected const string LogFormat = "[{0,-5} {1}]";
		
		/// <value>
		/// the current time using the TimeFormat format.
		/// </value>
		protected string Time {
			get {
				DateTime now = DateTime.Now;
				return string.Format (TimeFormat, now.Hour, now.Minute, now.Second, now.Millisecond);
			}
		}

		protected string FormatLogPrompt (LogLevel level)
		{
			string levelString = Enum.GetName (typeof (LogLevel), level);
			return string.Format (LogFormat, levelString, Time);
		}
		
		protected string FormatLogMessage (LogLevel level, string message)
		{
			return string.Format ("{0} {1}", FormatLogPrompt (level), message);
		}

		abstract public void Log (LogLevel level, string msg);
		
	}	
}
