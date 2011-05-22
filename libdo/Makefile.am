INCLUDES = -I$(top_srcdir)

AM_CFLAGS = -DG_LOG_DOMAIN=\"libdo\"	\
	    -DEGG_COMPILATION 		\
	    $(DEBUG_CFLAGS)		\
	    $(LIBDO_CFLAGS)


dolibdir = $(pkglibdir)
dolib_LTLIBRARIES = libdo.la

libdo_la_SOURCES =		\
	keybinder.h	\
	keybinder.c	\
	eggaccelerators.h	\
	eggaccelerators.c

libdo_la_LDFLAGS = -export-dynamic -module -avoid-version
libdo_la_LIBADD = $(LIBDO_LIBS)

maintainer-clean-local:
	rm -f Makefile.in
