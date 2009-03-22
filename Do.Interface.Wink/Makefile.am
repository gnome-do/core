# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Interface.Wink
TARGET = library

PKG_CONFIG_FILES = do.interface.wink.pc

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Interface.Wink/ClickAction.cs \
	src/Do.Interface.Wink/WindowControl.cs \
	src/Do.Interface.Wink/WindowUtils.cs

#RESOURCES = \
#	Resources/Do.Interface.Wnck.addin.xml

REFERENCES =  \
	System \
	System.Core \
	$(GTK_SHARP_20_LIBS) \
	$(WNCK_SHARP_10_LIBS)

PROJECT_REFERENCES = \
	Do.Platform

#module_DATA += $(ASSEMBLY).dll.config
#EXTRA_DIST += $(ASSEMBLY).dll.config
