# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Interface.Linux.Classic
TARGET = library

FILES = \
	AssemblyInfo.cs \
	src/ClassicRenderer.cs \
	src/ClassicTheme.cs

RESOURCES = \
	Resources/Do.Interface.Linux.Classic.addin.xml

REFERENCES =  \
	System \
	System.Core \
	Mono.Cairo \
	$(GTK_SHARP_20_LIBS) \
	$(GCONF_SHARP_20_LIBS)

PROJECT_REFERENCES = \
	Do.Interface.Linux \
	Do.Interface.Linux.AnimationBase \
	Do.Platform \
	Do.Platform.Linux
