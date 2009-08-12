//
// fspot-librawloader.cpp
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

#include "fspot-librawloader.h"
#include "fspot-librawloader-marshal.h"

#include <libraw/libraw.h>

#define return_null_if(cond) if ((cond)) { self->priv->running = false; return NULL; }

G_DEFINE_TYPE (FSpotLibrawLoader, fspot_librawloader, G_TYPE_OBJECT);

enum {
	PROGRESS_UPDATED,
	LAST_SIGNAL
};

enum {
	PROP_0,
	PROP_FILENAME,
	PROP_ABORTED
};

static guint signals[LAST_SIGNAL] = { 0 };

static void
fspot_librawloader_set_property (GObject	  *object,
								 guint		   property_id,
								 const GValue *value,
								 GParamSpec   *pspec);
static void
fspot_librawloader_get_property (GObject	  *object,
								 guint		   property_id,
								 GValue		  *value,
								 GParamSpec   *pspec);
static void fspot_librawloader_dispose (GObject *object);
static void fspot_librawloader_finalize (GObject *object);

static gboolean open_if_needed (FSpotLibrawLoader *self);
static void pixbuf_freed (guchar *pixels, gpointer data);

static int libraw_progress_callback (void *user_data, enum LibRaw_progress p, int iteration, int expected);

#define FSPOT_LIBRAWLOADER_GET_PRIVATE(obj) (G_TYPE_INSTANCE_GET_PRIVATE ((obj), FSPOT_TYPE_LIBRAWLOADER, FSpotLibrawLoaderPriv))

struct _FSpotLibrawLoaderPriv
{
	LibRaw *raw_proc;
	gchar *filename;

	gboolean opened;
	gboolean aborted;
	volatile gboolean running;
};

static void
fspot_librawloader_class_init (FSpotLibrawLoaderClass *klass)
{
	GObjectClass *gobject_class = G_OBJECT_CLASS (klass);
	GParamSpec *pspec;

	gobject_class->set_property = fspot_librawloader_set_property;
	gobject_class->get_property = fspot_librawloader_get_property;
	gobject_class->dispose      = fspot_librawloader_dispose;
	gobject_class->finalize     = fspot_librawloader_finalize;

	signals[PROGRESS_UPDATED] =
		g_signal_new ("progress-updated",
				G_OBJECT_CLASS_TYPE (gobject_class),
				G_SIGNAL_RUN_LAST,
				G_STRUCT_OFFSET (FSpotLibrawLoaderClass, progress_updated),
				NULL, NULL,
				fspot_librawloader_marshal_VOID__UINT_UINT,
				G_TYPE_NONE, 2,
				G_TYPE_UINT,
				G_TYPE_UINT);

	pspec = g_param_spec_string ("filename",
								 "The full path of the RAW files.",
								 "Set filename",
								 "",
							     (GParamFlags) (G_PARAM_READWRITE | G_PARAM_CONSTRUCT_ONLY));
	g_object_class_install_property (gobject_class,
									 PROP_FILENAME,
									 pspec);

	pspec = g_param_spec_boolean ("aborted",
								  "Whether the loading has been aborted.",
								  "When switched to true, loading is stopped.",
								  FALSE,
								  (GParamFlags) G_PARAM_READWRITE);
	g_object_class_install_property (gobject_class,
									 PROP_ABORTED,
									 pspec);

	g_type_class_add_private (klass, sizeof (FSpotLibrawLoaderPriv));
}

static void
fspot_librawloader_init (FSpotLibrawLoader *self)
{
	self->priv = FSPOT_LIBRAWLOADER_GET_PRIVATE (self);

	self->priv->raw_proc = new LibRaw;
	self->priv->opened = false;
	self->priv->aborted = false;
	self->priv->running = false;

	self->priv->raw_proc->set_progress_handler (libraw_progress_callback, self);
}

static void
fspot_librawloader_set_property (GObject	  *object,
								 guint		   property_id,
								 const GValue *value,
								 GParamSpec   *pspec)
{
	FSpotLibrawLoader *self = FSPOT_LIBRAWLOADER (object);

	switch (property_id)
	{
		case PROP_FILENAME:
			g_free (self->priv->filename);
			self->priv->filename = g_value_dup_string (value);
			break;

		case PROP_ABORTED:
			fspot_librawloader_set_aborted (self, g_value_get_boolean (value));
			break;

		default:
			G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
			break;
	}
}

static void
fspot_librawloader_get_property (GObject	  *object,
								 guint		   property_id,
								 GValue		  *value,
								 GParamSpec   *pspec)
{
	FSpotLibrawLoader *self = FSPOT_LIBRAWLOADER (object);

	switch (property_id)
	{
		case PROP_FILENAME:
			g_value_set_string (value, self->priv->filename);
			break;

		case PROP_ABORTED:
			g_value_set_boolean (value, self->priv->aborted);

		default:
			G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
			break;
	}
}

