dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])

AC_INIT([gnome-do], [0.9])
AM_INIT_AUTOMAKE([foreign tar-pax])

AC_CONFIG_MACRO_DIR([m4])

dnl -- Intl

IT_PROG_INTLTOOL([0.35.0])
SHAMROCK_CONFIGURE_I18N(gnome-do)

dnl -- Populate top_srcdir variable
top_srcdir=$(readlink -f $0 | sed -e s/configure$//)

dnl Plugin ABI version; bump whenever plugin interface changes
ABI_VERSION=0.11
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

dnl gio-sharp is an unstable api library
PKG_CHECK_MODULES([GIO_SHARP], [gio-sharp-2.0])
files="`$PKG_CONFIG --variable=Libraries gio-sharp-2.0`"
for file in $files; do
	GIO_SHARP_SOURCE_ASSEMBLIES="$GIO_SHARP_SOURCE_ASSEMBLIES $file"
	GIO_SHARP_ASSEMBLIES="$GIO_SHARP_ASSEMBLIES $(pwd)/build/$(basename $file)"
	[[ -r "$file.config" ]] && GIO_SHARP_SOURCE_ASSEMBLIES="$GIO_SHARP_SOURCE_ASSEMBLIES $file.config"
	[[ -r "$file.config" ]] && GIO_SHARP_ASSEMBLIES="$GIO_SHARP_ASSEMBLIES $(pwd)/build/$(basename $file).config"

	GIO_SHARP_LIBS="$GIO_SHARP_LIBS -r:$(pwd)/build/$(basename $file)"
done
AC_SUBST([GIO_SHARP_SOURCE_ASSEMBLIES])
AC_SUBST([GIO_SHARP_ASSEMBLIES])
AC_SUBST([GIO_SHARP_LIBS])

PKG_CHECK_MODULES([GNOME_KEYRING_SHARP], [gnome-keyring-sharp-1.0])
PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])
PKG_CHECK_MODULES([MONO_ADDINS], [mono-addins])
PKG_CHECK_MODULES([MONO_ADDINS_GUI], [mono-addins-gui])
PKG_CHECK_MODULES([MONO_ADDINS_SETUP], [mono-addins-setup])
PKG_CHECK_MODULES([NOTIFY_SHARP], [notify-sharp])
PKG_CHECK_MODULES([GKEYFILE_SHARP], [gkeyfile-sharp])


AC_SUBST([GCONF_SHARP_20_LIBS])
AC_SUBST([GLADE_SHARP_20_LIBS])
AC_SUBST([GLIB_SHARP_20_LIBS])
AC_SUBST([GNOME_KEYRING_SHARP_LIBS])
AC_SUBST([GTK_SHARP_20_LIBS])
AC_SUBST([MONO_ADDINS_LIBS])
AC_SUBST([MONO_ADDINS_GUI_LIBS])
AC_SUBST([MONO_ADDINS_SETUP_LIBS])
AC_SUBST([NOTIFY_SHARP_LIBS])
AC_SUBST([GKEYFILE_SHARP_LIBS])

dnl -- Use D-Bus# where available.
PKG_CHECK_MODULES([DBUS_SHARP], [dbus-sharp-1.0], HAVE_DBUS_SHARP=yes, HAVE_DBUS_SHARP=no)
PKG_CHECK_MODULES([DBUS_SHARP_GLIB], [dbus-sharp-glib-1.0], HAVE_DBUS_SHARP_GLIB=yes, HAVE_DBUS_SHARP_GLIB=no)

if test "x$HAVE_DBUS_SHARP" = "xyes" ; then
	DBUS_BUILD_DEFINES=-define:USE_DBUS_SHARP
else
   	PKG_CHECK_MODULES([DBUS_SHARP], [ndesk-dbus-1.0])
   	PKG_CHECK_MODULES([DBUS_SHARP_GLIB], [ndesk-dbus-glib-1.0])
fi

