using System;
using System.Collections.Generic;

using Do.Platform.Common;
using Do.Platform.ServiceStack;

using Gdk;

namespace Do.Platform
{
	public delegate void EventCallback (EventKey evtky);

	public interface IKeyBindingService : IInitializedService
	{
		bool RegisterKeyBinding (KeyBinding evnt);
		KeyBinding GetBinding (DoKeyEvents eventName);
		KeyBinding GetBinding (string keyString);
		bool SetKeyString (DoKeyEvents eventName, string keyString);
		IEnumerable<KeyBinding> Bindings { get; }
	}
}
