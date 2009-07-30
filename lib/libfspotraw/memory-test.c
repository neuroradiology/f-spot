#include <glib-object.h>
#include "fspot-librawloader.h"

int main (int argc, char **argv) {
	FSpotLibrawLoader *loader;
	GdkPixbuf *pixbuf;

	g_type_init ();

	g_assert (argc == 2);
	loader = g_object_new (FSPOT_TYPE_LIBRAWLOADER, "filename", argv[1], NULL);

	int orientation;
	pixbuf = fspot_librawloader_load_embedded (loader, &orientation);
	g_object_unref (pixbuf);
	pixbuf = NULL;

	pixbuf = fspot_librawloader_load_full (loader);
	g_object_unref (pixbuf);
	pixbuf = NULL;

	g_object_unref (loader);
	loader = NULL;


	return 0;
}
