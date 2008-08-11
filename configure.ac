dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])

dnl A change in the plugin API/ABI should be accompanied by a Minor/Major revision bump
dnl ...give or take!
AC_INIT([gnome-do], [0.5.99.0])
AM_INIT_AUTOMAKE([foreign])

AC_CONFIG_MACRO_DIR([m4])

dnl -- Intl

IT_PROG_INTLTOOL([0.35.0])
SHAMROCK_CONFIGURE_I18N(gnome-do)


ASM_VERSION="$VERSION"
AC_SUBST(ASM_VERSION)

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

AC_PROG_INSTALL
AC_PROG_CC
AC_HEADER_STDC

SHAMROCK_FIND_MONO_2_0_COMPILER
SHAMROCK_CHECK_LINQ_FLAG

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
PKG_CHECK_MODULES([MONO_ADDINS], [mono-addins])
PKG_CHECK_MODULES([MONO_ADDINS_GUI], [mono-addins-gui])
PKG_CHECK_MODULES([MONO_ADDINS_SETUP], [mono-addins-setup])
PKG_CHECK_MODULES([NDESK_DBUS_10], [ndesk-dbus-1.0])
PKG_CHECK_MODULES([NDESK_DBUS_GLIB_10], [ndesk-dbus-glib-1.0])
PKG_CHECK_MODULES([NOTIFY_SHARP], [notify-sharp])

AC_SUBST([GCONF_SHARP_20_LIBS])
AC_SUBST([GLADE_SHARP_20_LIBS])
AC_SUBST([GLIB_SHARP_20_LIBS])
AC_SUBST([GNOME_DESKTOP_SHARP_20_LIBS])
AC_SUBST([GNOME_KEYRING_SHARP_LIBS])
AC_SUBST([GNOME_SHARP_20_LIBS])
AC_SUBST([GNOME_VFS_SHARP_20_LIBS])
AC_SUBST([GTK_SHARP_20_LIBS])
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
libdo/Makefile
Do.Addins/Makefile
Makefile
Do.DBus/Makefile
Do/src/AssemblyInfo.cs
Do.Addins/AssemblyInfo.cs
Do.DBus/src/AssemblyInfo.cs
Do/Makefile
Do/gnome-do
Do/Do.exe.config
Do.Addins/do.addins.pc
Do.DBus/do.dbus.pc
m4/Makefile
po/Makefile.in
])

AC_OUTPUT