static void
fspot_librawloader_dispose (GObject *object)
{
	FSpotLibrawLoader *self = FSPOT_LIBRAWLOADER (object);

	self->priv->aborted = true;
	self->priv->raw_proc->recycle ();

	G_OBJECT_CLASS (fspot_librawloader_parent_class)->dispose (object);
}

static void
fspot_librawloader_finalize (GObject *object)
{
	FSpotLibrawLoader *self = FSPOT_LIBRAWLOADER (object);

	self->priv->raw_proc->recycle ();
	delete self->priv->raw_proc;
	g_free (self->priv->filename);

	G_OBJECT_CLASS (fspot_librawloader_parent_class)->finalize (object);
}

GdkPixbuf *
fspot_librawloader_load_embedded (FSpotLibrawLoader *self, int *orientation)
{
	int result;
	libraw_processed_image_t *image = NULL;
	GdkPixbufLoader *loader = NULL;
	GdkPixbuf *pixbuf = NULL;
	GError *error = NULL;

	self->priv->running = true;

	return_null_if (!open_if_needed (self));

	self->priv->raw_proc->unpack_thumb ();
	image = self->priv->raw_proc->dcraw_make_mem_thumb (&result);
	return_null_if (result != 0 || image == NULL);

	g_assert (image->type == LIBRAW_IMAGE_JPEG);

	loader = gdk_pixbuf_loader_new ();
	gdk_pixbuf_loader_write (loader, image->data, image->data_size, NULL);
	gdk_pixbuf_loader_close (loader, &error);
	g_assert (error == NULL);

	pixbuf = gdk_pixbuf_copy (gdk_pixbuf_loader_get_pixbuf (loader));
	*orientation = self->priv->raw_proc->imgdata.sizes.flip;

	g_object_unref (loader);
	g_free (image);

	self->priv->running = false;
	return pixbuf;
}

GdkPixbuf *
fspot_librawloader_load_full (FSpotLibrawLoader *self)
{
	int result;
	libraw_processed_image_t *image = NULL;
	GdkPixbuf *pixbuf = NULL;

	self->priv->running = true;

	return_null_if (!open_if_needed (self));

	result = self->priv->raw_proc->unpack ();
	return_null_if (result != 0);

	result = self->priv->raw_proc->dcraw_process ();
	return_null_if (result != 0);

	image = self->priv->raw_proc->dcraw_make_mem_image (&result);
	return_null_if (result != 0 || image == NULL);
	g_assert (image->type == LIBRAW_IMAGE_BITMAP);

	pixbuf = gdk_pixbuf_new_from_data (image->data,
									   GDK_COLORSPACE_RGB,
									   false,
									   image->bits,
									   image->width,
									   image->height,
									   image->width * 3, /* rowstride */
									   (GdkPixbufDestroyNotify) pixbuf_freed,
									   image);

	self->priv->running = false;
	return pixbuf;
}

static void
pixbuf_freed (guchar *pixels, gpointer data)
{
	libraw_processed_image_t *image = (libraw_processed_image_t *)data;
	g_free (image);
}

FSpotLibrawLoader *
fspot_librawloader_new (const gchar *filename)
{
	FSpotLibrawLoader *loader;

	loader = (FSpotLibrawLoader *) g_object_new (FSPOT_TYPE_LIBRAWLOADER,
												 "filename", filename,
												 NULL);

	return loader;
}

static gboolean
open_if_needed (FSpotLibrawLoader *self)
{
	if (!self->priv->opened) {
		self->priv->raw_proc->imgdata.params.use_camera_wb = 1;
		self->priv->raw_proc->imgdata.params.use_camera_matrix = 1;
		self->priv->raw_proc->imgdata.params.user_qual = 2;
		self->priv->raw_proc->imgdata.params.highlight = 2;
		int result = self->priv->raw_proc->open_file (self->priv->filename);
		if (result != 0)
			return FALSE;

		self->priv->opened = true;
	}
	return TRUE;
}

static int
libraw_progress_callback (void *user_data, enum LibRaw_progress p, int iteration, int expected)
{
	FSpotLibrawLoader *self = FSPOT_LIBRAWLOADER (user_data);
	g_signal_emit (self, signals[PROGRESS_UPDATED], 0, iteration, expected);
	return self->priv->aborted;
}

gboolean
fspot_librawloader_get_aborted (FSpotLibrawLoader *self)
{
	g_return_val_if_fail (FSPOT_IS_LIBRAWLOADER (self), false);
	return self->priv->aborted;
}

void
fspot_librawloader_set_aborted (FSpotLibrawLoader *self, gboolean aborted)
{
	g_return_if_fail (FSPOT_IS_LIBRAWLOADER (self));
	self->priv->aborted = aborted;

	while (self->priv->running)
		;
}
