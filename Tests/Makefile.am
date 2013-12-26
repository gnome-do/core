FILES = \
	TestItemSource.cs \
	TestLogging.cs \
	TestKeybindingService.cs \
	AssemblyInfo.cs

RESOURCES = \
	Tests.addin.xml

REFERENCES =  \
	-r:$(BUILD_DIR)/Do.exe \
	Mono.Posix \
	$(GLIB_SHARP_20_LIBS) \
	$(GTK_SHARP_20_LIBS) \
	$(MONO_ADDINS_LIBS) \
	$(MONO_ADDINS_GUI_LIBS) \
	$(MONO_ADDINS_SETUP_LIBS) \
	$(NUNIT_LIBS) \
	System \
	System.Core \
	System.Xml

PROJECT_REFERENCES = \
	Do.Universe \
	Do.Platform \
	Do.Interface.Linux

SOURCES_BUILD = $(addprefix $(srcdir)/, $(FILES))

RESOURCES_EXPANDED = $(addprefix $(srcdir)/, $(RESOURCES))
RESOURCES_BUILD = $(foreach resource, $(RESOURCES_EXPANDED), \
        -resource:$(resource),$(notdir $(resource)))

BUILD_DIR = $(top_builddir)/build

ASSEMBLY_EXTENSION = $(strip $(patsubst library, dll, $(TARGET)))
ASSEMBLY_FILE = $(BUILD_DIR)/$(ASSEMBLY).$(ASSEMBLY_EXTENSION)

STD_REFERENCES = $(foreach ref,$(filter-out -r:%,$(REFERENCES)),-r:$(ref))
BUILD_REFERENCES = $(filter -r:%,$(REFERENCES) $(STD_REFERENCES))
COMPONENT_REFERENCES = $(foreach ref, $(PROJECT_REFERENCES),-r:$(BUILD_DIR)/$(ref).dll)
COMPONENT_DEPS = $(foreach ref,$(PROJECT_REFERENCES),$(BUILD_DIR)/$(ref).dll)

MCS_FLAGS =  $(MCS_LINQ_FLAG) -noconfig -codepage:utf8 -warn:4

if ENABLE_DEBUG
MCS_FLAGS += -d:DEBUG -debug
COMPONENT_DEPS_DEBUG = $(foreach ref,$(COMPONENT_DEPS),$(ref).mdb)
DO_EXE_DEBUG = $(BUILD_DIR)/Do.exe.mdb
endif

Do.Tests.dll: $(SOURCES_BUILD) $(RESOURCES_EXPANDED) $(COMPONENT_DEPS) $(BUILD_DIR)/Do.exe test-environment
	@mkdir -p $(BUILD_DIR)
	@colors=no; \
	case $$TERM in \
                "xterm" | "rxvt" | "rxvt-unicode") \
                        test "x$$COLORTERM" != "x" && colors=yes ;; \
                "xterm-color") colors=yes ;; \
	esac; \
	if [ "x$$colors" = "xyes" ]; then \
                tty -s && true || { colors=no; true; } \
	fi; \
	test "x$$colors" = "xyes" && \
	        echo -e "\033[1mCompiling $(notdir $@)...\033[0m" || \
	        echo "Compiling $(notdir $@)...";
	@$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(BUILD_DEFINES) $(BUILD_REFERENCES) $(COMPONENT_REFERENCES) -r:$(BUILD_DIR)/Do.exe $(RESOURCES_BUILD) $(SOURCES_BUILD) 

export MONO_ENV_OPTIONS=--debug

test-environment: $(COMPONENT_DEPS) $(COMPONENT_DEPS_DEBUG) $(BUILD_DIR)/Do.exe $(DO_EXE_DEBUG)
	cp $(COMPONENT_DEPS) $(COMPONENT_DEPS_DEBUG) .
	cp $(BUILD_DIR)/Do.exe $(DO_EXE_DEBUG) .
	touch $@

TESTS=	Do.Tests.dll \
	$(BUILD_DIR)/Do.Platform.dll \
	$(BUILD_DIR)/Do.Platform.Linux.dll \
	$(BUILD_DIR)/Do.Interface.Linux.dll
TEST_EXTENSIONS = .dll
DLL_LOG_COMPILER = nunit-console
AM_DLL_LOG_FLAGS = -labels -nodots

CLEANFILES=$(foreach component, $(COMPONENT_DEPS) $(COMPONENT_DEPS_DEBUG) $(DO_EXE_DEBUG), $(notdir $(component))) Do.exe test-environment Do.Tests.dll Do.Tests.dll.mdb TestResult.xml
EXTRA_DIST=$(FILES) $(RESOURCES)
