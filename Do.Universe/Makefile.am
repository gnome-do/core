# Simple component buildsystem
include $(top_srcdir)/build.rules.mk

ASSEMBLY = Do.Universe
TARGET = library

PKG_CONFIG_FILES = do.universe.pc

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Universe/Act.cs \
	src/Do.Universe/ContactItem.cs \
	src/Do.Universe/Do.Universe.Safe/SafeAct.cs \
	src/Do.Universe/Do.Universe.Safe/SafeElement.cs \
	src/Do.Universe/Do.Universe.Safe/SafeItem.cs \
	src/Do.Universe/Do.Universe.Safe/SafeItemSource.cs \
	src/Do.Universe/Element.cs \
	src/Do.Universe/EmptyElement.cs \
	src/Do.Universe/EmptyItem.cs \
	src/Do.Universe/IApplicationItem.cs \
	src/Do.Universe/IBookmarkItem.cs \
	src/Do.Universe/IContactDetailItem.cs \
	src/Do.Universe/IFileItem.cs \
	src/Do.Universe/IItem.cs \
	src/Do.Universe/IOpenableItem.cs \
	src/Do.Universe/IRunnableItem.cs \
	src/Do.Universe/ITextItem.cs \
	src/Do.Universe/IUriItem.cs \
	src/Do.Universe/IUrlItem.cs \
	src/Do.Universe/Item.cs \
	src/Do.Universe/ItemSource.cs \
	src/Do.Universe/ProxyItem.cs

RESOURCES = \
	Resources/Do.Universe.addin.xml

PROJECT_REFERENCES = \
	Do.Platform

REFERENCES =  \
	System \
	System.Core \
	Mono.Posix
