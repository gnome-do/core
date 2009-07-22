using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Do.Platform.Common;

namespace Do.Platform.Common
{
	public abstract class AbstractKeyBindingService : IKeyBindingService
	{

		public abstract bool RegisterSummonKey (string keyString, EventCallback cb);
		public abstract bool UnRegisterSummonKey (string keyString);

		IPreferences prefs;

		// Platform.Common.DoKeyEvents -> KeyEvent
		Dictionary<DoKeyEvents, KeyBinding> KeyEvents;

		#region IInitializedService

		//internal Do keybinding code here...
		public void Initialize () {
			KeyEvents = new Dictionary<DoKeyEvents, KeyBinding> ();
			
			prefs = Services.Preferences.Get<AbstractKeyBindingService> ();
		}

		#endregion

		#region IKeyBindingService

		public bool RegisterKeyBinding (KeyBinding binding) {
			bool success = true;
			//first check if this event is already mapped
			if (GetBinding (binding.EventName) != null) {
				Log<AbstractKeyBindingService>.Error ("{0} is already mapped.", binding.EventName);
				return false;
			}

			//try to get the keystring from the prefs.  We default to the doEvent.KeyString, so we can later check
			//if the prefs value matches that, we're using the default, otherwise we're using a user specified value
			string prefsKeyString = prefs.Get (binding.EventName.ToString (), binding.KeyString);
			//if these values don't match then the user has specified a new keystring
			//update the KeyEvent then continue
			if (prefsKeyString != binding.KeyString)
				binding.KeyString = prefsKeyString;

			//if we are registering the summon key, do something special
			if (binding.EventName == DoKeyEvents.Summon) {
				//try to register the key from the prefs with the OS
				success = RegisterSummonKey (binding.KeyString, binding.Callback);
				//if we fail to register the summon key, try again with the default binding
				if (!success && RegisterSummonKey (binding.DefaultKeyString, binding.Callback)) {
					//if we succeeded now, change the event's keystring
					binding.KeyString = binding.DefaultKeyString;
					success = true;
				}
			}

			if (success) {
				//add the event to our mapped events dict
				KeyEvents.Add (binding.EventName, binding);
				//set the bound keystring in the prefs
				SetKeyString (binding.EventName, binding.KeyString);
			}

			return success;
		}

		public KeyBinding GetBinding (DoKeyEvents eventName) {
			if (KeyEvents.ContainsKey (eventName))
				return KeyEvents [eventName];
			return null;
		}

		public KeyBinding GetBinding (string keyString) {
			if (KeyEvents.Values.Where (ev => ev.KeyString == keyString).Any ())
				return KeyEvents.Values.First (ev => ev.KeyString == keyString);
			return null;
		}

		public bool SetKeyString (DoKeyEvents eventName, string keyString) {
			bool success = true;
			if (GetBinding (eventName) == null) {
				Log<AbstractKeyBindingService>.Error ("{0} is not mapped.", eventName);
				return false;
			}

			//if it's the summon key, reregister it with the OS
			if (eventName == DoKeyEvents.Summon) {
				//remove the old keystring from the OS
				UnRegisterSummonKey (KeyEvents [eventName].KeyString);
				//register again with the new keystring
				success = RegisterSummonKey (keyString, KeyEvents [eventName].Callback);
			}

			if (success) {
				//change the keystring in the dict of mapped events
				KeyEvents [eventName].KeyString = keyString;

				//save the new value in the prefs
				prefs.Set (KeyEvents [eventName].EventName.ToString (), KeyEvents [eventName].KeyString);

				Log<AbstractKeyBindingService>.Debug ("Event {0} now mapped to: {1}", eventName, keyString);
			}

			return success;
		}

		public IEnumerable<KeyBinding> Bindings {
			get {
				return KeyEvents.Values;
			}
		}

		#endregion
	}
}