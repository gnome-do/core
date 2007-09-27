// LBFrame.cs created with MonoDevelop
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
	
	public class LBDisplayText : Label
	{
		
		const string displayFormat = " <big>{0}</big> \n {1} ";
		
		string highlight;
		string name, description;
		
		public LBDisplayText () : base ()
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
				SetDisplayText (name, description);
			}
		}
		
		public void SetDisplayText (string name, string description) {
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
			Markup = string.Format (displayFormat, UnderlineStringWithString (name, highlight), description);
		}
		
		public string UnderlineStringWithString (string main, string underline) {
			int pos, len, match_pos, last_main_cut;
			string lower_main, lower_underline, result;
			
			result = "";
			match_pos = last_main_cut = 0;
			lower_main = main.ToLower ();
			lower_underline = underline.ToLower ();
			
			for (pos = 0; pos < underline.Length; ++pos) {
				for (len = 1; len < underline.Length - pos; ++len) {
					int tmp_match_pos = lower_main.IndexOf (lower_underline.Substring (pos, len));
					if (tmp_match_pos < 0) {
						--len;
						break;
					} else {
						match_pos = tmp_match_pos;
					}
				}
				if (0 < len) {
					// Theres a match starting at match_pos with positive length
					string skipped = main.Substring (last_main_cut, match_pos - last_main_cut);
					string matched = main.Substring (match_pos, len);
					string remainder = UnderlineStringWithString (main.Substring (match_pos + len), underline.Substring (pos + len));
					result = string.Format ("{0}<u>{1}</u>{2}", skipped, matched, remainder);
					break;
				}
			}
			if (result == "") {
				// no matches
				result = main;
			}
			return result;
		}
	}
}
