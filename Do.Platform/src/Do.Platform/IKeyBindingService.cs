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
		bool SetKeyString (KeyBinding binding, string keyString);
		Dictionary<string, KeyBinding> Bindings { get; }
	}
}
