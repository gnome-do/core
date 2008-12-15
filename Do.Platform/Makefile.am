# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Platform
TARGET = library

PKG_CONFIG_FILES = do.platform.pc

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Platform/Do.Platform.Common/AbstractLogService.cs \
	src/Do.Platform/Do.Platform.Common/DictionaryPreferencesService.cs \
	src/Do.Platform/Do.Platform.Common/FileLogService.cs \
	src/Do.Platform/Do.Platform.Default/CoreService.cs \
	src/Do.Platform/Do.Platform.Default/EnvironmentService.cs \
	src/Do.Platform/Do.Platform.Default/LogService.cs \
	src/Do.Platform/Do.Platform.Default/PreferencesService.cs \
	src/Do.Platform/Do.Platform.Preferences/PreferencesFactory.cs \
	src/Do.Platform/Do.Platform.Preferences/PreferencesImplementation.cs \
	src/Do.Platform/Do.Platform.ServiceStack/IInitializedService.cs \
	src/Do.Platform/Do.Platform.ServiceStack/IService.cs \
	src/Do.Platform/ICoreService.cs \
	src/Do.Platform/IEnvironmentService.cs \
	src/Do.Platform/ILogService.cs \
	src/Do.Platform/IPreferences.cs \
	src/Do.Platform/IPreferencesService.cs \
	src/Do.Platform/Icons.cs \
	src/Do.Platform/Log.cs \
	src/Do.Platform/Notifications.cs \
	src/Do.Platform/Paths.cs \
	src/Do.Platform/PreferencesChangedEventArgs.cs \
	src/Do.Platform/Services.cs \
	src/Do.Platform/StatusIcon.cs \
	src/Do.Platform/UniverseFactory.cs \
	src/Do.Platform/Windowing.cs \
	src/Do.Universe/Do.Universe.Common/EmailAction.cs \
	src/Do.Universe/Do.Universe.Common/OpenAction.cs \
	src/Do.Universe/Do.Universe.Common/OpenUrlAction.cs \
	src/Do.Universe/Do.Universe.Common/OpenWithAction.cs \
	src/Do.Universe/Do.Universe.Common/RevealAction.cs \
	src/Do.Universe/Do.Universe.Common/RunAction.cs \
	src/Do.Universe/Do.Universe.Common/TextItem.cs \
	src/EnumerableExtensions.cs

if HAVE_NUNIT
FILES += \
	src/Do.Platform/Do.Platform.Common/Tests/DictionaryPreferencesServiceTests.cs \
	src/Do.Platform/Do.Platform.Preferences/Tests/PreferencesImplementationTests.cs
endif

RESOURCES = \
	Resources/Do.Platform.addin.xml

PROJECT_REFERENCES= \
	Do.Universe

REFERENCES =  \
	System \
	System.Core \
	Mono.Posix \
	$(MONO_ADDINS_LIBS) \
	$(NUNIT_LIBS)
