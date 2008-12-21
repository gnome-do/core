# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Interface.Linux
TARGET = library
PKG_CONFIG_FILES = do.interface.linux.pc

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Interface/ClassicInterface/ClassicWindow.cs \
	src/Do.Interface/Do.Interface.AnimationBase/Bezel.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelColors.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelDefaults.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelDrawingArea.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelDrawingContext.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelGlassRenderClasses.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelGlassResults.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelGlassWindow.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelResultsDrawingArea.cs \
	src/Do.Interface/Do.Interface.AnimationBase/TextRenderer.cs \
	src/Do.Interface/Do.Interface.AnimationBase/IRenderInterfaces.cs \
	src/Do.Interface/Do.Interface.CairoUtils/CairoUtils.cs \
	src/Do.Interface/Do.Interface.Widgets/DefaultElements.cs \
	src/Do.Interface/Do.Interface.Widgets/Frame.cs \
	src/Do.Interface/Do.Interface.Widgets/GlossyRoundedFrame.cs \
	src/Do.Interface/Do.Interface.Widgets/HalfRoundedFrame.cs \
	src/Do.Interface/Do.Interface.Widgets/IconBox.cs \
	src/Do.Interface/Do.Interface.Widgets/ResultsWindow.cs \
	src/Do.Interface/Do.Interface.Widgets/SymbolDisplayLabel.cs \
	src/Do.Interface/Do.Interface.Widgets/TextFrame.cs \
	src/Do.Interface/IConfigurable.cs \
	src/Do.Interface/IDoController.cs \
	src/Do.Interface/IDoWindow.cs \
	src/Do.Interface/IRenderTheme.cs \
	src/Do.Interface/IUIContext.cs \
	src/Do.Interface/IconCache.cs \
	src/Do.Interface/IconProvider.cs \
	src/Do.Interface/InterfaceManager.cs \
	src/Do.Interface/PixbufSurfaceCache.cs \
	src/Do.Interface/PositionWindow.cs \
	src/Do.Interface/ResultsWindowSelectionEventArgs.cs \
	src/Do.Interface/SearchFinishState.cs \
	src/Do.Interface/UIContext.cs \
	src/Do.Interface/Util.cs \
	src/Do.Interface/Windowing.cs \
	src/Do.Universe/ImplicitTextItem.cs

RESOURCES = \
	Resources/Do.Interface.Linux.addin.xml \
	Resources/settings-triangle.png

REFERENCES =  \
	System \
	System.Core \
	Mono.Cairo \
	Mono.Posix \
	$(GCONF_SHARP_20_LIBS) \
	$(GTK_SHARP_20_LIBS) \
	$(MONO_ADDINS_LIBS) 

PROJECT_REFERENCES = \
	Do.Platform \
	Do.Universe
