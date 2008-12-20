# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Addins
TARGET = library
PKG_CONFIG_FILES = do.addins.pc

FILES = \
	AssemblyInfo.cs \
	src/Do.Addins/IConfigurable.cs 


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

PROJECT_REFERENCES = \
	Do.Universe \
	Do.Platform \
	Do.Platform.Linux \
	Do.Interface.Linux
