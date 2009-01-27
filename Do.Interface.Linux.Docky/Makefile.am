# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Interface.Linux.Docky
TARGET = library

FILES = \
	AssemblyInfo.cs \
	src/Docky.Core/DockServices.cs \
	src/Docky.Core/IDockService.cs \
	src/Docky.Core/IDrawingService.cs \
	src/Docky.Core/IItemsService.cs \
	src/Docky.Core/IPainterService.cs \
	src/Docky.Core/IDoInteropService.cs \
	src/Docky.Core/Docky.Core.Default/ItemsService.cs \
	src/Docky.Core/Docky.Core.Default/DrawingService.cs \
	src/Docky.Core/Docky.Core.Default/DoInteropService.cs \
	src/Docky.Core/Docky.Core.Default/PainterService.cs \
	src/Docky.Interface/BaseDockItem.cs \
	src/Docky.Interface/ApplicationDockItem.cs \
	src/Docky.Interface/ClickAnimationType.cs \
	src/Docky.Interface/DoDockItem.cs \
	src/Docky.Interface/DockArea.cs \
	src/Docky.Interface/DockAnimationState.cs \
	src/Docky.Interface/DockItem.cs \
	src/Docky.Interface/DoInteropService.cs \
	src/Docky.Interface/Docky.Interface.Menus/AbstractMenuButtonArgs.cs \
	src/Docky.Interface/Docky.Interface.Menus/DockItemMenu.cs \
	src/Docky.Interface/Docky.Interface.Menus/DockPopupMenu.cs \
	src/Docky.Interface/Docky.Interface.Menus/LaunchMenuButtonArgs.cs \
	src/Docky.Interface/Docky.Interface.Menus/RunnableMenuButtonArgs.cs \
	src/Docky.Interface/Docky.Interface.Menus/SimpleMenuButtonArgs.cs \
	src/Docky.Interface/Docky.Interface.Menus/SeparatorMenuButtonArgs.cs \	
	src/Docky.Interface/Docky.Interface.Menus/WindowMenuButtonArgs.cs \
	src/Docky.Interface/DockState.cs \
	src/Docky.Interface/DockWindow.cs \
	src/Docky.Interface/Docky.Interface.Renderers/DockBackgroundRenderer.cs \
	src/Docky.Interface/Docky.Interface.Renderers/SummonModeRenderer.cs \
	src/Docky.Interface/DragState.cs \
	src/Docky.Interface/DrawState.cs \
	src/Docky.Interface/IDockPainter.cs \
	src/Docky.Interface/IRightClickable.cs \
	src/Docky.Interface/ItemPositionProvider.cs \
	src/Docky.Interface/LayoutUtils.cs \
	src/Docky.Interface/PainterService.cs \
	src/Docky.Interface/SeparatorItem.cs \
	src/Docky.Interface/TrashDockItem.cs \
	src/Docky.Interface/UpdateRequestArgs.cs \
	src/Docky.Interface/Util.cs \
	src/Docky.Utilities/DockOrientation.cs \
	src/Docky.Utilities/DockPreferences.cs \
	src/Docky.Utilities/GtkUtils.cs \
	src/Docky.Utilities/WindowControl.cs \
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

module_DATA += $(ASSEMBLY).dll.config
EXTRA_DIST += $(ASSEMBLY).dll.config
