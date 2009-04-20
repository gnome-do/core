dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])

AC_INIT([gnome-do], [0.8.1.2])
AM_INIT_AUTOMAKE([foreign tar-pax])

AC_CONFIG_MACRO_DIR([m4])

dnl -- Intl

IT_PROG_INTLTOOL([0.35.0])
SHAMROCK_CONFIGURE_I18N(gnome-do)

dnl -- Populate top_srcdir variable
top_srcdir=$(readlink -f $0 | sed -e s/configure$//)

dnl Plugin ABI version; bump whenever plugin interface changes
ABI_VERSION=0.9
AC_SUBST(ABI_VERSION)

if test -d $top_srcdir/.bzr ; then
   VERSION_INFO=`bzr version-info --custom \
   --template="bzr {branch_nick} r{revno}"`
else
   VERSION_INFO="Release"
fi
AC_SUBST(VERSION_INFO)

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

AC_PROG_INSTALL
AC_PROG_CC
AC_HEADER_STDC

SHAMROCK_FIND_C_SHARP_3_0_COMPILER


dnl Find gconftool-2
AC_PATH_PROG(GCONFTOOL, gconftool-2)
AM_GCONF_SOURCE_2

dnl libtool for libdo
AM_DISABLE_STATIC
AM_PROG_LIBTOOL

dnl Expansions
SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR


PKG_CHECK_MODULES(LIBDO, glib-2.0 gdk-2.0 gdk-x11-2.0 gtk+-2.0)
AC_SUBST(LIBDO_CFLAGS)
AC_SUBST(LIBDO_LIBS)

PKG_CHECK_MODULES([GCONF_SHARP_20], [gconf-sharp-2.0])
PKG_CHECK_MODULES([GLADE_SHARP_20], [glade-sharp-2.0])
PKG_CHECK_MODULES([GLIB_SHARP_20], [glib-sharp-2.0])
PKG_CHECK_MODULES([GNOME_DESKTOP_SHARP_20], [gnome-desktop-sharp-2.0])
PKG_CHECK_MODULES([GNOME_KEYRING_SHARP], [gnome-keyring-sharp-1.0])
PKG_CHECK_MODULES([GNOME_SHARP_20], [gnome-sharp-2.0])
PKG_CHECK_MODULES([GNOME_VFS_SHARP_20], [gnome-vfs-sharp-2.0])
PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])
PKG_CHECK_MODULES([LIBRSVG_SHARP], [rsvg2-sharp-2.0])
PKG_CHECK_MODULES([MONO_ADDINS], [mono-addins])
PKG_CHECK_MODULES([MONO_ADDINS_GUI], [mono-addins-gui])
PKG_CHECK_MODULES([MONO_ADDINS_SETUP], [mono-addins-setup])
PKG_CHECK_MODULES([NDESK_DBUS_10], [ndesk-dbus-1.0])
PKG_CHECK_MODULES([NDESK_DBUS_GLIB_10], [ndesk-dbus-glib-1.0])
PKG_CHECK_MODULES([NOTIFY_SHARP], [notify-sharp])
PKG_CHECK_MODULES([WNCK_SHARP_10], [wnck-sharp-1.0])


AC_SUBST([GCONF_SHARP_20_LIBS])
AC_SUBST([GLADE_SHARP_20_LIBS])
AC_SUBST([GLIB_SHARP_20_LIBS])
AC_SUBST([GNOME_DESKTOP_SHARP_20_LIBS])
AC_SUBST([GNOME_KEYRING_SHARP_LIBS])
AC_SUBST([GNOME_SHARP_20_LIBS])
AC_SUBST([GNOME_VFS_SHARP_20_LIBS])
AC_SUBST([GTK_SHARP_20_LIBS])
AC_SUBST([LIBRSVG_SHARP_LIBS])
AC_SUBST([MONO_ADDINS_LIBS])
AC_SUBST([MONO_ADDINS_GUI_LIBS])
AC_SUBST([MONO_ADDINS_SETUP_LIBS])
AC_SUBST([NDESK_DBUS_10_LIBS])
AC_SUBST([NDESK_DBUS_GLIB_10_LIBS])
AC_SUBST([NOTIFY_SHARP_LIBS])


PKG_CHECK_MODULES([NUNIT], [nunit], have_nunit=yes, have_nunit=no)
AM_CONDITIONAL(HAVE_NUNIT, test "$have_nunit" = "yes")
if test "$have_nunit" != "yes" ; then
   NUNIT_LIBS=
fi
AC_SUBST([NUNIT_LIBS])


AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'DEBUG' Configuration [default=YES]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test x$enable_debug = xyes)
if test "x$enable_debug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'RELEASE' Configuration [default=NO]]),
		enable_release=yes, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test x$enable_release = xyes)
if test "x$enable_release" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
	AM_CONDITIONAL(ENABLE_DEBUG, true)
fi

AC_CONFIG_FILES([
BundledLibraries/Makefile
Do/Do.exe.config
Do/Makefile
Do/gnome-do
Do/src/AssemblyInfo.cs
Do.Interface.Linux/Makefile
Do.Interface.Linux/src/AssemblyInfo.cs
Do.Interface.Linux.AnimationBase/Makefile
Do.Interface.Linux.Classic/Makefile
Do.Interface.Linux.Docky/Makefile
Do.Interface.Linux.GlassFrame/Makefile
Do.Interface.Linux.HUD/Makefile
Do.Interface.Linux.Mini/Makefile
Do.Interface.Wink/Makefile
Do.Platform/Makefile
Do.Platform/src/AssemblyInfo.cs
Do.Platform.Linux/Makefile
Do.Platform.Linux/src/AssemblyInfo.cs
Do.Platform.OSX/AssemblyInfo.cs
Do.Platform.Windows/AssemblyInfo.cs
Do.Universe/Makefile
Do.Universe/src/AssemblyInfo.cs
libdo/Makefile
Makefile
data/Makefile
data/Docky.DefaultClockTheme/Makefile
data/icons/Makefile
data/icons/hicolor/16x16/Makefile
data/icons/hicolor/16x16/apps/Makefile
data/icons/hicolor/22x22/Makefile
data/icons/hicolor/22x22/apps/Makefile
data/icons/hicolor/24x24/Makefile
data/icons/hicolor/24x24/apps/Makefile
data/icons/hicolor/32x32/Makefile
data/icons/hicolor/32x32/apps/Makefile
data/icons/hicolor/48x48/Makefile
data/icons/hicolor/48x48/apps/Makefile
data/icons/hicolor/256x256/Makefile
data/icons/hicolor/256x256/apps/Makefile
data/icons/hicolor/scalable/Makefile
data/icons/hicolor/scalable/apps/Makefile
data/icons/hicolor/Makefile
m4/Makefile
po/Makefile.in
])

AC_OUTPUT
