
/* Keybindings.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Env = System.Environment;

using Do.Platform;

namespace Do
{

        class CoreKeybindings {
                Dictionary<string, string> KeycodeMap ; // keybinding -> shortcut name
                Dictionary<string, Shortcut> ShortcutMap ; // shortcut name -> shortcut
                Dictionary<string, string> DefaultShortcutMap ; // default keybinding -> shortcut name

                IPreferences Preferences { get; set; }

                public ArrayList Shortcuts ;
                public Dictionary<string, List<KeyChangedCb>> PreferencesCbs;
                public delegate void KeyChangedCb (object sender, PreferencesChangedEventArgs e);

                public CoreKeybindings ()
                {

                        Preferences = Services.Preferences.Get<CoreKeybindings> ();
                        Preferences.PreferencesChanged += PreferencesChanged;

                        Shortcuts = new ArrayList ();
                        KeycodeMap = new Dictionary<string, string> (); // keybinding -> shortcut name
                        ShortcutMap = new Dictionary<string, Shortcut> (); // shortcut name -> shortcut
                        DefaultShortcutMap = new Dictionary<string, string> (); // default keybinding -> shortcut name
                        PreferencesCbs = new Dictionary<string, List<KeyChangedCb>> ();

                        Initialize ();
                }

                public void Initialize ()
                {
                        // Read all values out of preferences and populate the KeybindingMap
                        ReadShortcuts ();
                }
                
                public bool RegisterShortcut (Shortcut sc, string defaultBinding)
                {
                        if (!RegisterShortcut (sc))
                                return false; 
                        if (!BindDefault (sc, defaultBinding))
                                return false;
                        return true;
                }

                public bool RegisterShortcut (Shortcut sc)
                {
                        if (Shortcuts.Contains (sc) || ShortcutMap.ContainsKey (sc.ShortcutName)) 
                            return false;

                        Shortcuts.Add (sc);
                        ShortcutMap [sc.ShortcutName] = sc;
                        PreferencesCbs [sc.ShortcutName] = new List<KeyChangedCb> ();
                        SaveShortcuts ();
                        return true;
                }

                public Shortcut GetShortcutByKeycode (string keycode)
                {
                        if (!KeycodeMap.ContainsKey (keycode)) 
                                return null;
                        
                        string scname = KeycodeMap [keycode];

                        if (!ShortcutMap.ContainsKey (scname)) 
                                return null;

                        return ShortcutMap [scname];

                }

                public string GetKeybinding (Shortcut sc)
                {
                        return GetKeybinding (sc.ShortcutName);
                }

                public string GetKeybinding (string sc)
                {

                        foreach (KeyValuePair<string, string> entry in KeycodeMap) {
                                if (entry.Value == sc) 
                                        return entry.Key;
                        }
                        return null;
                }

                public string GetDefaultKeybinding (Shortcut sc)
                {
                        return GetDefaultKeybinding (sc.ShortcutName);
                }

                public string GetDefaultKeybinding (string sc)
                {
                        foreach (KeyValuePair<string, string> entry in DefaultShortcutMap) {
                                if (entry.Value == sc) 
                                        return entry.Key;
                        }
                        return null;
                }


                public bool BindShortcut (Shortcut sc, string keycode)
                {
                        // Add this function to our keybinding map
                        return BindShortcut (sc.ShortcutName, keycode);

                }

                public bool BindShortcut (string sc, string keycode)
                {
                        string oldcode = GetKeybinding (sc);
                        if (oldcode != null)
                                KeycodeMap.Remove (oldcode); // remove the old keybinding from the map

                        KeycodeMap [keycode] = sc;
                        Preferences.Set (sc, keycode);

                        return true;
                }

                // Add Default Keycode mapping - used for resetting to default or not overwriting read values
                public bool BindDefault (Shortcut sc, string keycode)
                {
                        return BindDefault (sc.ShortcutName, keycode);

                }

                public bool BindDefault (string sc, string keycode)
                {

                        string assigned_keycode = GetKeybinding (sc);
                        if (assigned_keycode == null) {
                                // Put this shortcut in the mapping
                                BindShortcut (sc, keycode);
                        }

                        DefaultShortcutMap [keycode] = sc;
                        return true;

                }

                public bool UnregisterShortcut (Shortcut sc)
                {
                        if (!Shortcuts.Contains (sc))
                            return false;

                        Shortcuts.Remove (sc);
                        ShortcutMap.Remove (sc.ShortcutName);
                        SaveShortcuts ();
                        return true;
                }

                public bool RegisterNotification (Shortcut sc, KeyChangedCb cb)
                {
                        return RegisterNotification (sc.ShortcutName, cb);
                }

                public bool RegisterNotification (string scname, KeyChangedCb cb)
                {
                        PreferencesCbs [scname].Add (cb);
                        return true;
                }

                void SaveShortcuts () 
                {
                        string scstring = "";
                        foreach (Shortcut sc in Shortcuts) {
                                scstring += sc.ShortcutName.Trim () + ",";
                        }
                        Preferences.Set ("RegisteredShortcuts", scstring);
                } 

                void ReadShortcuts ()
                {
                        string scstring = Preferences.Get ("RegisteredShortcuts", "").Trim ();
                        if (scstring == "") 
                                return;

                        foreach (string sc in scstring.Split (',')) {
                                if (sc.Trim () == "") 
                                        continue;

                                string keycode = Preferences.Get (sc, "");
                                if (keycode != "")
                                        BindShortcut (sc, keycode);
                        }
                }

                void PreferencesChanged (object sender, PreferencesChangedEventArgs e)
                {

                        if (PreferencesCbs.ContainsKey (e.Key)) {   
                                foreach (KeyChangedCb cb in PreferencesCbs [e.Key]) {
                                        cb (this, e);
                                }
                        }
                }

        }

}
