//
// Fspot.Loaders.IImageLoader.cs
//
// Copyright (c) 2009 Novell, Inc.
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using FSpot.Utils;
using System;
using Gdk;

namespace FSpot.Loaders {
	public interface IImageLoader : IDisposable {
		// Completed is always emitted, either when the loading is completed,
		// or when an error is occured. Requesting the relevant item and
		// checking for a NULL value should always be done.
		event EventHandler<AreaPreparedEventArgs> AreaPrepared;
		event EventHandler<AreaUpdatedEventArgs> AreaUpdated;
		event EventHandler<ItemsCompletedEventArgs> Completed;
		event EventHandler<ProgressHintEventArgs> ProgressHint;

		ImageLoaderItem Load (ImageLoaderItem items, bool async);

		// Each of these properties should return a pixbuf that should be
		// disposed by the requestor.
		//
		// If something goes wrong with the loading or if the item isn't loaded
		// yet, NULL should be returned.
		Pixbuf Thumbnail { get; }
		Pixbuf Large { get; }
		Pixbuf Full { get; }

		PixbufOrientation ThumbnailOrientation { get; }
		PixbufOrientation LargeOrientation { get; }
		PixbufOrientation FullOrientation { get; }
	}
}
