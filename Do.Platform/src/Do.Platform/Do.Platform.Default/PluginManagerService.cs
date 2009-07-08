// PluginManagerService.cs
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

using System;
using System.Collections.Generic;

using Mono.Addins;

using Do.Platform.ServiceStack;

namespace Do.Platform.Default
{
	/// <summary>
	/// If this class loads, we have a serious plugin because that probably means we have no plugin manager.
	/// </summary>	
	public class PluginManagerService : IPluginManagerService
	{
		public void Install (Addin addin)
		{
			Log<PluginManagerService>.Error ("Using default service, could not install addin.");
			return;
		}
		
		public IEnumerable<Addin> GetAddins ()
		{
			Log<PluginManagerService>.Error ("Using default service, could not locate any addins");
			yield break;
		}
	}
}
