# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Platform
TARGET = library

PKG_CONFIG_FILES = do.platform.pc

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Platform/ActionableNotification.cs \
	src/Do.Platform/Do.Platform.Common/AbstractLogService.cs \
	src/Do.Platform/Do.Platform.Common/CommonPathsService.cs \
	src/Do.Platform/Do.Platform.Common/DictionaryPreferencesService.cs \
	src/Do.Platform/Do.Platform.Common/EventsOnlyNotificationsService.cs \
	src/Do.Platform/Do.Platform.Common/FileLogService.cs \
	src/Do.Platform/Do.Platform.Common/AbstractKeyBindingService.cs \
	src/Do.Platform/Do.Platform.Default/CoreService.cs \
	src/Do.Platform/Do.Platform.Default/DefaultApplicationService.cs \
	src/Do.Platform/Do.Platform.Default/DefaultSystemService.cs \
	src/Do.Platform/Do.Platform.Default/DefaultPackageManagerService.cs \
	src/Do.Platform/Do.Platform.Default/DefaultPathsService.cs \
	src/Do.Platform/Do.Platform.Default/EnvironmentService.cs \
	src/Do.Platform/Do.Platform.Default/LogService.cs \
	src/Do.Platform/Do.Platform.Default/NotificationsService.cs \
	src/Do.Platform/Do.Platform.Default/PluginManagerService.cs \
	src/Do.Platform/Do.Platform.Default/PreferencesService.cs \
	src/Do.Platform/Do.Platform.Default/SecurePreferencesService.cs \
	src/Do.Platform/Do.Platform.Default/UniverseFactoryService.cs \
	src/Do.Platform/Do.Platform.Default/WindowingService.cs \
	src/Do.Platform/Do.Platform.Default/NetworkService.cs \
	src/Do.Platform/Do.Platform.Default/KeyBindingService.cs \
	src/Do.Platform/Do.Platform.Preferences/PreferencesFactory.cs \
	src/Do.Platform/Do.Platform.Preferences/PreferencesImplementation.cs \
	src/Do.Platform/Do.Platform.ServiceStack/IInitializedService.cs \
	src/Do.Platform/Do.Platform.ServiceStack/IService.cs \
	src/Do.Platform/Do.Platform.ServiceStack/IStrictService.cs \
	src/Do.Platform/ICoreService.cs \
	src/Do.Platform/IEnvironmentService.cs \
	src/Do.Platform/ILogService.cs \
	src/Do.Platform/INotificationsService.cs \
	src/Do.Platform/IPluginManagerService.cs \
	src/Do.Platform/IPreferences.cs \
	src/Do.Platform/IPreferencesService.cs \
	src/Do.Platform/ISecurePreferencesService.cs \
	src/Do.Platform/IUniverseFactoryService.cs \
	src/Do.Platform/IWindowingService.cs \
	src/Do.Platform/LogBase.cs \
	src/Do.Platform/Log.cs \
	src/Do.Platform/LogTSender.cs \
	src/Do.Platform/Notification.cs \
	src/Do.Platform/NotificationEventArgs.cs \
	src/Do.Platform/AbstractApplicationService.cs \
	src/Do.Platform/AbstractPackageManagerService.cs \
	src/Do.Platform/AbstractSystemService.cs \
	src/Do.Platform/PathsService.cs \
	src/Do.Platform/PreferencesChangedEventArgs.cs \
	src/Do.Platform/SecurePreferencesWrapper.cs \
	src/Do.Platform/Services.cs \
	src/Do.Platform/INetworkService.cs \
	src/Do.Platform/NetworkStateChangedEventArgs.cs \
	src/Do.Platform/IKeyBindingService.cs \
	src/Do.Platform/KeyBinding.cs \
	src/Do.Universe/Do.Universe.Common/EmailAction.cs \
	src/Do.Universe/Do.Universe.Common/OpenAction.cs \
	src/Do.Universe/Do.Universe.Common/OpenUrlAction.cs \
	src/Do.Universe/Do.Universe.Common/OpenWithAction.cs \
	src/Do.Universe/Do.Universe.Common/RevealAction.cs \
	src/Do.Universe/Do.Universe.Common/RunAction.cs \
	src/Do.Universe/Do.Universe.Common/CopyToClipboardAction.cs \
	src/System/Linq/EnumerableExtensions.cs

if HAVE_NUNIT
FILES += \
	src/Do.Platform/Do.Platform.Common/Tests/DictionaryPreferencesServiceTests.cs \
	src/Do.Platform/Do.Platform.Preferences/Tests/PreferencesImplementationTests.cs
endif

#
# For reasons which escape me, building the addin.xml as a resource breaks
# prefix= builds.  Ship it separately instead.
#
# -- (David) but it completely breaks Do for me!
#
RESOURCES = \
	Resources/Do.Platform.addin.xml
# module_DATA += $(srcdir)/Resources/Do.Platform.addin.xml

PROJECT_REFERENCES= \
	Do.Universe

REFERENCES =  \
	System \
	System.Core \
	Mono.Posix \
	$(GTK_SHARP_20_LIBS) \
	$(MONO_ADDINS_LIBS) \
	$(NUNIT_LIBS)
