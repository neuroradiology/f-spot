//
// Fspot.ImageLoader.cs
//
// Copyright (c) 2009 Novell, Inc.
//
// Author(s)
//	Stephane Delcroix  <sdelcroix@novell.com>
//
// This is free software. See COPYING for details
//

using FSpot.Platform;
using FSpot.Utils;
using Gdk;
using System;
using System.Threading;

namespace FSpot.Loaders {
	public class GdkImageLoader : Gdk.PixbufLoader, IImageLoader
	{
		Uri uri;
		object sync_handle = new object ();
		bool is_disposed = false;
		Rectangle damage;

		public ImageLoaderItem ItemsRequested { get; private set; }
		public ImageLoaderItem ItemsCompleted { get; private set; }

		Pixbuf thumbnail;
		public Pixbuf Thumbnail {
			get { return thumbnail == null ? null : thumbnail.ShallowCopy (); }
			private set { thumbnail = value; }
		}
		public PixbufOrientation ThumbnailOrientation { get; private set; }

		public Pixbuf Large {
			get { return Pixbuf == null ? null : Pixbuf.ShallowCopy (); }
		}
		public PixbufOrientation LargeOrientation { get; private set; }

		public Pixbuf Full { get { return Large; } }
		public PixbufOrientation FullOrientation { get { return LargeOrientation; } }

		new public event EventHandler<AreaPreparedEventArgs> AreaPrepared;
		new public event EventHandler<AreaUpdatedEventArgs> AreaUpdated;
		public event EventHandler<ItemsCompletedEventArgs> Completed;
		public event EventHandler<ProgressHintEventArgs> ProgressHint;

		public bool Loading { get; private set; }

#region public api
		public GdkImageLoader (Uri uri) : base ()
		{
			this.uri = uri;
			Loading = false;

			ItemsRequested = ImageLoaderItem.None;
			ItemsCompleted = ImageLoaderItem.None;
		}

		public ImageLoaderItem Load (ImageLoaderItem items, bool async)
		{
			if (is_disposed)
				return ImageLoaderItem.None;

			ItemsRequested |= items;

			StartLoading ();

			if (!async)
				WaitForCompletion (items);

			return ItemsCompleted & items;
		}

		public override void Dispose ()
		{
			if (is_disposed)
				return;

			is_disposed = true;
			if (image_stream != null)
				try {
					image_stream.Close ();
				} catch (GLib.GException)
				{
				}
			Close ();
			if (thumbnail != null) {
				thumbnail.Dispose ();
				thumbnail = null;
			}
			base.Dispose ();
		}

		public new bool Close ()
		{
			lock (sync_handle) {
				return base.Close (true);
			}
		}
#endregion

#region event handlers
		protected override void OnAreaPrepared ()
		{
			if (is_disposed)
				return;

			base.OnAreaPrepared ();
			SignalAreaPrepared (ImageLoaderItem.Large | ImageLoaderItem.Full);
		}

		protected override void OnAreaUpdated (int x, int y, int width, int height)
		{
			if (is_disposed)
				return;

			Rectangle area = new Rectangle (x, y, width, height);
			base.OnAreaUpdated (x, y, width, height);
			SignalAreaUpdated (ImageLoaderItem.Large | ImageLoaderItem.Full, area);
		}

		protected virtual void OnCompleted ()
		{
			if (is_disposed)
				return;

			SignalItemCompleted (ImageLoaderItem.Large | ImageLoaderItem.Full);
		}
#endregion

#region private stuffs
		System.IO.Stream image_stream;
		const int count = 1 << 16;
		byte [] buffer = new byte [count];

		void StartLoading ()
		{
			lock (sync_handle) {
				if (Loading)
					return;
				Loading = true;
			}

			// Load thumbnail immediately, if required
			if (!ItemsCompleted.Contains (ImageLoaderItem.Thumbnail) &&
				 ItemsRequested.Contains (ImageLoaderItem.Thumbnail)) {
				LoadThumbnail ();
			}

			ThreadPool.QueueUserWorkItem (delegate {
					try {
						DoLoad ();
					} catch (Exception e) {
						Log.Debug (e.ToString ());
						Log.Debug ("Requested: {0}, Done: {1}", ItemsRequested, ItemsCompleted);
						Gtk.Application.Invoke (delegate { throw e; });
					}
				});
		}

		void DoLoad ()
		{
			while (!is_disposed && !ItemsCompleted.Contains (ItemsRequested)) {
				if (ItemsRequested.Contains (ImageLoaderItem.Thumbnail) && !ItemsCompleted.Contains (ImageLoaderItem.Thumbnail))
					LoadThumbnail ();

				if (ItemsRequested.Contains (ImageLoaderItem.Large) && !ItemsCompleted.Contains (ImageLoaderItem.Large))
					LoadLarge ();
			}

			lock (sync_handle) {
				Loading = false;
			}
		}

