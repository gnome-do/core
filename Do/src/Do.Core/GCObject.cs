/* ${FileName}
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
using System.Collections;
using System.Collections.Generic;
using Gdk;

using Do.Universe;

namespace Do.Core
{

	public abstract class GCObject : IObject
	{
		
		public static readonly string DefaultItemName = "No name";
		
		protected int _score;
		
		public abstract string Name { get; }
		
		public abstract string Description { get; }
		
		public abstract string Icon { get; }
		
		public int Score {
			get { return _score; }
			set { _score = value; }
		}
		
		public int ScoreForAbbreviation (string ab)
		{
			float similarity;
			
			if (ab == "") {
				return 100;
			} else {
				similarity = Util.StringScoreForAbbreviation (Name, ab);
			}
			return (int) (100 * similarity);
		}
		
		public override string ToString ()
		{
			return Name;
		}
		
	}
	
	public class GCObjectScoreComparer : IComparer<GCObject> {
		public int Compare (GCObject x, GCObject y) {
			float xscore, yscore;
			
			if (x == null)
				return y == null ? 0 : 1;
			else if (y == null)
				return 1;
			
			xscore = (x as GCObject).Score;
			yscore = (y as GCObject).Score;
			if (xscore == yscore)
				return 0;
			else
				return xscore > yscore ? -1 : 1;
		}

	}
}
