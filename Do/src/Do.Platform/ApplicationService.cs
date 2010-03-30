// ApplicationService.cs
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

namespace Do.Platform
{
	
	class ApplicationService : AbstractApplicationService
	{

		IEnumerable<IRunnableItem> items = new IRunnableItem [] {
			new AboutItem (),
			new PreferencesItem (),
			new DonateItem (),
			new QuitItem (),
		};

		public ApplicationService ()
		{
			Do.Controller.Summoned += (sender, e) => OnSummoned ();
		}

		public override IEnumerable<IRunnableItem> MainMenuItems {
			get { return items; }
		}

		public override Thread RunOnThread (Action action)
		{
			if (action == null) throw new ArgumentNullException ("action");

			Thread newThread = new Thread (() => {
				try {
					action ();
				} catch (ThreadAbortException) {
				} catch (Exception e) {
					Log.Error ("Error in RunOnThread: {0}", e.Message);
					Log.Debug (e.StackTrace);
				}
			});
			
			newThread.Start ();
			
			return newThread;
		}

		public override void RunOnMainThread (Action action)
		{
			if (action == null) throw new ArgumentNullException ("action");

			Gtk.Application.Invoke ((sender, e) => {
				try {
					action ();
				} catch (Exception ex) {
					Log.Error ("Error in RunOnMainThread: {0}", ex.Message);
					Log.Debug (ex.StackTrace);
				}
			});
		}

		public override void FlushMainThreadQueue ()
		{
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}

	}
}
