using Gtk;
using System;

using Mono.Addins;

using Do.Core;

namespace Do.UI
{
	public partial class PluginConfigurationWindow : Gtk.Window
	{
		public PluginConfigurationWindow (string id) : 
				base(Gtk.WindowType.Toplevel)
		{
			Addin addin;
			DoObject[] configs;
			
			Build ();
			
			addin = AddinManager.Registry.GetAddin (id);
			configs = Do.PluginManager.ConfigurablesForAddin (id);
			Title = string.Format ("{0} Configuration", addin.Name);
			notebook.RemovePage (0);
			notebook.ShowTabs = configs.Length > 1;
			
			foreach (DoObject configurable in configs) {
				Bin config;

				config = configurable.GetConfiguration ();
				notebook.AppendPage (config, new Label (configurable.Name));
				config.ShowAll ();
			}
		}

		protected virtual void OnBtnCloseClicked (object sender, System.EventArgs e)
		{
			Hide ();
		}
	}
}
