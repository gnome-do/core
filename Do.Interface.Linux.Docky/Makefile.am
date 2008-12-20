# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Interface.Linux.Docky
TARGET = library

FILES = \
	AssemblyInfo.cs \
	src/Docky.Interface/AbstractDockItem.cs \
	src/Docky.Interface/ApplicationDockItem.cs \
	src/Docky.Interface/DoDockItem.cs \
	src/Docky.Interface/DockArea.cs \
	src/Docky.Interface/DockItem.cs \
	src/Docky.Interface/DockItemProvider.cs \
	src/Docky.Interface/DockState.cs \
	src/Docky.Interface/DockWindow.cs \
	src/Docky.Interface/Docky.Interface.Renderers/DockBackgroundRenderer.cs \
	src/Docky.Interface/Docky.Interface.Renderers/SummonModeRenderer.cs \
	src/Docky.Interface/DrawState.cs \
	src/Docky.Interface/IDockItem.cs \
	src/Docky.Interface/ItemMenu.cs \
	src/Docky.Interface/SeparatorItem.cs \
	src/Docky.Interface/Util.cs \
	src/Docky.Utilities/DockPreferences.cs \
	src/Docky.Utilities/GtkUtils.cs \
	src/Docky.Utilities/WindowUtils.cs \
	src/XLib/X11Atoms.cs \
	src/XLib/Xlib.cs

RESOURCES = \
	Resources/Do.Interface.Linux.Docky.addin.xml

REFERENCES =  \
	System \
	System.Core \
	Mono.Cairo \
	Mono.Posix \
	$(GTK_SHARP_20_LIBS) \
	$(GNOME_DESKTOP_SHARP_20_LIBS) \
	$(WNCK_SHARP_10_LIBS)

PROJECT_REFERENCES = \
	Do.Interface.Linux \
	Do.Platform \
	Do.Universe
