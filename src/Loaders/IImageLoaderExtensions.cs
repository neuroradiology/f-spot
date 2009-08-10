//
// Fspot.Loaders.IImageLoaderExtensions.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using Gdk;
using System;
using FSpot.Utils;

namespace FSpot.Loaders {
	public static class IImageLoaderExtensions {
		// Async loading
		public static void Load (this IImageLoader loader, ImageLoaderItem items, EventHandler<ItemsCompletedEventArgs> handler)
		{
			loader.Completed += handler;
			ImageLoaderItem loaded = loader.Load (items, true);
			if (loaded != ImageLoaderItem.None)
				handler (loader, new ItemsCompletedEventArgs (loaded));
		}

		// Sync loading
		public static void Load (this IImageLoader loader, ImageLoaderItem items)
		{
			loader.Load (items, false);
		}

		// Accessors
		public static Pixbuf Pixbuf (this IImageLoader loader, ImageLoaderItem item)
		{
			if (item == ImageLoaderItem.Thumbnail)
				return loader.Thumbnail;
			else if (item == ImageLoaderItem.Large)
				return loader.Large;
			else if (item == ImageLoaderItem.Full)
				return loader.Full;
			throw new Exception ("Unknown item requested: "+item);
		}

		public static PixbufOrientation PixbufOrientation (this IImageLoader loader, ImageLoaderItem item)
		{
			if (item == ImageLoaderItem.Thumbnail)
				return loader.ThumbnailOrientation;
			else if (item == ImageLoaderItem.Large)
				return loader.LargeOrientation;
			else if (item == ImageLoaderItem.Full)
				return loader.FullOrientation;
			throw new Exception ("Unknown item requested: "+item);
		}
	}
}
