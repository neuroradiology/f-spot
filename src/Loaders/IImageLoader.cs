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
		event EventHandler<AreaPreparedEventArgs> AreaPrepared;
		event EventHandler<AreaUpdatedEventArgs> AreaUpdated;
		event EventHandler<ItemsCompletedEventArgs> Completed;
		event EventHandler<ProgressHintEventArgs> ProgressHint;

		ImageLoaderItem Load (ImageLoaderItem items, bool async);

		Pixbuf Thumbnail { get; }
		PixbufOrientation ThumbnailOrientation { get; }
		Pixbuf Large { get; }
		PixbufOrientation LargeOrientation { get; }
		Pixbuf Full { get; }
		PixbufOrientation FullOrientation { get; }
	}
}
