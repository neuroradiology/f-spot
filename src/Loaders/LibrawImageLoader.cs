//
// Fspot.Loaders.LibrawImageLoader.cs
//
// Copyright (c) 2009 Novell, Inc.
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using Gdk;
using System;
using FSpot.Utils;
using FSpot.Loaders.Native;

namespace FSpot.Loaders {
	public class LibrawImageLoader : IImageLoader {
		NativeLibrawLoader loader;
		Uri uri;
		bool is_disposed = false;
		bool is_loading = false;

		public void Load (Uri uri)
		{
			if (this.uri != null)
				throw new Exception ("You should only request one image per loader!");
			this.uri = uri;

			if (is_disposed)
				return;

			loader = new NativeLibrawLoader (uri.AbsolutePath);

			Pixbuf thumb = loader.LoadThumbnail ();
			PixbufOrientation = PixbufOrientation.TopLeft;
			Pixbuf = thumb;
			EventHandler<AreaPreparedEventArgs> prep = AreaPrepared;
			if (prep != null)
				prep (this, new AreaPreparedEventArgs (true));
			EventHandler<AreaUpdatedEventArgs> upd = AreaUpdated;
			if (upd != null)
				upd (this, new AreaUpdatedEventArgs (new Rectangle (0, 0, thumb.Width, thumb.Height)));

		}

		public event EventHandler<AreaPreparedEventArgs> AreaPrepared;
		public event EventHandler<AreaUpdatedEventArgs> AreaUpdated;
		public event EventHandler Completed;

		public bool Loading {
			get { return is_loading; }
		}

		public void Dispose ()
		{

		}

		public Pixbuf Pixbuf { get; private set; }

		public PixbufOrientation PixbufOrientation { get; private set; }
	}
}
