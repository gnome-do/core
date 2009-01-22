/* NullApplicationItem.cs
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

using Mono.Unix;

using Do.Universe;
using Do.Platform;

namespace Do.Universe.Linux
{

	class NullApplicationItem : Item, IApplicationItem
	{

		string ApplicationPath { get; set; }
		
		public NullApplicationItem (string applicationPath)
		{
			ApplicationPath = applicationPath ?? "";
		}
		
		public override string Name {
			get { return Path.GetFileName (ApplicationPath); }
		}

		public override string Description {
			get {
				string warning = Catalog.GetString ("Error reading {0}.");
				return string.Format (warning, ApplicationPath);
			}
		}
		
		public override string Icon {
			get { return "applications-other"; }
		}
		
		public string Exec {
			get { return ""; }
		}

		public void Run ()
		{
			Log<NullApplicationItem>.Warn ("Cannot Run because {0} contains errors.", ApplicationPath);
		}

		public void LaunchWithFiles (IEnumerable<IFileItem> files)
		{
			Log<NullApplicationItem>.Warn ("Cannot LaunchWithFiles because {0} contains errors.", ApplicationPath);
		}
	}
}
