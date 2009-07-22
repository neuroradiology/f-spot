include $(top_srcdir)/Makefile.include

UNSAFE = -unsafe
DEBUG = -debug

SRCS =

CUSTOMS =

ASSEMBLIES =				\
	-pkg:gtk-sharp-2.0

all: libfspotraw-sharp.dll



# The stuff below is needed because gapi2-parser doesn't like .cpp files
libfspotraw-api.raw: libfspotraw.sources $(srcdir)/../libfspotraw/fspot-librawloader.cpp $(srcdir)/../libfspotraw/fspot-librawloader.h
	@echo -e "\n*** Extracting the API"
	touch $@
	cp $(srcdir)/../libfspotraw/fspot-librawloader.cpp $(srcdir)/../libfspotraw/fspot-librawloader.c
	gapi2-parser $<
	rm $(srcdir)/../libfspotraw/fspot-librawloader.c

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
assembly_DATA =						\
	libfspotraw-sharp.dll			\
	libfspotraw-sharp.dll.config

CLEANFILES = 						\
	generated/*.cs 					\
	libfspotraw-api.raw				\
	libfspotraw-sharp.dll 			\
	libfspotraw-sharp.dll.mdb 		\
	libfspotraw-sharp.dll.config	\
	libfspotraw-api.xml

EXTRA_DIST =						\
	$(SRCS)							\
	$(CUSTOMS)						\
	libfspotraw.metadata			\
	libfspotraw-sharp.dll.config.in	\
	libfspotraw-api.raw
