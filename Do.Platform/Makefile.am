EXTRA_DIST = 

include $(top_srcdir)/Makefile.include

ASSEMBLY_COMPILER_COMMAND = $(MCS)
ASSEMBLY_COMPILER_FLAGS = $(MCS_LINQ_FLAG) -noconfig -codepage:utf8 -warn:4
ASSEMBLY = Do.Platform.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library

if ENABLE_DEBUG
ASSEMBLY_COMPILER_FLAGS += -debug -d:DEBUG
endif

all: $(ASSEMBLY)

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Platform/Core.cs \
	src/Do.Platform/Do.Platform.Common/AbstractLogImplementation.cs \
	src/Do.Platform/Do.Platform.Common/FileLogImplementation.cs \
	src/Do.Platform/Do.Platform.Default/EnvironmentService.cs \
	src/Do.Platform/Do.Platform.ServiceStack/IService.cs \
	src/Do.Platform/IEnvironmentService.cs \
	src/Do.Platform/Icons.cs \
	src/Do.Platform/Log.cs \
	src/Do.Platform/Notifications.cs \
	src/Do.Platform/Paths.cs \
	src/Do.Platform/Preferences.cs \
	src/Do.Platform/Services.cs \
	src/Do.Platform/StatusIcon.cs \
	src/Do.Platform/UniverseFactory.cs \
	src/Do.Platform/Windowing.cs \
	src/Do.Universe/AbstractAction.cs \
	src/Do.Universe/Do.Universe.Common/BookmarkItem.cs \
	src/Do.Universe/Do.Universe.Common/ContactItem.cs \
	src/Do.Universe/Do.Universe.Common/EmailAction.cs \
	src/Do.Universe/Do.Universe.Common/OpenAction.cs \
	src/Do.Universe/Do.Universe.Common/OpenURLAction.cs \
	src/Do.Universe/Do.Universe.Common/OpenWithAction.cs \
	src/Do.Universe/Do.Universe.Common/RevealAction.cs \
	src/Do.Universe/Do.Universe.Common/RunAction.cs \
	src/Do.Universe/Do.Universe.Common/TextItem.cs \
	src/Do.Universe/IAction.cs \
	src/Do.Universe/IApplicationItem.cs \
	src/Do.Universe/IFileItem.cs \
	src/Do.Universe/IItem.cs \
	src/Do.Universe/IItemSource.cs \
	src/Do.Universe/IObject.cs \
	src/Do.Universe/IOpenableItem.cs \
	src/Do.Universe/IRunnableItem.cs \
	src/Do.Universe/ITextItem.cs \
	src/Do.Universe/IURIItem.cs \
	src/Do.Universe/IURLItem.cs \
	src/EnumerableExtensions.cs

RESOURCES = \
	Resources/Do.Platform.addin.xml

REFERENCES =  \
	System \
	System.Core \
	$(MONO_ADDINS_LIBS) \
	Mono.Posix

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list)
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
