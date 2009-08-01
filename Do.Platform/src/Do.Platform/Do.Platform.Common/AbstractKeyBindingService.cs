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
			Bindings = new List<KeyBinding> ();
			
			prefs = Services.Preferences.Get<AbstractKeyBindingService> ();
		}

		#endregion

		#region IKeyBindingService

		public List<KeyBinding> Bindings { get; private set; }

		public bool RegisterKeyBinding (KeyBinding binding) {
			bool success = true;
			
			//first check if this keystring is already used
			if (Bindings.Any (k => k.KeyString == binding.KeyString)) {
				Log<AbstractKeyBindingService>.Error ("Key '{0}' is already mapped.", binding.KeyString);
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
				if (!string.IsNullOrEmpty (binding.KeyString))
					success = RegisterOSKey (binding.KeyString, binding.Callback);
				//if we fail to register the summon key, try again with the default binding
				if (!success && !string.IsNullOrEmpty (binding.DefaultKeyString)) {
				    success = RegisterOSKey (binding.DefaultKeyString, binding.Callback);
					//if we succeeded now, change the event's keystring
					if (success)
						binding.KeyString = binding.DefaultKeyString;
				}
			}

			if (success) {
				//add the event to the list of bindings
				Bindings.Add (binding);
				//set the bound keystring in the prefs
				prefs.Set (binding.Description.Replace (' ', '_'), binding.KeyString);
			}

			return success;
		}

		public bool SetKeyString (KeyBinding binding, string newKeyString) {
			bool success = true;
			
			//first check if this keystring exists
			if (!Bindings.Any (k => k.KeyString == binding.KeyString)) {
				Log<AbstractKeyBindingService>.Error ("Key '{0}' is not mapped.", binding.KeyString);
				return false;
			}
						
			//if this key should be registered with the OS
			if (binding.IsOSKey) {
				//remove the old keystring from the OS
				if (!string.IsNullOrEmpty (binding.KeyString))
					UnRegisterOSKey (binding.KeyString);
				//register again with the new keystring
				if (!string.IsNullOrEmpty (newKeyString))
					success = RegisterOSKey (newKeyString, binding.Callback);
			}

			if (success) {
				//set the new keystring
				Bindings.First (k => k.KeyString == binding.KeyString).KeyString = newKeyString;
				
				//save the new value in the prefs
				prefs.Set (binding.Description.Replace (' ', '_'), binding.KeyString);

				Log<AbstractKeyBindingService>.Debug ("\"{0}\" now mapped to '{1}'", binding.Description, binding.KeyString);
			}

			return success;
		}

		#endregion
	}
}