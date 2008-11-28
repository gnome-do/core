EXTRA_DIST = 

include $(top_srcdir)/Makefile.include

ASSEMBLY_COMPILER_COMMAND = $(MCS)
ASSEMBLY_COMPILER_FLAGS = $(MCS_LINQ_FLAG) -noconfig -codepage:utf8 -warn:4
ASSEMBLY = Do.Interface.Linux.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library

if ENABLE_DEBUG
ASSEMBLY_COMPILER_FLAGS += -debug -d:DEBUG
endif

all: $(ASSEMBLY)

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Interface.Linux/IconProvider.cs

REFERENCES =  \
	System \
	System.Core \
	$(GTK_SHARP_20_LIBS) \
	../Do.Platform/Do.Platform.dll \
	Mono.Posix

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list)
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
