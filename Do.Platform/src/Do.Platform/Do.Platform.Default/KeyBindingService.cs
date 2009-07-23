using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Do.Platform.Common;

namespace Do.Platform.Default
{
	class KeyBindingService : IKeyBindingService
	{
		public bool RegisterKeyBinding (KeyBinding evnt) {
			Log.Error ("Default keybinding service cannot register key events. {0}", evnt.KeyString);
			return false;
		}
		public bool SetKeyString (KeyBinding binding, string keyString) {
			return false;
		}
		public Dictionary<string, KeyBinding> Bindings { get { return null; } }
		public void Initialize () {
		}
	}
}
