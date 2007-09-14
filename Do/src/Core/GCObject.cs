// GCObject.cs created with MonoDevelop
// User: dave at 1:45 PM 8/25/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections;
using System.Collections.Generic;
using Gdk;

namespace Do.Core
{

	public abstract class GCObject
	{
		
		public const int DefaultIconSize = 48;
		public static readonly string DefaultItemName = "No name";
		
		protected Pixbuf _pixbuf;
		protected int _score;
		
		public abstract string Name { get; }
		
		public abstract string Icon { get; }
		
		public int Score {
			get { return _score; }
			set { _score = value; }
		}
		
		public virtual Pixbuf Pixbuf {
			get {
				if (_pixbuf == null) {
					_pixbuf = Util.PixbufFromIconName (Icon, DefaultIconSize);
				}
				return _pixbuf;
			}
			set { _pixbuf = value; }
		}
		
		public int ScoreForAbbreviation (string ab)
		{
			float similarity;
			
			if (ab == "") {
				return int.MaxValue;
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
