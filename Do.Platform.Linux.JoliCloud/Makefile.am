# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

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
	$(NDESK_DBUS_10_LIBS) \
	$(NUNIT_LIBS) 

PROJECT_REFERENCES = \
	Do.Platform \
	Do.Platform.Linux
