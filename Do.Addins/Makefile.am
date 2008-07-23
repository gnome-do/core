
EXTRA_DIST = 

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -langversion:linq -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ./bin/Debug/Do.Addins.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Debug


endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -langversion:linq -noconfig -codepage:utf8 -warn:4
ASSEMBLY = ./bin/Release/Do.Addins.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Release


endif


LINUX_PKGCONFIG = \
	$(DO_ADDINS_PC)  


	
all: $(ASSEMBLY) $(LINUX_PKGCONFIG) 

FILES =  \
	AssemblyInfo.cs \
	gtk-gui/Do.UI.AbstractLoginWidget.cs \
	gtk-gui/generated.cs \
	src/Do.Addins/IConfigurable.cs \
	src/Do.Addins/IDoController.cs \
	src/Do.Addins/IPreferences.cs \
	src/Do.Addins/SearchContext.cs \
	src/Do.Addins/Util.cs \
	src/Do.UI/AbstractLoginWidget.cs \
	src/Do.UI/BaseWidgets/DefaultObjects.cs \
	src/Do.UI/BaseWidgets/Frame.cs \
	src/Do.UI/BaseWidgets/GlossyRoundedFrame.cs \
	src/Do.UI/BaseWidgets/HalfRoundedFrame.cs \
	src/Do.UI/BaseWidgets/IconBox.cs \
	src/Do.UI/BaseWidgets/ResultsWindow.cs \
	src/Do.UI/BaseWidgets/SymbolDisplayLabel.cs \
	src/Do.UI/BaseWidgets/TextFrame.cs \
	src/Do.UI/ClassicWindow/ClassicWindow.cs \
	src/Do.UI/DarkFrame/GlassFrame.cs \
	src/Do.UI/DarkFrame/GlassIconBox.cs \
	src/Do.UI/DarkFrame/GlassWindow.cs \
	src/Do.UI/IconCache.cs \
	src/Do.UI/IconProvider.cs \
	src/Do.UI/IDoWindow.cs \
	src/Do.UI/IUIContext.cs \
	src/Do.UI/MiniWindow/MiniIconBox.cs \
	src/Do.UI/MiniWindow/MiniWindow.cs \
	src/Do.UI/MiniWindow/MiniWindowFrame.cs \
	src/Do.UI/NotificationBridge.cs \
	src/Do.UI/PositionWindow.cs \
	src/Do.UI/ResultsWindowSelectionEventArgs.cs \
	src/Do.UI/UIContext.cs \
	src/Do.Universe/ApplicationItem.cs \
	src/Do.Universe/ApplicationItemSource.cs \
	src/Do.Universe/BookmarkItem.cs \
	src/Do.Universe/ContactItem.cs \
	src/Do.Universe/CopyToClipboard.cs \
	src/Do.Universe/EmailAction.cs \
	src/Do.Universe/FileItem.cs \
	src/Do.Universe/GNOMESpecialLocationsItemSource.cs \
	src/Do.Universe/IAction.cs \
	src/Do.Universe/IItem.cs \
	src/Do.Universe/IItemSource.cs \
	src/Do.Universe/IObject.cs \
	src/Do.Universe/OpenAction.cs \
	src/Do.Universe/OpenURLAction.cs \
	src/Do.Universe/OpenWithAction.cs \
	src/Do.Universe/Pluggable.cs \
	src/Do.Universe/RevealAction.cs \
	src/Do.Universe/RunAction.cs \
	src/Do.Universe/TextItem.cs \
	src/Paths.cs 

DATA_FILES = 

RESOURCES =  \
	gtk-gui/gui.stetic \
	gtk-gui/objects.xml \
	Resources/Do.Addins.addin.xml \
	Resources/settings-triangle.png 

EXTRAS = \
	do.addins.pc.in 

REFERENCES =  \
	Mono.Cairo \
	Mono.Posix \
	-pkg:gconf-sharp-2.0 \
	-pkg:glib-sharp-2.0 \
	-pkg:gnome-desktop-sharp-2.0 \
	-pkg:gnome-keyring-sharp \
	-pkg:gnome-sharp-2.0 \
	-pkg:gnome-vfs-sharp-2.0 \
	-pkg:gtk-sharp-2.0 \
	System

DLL_REFERENCES = 

DO_ADDINS_PC = $(BUILD_DIR)/do.addins.pc

$(DO_ADDINS_PC): do.addins.pc
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
