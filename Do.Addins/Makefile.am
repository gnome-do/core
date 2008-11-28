
EXTRA_DIST = 

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = $(MCS)
ASSEMBLY_COMPILER_FLAGS =  $(MCS_LINQ_FLAG) -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ./bin/Debug/Do.Addins.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Debug


endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = $(MCS)
ASSEMBLY_COMPILER_FLAGS =  $(MCS_LINQ_FLAG) -noconfig -codepage:utf8 -warn:4
ASSEMBLY = ./bin/Release/Do.Addins.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Release


endif


LINUX_PKGCONFIG = \
	$(DO_ADDINS_PC)  

all: $(ASSEMBLY) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	gtk-gui/Do.UI.AbstractLoginWidget.cs \
	gtk-gui/generated.cs \
	src/Do.Addins/CairoUtils.cs \
	src/Do.Addins/IConfigurable.cs \
	src/Do.Addins/IDoController.cs \
	src/Do.Addins/IStatistics.cs \
	src/Do.Addins/PixbufSurfaceCache.cs \
	src/Do.Addins/SearchFinishState.cs \
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
	src/Do.UI/Bezel/BezelColors.cs \
	src/Do.UI/Bezel/Bezel.cs \
	src/Do.UI/Bezel/BezelDefaults.cs \
	src/Do.UI/Bezel/BezelDrawingArea.cs \
	src/Do.UI/Bezel/BezelDrawingContext.cs \
	src/Do.UI/Bezel/BezelGlassResults.cs \
	src/Do.UI/Bezel/BezelGlassWindow.cs \
	src/Do.UI/Bezel/BezelTextUtils.cs \
	src/Do.UI/Bezel/Renderers/BezelGlassRenderClasses.cs \
	src/Do.UI/Bezel/Renderers/ClassicRenderer.cs \
	src/Do.UI/Bezel/Renderers/ClassicTheme.cs \
	src/Do.UI/Bezel/Renderers/GlassFrameRenderer.cs \
	src/Do.UI/Bezel/Renderers/GlassFrameTheme.cs \
	src/Do.UI/Bezel/Renderers/HUDRenderer.cs \
	src/Do.UI/Bezel/Renderers/HUDTheme.cs \
	src/Do.UI/Bezel/Renderers/IRenderInterfaces.cs \
	src/Do.UI/Bezel/Renderers/MiniRenderer.cs \
	src/Do.UI/Bezel/Renderers/MiniTheme.cs \
	src/Do.UI/ClassicWindow/ClassicWindow.cs \
	src/Do.UI/IconCache.cs \
	src/Do.UI/IDoWindow.cs \
	src/Do.UI/IRenderTheme.cs \
	src/Do.UI/IUIContext.cs \
	src/Do.UI/MonoDock/MonoDock.UI/ApplicationDockItem.cs \
	src/Do.UI/MonoDock/MonoDock.UI/CustomDockItems.cs \
	src/Do.UI/MonoDock/MonoDock.UI/DockArea.cs \
	src/Do.UI/MonoDock/MonoDock.UI/DockItem.cs \
	src/Do.UI/MonoDock/MonoDock.UI/DockItemProvider.cs \
	src/Do.UI/MonoDock/MonoDock.UI/DockState.cs \
	src/Do.UI/MonoDock/MonoDock.UI/DockWindow.cs \
	src/Do.UI/MonoDock/MonoDock.UI/IDockItem.cs \
	src/Do.UI/MonoDock/MonoDock.UI/ItemMenu.cs \
	src/Do.UI/MonoDock/MonoDock.UI/SeparatorItem.cs \
	src/Do.UI/MonoDock/MonoDock.UI/Util.cs \
	src/Do.UI/MonoDock/MonoDock.UI/MonoDock.UI.Renderers/DockBackgroundRenderer.cs \
	src/Do.UI/MonoDock/MonoDock.UI/MonoDock.UI.Renderers/SummonModeRenderer.cs \
	src/Do.UI/MonoDock/MonoDock.Util/DockPreferences.cs \
	src/Do.UI/MonoDock/MonoDock.Util/GtkUtils.cs \
	src/Do.UI/MonoDock/MonoDock.Util/WindowUtils.cs \
	src/Do.UI/MonoDock/XLib/X11Atoms.cs \
	src/Do.UI/MonoDock/XLib/Xlib.cs \
	src/Do.UI/PositionWindow.cs \
	src/Do.UI/ResultsWindowSelectionEventArgs.cs \
	src/Do.UI/UIContext.cs

DATA_FILES = 

RESOURCES =  \
	gtk-gui/gui.stetic \
	gtk-gui/objects.xml \
	Resources/Do.Addins.addin.xml \
	Resources/settings-triangle.png 

EXTRAS = \
	do.addins.pc.in 

REFERENCES =  \
	System \
	System.Core \
	Mono.Cairo \
	Mono.Posix \
	$(GCONF_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS) \
	$(GNOME_DESKTOP_SHARP_20_LIBS) \
	$(GNOME_KEYRING_SHARP_LIBS) \
	$(GNOME_SHARP_20_LIBS) \
	$(GNOME_VFS_SHARP_20_LIBS) \
	$(GTK_SHARP_20_LIBS) \
	$(WNCK_SHARP_10_LIBS)

DLL_REFERENCES = \
	../Do.Platform/Do.Platform.dll \
	../Do.Platform.Linux/Do.Platform.Linux.dll \
	../Do.Interface.Linux/Do.Interface.Linux.dll

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
