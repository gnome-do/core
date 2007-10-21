// This class was taken from Tomboy.
// http://svn.gnome.org/viewvc/tomboy/trunk/Tomboy/XKeybinder.cs?view=markup

using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mono.Unix;

using Do;

namespace Tomboy
{
	public class XKeybinder 
	{
		[DllImport("/usr/lib/tomboy/libtomboy.so")]
		static extern void tomboy_keybinder_init ();

		[DllImport("/usr/lib/tomboy/libtomboy.so")]
		static extern void tomboy_keybinder_bind (string keystring,
							  BindkeyHandler handler);

		[DllImport("/usr/lib/tomboy/libtomboy.so")]
		static extern void tomboy_keybinder_unbind (string keystring,
							    BindkeyHandler handler);

		public delegate void BindkeyHandler (string key, IntPtr user_data);

		ArrayList      bindings;
		BindkeyHandler key_handler;

		struct Binding {
			internal string       keystring;
			internal EventHandler handler;
		}

		public XKeybinder ()
			: base ()
		{
			bindings = new ArrayList ();
			key_handler = new BindkeyHandler (KeybindingPressed);
			
			try {
				tomboy_keybinder_init ();
			} catch (DllNotFoundException) {
				Log.Error ("libtomboy not found - keybindings will not work.");
			}
		}

		void KeybindingPressed (string keystring, IntPtr user_data)
		{
			foreach (Binding bind in bindings) {
				if (bind.keystring == keystring) {
					bind.handler (this, new EventArgs ());
				}
			}
		}

		public void Bind (string       keystring, 
				  EventHandler handler)
		{
			Binding bind = new Binding ();
			bind.keystring = keystring;
			bind.handler = handler;
			bindings.Add (bind);
			
			tomboy_keybinder_bind (bind.keystring, key_handler);
		}

		public void Unbind (string keystring)
		{
			foreach (Binding bind in bindings) {
				if (bind.keystring == keystring) {
					tomboy_keybinder_unbind (bind.keystring,
								 key_handler);

					bindings.Remove (bind);
					break;
				}
			}
		}

		public virtual void UnbindAll ()
		{
			foreach (Binding bind in bindings) {
				tomboy_keybinder_unbind (bind.keystring, key_handler);
			}

			bindings.Clear ();
		}
	}

	public class GConfXKeybinder : XKeybinder
	{
		GConf.Client client;
		ArrayList bindings;
		
		public GConfXKeybinder ()
		{
			client = new GConf.Client ();
			bindings = new ArrayList ();
		}

		public void Bind (string       gconf_path, 
				  string       default_binding, 
				  EventHandler handler)
		{
			try {
				Binding binding = new Binding (gconf_path, 
							       default_binding,
							       handler,
							       this);
				bindings.Add (binding);
			} catch (Exception e){
				Log.Error ("Could not add global keybinding: {0}", e.Message);
			}
		}

		public override void UnbindAll ()
		{
			try {
				bindings.Clear ();
				base.UnbindAll ();
			} catch (Exception e) {
				Log.Error ("Could not remove global keybinding: {0}", e.Message);
			}
		}

		class Binding 
		{
			public string   gconf_path;
			public string   key_sequence;
			EventHandler    handler;
			GConfXKeybinder parent;

			public Binding (string          gconf_path, 
					string          default_binding,
					EventHandler    handler,
					GConfXKeybinder parent)
			{
				this.gconf_path = gconf_path;
				this.key_sequence = default_binding;
				this.handler = handler;
				this.parent = parent;

				try {
					key_sequence = (string) parent.client.Get (gconf_path);
				} catch {
					Log.Info ("GConf key '{0}' does not exist, using default.", gconf_path);
				}

				SetBinding ();

				parent.client.AddNotify (
					gconf_path, 
					new GConf.NotifyEventHandler (BindingChanged));
			}

			void BindingChanged (object sender, GConf.NotifyEventArgs args)
			{
				if (args.Key == gconf_path) {
					Log.Info ("Binding for '{0}' changed to '{1}'!", gconf_path, args.Value);

					UnsetBinding ();

					key_sequence = (string) args.Value;
					SetBinding ();
				}
			}

			public void SetBinding ()
			{
				if (key_sequence == null || 
				    key_sequence == String.Empty || 
				    key_sequence == "disabled")
					return;

				Log.Info ("Binding key '{0}' for '{1}'",
						   key_sequence,
						   gconf_path);

				parent.Bind (key_sequence, handler);
			}

			public void UnsetBinding ()
			{
				if (key_sequence == null)
					return;

				Log.Info ("Unbinding key '{0}' for '{1}'",
						   key_sequence,
						   gconf_path);

				parent.Unbind (key_sequence);
			}
		}
	}
}
