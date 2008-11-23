// RelevanceProviderTests.cs
// 
// Copyright (C) 2008 GNOME Do
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

using Do;
using Do.Universe;

using NUnit.Framework;

namespace Do.Core
{
	
	[TestFixture ()]
	public class RelevanceProviderTests
	{
		
		static IRelevanceProvider provider = RelevanceProvider.DefaultProvider;
		
		[Test ()]
		public void TestZeroMatchFilter ()
		{
			string filter = "abc";
			
			//less than 0
			DoObject obj1 = new DoObject (new TextItem ("def"));
			DoObject obj2 = new DoObject (new TextItem ("ghe"));
			DoObject obj3 = new DoObject (new TextItem ("foo"));
			
			//greater than 0
			DoObject obj4 = new DoObject (new TextItem ("abc"));
		
			//why the HELL wont this compile the right way?
			// obj1.UpdateRelevance (filter, null) == compile error!  wtf???
			DoObject_RelevanceProvider.UpdateRelevance (obj1, filter, null);
			DoObject_RelevanceProvider.UpdateRelevance (obj2, filter, null);
			DoObject_RelevanceProvider.UpdateRelevance (obj3, filter, null);
			DoObject_RelevanceProvider.UpdateRelevance (obj4, filter, null);
			
			
			Assert.AreEqual (0, obj1.Relevance, 0);
			Assert.AreEqual (0, obj2.Relevance, 0);
			Assert.AreEqual (0, obj3.Relevance, 0);
			Assert.AreNotEqual (0, obj4.Relevance);
		}
		
		[Test ()]
		public void TestEmptyStringMatch ()
		{
			string filter = "";
			
			DoObject obj = new DoObject (new TextItem ("Test Item"));
			
			try {
				DoObject_RelevanceProvider.UpdateRelevance (obj, filter, null);
			} catch {
				Assert.Fail ("Crashed getting relevance for empty string");
			}
			
			Assert.AreNotEqual (obj.Relevance, 0);
		}
		
		[Test ()]
		public void TestIncreaseRelevance ()
		{
			DoObject obj = new DoObject (new TextItem ("Unique Test Item 1"));
			string filter = "test";
			
			DoObject_RelevanceProvider.UpdateRelevance (obj, filter, null);
			float compare_score = obj.Relevance;
			
			DoObject_RelevanceProvider.IncreaseRelevance (obj, filter, null);
			DoObject_RelevanceProvider.UpdateRelevance (obj, filter, null);
			Assert.IsTrue ( obj.Relevance > compare_score, "Failed to increase relevance");
			
			DoObject_RelevanceProvider.DecreaseRelevance (obj, filter, null);
			DoObject_RelevanceProvider.UpdateRelevance (obj, filter, null);
			compare_score = obj.Relevance;
			
			DoObject_RelevanceProvider.IncreaseRelevance (obj, filter, null);
			DoObject_RelevanceProvider.UpdateRelevance (obj, filter, null);
			Assert.IsTrue ( obj.Relevance > compare_score, "Failed to increase relevance after a decrease");
		}
		
		[Test ()]
		public void TestDecreaseRelevance ()
		{
			DoObject obj = new DoObject (new TextItem ("Unique Test Item 2"));
			string filter = "test";
			
			DoObject_RelevanceProvider.UpdateRelevance (obj, filter, null);
			float compare_score = obj.Relevance;
			
			DoObject_RelevanceProvider.DecreaseRelevance (obj, filter, null);
			DoObject_RelevanceProvider.UpdateRelevance (obj, filter, null);
			Assert.IsTrue ( obj.Relevance < compare_score, "Failed to decrease relevance");
		}
		
		[Test ()]
		public void TestAccronymRelevance ()
		{
			DoObject obj1 = new DoObject (new TextItem ("Accronym Item One"));
			DoObject obj2 = new DoObject (new TextItem ("Accronym Item Two"));
			
			string filter = "aio";
			float rel1 = provider.GetRelevance (obj1, filter, null);
			float rel2 = provider.GetRelevance (obj2, filter, null);
			
			Assert.IsTrue (rel1 > rel2);
		}
		
		[Test ()]
		public void TestItemLengthRelevance ()
		{
			DoObject obj1 = new DoObject (new TextItem ("Accronym Item Onez"));
			DoObject obj2 = new DoObject (new TextItem ("Accronym Item Twooz"));
			DoObject obj3 = new DoObject (new TextItem ("Accronym Item Threez"));
			DoObject obj4 = new DoObject (new TextItem ("Accronym Item Fourrrz"));
			
			string filter = "az";
			float rel1 = provider.GetRelevance (obj1, filter, null);
			float rel2 = provider.GetRelevance (obj2, filter, null);
			float rel3 = provider.GetRelevance (obj3, filter, null);
			float rel4 = provider.GetRelevance (obj4, filter, null);
			
			Assert.IsTrue (rel1 > rel2 && rel2 > rel3 && rel3 > rel4);
			
			filter = "a";
			rel1 = provider.GetRelevance (obj1, filter, null);
			rel2 = provider.GetRelevance (obj2, filter, null);
			rel3 = provider.GetRelevance (obj3, filter, null);
			rel4 = provider.GetRelevance (obj4, filter, null);
			
			Assert.IsTrue (rel1 == rel2 && rel2 == rel3 && rel3 == rel4);
		}
	}
}
