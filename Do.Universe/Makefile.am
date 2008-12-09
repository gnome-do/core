EXTRA_DIST = 

include $(top_srcdir)/Makefile.include

ASSEMBLY_COMPILER_COMMAND = $(MCS)
ASSEMBLY_COMPILER_FLAGS = $(MCS_LINQ_FLAG) -noconfig -codepage:utf8 -warn:4
ASSEMBLY = Do.Universe.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library

if ENABLE_DEBUG
ASSEMBLY_COMPILER_FLAGS += -debug -d:DEBUG
endif

all: $(ASSEMBLY)

EXTRAS := do.universe.pc.in
LINUX_PKGCONFIG := do.universe.pc

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Universe/EmptyItem.cs \
	src/Do.Universe/EmptyObject.cs \
	src/Do.Universe/IApplicationItem.cs \
	src/Do.Universe/IBookmarkItem.cs \
	src/Do.Universe/IUriItem.cs \
	src/Do.Universe/IItem.cs \
	src/Do.Universe/IUrlItem.cs \
	src/Do.Universe/ITextItem.cs \
	src/Do.Universe/IOpenableItem.cs \
	src/Do.Universe/IItemSource.cs \
	src/Do.Universe/AbstractAction.cs \
	src/Do.Universe/IRunnableItem.cs \
	src/Do.Universe/IFileItem.cs \
	src/Do.Universe/IContactDetailItem.cs \
	src/Do.Universe/ContactItem.cs \
	src/Do.Universe/IObject.cs \
	src/Do.Universe/IAction.cs

RESOURCES = \
	Resources/Do.Universe.addin.xml

REFERENCES =  \
	System \
	System.Core \
	Mono.Posix

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list)
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
