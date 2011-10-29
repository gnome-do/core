EXTRA_DIST = COPYRIGHT

ACLOCAL_AMFLAGS = -I m4 -I m4/shamrock

SUBDIRS = \
	. \
	data \
	Do.Universe \
	Do.Platform \
	Do.Interface.Linux \
	Do.Platform.Linux \
	Do.Platform.Linux.JoliCloud \
	Do.Interface.Linux.AnimationBase \
	Do.Interface.Linux.Classic \
	Do.Interface.Linux.GlassFrame \
	Do.Interface.Linux.HUD \
	Do.Interface.Linux.Mini \
	Do \
	po \
	libdo \
	m4

if HAVE_NUNIT
SUBDIRS += Tests
endif

# Get rid of the build dir on clean.
clean-local:
	rm -rf $(top_builddir)/build
