SUBDIRS = icons

EXTRA_DIST = $(schemas_in_files) gnome-do.desktop.in 
CLEANFILES = $(schemas_DATA) gnome-do.desktop

schemasdir	 = $(GCONF_SCHEMA_FILE_DIR)
schemas_in_files = gnome-do.schemas.in
schemas_DATA     = $(schemas_in_files:.schemas.in=.schemas)

applicationsdir = $(datadir)/applications
applications_DATA = gnome-do.desktop

startupdir = $(sysconfdir)/xdg/autostart
startup_DATA = gnome-do.desktop

@INTLTOOL_DESKTOP_RULE@

@INTLTOOL_SCHEMAS_RULE@

if GCONF_SCHEMAS_INSTALL
install-data-local:
	GCONF_CONFIG_SOURCE=$(GCONF_SCHEMA_CONFIG_SOURCE) \
	$(GCONFTOOL) --makefile-install-rule $(schemas_DATA)
endif