		void LoadThumbnail ()
		{
			if (is_disposed)
				return;

			// Check if the thumbnail exists, if not: try to create it from the
			// Large image. Will request Large if it is not present and wait
			// for the next call to generate it (see the loop in DoLoad).
			if (!ThumbnailFactory.ThumbnailExists (uri)) {
				if (ItemsCompleted.Contains (ImageLoaderItem.Large)) {
					if (Pixbuf != null)
						using (Pixbuf scaled = PixbufUtils.ScaleToMaxSize (Pixbuf, 256, 256, false)) {
							Pixbuf rotated = FSpot.Utils.PixbufUtils.TransformOrientation (scaled, LargeOrientation);
							ThumbnailFactory.SaveThumbnail (rotated, uri);
							if (rotated != scaled)
								rotated.Dispose ();
						}
				} else {
					ItemsRequested |= ImageLoaderItem.Large;
					return;
				}
			}

			Thumbnail = ThumbnailFactory.LoadThumbnail (uri);
			ThumbnailOrientation = PixbufOrientation.TopLeft;

			if (thumbnail != null) {
				SignalAreaPrepared (ImageLoaderItem.Thumbnail);
				SignalAreaUpdated (ImageLoaderItem.Thumbnail, new Rectangle (0, 0, thumbnail.Width, thumbnail.Height));
			}
			SignalItemCompleted (ImageLoaderItem.Thumbnail);
		}

		void LoadLarge ()
		{
			if (is_disposed)
				return;

			try {
				image_stream = new GLib.GioStream (GLib.FileFactory.NewForUri (uri).Read (null));
				using (ImageFile image_file = ImageFile.Create (uri)) {
					LargeOrientation = image_file.Orientation;
				}
			} catch (GLib.GException) {
				SignalItemCompleted (ImageLoaderItem.Large | ImageLoaderItem.Full);
				return;
			}

			while (Loading && !is_disposed) {
				int byte_read = image_stream.Read (buffer, 0, count);

				if (byte_read == 0) {
					image_stream.Close ();
                    Close ();
					Loading = false;
					SignalItemCompleted (ImageLoaderItem.Large | ImageLoaderItem.Full);
				} else {
					try {
						Write (buffer, (ulong)byte_read);
					} catch (System.ObjectDisposedException) {
					} catch (GLib.GException) {
					}
				}
			}
		}

		void WaitForCompletion (ImageLoaderItem items)
		{
			while (!ItemsCompleted.Contains(items)) {
				Log.Debug ("Waiting for completion of {0} (done: {1})", ItemsRequested, ItemsCompleted);
				Monitor.Enter (sync_handle);
				Monitor.Wait (sync_handle);
				Monitor.Exit (sync_handle);
				Log.Debug ("Woke up after waiting for {0} (done: {1})", ItemsRequested, ItemsCompleted);
			}
		}

		void SignalAreaPrepared (ImageLoaderItem item) {
			damage = Rectangle.Zero;
			EventHandler<AreaPreparedEventArgs> eh = AreaPrepared;
			if (eh != null)
				GLib.Idle.Add (delegate {
					eh (this, new AreaPreparedEventArgs (item));
					return false;
				});
		}

		void SignalAreaUpdated (ImageLoaderItem item, Rectangle area) {
			EventHandler<AreaUpdatedEventArgs> eh = AreaUpdated;
			if (eh == null)
				return;

			lock (sync_handle) {
				if (damage == Rectangle.Zero) {
					damage = area;
					GLib.Idle.Add (delegate {
						Rectangle to_signal;
						lock (sync_handle) {
							to_signal = damage;
							damage = Rectangle.Zero;
						}
						eh (this, new AreaUpdatedEventArgs (item, to_signal));
						return false;
					});
				} else {
					damage = damage.Union (area);
				}
			}
		}

		void SignalItemCompleted (ImageLoaderItem item)
		{
			ItemsCompleted |= item;
			Log.Debug ("Notifying completion of {0} (done: {1}, requested: {2})", item, ItemsCompleted, ItemsRequested);

			Monitor.Enter (sync_handle);
			Monitor.PulseAll (sync_handle);
			Monitor.Exit (sync_handle);

			Log.Debug ("Signalled!");

			EventHandler<ItemsCompletedEventArgs> eh = Completed;
			if (eh != null)
				GLib.Idle.Add (delegate {
					eh (this, new ItemsCompletedEventArgs (item));
					return false;
				});
		}
#endregion
	}
}	       
