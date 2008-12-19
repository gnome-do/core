# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.DBus
TARGET = library

FILES =  \
	src/AssemblyInfo.cs \
	src/DBusRegistrar.cs \
	src/IController.cs \
	src/PowerState.cs 

REFERENCES =  \
	$(NDESK_DBUS_10_LIBS) \
	$(NDESK_DBUS_GLIB_10_LIBS) \
	System

PKG_CONFIG_FILES = do.dbus.pc
