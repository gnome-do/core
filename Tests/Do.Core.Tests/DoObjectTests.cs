// DoObjectTests.cs created with MonoDevelop
// User: david at 10:37 AM 11/22/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using NUnit.Framework;

using Do.Universe;

namespace Do.Core
{
	
	
	[TestFixture()]
	public class DoObjectTests
	{

		class SimpleObject : Element
		{
			public string Name { get; set; }
			public string Description { get; set; }
			public string Icon { get; set; }
		}
		
		[Test()]
		public void Unwrap_Identity ()
		{
			Element iob = new SimpleObject ();
			Assert.AreSame (iob, DoObject.Unwrap (iob));
		}

		[Test()]
		public void Unwrap_Basic ()
		{
			Element iob = new SimpleObject ();
			Element dob = new DoObject (iob);
			Assert.AreSame (iob, DoObject.Unwrap (dob));
		}

		[Test()]
		public void Unwrap_Recursive ()
		{
			Element iob = new SimpleObject ();
			Element dob = new DoObject (new DoObject (iob));
			Assert.AreSame (iob, DoObject.Unwrap (dob));
		}

		[Test()]
		public void Wrap_Identity ()
		{
			Element dob = new DoObject (new SimpleObject ());
			Assert.AreSame (dob, DoObject.Wrap (dob));
		}

		[Test()]
		public void Wrap_Basic ()
		{
			Element iob = new SimpleObject ();
			Type wrapperT = DoObject.Wrap (iob).GetType ();
			Assert.IsTrue (typeof (DoObject).IsAssignableFrom (wrapperT));
		}
		
	}
}
