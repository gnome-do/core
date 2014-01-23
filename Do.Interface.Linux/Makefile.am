# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Interface.Linux
TARGET = library
PKG_CONFIG_FILES = do.interface.linux.pc

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Interface/ClassicInterface/ClassicWindow.cs \
	src/Do.Interface/Do.Interface.CairoUtils/CairoUtils.cs \
	src/Do.Interface/Do.Interface.Widgets/DefaultItems.cs \
	src/Do.Interface/Do.Interface.Widgets/Frame.cs \
	src/Do.Interface/Do.Interface.Widgets/GlossyRoundedFrame.cs \
	src/Do.Interface/Do.Interface.Widgets/HalfRoundedFrame.cs \
	src/Do.Interface/Do.Interface.Widgets/IconBox.cs \
	src/Do.Interface/Do.Interface.Widgets/ResultsWindow.cs \
	src/Do.Interface/Do.Interface.Widgets/SymbolDisplayLabel.cs \
	src/Do.Interface/Do.Interface.Widgets/TextFrame.cs \
	src/Do.Interface/IDoController.cs \
	src/Do.Interface/IDoWindow.cs \
	src/Do.Interface/IUIContext.cs \
	src/Do.Interface/IconCache.cs \
	src/Do.Interface/InterfaceDescription.cs \
	src/Do.Interface/InterfaceManager.cs \
	src/Do.Interface/PixbufSurfaceCache.cs \
	src/Do.Interface/PositionWindow.cs \
	src/Do.Interface/ResultsWindowSelectionEventArgs.cs \
	src/Do.Interface/SearchFinishState.cs \
	src/Do.Interface/UIContext.cs \
	src/Do.Interface/Util.cs \
	src/Do.Interface/Windowing.cs \
	src/Do.Universe/ImplicitTextItem.cs

if HAVE_NUNIT
FILES += \
	src/Do.Interface/Tests/TestPositionWindow.cs
endif

if NEED_CAIRO_SHIM
FILES += \
	src/Do.Interface/Do.Interface.CairoUtils/Cairo32Compatibility.cs

BUILD_DEFINES = \
	-define:USING_OLD_CAIRO
endif

# Automake won't do this for us
EXTRA_DIST += \
	src/Do.Interface/Do.Interface.CairoUtils/Cairo32Compatibility.cs

RESOURCES = \
	Resources/Do.Interface.Linux.addin.xml \
	Resources/settings-triangle.png

REFERENCES =  \
	System \
	System.Core \
	Mono.Cairo \
	Mono.Posix \
	$(GTK_SHARP_20_LIBS) \
	$(MONO_ADDINS_LIBS) \
	$(NUNIT_LIBS)

PROJECT_REFERENCES = \
	Do.Platform \
	Do.Platform.Linux \
	Do.Universe
