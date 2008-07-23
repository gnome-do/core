EXTRA_DIST = 
include $(top_srcdir)/Makefile.include

CLEANFILES += gnome-do.desktop

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -langversion:linq -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ./bin/Debug/Do.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../Do.Addins/bin/Debug/Do.Addins.dll \
	../Do.DBus/bin/Debug/Do.DBus.dll
BUILD_DIR = ./bin/Debug

DO_ADDINS_DLL_SOURCE=../Do.Addins/bin/Debug/Do.Addins.dll
GNOME_DO_DESKTOP_SOURCE=./gnome-do.desktop
DO_DBUS_DLL_SOURCE=../Do.DBus/bin/Debug/Do.DBus.dll
DO_DBUS_DLL_MDB_SOURCE=../Do.DBus/bin/Debug/Do.DBus.dll.mdb
DO_DBUS_DLL_MDB=$(BUILD_DIR)/Do.DBus.dll.mdb
DO_ADDINS_DLL_MDB_SOURCE=../Do.Addins/bin/Debug/Do.Addins.dll.mdb
DO_ADDINS_DLL_MDB=$(BUILD_DIR)/Do.Addins.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -langversion:linq -noconfig -codepage:utf8 -warn:4
ASSEMBLY = ./bin/Release/Do.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../Do.Addins/bin/Release/Do.Addins.dll \
	../Do.DBus/bin/Release/Do.DBus.dll
BUILD_DIR = ./bin/Release

DO_ADDINS_DLL_SOURCE=../Do.Addins/bin/Release/Do.Addins.dll
GNOME_DO_DESKTOP_SOURCE=./gnome-do.desktop
DO_DBUS_DLL_SOURCE=../Do.DBus/bin/Release/Do.DBus.dll
DO_DBUS_DLL_MDB=
DO_ADDINS_DLL_MDB=

endif


PROGRAMFILES = \
	$(DO_ADDINS_DLL) \
	$(DO_DBUS_DLL) \
	$(DO_DBUS_DLL_MDB) \
	$(DO_ADDINS_DLL_MDB) \
	$(DO_EXE_CONFIG)


LINUX_DESKTOPAPPLICATIONS = \
	$(GNOME_DO_DESKTOP)  

BINARIES = \
	$(GNOME_DO)

all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

FILES =  \
	gtk-gui/Do.UI.GeneralPreferencesWidget.cs \
	gtk-gui/Do.UI.KeybindingsPreferencesWidget.cs \
	gtk-gui/Do.UI.ManagePluginsPreferencesWidget.cs \
	gtk-gui/Do.UI.PluginConfigurationWindow.cs \
	gtk-gui/Do.UI.PreferencesWindow.cs \
	gtk-gui/generated.cs \
	src/AssemblyInfo.cs \
	src/ConsoleCrayon.cs \
	src/Do.Core/Controller.cs \
	src/Do.Core/DoAction.cs \
	src/Do.Core/DoItem.cs \
	src/Do.Core/DoItemSource.cs \
	src/Do.Core/DoObject.cs \
	src/Do.Core/HistogramRelevanceProvider.cs \
	src/Do.Core/IUniverseManager.cs \
	src/Do.Core/PluginManager.cs \
	src/Do.Core/RelevanceProvider.cs \
	src/Do.Core/SearchControllers/FirstSearchController.cs \
	src/Do.Core/SearchControllers/ISearchController.cs \
	src/Do.Core/SearchControllers/SecondSearchController.cs \
	src/Do.Core/SearchControllers/SimpleSearchContext.cs \
	src/Do.Core/SearchControllers/SimpleSearchController.cs \
	src/Do.Core/SearchControllers/ThirdSearchController.cs \
	src/Do.Core/SimpleUniverseManager.cs \
	src/Do.Core/Tests/UniverseAddItemTest.cs \
	src/Do.Core/Tests/UniverseRemoveItem.cs \
	src/Do.cs \
	src/Do.UI/GeneralPreferencesWidget.cs \
	src/Do.UI/KeybindingsPreferencesWidget.cs \
	src/Do.UI/KeybindingTreeView.cs \
	src/Do.UI/MainMenu.cs \
	src/Do.UI/ManagePluginsPreferencesWidget.cs \
	src/Do.UI/NotificationIcon.cs \
	src/Do.UI/PluginConfigurationWindow.cs \
	src/Do.UI/PluginNodeView.cs \
	src/Do.UI/PluginSelectionEventArgs.cs \
	src/Do.UI/PreferencesWindow.cs \
	src/Do.Universe/AliasAction.cs \
	src/Do.Universe/AliasItemSource.cs \
	src/Do.Universe/DeleteAliasAction.cs \
	src/Do.Universe/DoQuitItem.cs \
	src/Do.Universe/DoTextItem.cs \
	src/Do.Universe/EmptyItem.cs \
	src/Do.Universe/InternalItemSource.cs \
	src/Do.Universe/ItemSourceItemSource.cs \
	src/Do.Universe/PreferencesItem.cs \
	src/Do.Universe/ProxyItem.cs \
	src/Do.Universe/SelectedTextItem.cs \
	src/DoPreferences.cs \
	src/GconfPreferencesBackend.cs \
	src/IPreferencesBackend.cs \
	src/Log.cs \
	src/Preferences.cs \
	src/Util.cs \
	src/XKeybinder.cs 

