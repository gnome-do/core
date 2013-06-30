# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Interface.Linux.HUD
TARGET = library

FILES = \
	AssemblyInfo.cs \
	src/HUDRenderer.cs \
	src/HUDTheme.cs

RESOURCES = \
	Resources/Do.Interface.Linux.HUD.addin.xml

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
