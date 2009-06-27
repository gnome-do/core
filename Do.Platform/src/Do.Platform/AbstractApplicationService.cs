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
using System.Threading;
using System.Collections.Generic;

using Do.Universe;
using Do.Platform.ServiceStack;

namespace Do.Platform
{
	
	public abstract class AbstractApplicationService : IService
	{
		//// <value>
		/// This event raises on the main application thread when
		/// the launcher is summoned.
		/// </value>
		public event EventHandler Summoned;

		//// <value>
		/// A list of main menu items, in the preferred presentation order.
		/// </value>
		public abstract IEnumerable<IRunnableItem> MainMenuItems { get; }

		/// <summary>
		/// Run an action on a worker thread.
		/// </summary>
		/// <param name="action">
		/// A <see cref="Action"/> to run on a worker thread.
		/// </param>
		/// <returns>
		/// A reference to the created <see cref="Thread"/>.
		/// </returns>
		public abstract Thread RunOnThread (Action action);

		/// <summary>
		/// Run an action on a worker thread after a delay (ms).
		/// </summary>
		/// <param name="action">
		/// A <see cref="Action"/> to run on a worker thread.
		/// </param>
		/// <param name="delay">
		/// A <see cref="System.Int32"/> delay (in millseconds)
		/// to wait before running the action.
		/// </param>
		/// <returns>
		/// A reference to the created <see cref="Thread"/>.
		/// </returns>
		public Thread RunOnThread (Action action, int delay)
		{
			return RunOnThread (action, new TimeSpan (0, 0, 0, 0, delay));
		}

		/// <summary>
		/// Run an action on a worker thread after a delay. 
		/// </summary>
		/// <param name="action">
		/// A <see cref="Action"/> to run on a worker thread.
		/// </param>
		/// <param name="delay">
		/// A <see cref="TimeSpan"/> delay to wait before running
		/// the action.
		/// </param>
		/// <returns>
		/// A reference to the created <see cref="Thread"/>.
		/// </returns>
		public Thread RunOnThread (Action action, TimeSpan delay)
		{
			return RunOnThread (() => {
				Thread.Sleep (delay);
				action ();
			});
		}

		/// <summary>
		/// Run an action on the main application (GUI) thread.
		/// </summary>
		/// <param name="action">
		/// A <see cref="Action"/> to run on the main thread.
		/// </param>
		public abstract void RunOnMainThread (Action action);

		/// <summary>
		/// Run an action on the main thread after a delay (ms).
		/// </summary>
		/// <param name="action">
		/// A <see cref="Action"/> to run on the main thread.
		/// </param>
		/// <param name="delay">
		/// A <see cref="System.Int32"/> delay (in millseconds)
		/// to wait before running the action.
		/// </param>
		public void RunOnMainThread (Action action, int delay)
		{
			RunOnMainThread (action, new TimeSpan (0, 0, 0, 0, delay));
		}

		/// <summary>
		/// Run an action on the main thread after a delay. 
		/// </summary>
		/// <param name="action">
		/// A <see cref="Action"/> to run on the main thread.
		/// </param>
		/// <param name="delay">
		/// A <see cref="TimeSpan"/> delay to wait before running
		/// the action.
		/// </param>
		public void RunOnMainThread (Action action, TimeSpan delay)
		{
			RunOnThread (() => RunOnMainThread (action), delay);
		}

		/// <summary>
		/// Pumps the main event queue until it is empty.
		/// </summary>
		/// <remarks>
		/// It is only safe to call this from the main application thread.
		/// </remarks>
		public abstract void FlushMainThreadQueue ();

		/// <summary>
		/// Raises the Summoned event.
		/// </summary>
		protected void OnSummoned ()
		{
			if (Summoned != null)
				Summoned (this, EventArgs.Empty);
		}
	}
}
