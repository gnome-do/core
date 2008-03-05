
EXTRA_DIST = 

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ./bin/Debug/Do.Addins.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Debug


endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4
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
	src/Paths.cs \
	src/Do.Addins/Do.Addins.UI/BaseWidgets/DefaultObjects.cs \
	src/Do.Addins/Do.Addins.UI/BaseWidgets/Frame.cs \
	src/Do.Addins/Do.Addins.UI/BaseWidgets/GlossyRoundedFrame.cs \
	src/Do.Addins/Do.Addins.UI/BaseWidgets/HalfRoundedFrame.cs \
	src/Do.Addins/Do.Addins.UI/BaseWidgets/IconBox.cs \
	src/Do.Addins/Do.Addins.UI/BaseWidgets/IconProvider.cs \
	src/Do.Addins/Do.Addins.UI/BaseWidgets/ResultsWindow.cs \
	src/Do.Addins/Do.Addins.UI/BaseWidgets/SymbolDisplayLabel.cs \
	src/Do.Addins/Do.Addins.UI/BaseWidgets/TextFrame.cs \
	src/Do.Addins/Do.Addins.UI/ClassicWindow/ClassicWindow.cs \
	src/Do.Addins/Do.Addins.UI/DarkFrame/DarkFrame.cs \
	src/Do.Addins/Do.Addins.UI/IDoWindow.cs \
	src/Do.Addins/Do.Addins.UI/MiniWindow/MiniIconBox.cs \
	src/Do.Addins/Do.Addins.UI/MiniWindow/MiniWindow.cs \
	src/Do.Addins/Do.Addins.UI/MiniWindow/MiniWindowFrame.cs \
	src/Do.Addins/Do.Addins.UI/ResultsWindowSelectionEventArgs.cs \
	src/Do.Addins/IDoController.cs \
	src/Do.Addins/SearchContext.cs \
	src/Do.Addins/Util.cs \
	src/Do.Universe/ApplicationItem.cs \
	src/Do.Universe/ApplicationItemSource.cs \
	src/Do.Universe/BookmarkItem.cs \
	src/Do.Universe/ContactItem.cs \
	src/Do.Universe/DefineWordAction.cs \
	src/Do.Universe/FileItem.cs \
	src/Do.Universe/FileItemActions.cs \
	src/Do.Universe/FileItemSource.cs \
	src/Do.Universe/FirefoxBookmarkItemSource.cs \
	src/Do.Universe/GNOMESpecialLocationsItemSource.cs \
	src/Do.Universe/IAction.cs \
	src/Do.Universe/ICommand.cs \
	src/Do.Universe/IItem.cs \
	src/Do.Universe/IItemSource.cs \
	src/Do.Universe/ImageFileItem.cs \
	src/Do.Universe/IObject.cs \
	src/Do.Universe/MailtoAction.cs \
	src/Do.Universe/ManuallyTypedPathItemSource.cs \
	src/Do.Universe/OpenAction.cs \
	src/Do.Universe/OpenTerminalHereAction.cs \
	src/Do.Universe/OpenURLAction.cs \
	src/Do.Universe/OpenWithAction.cs \
	src/Do.Universe/RecentFileItemSource.cs \
	src/Do.Universe/RevealAction.cs \
	src/Do.Universe/RunAction.cs \
	src/Do.Universe/RunInTerminalAction.cs \
	src/Do.Universe/TextItem.cs \
	src/Do.Universe/VoidAction.cs 

DATA_FILES = 

RESOURCES = ../Do/Resources/settings-triangle.png 

EXTRAS = \
	do.addins.pc.in 

REFERENCES =  \
	Mono.Cairo \
	Mono.Posix \
	-pkg:gconf-sharp-2.0 \
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
