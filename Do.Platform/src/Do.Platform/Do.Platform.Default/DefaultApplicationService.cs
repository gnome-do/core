// DefaultApplicationService.cs
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
using System.Threading;

using Do.Universe;

namespace Do.Platform
{
	
	class DefaultApplicationService : AbstractApplicationService
	{
		public override IEnumerable<IRunnableItem> MainMenuItems {
			get {
				Log<DefaultApplicationService>.Debug ("Cannot provide MainMenuItems.");
				yield break;
			}
		}

		public override Thread RunOnThread (Action action)
		{
			Log<DefaultApplicationService>.Debug ("Cannot run action on a thread.");
			action ();
			return null;
		}

		public override void RunOnMainThread (Action action)
		{
			Log<DefaultApplicationService>.Debug ("Cannot run action on the main thread.");
			action ();
		}

		public override void FlushMainThreadQueue ()
		{
			Log<DefaultApplicationService>.Debug ("Cannot flush main thread queue.");
		}
	}
}
