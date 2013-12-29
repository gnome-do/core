# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do
TARGET = exe

FILES = \
	gtk-gui/Do.UI.ColorConfigurationWidget.cs \
	gtk-gui/Do.UI.DoAddinInstallerDialog.cs \
	gtk-gui/Do.UI.GeneralPreferencesWidget.cs \
	gtk-gui/Do.UI.KeybindingsPreferencesWidget.cs \
	gtk-gui/Do.UI.ManagePluginsPreferencesWidget.cs \
	gtk-gui/Do.UI.PluginConfigurationWindow.cs \
	gtk-gui/Do.UI.PluginErrorDialog.cs \
	gtk-gui/Do.UI.PreferencesWindow.cs \
	gtk-gui/generated.cs \
	src/AssemblyInfo.cs \
	src/CorePreferences.cs \
	src/Do.Core/Do.Core.Addins/AddinClassifier.cs \
	src/Do.Core/Do.Core.Addins/CommunityAddinClassifier.cs \
	src/Do.Core/Do.Core.Addins/GreedyAddinClassifier.cs \
	src/Do.Core/Do.Core.Addins/OfficialAddinClassifier.cs \
	src/Do.Core/Controller.cs \
	src/Do.Core/DoAddinInstaller.cs \
	src/Do.Core/FirstSearchController.cs \
	src/Do.Core/HistogramRelevanceProvider.cs \
	src/Do.Core/ISearchController.cs \
	src/Do.Core/ItemExtensions.cs \
	src/Do.Core/Paths.cs \
	src/Do.Core/PluginManager.cs \
	src/Do.Core/RelevanceProvider.cs \
	src/Do.Core/SecondSearchController.cs \
	src/Do.Core/Shortcut.cs \
	src/Do.Core/ShortcutCallback.cs \
	src/Do.Core/SimpleSearchContext.cs \
	src/Do.Core/SimpleSearchController.cs \
	src/Do.Core/ThirdSearchController.cs \
	src/Do.Core/UniverseManager.cs \
	src/Do.Platform/CoreService.cs \
	src/Do.Platform/ApplicationService.cs \
	src/Do.Platform/WindowingService.cs \
	src/Do.Platform/PluginManagerService.cs \
	src/Do.UI/ColorConfigurationWidget.cs \
	src/Do.UI/DoAddinInstallerDialog.cs \
	src/Do.UI/GeneralPreferencesWidget.cs \
	src/Do.UI/KeybindingTreeView.cs \
	src/Do.UI/KeybindingsPreferencesWidget.cs \
	src/Do.UI/MainMenu.cs \
	src/Do.UI/ManagePluginsPreferencesWidget.cs \
	src/Do.UI/PluginConfigurationWindow.cs \
	src/Do.UI/PluginErrorDialog.cs \
	src/Do.UI/PluginNodeView.cs \
	src/Do.UI/PluginSelectionEventArgs.cs \
	src/Do.UI/PluginUpdateNodeView.cs \
	src/Do.UI/PreferencesWindow.cs \
	src/Do.UI/SearchEntry.cs \
	src/Do.UI/HoverImageButton.cs \
	src/Do.Universe/ClearLearningItem.cs \
	src/Do.Universe/EvilAction.cs \
	src/Do.Universe/EvilElement.cs \
	src/Do.Universe/EvilItem.cs \
	src/Do.Universe/EvilItemSource.cs \
	src/Do.Universe/InternalItemSource.cs \
	src/Do.Universe/ItemSourceItemSource.cs \
	src/Do.Universe/PreferencesItem.cs \
	src/Do.Universe/DonateItem.cs \
	src/Do.Universe/DebugItemAction.cs \
	src/Do.Universe/AboutItem.cs \
	src/Do.Universe/QuitItem.cs \
	src/Do.Universe/SelectedTextItem.cs \
	src/Do.cs \
	src/Mono.Addins/AddinExtensions.cs \
	src/Mono.Addins/Mono.Addins.Setup/AddinRepositoryEntryExtensions.cs \
	src/PathExtensions.cs



RESOURCES = \
	gtk-gui/gui.stetic \
	Resources/Do.addin.xml

REFERENCES =  \
	Mono.Posix \
	$(GLIB_SHARP_20_LIBS) \
	$(GTK_SHARP_20_LIBS) \
	$(MONO_ADDINS_LIBS) \
	$(MONO_ADDINS_GUI_LIBS) \
	$(MONO_ADDINS_SETUP_LIBS) \
	$(NUNIT_LIBS) \
	System \
	System.Core \
	System.Xml

PROJECT_REFERENCES = \
	Do.Universe \
	Do.Platform \
	Do.Platform.Linux \
	Do.Interface.Linux \
	Do.Interface.Linux.AnimationBase

#
# Extra rules for our binary, not worth factoring out
#

CLEANFILES += gnome-do
EXTRA_DIST += gnome-do.in Do.addins

module_DATA += Do.addins
bin_SCRIPTS = gnome-do

MCS_FLAGS += -unsafe
