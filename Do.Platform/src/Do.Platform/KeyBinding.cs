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
		public string Description { get; private set; }
		public EventCallback Callback { get; private set; }
		public string KeyString { get; set; }
		public string DefaultKeyString { get; private set; }
		public bool IsOSKey { get; private set; }

		public KeyBinding (string description, string keyString, EventCallback eventFunc) : this (description, keyString, eventFunc, false)
		{
		}
		
		public KeyBinding (string description, string keyString, EventCallback eventFunc, bool isoskey)
		{
			this.Description = description;
			this.KeyString = keyString;
			this.DefaultKeyString = keyString;
			this.Callback = eventFunc;
			this.IsOSKey = isoskey;
		}
	}
}