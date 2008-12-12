/* StalledActionNotification.cs
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

using Mono.Unix;

using Do.Platform;

namespace Do.Core
{
	
	
	class StalledActionNotification : Notification
	{
		const string Icon = "dialog-error";

		static readonly string Title = Catalog.GetString ("GNOME Do");
		static readonly string Body = Catalog.GetString (
			"Do is still performing the last action. Please wait for it to finish or click \"End Now\" to interrupt.");
		static readonly string ActionLabel = Catalog.GetString ("End Now");
		
		public StalledActionNotification ()
			: base (Title, Body, Icon, ActionLabel, Action)
		{
		}

		static void Action ()
		{
			Environment.Exit (20);
		}
	}
}
