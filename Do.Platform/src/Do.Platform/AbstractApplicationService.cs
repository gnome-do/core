// AbstractApplicationService.cs
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

using Do.Universe;
using Do.Platform.ServiceStack;

namespace Do.Platform
{
	
	public abstract class AbstractApplicationService : IService
	{
		public event EventHandler Summoned;

		public abstract IEnumerable<IRunnableItem> MainMenuItems { get; }

		public void RunOnThread (Action action)
		{
			RunOnThread (action, 0);
		}

		public void RunOnThread (Action action, int delay)
		{
			RunOnThread (action, new TimeSpan (0, 0, 0, 0, delay));
		}

		public abstract void RunOnThread (Action action, TimeSpan delay);

		public void RunOnMainThread (Action action, int delay)
		{
			RunOnMainThread (action, new TimeSpan (0, 0, 0, 0, delay));
		}

		public void RunOnMainThread (Action action, TimeSpan delay)
		{
			RunOnThread (() => RunOnMainThread (action), delay);
		}

		public abstract void RunOnMainThread (Action action);

		public abstract void FlushMainThreadQueue ();

		protected void OnSummoned ()
		{
			if (Summoned != null)
				Summoned (this, EventArgs.Empty);
		}
	}
}
