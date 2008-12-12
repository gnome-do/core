EXTRA_DIST = 

include $(top_srcdir)/Makefile.include

ASSEMBLY_COMPILER_COMMAND = $(MCS)
ASSEMBLY_COMPILER_FLAGS = $(MCS_LINQ_FLAG) -noconfig -codepage:utf8 -warn:4
ASSEMBLY = Do.Platform.Linux.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
BUILD_DIR = ./fake

if ENABLE_DEBUG
ASSEMBLY_COMPILER_FLAGS += -debug -d:DEBUG
endif

all: $(ASSEMBLY)

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Platform/ConsoleCrayon.cs \
	src/Do.Platform/Do.Platform.Linux/ConsoleLogService.cs \
	src/Do.Platform/Do.Platform.Linux/EnvironmentService.cs \
	src/Do.Platform/Do.Platform.Linux/GConfPreferencesService.cs \
	src/Do.Platform/Do.Platform.Linux/UniverseFactoryService.cs \
	src/Do.Platform/Do.Platform.Linux/NotificationsService.cs \
	src/Do.Platform/PathsImplementation.cs \
	src/Do.Platform/StatusIconImplementation.cs \
	src/Do.Universe/ApplicationItem.cs \
	src/Do.Universe/FileItem.cs \
	src/Do.Universe/ApplicationItemSource.cs \
	src/Do.Universe/CopyToClipboardAction.cs \
	src/Do.Universe/GNOMESpecialLocationsItemSource.cs

if HAVE_NUNIT
FILES += \
	src/Do.Platform/Do.Platform.Linux/Tests/GConfPreferencesServiceTests.cs
endif

RESOURCES =  \
	Resources/Do.Platform.Linux.addin.xml

EXTRAS := do.platform.linux.pc.in
LINUX_PKGCONFIG := do.platform.linux.pc

REFERENCES =  \
	System \
	System.Core \
	Mono.Posix \
	$(NUNIT_LIBS) \
	$(GTK_SHARP_20_LIBS) \
	$(GNOME_SHARP_20_LIBS) \
	$(GNOME_DESKTOP_SHARP_20_LIBS) \
	$(GCONF_SHARP_20_LIBS) \
	$(NOTIFY_SHARP_LIBS) \
	../Do.Universe/Do.Universe.dll \
	../Do.Platform/Do.Platform.dll \
	../Do.Interface.Linux/Do.Interface.Linux.dll

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list)
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
