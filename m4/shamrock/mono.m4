AC_DEFUN([SHAMROCK_FIND_MONO_1_0_COMPILER],
[
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MCS, mcs)
])

AC_DEFUN([SHAMROCK_FIND_MONO_2_0_COMPILER],
[
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MCS, gmcs)
])

AC_DEFUN([SHAMROCK_FIND_C_SHARP_3_0_COMPILER],
[	
	AC_REQUIRE([SHAMROCK_FIND_MONO_RUNTIME])
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MCS, gmcs)
	changequote(<<, >>)
	MCS_VERSION=$($MCS --version | egrep -o "([[:digit:]]\.)+[[:digit:]]+")
	changequote([, ])
	AS_VERSION_COMPARE([$MCS_VERSION], [2.0], [MCS_TOO_OLD="true"])
	if test "$MCS_TOO_OLD" = "true" ; then
	   AC_MSG_WARN(["System gmcs too old (found $MCS_VERSION, need >= 2.0).  Using internal copy"])
	   MCS="$MONO $top_srcdir/BundledLibraries/gmcs.exe"
	fi
])

AC_DEFUN([SHAMROCK_FIND_MONO_RUNTIME],
[
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MONO, mono)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_MODULE],
[
	PKG_CHECK_MODULES(MONO_MODULE, mono >= $1)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_MODULE_NOBAIL],
[
	PKG_CHECK_MODULES(MONO_MODULE, mono >= $1, 
		HAVE_MONO_MODULE=yes, HAVE_MONO_MODULE=no)
	AC_SUBST(HAVE_MONO_MODULE)
])

AC_DEFUN([SHAMROCK_CHECK_LINQ_FLAG],
[
	AC_MSG_CHECKING([for LINQ flag for mcs])
	if $PKG_CONFIG --atleast-version=1.9 mono ; then
	   AC_MSG_RESULT([none needed])
	   MCS_LINQ_FLAG=
	else
	   AC_MSG_RESULT([-langversion:linq])
	   MCS_LINQ_FLAG=-langversion:linq
	fi
	AC_SUBST(MCS_LINQ_FLAG)		
])


AC_DEFUN([_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES],
[
	for asm in $(echo "$*" | cut -d, -f2- | sed 's/\,/ /g')
	do
		AC_MSG_CHECKING([for Mono $1 GAC for $asm.dll])
		if test \
			-e "$($PKG_CONFIG --variable=libdir mono)/mono/$1/$asm.dll" -o \
			-e "$($PKG_CONFIG --variable=prefix mono)/lib/mono/$1/$asm.dll"; \
			then \
			AC_MSG_RESULT([found])
		else
			AC_MSG_RESULT([not found])
			AC_MSG_ERROR([missing reqired Mono $1 assembly: $asm.dll])
		fi
	done
])

AC_DEFUN([SHAMROCK_CHECK_MONO_1_0_GAC_ASSEMBLIES],
[
	_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES(1.0, $*)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_2_0_GAC_ASSEMBLIES],
[
	_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES(2.0, $*)
])


