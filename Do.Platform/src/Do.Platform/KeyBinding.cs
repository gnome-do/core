using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Do.Platform.Common;

using Gdk;

namespace Do.Platform
{
	public class KeyBinding
	{
		public DoKeyEvents EventName { get; private set; }
		public string DisplayName { get; private set; }
		public EventCallback Callback { get; private set; }
		public string KeyString { get; set; }
		public string DefaultKeyString { get; private set; }

		public KeyBinding (DoKeyEvents eventName, string displayName, string keyString, EventCallback eventFunc) {
			this.EventName = eventName;
			this.DisplayName = displayName;
			this.KeyString = keyString;
			this.DefaultKeyString = keyString;
			this.Callback = eventFunc;
		}
	}
}