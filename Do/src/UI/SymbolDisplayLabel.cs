// RoundedFrame.cs created with MonoDevelop
// User: dave at 11:15 AM 8/25/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using Gdk;

using Do.Core;
using Do.PluginLib;

namespace Do.UI
{
	
	public class SymbolDisplayLabel : Label
	{
		
		// const string displayFormat = " <big>{0}</big> \n {1} ";
		
		// Description only:
		const string displayFormat = "<span size=\"medium\"> {1} </span>";
		
		string highlight;
		string name, description;
		
		public SymbolDisplayLabel () : base ()
		{		
			Build ();
			highlight = name = description = "";
		}
		
		void Build ()
		{
			UseMarkup = true;
			Ellipsize = Pango.EllipsizeMode.End;
			Justify = Justification.Center;
			ModifyFg (StateType.Normal, Style.White);
		}
		
		public IObject DisplayObject {
			set {
				IObject displayObject;
				
				displayObject = value;
				name = description = highlight = "";
				if (displayObject != null) {
					name = displayObject.Name;
					description = displayObject.Description;
				}
				SetdisplayLabel (name, description);
			}
		}
		
		public void SetdisplayLabel (string name, string description) {
			this.name = (name == null ? "" : name);
			this.description = (description == null ? "" : description);
			highlight = "";
			UpdateText ();
		}
		
		public string Highlight {
			get { return highlight; }
			set {
				highlight = (value == null ? "" : value);
				UpdateText ();
			}
		}
		
		void UpdateText ()
		{
			string highlighted, safe_name, safe_description;

			safe_name = Util.Appearance.MarkupSafeString (name);
			safe_description = Util.Appearance.MarkupSafeString (description);
			highlighted = Util.FormatCommonSubstrings(safe_name,
																								highlight,
																								"<u>{0}</u>");
			Markup = string.Format (displayFormat, highlighted, safe_description);
		}
		
	}
}
