EXTRA_DIST = 

include $(top_srcdir)/Makefile.include

ASSEMBLY_COMPILER_COMMAND = $(MCS)
ASSEMBLY_COMPILER_FLAGS = $(MCS_LINQ_FLAG) -noconfig -codepage:utf8 -warn:4
ASSEMBLY = Do.Platform.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
BUILD_DIR = ./fake

if ENABLE_DEBUG
ASSEMBLY_COMPILER_FLAGS += -debug -d:DEBUG
endif

all: $(ASSEMBLY)

EXTRAS := do.platform.pc.in
LINUX_PKGCONFIG := do.platform.pc

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Platform/Do.Platform.Common/AbstractLogService.cs \
	src/Do.Platform/Do.Platform.Common/DictionaryPreferencesService.cs \
	src/Do.Platform/Do.Platform.Common/FileLogService.cs \
	src/Do.Platform/Do.Platform.Default/CoreService.cs \
	src/Do.Platform/Do.Platform.Default/EnvironmentService.cs \
	src/Do.Platform/Do.Platform.Default/LogService.cs \
	src/Do.Platform/Do.Platform.Default/NotificationsService.cs \
	src/Do.Platform/Do.Platform.Default/PreferencesService.cs \
	src/Do.Platform/Do.Platform.Default/UniverseFactoryService.cs \
	src/Do.Platform/Do.Platform.Default/WindowingService.cs \
	src/Do.Platform/Do.Platform.Preferences/PreferencesFactory.cs \
	src/Do.Platform/Do.Platform.Preferences/PreferencesImplementation.cs \
	src/Do.Platform/Do.Platform.ServiceStack/IInitializedService.cs \
	src/Do.Platform/Do.Platform.ServiceStack/IService.cs \
	src/Do.Platform/ICoreService.cs \
	src/Do.Platform/IEnvironmentService.cs \
	src/Do.Platform/ILogService.cs \
	src/Do.Platform/INotificationsService.cs \
	src/Do.Platform/NotificationEventArgs.cs \
	src/Do.Platform/IPreferences.cs \
	src/Do.Platform/IPreferencesService.cs \
	src/Do.Platform/IUniverseFactoryService.cs \
	src/Do.Platform/IWindowingService.cs \
	src/Do.Platform/Log.cs \
	src/Do.Platform/Paths.cs \
	src/Do.Platform/PreferencesChangedEventArgs.cs \
	src/Do.Platform/Services.cs \
	src/Do.Platform/StatusIcon.cs \
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

REFERENCES =  \
	System \
	System.Core \
	Mono.Posix \
	$(MONO_ADDINS_LIBS) \
	$(NUNIT_LIBS) \
	../Do.Universe/Do.Universe.dll

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list)
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
