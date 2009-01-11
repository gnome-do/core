EXTRA_DIST = COPYRIGHT

ACLOCAL_AMFLAGS = -I m4

SUBDIRS = \
	. \
	Do.Universe \
	Do.Platform \
	Do.Interface.Linux \
	Do.Interface.Linux.Classic \
	Do.Interface.Linux.Docky \
	Do.Interface.Linux.GlassFrame \
	Do.Interface.Linux.HUD \
	Do.Interface.Linux.Mini \
	Do.Platform.Linux \
	Do.DBus \
	Do \
	po \
	libdo \
	m4 \
	data

# Get rid of the build dir on clean.
clean-local:
	rm -rf $(top_builddir)/build
