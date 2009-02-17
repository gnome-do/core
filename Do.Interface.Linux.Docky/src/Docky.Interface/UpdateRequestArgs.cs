// UpdateRequestArgs.cs
// 
// Copyright (C) 2008 GNOME Do
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

namespace Docky.Interface
{
	/// <summary>
	/// An update request
	/// </summary>
	public enum UpdateRequestType {
		NeedsAttentionSet,
		NeedsAttentionUnset,
		IconChanged,
		NameChanged,
	}
	
	public class UpdateRequestArgs
	{
	
		/// <value>
		/// The item requesting the update
		/// </value>
		public AbstractDockItem Item {
			get; private set;
		}
		
		/// <summary>
		/// The type of update
		/// </summary>
		public UpdateRequestType Type {
			get; private set;
		}
		
		public UpdateRequestArgs(AbstractDockItem item, UpdateRequestType type)
		{
			Item = item;
			Type = type;
		}
	}
}
