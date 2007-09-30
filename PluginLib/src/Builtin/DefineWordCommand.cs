// GCRunCommand.cs created with MonoDevelop
// User: dave at 12:54 AM 8/18/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Text.RegularExpressions;

using Do.PluginLib;

namespace Do.PluginLib.Builtin
{
	
	public class DefineWordCommand : ICommand
	{
	
		const string wordPattern = @"^([^\W0-9_]+([ ]|[^\W0-9_])*)$";

		Regex wordRegex;
		
		public DefineWordCommand ()
		{
			wordRegex = new Regex (wordPattern, RegexOptions.Compiled);
		}
		
		public string Name {
			get { return "Define Word"; }
		}
		
		public string Description {
			get { return "Define a given word."; }
		}
		
		public string Icon {
			get { return "accessories-dictionary.png"; }
		}
		
		public Type[] SupportedTypes {
			get {
				return new Type[] {
					typeof (ITextItem),
				};
			}
		}
		
		public Type[] SupportedModifierTypes {
			get { return null; }
		}

		public bool SupportsItem (IItem item) {
			string word;

			word = null;
			if (item is ITextItem) {
				word = (item as ITextItem).Text;
			}

			if (word != null) {
				return wordRegex.IsMatch (word);
			}
			return false;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			string word, cmd;
			foreach (IItem item in items) {
				if (item is ITextItem) {
					word = (item as ITextItem).Text;
				} else {
					continue;
				}

				cmd = string.Format ("gnome-dictionary --look-up \"{0}\"", word);
				try {
					System.Diagnostics.Process.Start (cmd);
				} catch (Exception e) {
					Console.WriteLine ("Failed to define word: \"{0}\"", e.Message);
				}
			}
		}
	
		
	}
	
}
