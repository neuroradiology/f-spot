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

using FSpot.Loaders.Native;
using FSpot.Utils;
using Gdk;
using System;
using System.Threading;

namespace FSpot.Loaders {
	public class LibrawImageLoader : IImageLoader {
		NativeLibrawLoader loader;
		Uri uri;
		bool is_disposed = false;
		bool is_loading = false;

		Pixbuf thumb, full;

		public void Load (Uri uri)
		{
			if (this.uri != null)
				throw new Exception ("You should only request one image per loader!");
			this.uri = uri;

			if (is_disposed)
				return;

			loader = new NativeLibrawLoader (uri.AbsolutePath);
			LoadThumbnail ();
			ThreadPool.QueueUserWorkItem (delegate { LoadFull (); });
		}

		void LoadThumbnail ()
		{
			thumb = loader.LoadThumbnail ();
			PixbufOrientation = PixbufOrientation.TopLeft;
			GLib.Idle.Add (delegate {
				EventHandler<AreaPreparedEventArgs> prep = AreaPrepared;
				if (prep != null)
					prep (this, new AreaPreparedEventArgs (true));
				EventHandler<AreaUpdatedEventArgs> upd = AreaUpdated;
				if (upd != null)
					upd (this, new AreaUpdatedEventArgs (new Rectangle (0, 0, thumb.Width, thumb.Height)));
				return false;
			});
		}

		void LoadFull ()
		{
			loader.ProgressUpdated += delegate (object o, ProgressUpdatedArgs args) {
				Log.Debug ("Loading RAW: {0}/{1}", args.Done, args.Total);
			};
			full = loader.LoadFull ();
			PixbufOrientation = PixbufOrientation.TopLeft;
			GLib.Idle.Add (delegate {
				EventHandler<AreaPreparedEventArgs> prep = AreaPrepared;
				if (prep != null)
					prep (this, new AreaPreparedEventArgs (false));
				EventHandler<AreaUpdatedEventArgs> upd = AreaUpdated;
				if (upd != null)
					upd (this, new AreaUpdatedEventArgs (new Rectangle (0, 0, full.Width, full.Height)));
				EventHandler eh = Completed;
				if (eh != null)
					eh (this, EventArgs.Empty);
				return false;
			});
		}

		public event EventHandler<AreaPreparedEventArgs> AreaPrepared;
		public event EventHandler<AreaUpdatedEventArgs> AreaUpdated;
		public event EventHandler Completed;

		public bool Loading {
			get { return is_loading; }
		}

		public void Dispose ()
		{
			// TODO: Abort the NativeLibrawLoader
			thumb.Dispose ();
			full.Dispose ();
		}

		public Pixbuf Pixbuf {
			get {
				return full == null ? thumb : full;
			}
		}

		public PixbufOrientation PixbufOrientation { get; private set; }
	}
}
