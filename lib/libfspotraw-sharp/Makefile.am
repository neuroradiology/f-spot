include $(top_srcdir)/Makefile.include

UNSAFE = -unsafe
DEBUG = -debug

SRCS =

CUSTOMS =

ASSEMBLIES =				\
	-pkg:gtk-sharp-2.0

all: libfspotraw-sharp.dll

libfspotraw-api.raw: libfspotraw.sources
	@echo -e "\n*** Extracting the API"
	touch $@
	gapi2-parser $<

libfspotraw-api.xml: $(srcdir)/libfspotraw-api.raw $(srcdir)/libfspotraw.metadata
	@echo -e "\n*** Massaging the raw api into $@"
	cp $(srcdir)/libfspotraw-api.raw libfspotraw-api.xml
	chmod +w libfspotraw-api.xml
	gapi2-fixup --api=libfspotraw-api.xml --metadata=$(srcdir)/libfspotraw.metadata

generated/LibrawLoader.cs: libfspotraw-api.xml $(CUSTOMS)
	@echo -e "\n*** Generating C# code"
	gapi2-codegen --outdir=generated --customdir=$(srcdir) --generate libfspotraw-api.xml $(GTKSHARP_CFLAGS)

libfspotraw-sharp.dll: generated/LibrawLoader.cs $(SRCS)
	@echo -e "\n*** Building $@"
	$(CSC) -target:library -out:$@ $(UNSAFE) $(DEBUG) $(ASSEMBLIES) $(SRCS) generated/*.cs

assemblydir = $(pkglibdir)
assembly_DATA =			\
	libfspotraw-sharp.dll		\
	libfspotraw-sharp.dll.config

CLEANFILES = 			\
	libfspotraw-sharp.dll 	\
	libfspotraw-sharp.dll.mdb \
	libfspotraw-api.xml

DISTCLEANFILES =		\
	libfspotraw-api.raw	\
	generated/*.cs

EXTRA_DIST =			\
	$(SRCS)			\
	$(CUSTOMS)		\
	libfspotraw.metadata		\
	libfspotraw-sharp.dll.config	\
	libfspotraw-api.raw