DATA_FILES = \
	gnome-do.desktop.in 

RESOURCES = \
	gtk-gui/gui.stetic \
	gnome-do.desktop \
	Resources/Do.addin.xml

EXTRAS = \
	gnome-do.in \
	Do.exe.config.in

REFERENCES =  \
	../Do.Addins/bin/Debug/Do.Addins.dll \
	../Do.DBus/bin/Debug/Do.DBus.dll \
	Mono.Posix \
	-pkg:gconf-sharp-2.0 \
	-pkg:glib-sharp-2.0 \
	-pkg:gtk-sharp-2.0 \
	-pkg:mono-addins \
	-pkg:mono-addins-gui \
	-pkg:mono-addins-setup \
	-pkg:notify-sharp \
	-pkg:nunit-2.2 \
	System

DLL_REFERENCES = 

DO_ADDINS_DLL = $(BUILD_DIR)/Do.Addins.dll
GNOME_DO_DESKTOP = $(BUILD_DIR)/gnome-do.desktop
DO_DBUS_DLL = $(BUILD_DIR)/Do.DBus.dll
GNOME_DO = $(BUILD_DIR)/gnome-do
DO_EXE_CONFIG = $(BUILD_DIR)/Do.exe.config

@INTLTOOL_DESKTOP_RULE@

$(DO_ADDINS_DLL): $(DO_ADDINS_DLL_SOURCE)
	mkdir -p $(BUILD_DIR)
	cp '$<' '$@'

$(GNOME_DO_DESKTOP): $(GNOME_DO_DESKTOP_SOURCE)
	mkdir -p $(BUILD_DIR)
	cp '$<' '$@'

$(DO_DBUS_DLL): $(DO_DBUS_DLL_SOURCE)
	mkdir -p $(BUILD_DIR)
	cp '$<' '$@'

$(DO_DBUS_DLL_MDB): $(DO_DBUS_DLL_MDB_SOURCE)
	mkdir -p $(BUILD_DIR)
	cp '$<' '$@'

$(GNOME_DO): gnome-do
	mkdir -p $(BUILD_DIR)
	chmod 755 '$<'
	cp '$<' '$@'

$(DO_EXE_CONFIG): Do.exe.config
	mkdir -p $(BUILD_DIR)
	cp '$<' '$@'

$(DO_ADDINS_DLL_MDB): $(DO_ADDINS_DLL_MDB_SOURCE)
	mkdir -p $(BUILD_DIR)
	cp '$<' '$@'


$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list)
	mkdir -p $(dir $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)

