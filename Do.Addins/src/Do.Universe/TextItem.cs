using System;
using System.Collections.Generic;

namespace Do.Universe
{

	public class TextItem : ITextItem
	{
		
		protected string text;
		
		public TextItem (string text)
		{
			this.text = text;
		}
		
		public string Name {
			get { return text; }
		}
		
		public string Description {
			get { return "Raw input text"; }
		}
		
		public string Icon {
			get { return "gnome-mime-text"; }
		}
		
		public string Text {
			get { return text; }
			set { text = value; }
		}

	}
}
