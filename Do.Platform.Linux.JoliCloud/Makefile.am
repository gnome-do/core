# Simple component buildsystem
if ENABLE_JOLICLOUD
include $(top_srcdir)/build.rules.mk
endif

ASSEMBLY = Do.Platform.Linux.JoliCloud
TARGET = library

FILES = \
	src/Do.Platform/Do.Platform.Linux/Do.Platform.Linux.JoliCloud/PackageManagerService.cs

if HAVE_NUNIT
FILES += \
	src/Do.Platform/Do.Platform.Linux/Do.Platform.Linux.JoliCloud/Tests/PackageManagerServiceTests.cs
endif

RESOURCES =  \
	Resources/Do.Platform.Linux.JoliCloud.addin.xml


PKG_CONFIG_FILES = do.platform.linux.jolicloud.pc

REFERENCES =  \
	System \
	System.Core \
	Mono.Posix \
	$(MONO_ADDINS_LIBS) \
	$(DBUS_SHARP_LIBS) \
	$(DBUS_SHARP_GLIB_LIBS) \
	$(NUNIT_LIBS) 

PROJECT_REFERENCES = \
	Do.Platform \
	Do.Platform.Linux
