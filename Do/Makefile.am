
EXTRA_DIST = 

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
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
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4
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
	$(DO_ADDINS_DLL_MDB)  

LINUX_DESKTOPAPPLICATIONS = \
	$(GNOME_DO_DESKTOP)  

BINARIES = \
	$(GNOME_DO)  


	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

FILES =  \
	gtk-gui/generated.cs \
	src/AssemblyInfo.cs \
	src/DefaultCommander.cs \
	src/Do.Core/Commander.cs \
	src/Do.Core/DoCommand.cs \
	src/Do.Core/DoItem.cs \
	src/Do.Core/DoItemSource.cs \
	src/Do.Core/DoObject.cs \
	src/Do.Core/RelevanceSorter.cs \
	src/Do.Core/SearchContext.cs \
	src/Do.Core/UniverseManager.cs \
	src/Do.cs \
	src/Do.UI/IconBox.cs \
	src/Do.UI/MainMenu.cs \
	src/Do.UI/ResultsWindow.cs \
	src/Do.UI/RoundedFrame.cs \
	src/Do.UI/SymbolDisplayLabel.cs \
	src/Do.UI/SymbolWindow.cs \
	src/Log.cs \
	src/Util.cs \
	src/XKeybinder.cs 

DATA_FILES = \
	gnome-do.desktop 

RESOURCES = \
	gtk-gui/gui.stetic \
	Resources/settings-triangle.png 

EXTRAS = \
	src/TrayLib.cs \
	gnome-do.in 

REFERENCES =  \
	../Do.Addins/bin/Debug/Do.Addins.dll \
	../Do.DBus/bin/Debug/Do.DBus.dll \
	Mono.Cairo \
	Mono.Posix \
	-pkg:gconf-sharp-2.0 \
	-pkg:glade-sharp-2.0 \
	-pkg:glib-sharp-2.0 \
	-pkg:gtk-sharp-2.0 \
	System

DLL_REFERENCES = 

DO_ADDINS_DLL = $(BUILD_DIR)/Do.Addins.dll
GNOME_DO_DESKTOP = $(BUILD_DIR)/gnome-do.desktop
DO_DBUS_DLL = $(BUILD_DIR)/Do.DBus.dll
GNOME_DO = $(BUILD_DIR)/gnome-do

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

include $(top_srcdir)/Makefile.include
