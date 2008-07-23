
EXTRA_DIST = 

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -langversion:linq -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ./bin/Debug/Do.DBus.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Debug


endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -langversion:linq -noconfig -codepage:utf8 -warn:4
ASSEMBLY = ./bin/Release/Do.DBus.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Release


endif


LINUX_PKGCONFIG = \
	$(DO_DBUS_PC)  


	
all: $(ASSEMBLY) $(LINUX_PKGCONFIG) 

FILES =  \
	src/AssemblyInfo.cs \
	src/DBusRegistrar.cs \
	src/IController.cs \
	src/PowerState.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	do.dbus.pc.in 

REFERENCES =  \
	-pkg:ndesk-dbus-1.0 \
	-pkg:ndesk-dbus-glib-1.0 \
	System

DLL_REFERENCES = 

DO_DBUS_PC = $(BUILD_DIR)/do.dbus.pc

$(DO_DBUS_PC): do.dbus.pc
	mkdir -p $(BUILD_DIR)
	cp '$<' '$@'



$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list)
	mkdir -p $(dir $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)

include $(top_srcdir)/Makefile.include
