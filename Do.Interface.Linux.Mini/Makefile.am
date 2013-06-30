# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Interface.Linux.Mini
TARGET = library

FILES = \
	AssemblyInfo.cs \
	src/MiniRenderer.cs \
	src/MiniTheme.cs

RESOURCES = \
	Resources/Do.Interface.Linux.Mini.addin.xml

REFERENCES =  \
	System \
	System.Core \
	Mono.Cairo \
	$(GTK_SHARP_20_LIBS)

PROJECT_REFERENCES = \
	Do.Interface.Linux \
	Do.Interface.Linux.AnimationBase \
	Do.Platform \
	Do.Platform.Linux
