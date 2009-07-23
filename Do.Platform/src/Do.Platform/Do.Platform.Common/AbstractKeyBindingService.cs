using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Do.Platform.Common;

namespace Do.Platform.Common
{
	public abstract class AbstractKeyBindingService : IKeyBindingService
	{

		public abstract bool RegisterOSKey (string keyString, EventCallback cb);
		public abstract bool UnRegisterOSKey (string keyString);

		IPreferences prefs;

		#region IInitializedService

		public void Initialize () {
			Bindings = new Dictionary<string, KeyBinding> ();
			
			prefs = Services.Preferences.Get<AbstractKeyBindingService> ();
		}

		#endregion

		#region IKeyBindingService

		// keyString -> KeyEvent
		public Dictionary<string, KeyBinding> Bindings { get; private set; }

		public bool RegisterKeyBinding (KeyBinding binding) {
			bool success = true;
			
			//first check if this event is already mapped
			if (Bindings.ContainsKey (binding.KeyString)) {
				Log<AbstractKeyBindingService>.Error ("'{0}' is already mapped", binding.KeyString);
				return false;
			}

			//try to get the keystring from the prefs.  We default to the KeyBinding.KeyString, so we can later check
			//if the prefs value matches that, we're using the default, otherwise we're using a user specified value
			string prefsKeyString = prefs.Get (binding.Description.Replace (' ', '_'), binding.KeyString);
			//if these values don't match then the user has specified a new keystring
			//update the KeyEvent then continue
			if (prefsKeyString != binding.KeyString)
				binding.KeyString = prefsKeyString;

			//if we are registering a key with the OS, do something special
			if (binding.IsOSKey) {
				//try to register the key from the prefs with the OS
				success = RegisterOSKey (binding.KeyString, binding.Callback);
				//if we fail to register the summon key, try again with the default binding
				if (!success && RegisterOSKey (binding.DefaultKeyString, binding.Callback)) {
					//if we succeeded now, change the event's keystring
					binding.KeyString = binding.DefaultKeyString;
					success = true;
				}
			}

			if (success) {
				//add the event to our mapped events dict
				Bindings.Add (binding.KeyString, binding);
				//set the bound keystring in the prefs
				SetKeyString (binding, binding.KeyString);
			}

			return success;
		}

		public bool SetKeyString (KeyBinding binding, string newKeyString) {
			bool success = true;
						
			//if this key should be registered with the OS
			if (binding.IsOSKey) {
				//remove the old keystring from the OS
				UnRegisterOSKey (binding.KeyString);
				//register again with the new keystring
				success = RegisterOSKey (newKeyString, binding.Callback);
			}

			if (success) {				
				//first remove the old binding
				Bindings.Remove (binding.KeyString);
				//next set the new keystring
				binding.KeyString = newKeyString;
				//now add it back to the dict of bindings
				Bindings.Add (binding.KeyString, binding);
				
				//save the new value in the prefs
				prefs.Set (binding.Description.Replace (' ', '_'), binding.KeyString);

				Log<AbstractKeyBindingService>.Debug ("Event '{0}' now mapped to: {1}", binding.Description, binding.KeyString);
			}

			return success;
		}

		#endregion
	}
}