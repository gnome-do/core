using System;
using System.Text.RegularExpressions;

namespace Do.Universe
{
	
	public class OpenURLCommand : ICommand
	{
	
		const string urlPattern = @"(^\w+:\/\/\w+)|(\w+\.\w+$)";
		
		Regex urlRegex;
		
		public OpenURLCommand ()
		{
			urlRegex = new Regex (urlPattern, RegexOptions.Compiled);
		}
		
		public string Name {
			get { return "Open URL"; }
		}
		
		public string Description {
			get { return "Opens bookmarks and manually-typed URLs."; }
		}
		
		public string Icon {
			get { return "web-browser"; }
		}
		
		public Type[] SupportedTypes {
			get {
				return new Type[] {
					typeof (IURLItem),
					typeof (ITextItem),
				};
			}
		}
		
		public Type[] SupportedModifierTypes {
			get {
				return null;
			}
		}

		public bool SupportsItem (IItem item) {
			if (item is ITextItem) {
				return urlRegex.IsMatch ((item as ITextItem).Text);
			} else if (item is IURLItem) {
				return true;
			}
			return false;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			string url;
			
			url = null;
			foreach (IItem item in items) {
				if (item is IURLItem) {
					url = (item as IURLItem).URL;
				} else if (item is ITextItem) {
					url = (item as ITextItem).Text;
				}
				
				// Use gnome-open to open the url
				if (url != null) {
					Console.WriteLine ("Opening URL \"{0}\"...", url);
					try {
						System.Diagnostics.Process.Start ("gnome-open", string.Format ("\"{0}\"", url));
					} catch (Exception e) {
						Console.WriteLine ("Failed to open \"{0}\": ", e.Message);
					}
				}
			}
		}
		
	}
}
