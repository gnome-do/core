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

		class Binding {
			public string KeyString;
			public EventCallback Handler;
			public Binding (string keyString, EventCallback handler) 
			{
				this.KeyString = keyString;
				this.Handler = handler;
			}
		}
		
		[DllImport("libdo")]
		static extern void gnomedo_keybinder_init ();

		[DllImport("libdo")]
		static extern void gnomedo_keybinder_bind (string keystring, BindkeyHandler handler);

		[DllImport("libdo")]
		static extern void gnomedo_keybinder_unbind (string keystring, BindkeyHandler handler);

		public delegate void BindkeyHandler (string key, IntPtr user_data);
		
		BindkeyHandler key_handler;
		Dictionary <string, Binding> BoundKeys;
		
		public KeyBindingService () : base ()
		{
			key_handler = new BindkeyHandler (KeybindingPressed);
			BoundKeys = new Dictionary <string, Binding> ();
			
			try {
				Console.WriteLine ("INITIALIZING HERE!!!!!");
				gnomedo_keybinder_init ();
			} catch (DllNotFoundException) {
				Log.Error ("libdo not found - keybindings will not work.");
			}
		}
		
		void KeybindingPressed (string keystring, IntPtr user_data)	{
			//Console.WriteLine ("{0} was pressed.", keystring);
			if (BoundKeys.ContainsKey (keystring)) {
				BoundKeys.Values.First (b => b.KeyString == keystring).Handler (null);
			}
		}
		
		public override bool RegisterSummonKey (string keyString, EventCallback cb) {

			Binding bind = new Binding (keyString, cb);
			BoundKeys.Add (keyString, bind);
			
			Console.WriteLine ("TRYING TO BIND HERE!!!!");
			//gnomedo_keybinder_bind (bind.KeyString, key_handler);
			
			//libdo apparently provides no way to tell if the binding was successful, assume it was			
			return true; 
		}

		public override bool UnRegisterSummonKey (string keyString) {
			
			if (BoundKeys.ContainsKey (keyString)) {
				//gnomedo_keybinder_unbind (BoundKeys [keyString].KeyString, key_handler);

				BoundKeys.Remove (keyString);
			}
				
			return true;
		}
	}
}