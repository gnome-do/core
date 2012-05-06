using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Unix;

namespace Do.Platform.Common
{
	public abstract class AbstractKeyBindingService : IKeyBindingService
	{

		public abstract bool RegisterOSKey (string keyString, EventCallback cb);
		public abstract bool UnRegisterOSKey (string keyString);

		IPreferences prefs;

#region IInitializedService

		public void Initialize () 
		{
			Bindings = new List<KeyBinding> ();	
			prefs = Services.Preferences.Get<AbstractKeyBindingService> ();
		}

#endregion

#region IKeyBindingService

		public List<KeyBinding> Bindings { get; private set; }

		public bool RegisterKeyBinding (KeyBinding binding) 
		{
			//try to get the keystring from the prefs.  We default to the KeyBinding.KeyString, so we can later check
			//if the prefs value matches that, we're using the default, otherwise we're using a user specified value
			string prefsKeyString = prefs.Get (binding.PreferenceName, binding.KeyString);
			
			//if these values don't match then the user has specified a new keystring
			//update the KeyEvent then continue
			if (prefsKeyString != binding.KeyString)
				binding.KeyString = prefsKeyString;
			
			//check if this keystring is already used
			if (Bindings.Any (k => k.KeyString == binding.KeyString)) {
				Log<AbstractKeyBindingService>.Error ("Key \"{0}\" is already mapped.", binding.KeyString);
				binding.KeyString = "";
			}

			//if we are registering a key with the OS, do something special
			if (binding.IsOSKey) {
				//try to register the key from the prefs with the OS
				if (!RegisterOSKey (binding.KeyString, binding.Callback)) {
					//if we fail to register the summon key, try again with the default binding
					if (!string.IsNullOrEmpty (binding.DefaultKeyString) && RegisterOSKey (binding.DefaultKeyString, binding.Callback)) {
						binding.KeyString = binding.DefaultKeyString;
					} else if (!string.IsNullOrEmpty (binding.KeyString) && !string.IsNullOrEmpty (binding.DefaultKeyString)) {
						Log<AbstractKeyBindingService>.Error ("Failed to bind \"{0}\" to \"{1}\"", binding.Description, 
							binding.KeyString);
						binding.KeyString = "";
					}
				}
			}

			//add the event to the list of bindings
			Bindings.Add (binding);
			//set the bound keystring in the prefs
			prefs.Set (binding.PreferenceName, binding.KeyString);

			return true;
		}

		public bool SetKeyString (KeyBinding binding, string newKeyString)
		{
			//first check if this keystring exists
			if (!Bindings.Any (k => k.KeyString == binding.KeyString))
				return false;

			//if this key should be registered with the OS
			if (binding.IsOSKey) {
				//register again with the new keystring

				//FIXME: Unsetting bindings should probably be a separate exported function.
				if (newKeyString != "" && !RegisterOSKey (newKeyString, binding.Callback))
					return false;

				//remove the old keystring from the OS
				UnRegisterOSKey (binding.KeyString);
			}

			//set the new keystring
			Bindings.First (k => k.Description == binding.Description).KeyString = newKeyString;
			
			//save the new value in the prefs
			prefs.Set (binding.PreferenceName, binding.KeyString);

			if (!string.IsNullOrEmpty (binding.KeyString))
				Log<AbstractKeyBindingService>.Debug ("\"{0}\" now mapped to \"{1}\"", binding.Description, binding.KeyString);

			return true;
		}
		
		/// <summary>
		/// Converts a keypress into a human readable string for comparing
		/// against values in GConf.
		/// </summary>
		/// <param name="evnt">
		/// A <see cref="EventKey"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> in the form "<Modifier>key"
		/// </returns>
		public string KeyEventToString (uint keycode, uint modifierCode) {
			// FIXME: This should really use Gtk.Accelerator.Name (key, modifier)
			// Beware of bug #903566 when doing that!
			
			string modifier = "";
			if ((modifierCode & (uint)Gdk.ModifierType.ControlMask) != 0) {
				modifier += "<Control>";
			}
			if ((modifierCode & (uint)Gdk.ModifierType.SuperMask) != 0) {
				modifier += "<Super>";
			}
			if ((modifierCode & (uint)Gdk.ModifierType.Mod1Mask) != 0) {
				modifier += "<Alt>";
			}
			if ((modifierCode & (uint)Gdk.ModifierType.ShiftMask) != 0) {
				modifier += "<Shift>";
				//if we're pressing shift, and the key is ISO_Left_Tab,
				//just make it Tab
				if (keycode == (uint)Gdk.Key.ISO_Left_Tab)
					return string.Format ("{0}{1}", modifier, Gdk.Key.Tab);
			}
			return string.Format ("{0}{1}", modifier, Gtk.Accelerator.Name (keycode, Gdk.ModifierType.None));
		}
#endregion
	}
}
