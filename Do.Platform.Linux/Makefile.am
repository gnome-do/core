# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Platform.Linux
TARGET = library

FILES = \
	gtk-gui/Do.Platform.Linux.AbstractLoginWidget.cs \
        gtk-gui/generated.cs \
	src/AssemblyInfo.cs \
	src/Do.Platform/ConsoleCrayon.cs \
	src/Do.Platform/Do.Platform.Linux/ConsoleLogService.cs \
	src/Do.Platform/Do.Platform.Linux/EnvironmentService.cs \
	src/Do.Platform/Do.Platform.Linux/GConfPreferencesService.cs \
	src/Do.Platform/Do.Platform.Linux/GnomeKeyringSecurePreferencesService.cs \
	src/Do.Platform/Do.Platform.Linux/NotificationHelper.cs \
	src/Do.Platform/Do.Platform.Linux/TrayIconPreferences.cs \
	src/Do.Platform/Do.Platform.Linux/TrayIconService.cs \
	src/Do.Platform/Do.Platform.Linux/UniverseFactoryService.cs \
	src/Do.Universe/ApplicationItem.cs \
	src/Do.Universe/FileItem.cs \
	src/Do.Universe/ApplicationItemSource.cs \
	src/Do.Universe/CopyToClipboardAction.cs \
	src/Do.Universe/GNOMESpecialLocationsItemSource.cs \
	src/Do.Widgets/AbstractLoginWidget.cs

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
	$(NUNIT_LIBS) \
	$(GTK_SHARP_20_LIBS) \
	$(GNOME_KEYRING_SHARP_LIBS) \
	$(GNOME_SHARP_20_LIBS) \
	$(GNOME_DESKTOP_SHARP_20_LIBS) \
	$(GCONF_SHARP_20_LIBS) \
	$(NOTIFY_SHARP_LIBS)

PROJECT_REFERENCES = \
	Do.Universe \
	Do.Platform \
	Do.Interface.Linux
