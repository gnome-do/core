// Windowing.cs created with MonoDevelop
// User: david at 5:35 PM 11/22/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Do.Platform
{
	
	
	public static class Windowing
	{
		public interface Implementation
		{
			void Summon ();
			void Vanish ();
		}
		
		static Implementation imp;
		
		public static void Initialize (Implementation anImp)
		{
			if (imp != null)
				throw new Exception ("Already has Implementation");
			if (anImp == null)
				throw new ArgumentNullException ("Implementation may not be null");
			
			imp = anImp;
		}
		
		public static void Summon ()
		{
			imp.Summon ();
		}
		
		public static void Vanish ()
		{
			imp.Vanish ();
		}
	}
}
