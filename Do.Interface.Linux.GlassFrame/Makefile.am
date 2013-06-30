# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Interface.Linux.GlassFrame
TARGET = library

FILES = \
	AssemblyInfo.cs \
	src/GlassFrameRenderer.cs \
	src/GlassFrameTheme.cs

RESOURCES = \
	Resources/Do.Interface.Linux.GlassFrame.addin.xml

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
