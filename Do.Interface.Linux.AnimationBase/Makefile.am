# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Interface.Linux.AnimationBase
TARGET = library
PKG_CONFIG_FILES = do.interface.linux.animationbase.pc

FILES = \
	AssemblyInfo.cs \
	src/Do.Interface/IRenderTheme.cs \
	src/Do.Interface/Do.Interface.AnimationBase/AnimationBaseConfigurationWidget.cs \
	src/Do.Interface/Do.Interface.AnimationBase/Bezel.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelColors.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelDefaults.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelDrawingArea.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelDrawingContext.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelGlassRenderClasses.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelGlassResults.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelGlassWindow.cs \
	src/Do.Interface/Do.Interface.AnimationBase/BezelResultsDrawingArea.cs \
	src/Do.Interface/Do.Interface.AnimationBase/IRenderInterfaces.cs \
	src/Do.Interface/Do.Interface.AnimationBase/TextRenderer.cs \
	gtk-gui/Do.Interface.AnimationBase.AnimationBaseConfigurationWidget.cs \
	gtk-gui/generated.cs


RESOURCES = \
	gtk-gui/gui.stetic \
	Resources/Do.Interface.Linux.AnimationBase.addin.xml

REFERENCES =  \
	System \
	System.Core \
	Mono.Cairo \
	Mono.Posix \
	$(GCONF_SHARP_20_LIBS) \
	$(GTK_SHARP_20_LIBS)

PROJECT_REFERENCES = \
	Do.Platform.Linux \
	Do.Platform \
	Do.Interface.Linux \
	Do.Universe
