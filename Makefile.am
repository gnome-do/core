EXTRA_DIST = COPYRIGHT

ACLOCAL_AMFLAGS = -I m4 -I m4/shamrock

SUBDIRS = \
	. \
	BundledLibraries \
	data \
	Do.Universe \
	Do.Platform \
	Do.Interface.Wink \
	Do.Interface.Linux \
	Do.Platform.Linux \
	Do.Platform.Linux.JoliCloud \
	Do.Interface.Linux.AnimationBase \
	Do.Interface.Linux.Classic \
	Do.Interface.Linux.Docky \
	Do.Interface.Linux.GlassFrame \
	Do.Interface.Linux.HUD \
	Do.Interface.Linux.Mini \
	Do \
	po \
	libdo \
	m4

# Get rid of the build dir on clean.
clean-local:
	rm -rf $(top_builddir)/build
