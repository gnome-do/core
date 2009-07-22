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
		public KeyBinding GetBinding (DoKeyEvents eventName) {
			return null;
		}
		public KeyBinding GetBinding (string KeyString) {
			return null;
		}
		public bool SetKeyString (DoKeyEvents eventName, string keyString) {
			return false;
		}
		public IEnumerable<KeyBinding> Bindings { get { return Enumerable.Empty<KeyBinding> (); } }
		public void Initialize () {
		}
	}
}
