using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Mono.Unix;
using Gdk;
using Do.Platform.Common;

namespace Do.Platform.Linux
{
	class KeyBindingService : AbstractKeyBindingService
	{
		
		[DllImport("libdo")]
		static extern void gnomedo_keybinder_init ();

		[DllImport("libdo")]
		static extern bool gnomedo_keybinder_bind (string keystring, BindkeyHandler handler);

		[DllImport("libdo")]
		static extern bool gnomedo_keybinder_unbind (string keystring, BindkeyHandler handler);

		public delegate void BindkeyHandler (string key, IntPtr user_data);
		
		BindkeyHandler key_handler;
		
		public KeyBindingService () : base ()
		{
			key_handler = new BindkeyHandler (KeybindingPressed);
			
			try {
				gnomedo_keybinder_init ();
			} catch (DllNotFoundException) {
				Log.Error ("libdo not found - keybindings will not work.");
			}
		}
		
		void KeybindingPressed (string keystring, IntPtr user_data)	
		{
			if (Bindings.Any (k => k.KeyString == keystring)) {
				Bindings.First (k => k.KeyString == keystring).Callback (null);
			}
		}
		
		public override bool RegisterOSKey (string keyString, EventCallback cb)
		{
			if (string.IsNullOrEmpty (keyString) || cb == null)
				return false;
			return gnomedo_keybinder_bind (keyString, key_handler);
		}

		public override bool UnRegisterOSKey (string keyString)
		{
			if (Bindings.Any (k => k.KeyString == keyString))
				return gnomedo_keybinder_unbind (keyString, key_handler);
			return false;
		}
	}
}
