// SearchContext.cs created with MonoDevelop
// User: dave at 11:11 PM 8/30/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Do.Core
{
	public enum ContextRelation {
		Fresh,
		Repeat,
		Continuation
	}
	
	public class SearchContext
	{
		Item item, iitem;
		Command command;
		string itemSearchString, indirectItemSearchString, commandSearchString;
		
		GCObject [] results;
				
		public SearchContext ()
		{
		}
		
		public Item Item {
			get {
				return item;
			}
			set {
				item = value;
			}
		}

		public Item IndirectItem {
			get {
				return iitem;
			}
			set {
				iitem = value;
			}
		}

		public Command Command {
			get {
				return command;
			}
			set {
				command = value;
			}
		}

		public string ItemSearchString {
			get {
				return itemSearchString;
			}
			set {
				itemSearchString = value;
			}
		}

		public string IndirectItemSearchString {
			get {
				return indirectItemSearchString;
			}
			set {
				indirectItemSearchString = value;
			}
		}

		public string CommandSearchString {
			get {
				return commandSearchString;
			}
			set {
				commandSearchString = value;
			}
		}

		public GCObject[] Results {
			get {
				return results;
			}
			set {
				results = value;
			}
		}
	}
}
