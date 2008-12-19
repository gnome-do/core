/* InternalItemSource.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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
using System.Collections.Generic;
using Mono.Unix;

namespace Do.Universe
{
	
	public class InternalItemSource : ItemSource
	{
		
		public override IEnumerable<Type> SupportedItemTypes {
			get { yield break; }
		}
		
		protected override string Name {
			get { return Catalog.GetString ("Internal GNOME Do Items"); }
		}
		
		protected override string Description {
			get { return Catalog.GetString ("Special items relevant to the inner-workings of GNOME Do."); }
		}
		
		protected override string Icon {
			get { return "gnome-system"; }
		}
		
		protected override IEnumerable<Item> Items {
			get {
				yield return new SelectedTextItem ();
				yield return new PreferencesItem ();
				yield return new QuitItem ();
				yield return new ClearLearningItem ();
			}
		}
			
	}
}