AC_SUBST([DBUS_BUILD_DEFINES])
AC_SUBST([DBUS_SHARP_LIBS])
AC_SUBST([DBUS_SHARP_GLIB_LIBS])

PKG_CHECK_MODULES([NUNIT], [nunit >= 2.5], have_nunit=yes, have_nunit=no)
AM_CONDITIONAL(HAVE_NUNIT, test "$have_nunit" = "yes")
if test "$have_nunit" != "yes" ; then
   NUNIT_LIBS=
fi
AC_SUBST([NUNIT_LIBS])

dnl {{{ enable debug release configuration
AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'DEBUG' Configuration [default=YES]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test "x$enable_debug" = "xyes")
if test "x$enable_debug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
dnl }}}

dnl {{{ enable release build configuration
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
dnl }}}

dnl Use stricter CFLAGS for non-release builds.
if test "x$enable_release" != "xyes" ; then
   	DEBUG_CFLAGS="-Wall -Werror \
		      -DGTK_DISABLE_DEPRECATED 	\
		      -DGDK_DISABLE_DEPRECATED 	\
		      -DG_DISABLE_DEPRECATED"
fi
AC_SUBST([DEBUG_CFLAGS])

dnl {{{ optionally enable jolicloud package service
AC_ARG_ENABLE(jolicloud,
	AC_HELP_STRING([--enable-jolicloud],
	[Build Do with support for the Jolicloud package manager [default=NO]]),
	enable_jolicloud=yes, enable_jolicloud=no)
AM_CONDITIONAL(ENABLE_JOLICLOUD, test "x$enable_jolicloud" = "xyes")
dnl }}}

AC_CONFIG_FILES([
Do/Makefile
Do/gnome-do
Do/src/AssemblyInfo.cs
Do.Interface.Linux/Makefile
Do.Interface.Linux/src/AssemblyInfo.cs
Do.Interface.Linux.AnimationBase/Makefile
Do.Interface.Linux.Classic/Makefile
Do.Interface.Linux.GlassFrame/Makefile
Do.Interface.Linux.HUD/Makefile
Do.Interface.Linux.Mini/Makefile
Do.Platform/Makefile
Do.Platform/src/AssemblyInfo.cs
Do.Platform.Linux/Makefile
Do.Platform.Linux/Do.Platform.Linux.dll.config
Do.Platform.Linux/src/AssemblyInfo.cs
Do.Platform.Linux.JoliCloud/Makefile
Do.Platform.Linux.JoliCloud/src/AssemblyInfo.cs
Do.Universe/Makefile
Do.Universe/src/AssemblyInfo.cs
libdo/Makefile
Makefile
data/Makefile
data/icons/Makefile
data/icons/hicolor/16x16/Makefile
data/icons/hicolor/16x16/apps/Makefile
data/icons/hicolor/24x24/Makefile
data/icons/hicolor/24x24/apps/Makefile
data/icons/hicolor/32x32/Makefile
data/icons/hicolor/32x32/apps/Makefile
data/icons/hicolor/48x48/Makefile
data/icons/hicolor/48x48/apps/Makefile
data/icons/hicolor/64x64/Makefile
data/icons/hicolor/64x64/apps/Makefile
data/icons/hicolor/128x128/Makefile
data/icons/hicolor/128x128/apps/Makefile
data/icons/hicolor/scalable/Makefile
data/icons/hicolor/scalable/apps/Makefile
data/icons/hicolor/Makefile
m4/Makefile
po/Makefile.in
Tests/Makefile
Tests/AssemblyInfo.cs
])

AC_OUTPUT

cat <<EOF

${PACKAGE}-${VERSION}

  Build Environment
    Install Prefix:     ${prefix}
    Mono C# Compiler:   ${MCS} ${GMCS_FLAGS}
    Mono Runtime:       ${MONO}

  Package Manager
    Jolicloud:          ${enable_jolicloud}

  Build/Development
    Release Build:      ${enable_release}
    NUnit:              ${have_nunit}

EOF
