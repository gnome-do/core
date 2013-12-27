# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Platform.Linux
TARGET = library

FILES = \
	gtk-gui/Do.Platform.Linux.AbstractLoginWidget.cs \
	gtk-gui/Do.Platform.Linux.PluginAvailableDialog.cs \
	gtk-gui/generated.cs \
	src/AssemblyInfo.cs \
	src/Do.Platform/ConsoleCrayon.cs \
	src/Do.Platform/Do.Platform.Linux/Do.Platform.Linux.DBus/IController.cs \
	src/Do.Platform/Do.Platform.Linux/Do.Platform.Linux.DBus/Registrar.cs \
	src/Do.Platform/Do.Platform.Linux/ConsoleLogService.cs \
	src/Do.Platform/Do.Platform.Linux/EnvironmentService.cs \
	src/Do.Platform/Do.Platform.Linux/EnvironmentServiceExtensions.cs \
	src/Do.Platform/Do.Platform.Linux/SystemService.cs \
	src/Do.Platform/Do.Platform.Linux/GConfPreferencesService.cs \
	src/Do.Platform/Do.Platform.Linux/GnomeKeyringSecurePreferencesService.cs \
	src/Do.Platform/Do.Platform.Linux/IConfigurable.cs \
	src/Do.Platform/Do.Platform.Linux/IconProvider.cs \
	src/Do.Platform/Do.Platform.Linux/NotificationHelper.cs \
	src/Do.Platform/Do.Platform.Linux/TrayIconPreferences.cs \
	src/Do.Platform/Do.Platform.Linux/TrayIconService.cs \
	src/Do.Platform/Do.Platform.Linux/UniverseFactoryService.cs \
	src/Do.Platform/Do.Platform.Linux/NetworkService.cs \
	src/Do.Platform/Do.Platform.Linux/KeyBindingService.cs \
	src/Do.Universe/ApplicationItem.cs \
	src/Do.Universe/CategoryItem.cs \
	src/Do.Universe/Desktop.cs \
	src/Do.Universe/DesktopThumbnailSize.cs \
	src/Do.Universe/NullApplicationItem.cs \
	src/Do.Universe/FileItem.cs \
	src/Do.Universe/ApplicationItemSource.cs \
	src/Do.Universe/GNOMEBookmarkItem.cs \
	src/Do.Universe/GNOMESpecialLocationsItemSource.cs \
	src/Do.Universe/GNOMETrashItem.cs \
	src/Do.Widgets/AbstractLoginWidget.cs \
	src/Do.Widgets/PluginAvailableDialog.cs

if HAVE_NUNIT
FILES += \
	src/Do.Platform/Do.Platform.Linux/Tests/GConfPreferencesServiceTests.cs
endif

RESOURCES =  \
	gtk-gui/gui.stetic \
	Resources/Do.Platform.Linux.addin.xml


PKG_CONFIG_FILES = do.platform.linux.pc

REFERENCES =  \
	System \
	System.Core \
	Mono.Posix \
	$(GTK_SHARP_20_LIBS) \
	$(GNOME_KEYRING_SHARP_LIBS) \
	$(GKEYFILE_SHARP_LIBS) \
	$(GIO_SHARP_LIBS) \
	$(GNOME_DESKTOP_SHARP_20_LIBS) \
	$(GCONF_SHARP_20_LIBS) \
	$(MONO_ADDINS_LIBS) \
	$(DBUS_SHARP_LIBS) \
	$(DBUS_SHARP_GLIB_LIBS) \
	$(NOTIFY_SHARP_LIBS) \
	$(NUNIT_LIBS)

PROJECT_REFERENCES = \
	Do.Universe \
	Do.Platform

CLEANFILES += Do.Platform.Linux.dll.config
EXTRA_DIST += Do.Platform.Linux.dll.config.in

module_DATA += Do.Platform.Linux.dll.config
