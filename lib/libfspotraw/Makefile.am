include $(top_srcdir)/Makefile.include

LIBRAW = LibRaw-0.8.2
INCLUDES = -I $(top_srcdir) -I $(srcdir)/LibRaw $(F_CFLAGS)

fspotlibdir = $(libdir)/f-spot
fspotlib_LTLIBRARIES = libfspotraw.la
libfspotraw_files = \
	LibRaw/libraw/libraw_const.h 					\
	LibRaw/libraw/libraw.h 							\
	LibRaw/libraw/libraw_version.h 					\
	LibRaw/libraw/libraw_types.h 					\
	LibRaw/libraw/libraw_internal.h 				\
	LibRaw/libraw/libraw_alloc.h 					\
	LibRaw/libraw/libraw_datastream.h 				\
	LibRaw/internal/var_defines.h 					\
	LibRaw/internal/libraw_internal_funcs.h 		\
	LibRaw/internal/dcraw_fileio.cpp 				\
	LibRaw/internal/defines.h 						\
	LibRaw/internal/foveon.cpp 						\
	LibRaw/internal/dcraw_common.cpp 				\
	LibRaw/dcraw/dcraw.c 							\
	LibRaw/src/libraw_c_api.cpp 					\
	LibRaw/src/libraw_cxx.cpp 						\
	fspot-librawloader.cpp 							\
	fspot-librawloader.h

libfspotraw_la_SOURCES = 							\
	fspot-librawloader-marshal.c					\
	fspot-librawloader-marshal.h					\
	$(libfspotraw_files)

extract: $(LIBRAW).tar.gz
	rm -rf LibRaw
	tar xvpzf $(LIBRAW).tar.gz
	mv $(LIBRAW) LibRaw
	git add LibRaw

EXTRA_DIST = 						\
	fspot-librawloader-marshal.list	\
	LibRaw/COPYRIGHT

MPREFIX = fspot_librawloader_marshal
TMPNAME = $(srcdir)/fspot-librawloader-marshal.tmp
LIST = $(srcdir)/fspot-librawloader-marshal.list

fspot-librawloader-marshal.h: $(LIST)
	( @GLIB_GENMARSHAL@ --prefix=$(MPREFIX) $< --header > $(TMPNAME) \
	&& mv $(TMPNAME) $@  ) || ( rm -f $(TMPNAME) && exit 1 )

fspot-librawloader-marshal.c: fspot-librawloader-marshal.h
	( @GLIB_GENMARSHAL@ --prefix=$(MPREFIX) $(LIST) --body > $(TMPNAME) \
	&& mv $(TMPNAME) $@ ) || ( rm -f $(TMPNAME) && exit 1 )

MARSHAL_GENERATED = fspot-librawloader-marshal.c fspot-librawloader-marshal.h

BUILT_SOURCES = $(MARSHAL_GENERATED)

CLEANFILES = $(MARSHAL_GENERATED)

# The stuff below builds a special, non-optimized version of libfspotraw,
# for valgrinding purposes.

libraw_dcraw_common.vo: LibRaw/internal/dcraw_common.cpp
	gcc -g -O0 -c -o $@ $< -I LibRaw

libraw_foveon.vo: LibRaw/internal/foveon.cpp
	gcc -g -O0 -c -o $@ $< -I LibRaw

libraw_cxx.vo: LibRaw/src/libraw_cxx.cpp
	g++ -g -O0 -c -o $@ $< -I LibRaw

libraw_c_api.vo: LibRaw/src/libraw_c_api.cpp
	g++ -g -O0 -c -o $@ $< -I LibRaw

libraw_fileio.vo: LibRaw/internal/dcraw_fileio.cpp
	g++ -g -O0 -c -o $@ $< -I LibRaw

fspot-librawloader_marshal.vo: fspot-librawloader-marshal.c
	gcc -g -O0 -c -o $@ $< `pkg-config --cflags gtk+-2.0`

fspot-librawloader.vo: fspot-librawloader.cpp fspot-librawloader.h
	g++ -g -O0 -c -o $@ $< `pkg-config --cflags gtk+-2.0` -I LibRaw

memory-test.vo: memory-test.c
	gcc -g -O0 -c -o $@ $< `pkg-config --cflags gtk+-2.0`

MEMORY_TEST_OBJECTS =				\
	libraw_dcraw_common.vo			\
	libraw_foveon.vo				\
	libraw_cxx.vo					\
	libraw_c_api.vo					\
	libraw_fileio.vo				\
	fspot-librawloader_marshal.vo	\
	fspot-librawloader.vo			\
	memory-test.vo

memory-test: $(MEMORY_TEST_OBJECTS)
	g++ -g -O0 -o memory-test $(MEMORY_TEST_OBJECTS) `pkg-config --cflags --libs gtk+-2.0` -ljpeg -lm -llcms -lstdc++

valgrind: memory-test
	G_SLICE=always-malloc G_DEBUG=gc-friendly valgrind --tool=memcheck --leak-check=full --num-callers=20 $O ./memory-test $F
