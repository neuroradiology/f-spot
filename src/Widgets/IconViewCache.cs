/*
 * FSpot.Widgets.IconViewCache.cs
 *
 * Author(s):
 * 	Larry Ewing <lewing@novell.com>
 *
 * This is free software. See COPYING for details.
 */

using Gdk;

using System;
using System.Collections;
using System.Threading;

using FSpot.Utils;
using FSpot.Platform;
using FSpot.Loaders;

namespace FSpot.Widgets {
	public class IconViewCache {
		Hashtable items;
		ArrayList items_mru;
		int total_size;
		int max_size = 256 * 256 * 4 * 30;

		private Thread worker;

		public delegate void PixbufLoadedHandler (IconViewCache cache, CacheEntry entry);
		public event PixbufLoadedHandler OnPixbufLoaded;

		public IconViewCache ()
		{
			items = new Hashtable ();
			items_mru = new ArrayList ();

			worker = new Thread (new ThreadStart (WorkerTask));
			worker.Start ();

			ThumbnailGenerator.Default.OnPixbufLoaded += HandleThumbnailLoaded;
		}

		public void HandleThumbnailLoaded (ImageLoaderThread loader, Uri uri, int order, Gdk.Pixbuf result)
		{
			if (result != null)
				Reload (uri);
		}

		public void Request (Uri uri, object closure, int max_width, int max_height)
		{
			lock (items) {
				CacheEntry entry = items[uri] as CacheEntry;

				if (entry == null) {
					entry = new CacheEntry (this, uri, closure, max_width, max_height);
					items [uri] = entry;
					items_mru.Add (entry);
				} else {
					MoveForward (entry);
					entry.Data = closure;
				}
				Monitor.Pulse (items);
			}
		}

//		public void Update (Uri uri, Gdk.Pixbuf pixbuf)
//		{
//			lock (items) {
//				CacheEntry entry = (CacheEntry) items [uri];
//				if (entry != null) {
//					entry.SetPixbufExtended (pixbuf, true);
//				}
//			}
//		}

		public void Update (CacheEntry entry, Gdk.Pixbuf pixbuf)
		{
			lock (items) {
				entry.SetPixbufExtended (pixbuf, true);
			}
		}

		public void Reload (CacheEntry entry, object data, int max_width, int max_height)
		{
			lock (items) {
				lock (entry) {
					entry.Reload = true;
					entry.MaxWidth = max_width;
					entry.MaxHeight = max_height;
					entry.Data = data;
				}
				Monitor.Pulse (items);
			}
		}

		public void Reload (Uri uri)
		{
			CacheEntry entry;

			lock (items) {
				entry = (CacheEntry) items [uri];
				if (entry != null) {
					lock (entry) {
						entry.Reload = true;
					}
					Monitor.Pulse (items);
				}
			}
		}

		private CacheEntry FindNext ()
		{
			CacheEntry entry;
			int i = items_mru.Count;
			int size = 0;
			if (total_size > max_size * 4) {
				//System.Console.WriteLine ("Hit major limit ({0}) out of {1}",
				//			  total_size, max_size);
				return null;
			}
			while (i-- > 0) {
				entry = (CacheEntry) items_mru [i];
				lock (entry) {
					if (entry.Reload) {
						entry.Reload = false;
						return entry;
					}

					//if the depth of the queue is so large that we've reached double our limit
					//break out of here and let the queue shrink.
					if (entry.Pixbuf != null)
						size += entry.Size;

					if (size > max_size * 2) {
						//System.Console.WriteLine ("Hit limit ({0},{1}) out of {2}",
						//			  size, total_size,max_size);
						return null;
					}
				}
			}
			return null;
		}

		private bool ShrinkIfNeeded ()
		{
			int num = 0;
			while ((items_mru.Count - num) > 10 && total_size > max_size) {
				CacheEntry entry = (CacheEntry) items_mru [num++];
				items.Remove (entry.Uri);
				entry.Dispose ();
			}
			if (num > 0) {
				//System.Console.WriteLine ("removing {0} out of {3}  ({1} > {2})",
				//			  num, total_size, max_size, items_mru.Count);
				items_mru.RemoveRange (0, num);
				return true;
			}
			return false;
		}

		private void WorkerTask ()
		{
			CacheEntry current = null;
			//ThumbnailGenerator.Default.PushBlock ();
			while (true) {
				try {
					lock (items) {
						/* find the next item */
						while ((current = FindNext ()) == null) {
							if (!ShrinkIfNeeded ()){
								//ThumbnailGenerator.Default.PopBlock ();
								Monitor.Wait (items);
								//ThumbnailGenerator.Default.PushBlock ();
							}
						}
					}
					ProcessRequest (current);
					QueueLast (current);
				} catch (System.Exception e) {
					System.Console.WriteLine (e);
					current = null;
				}
			}
		}

		protected virtual void ProcessRequest (CacheEntry entry)
		{
			Gdk.Pixbuf loaded = null;
			try {
				using (IImageLoader loader = ImageLoader.Create (entry.Uri)) {
					loader.Load (ImageLoaderItem.Thumbnail);
					loaded = loader.Thumbnail;
				}
				this.Update (entry, loaded);
			} catch (GLib.GException){
				if (loaded != null)
					loaded.Dispose ();
				return;
			}
		}

		private void QueueLast (CacheEntry entry)
		{
			Gtk.Application.Invoke (delegate (object obj, System.EventArgs args) {
				if (entry.Uri != null && OnPixbufLoaded != null)
					OnPixbufLoaded (this, entry);
			});
		}

		private void MoveForward (CacheEntry entry)
		{
#if true
			int i = items_mru.Count;
			CacheEntry tmp1 = entry;
			CacheEntry tmp2;
			while (i-- > 0) {
				tmp2 = (CacheEntry) items_mru [i];
				items_mru [i] = tmp1;
				tmp1 = tmp2;
				if (tmp2 == entry)
					return;
			}
			throw new System.Exception ("move forward failed");
#else
			items_mru.Remove (entry);
			items_mru.Add (entry);
#endif
		}


		private CacheEntry ULookup (Uri uri)
		{
			CacheEntry entry = (CacheEntry) items [uri];
			if (entry != null) {
				MoveForward (entry);
			}
			return (CacheEntry) entry;
		}

		public CacheEntry Lookup (Uri uri)
		{
			lock (items) {
				return ULookup (uri);
			}
		}

		private void URemove (Uri uri)
		{
			CacheEntry entry = (CacheEntry) items [uri];
			if (entry != null) {
				items.Remove (uri);
				items_mru.Remove (entry);
				entry.Dispose ();
			}
		}

		public void Remove (Uri uri)
		{
			lock (items) {
				URemove (uri);
			}
		}

		public class CacheEntry : System.IDisposable {
			private Gdk.Pixbuf pixbuf;
			private object data;
			private IconViewCache cache;

			public CacheEntry (IconViewCache cache, Uri uri, object closure, int max_width, int max_height)
			{
				this.Uri = uri;
				this.MaxHeight = max_height;
				this.MaxWidth = max_width;
				this.data = closure;
				this.Reload = true;
				this.cache = cache;
				cache.total_size += this.Size;
			}

			public bool Reload { get; set; }
			public Uri Uri { get; private set; }
			public int MaxHeight { get; set; }
			public int MaxWidth { get; set; }

			public object Data {
				get {
					lock (this) {
						return data;
					}
				}
				set {
					lock (this) {
						data = value;
					}
				}
			}

			public bool IsDisposed { get { return Uri == null; } }

			public void SetPixbufExtended (Gdk.Pixbuf value, bool ignore_undead)
			{
				lock (this) {
					if (IsDisposed) {
						if (ignore_undead) {
							return;
						} else {
							throw new System.Exception ("I don't want to be undead");
						}
					}

					Gdk.Pixbuf old = this.Pixbuf;
					cache.total_size -= this.Size;
					this.pixbuf = value;
					cache.total_size += this.Size;
					this.Reload = false;

					if (old != null)
						old.Dispose ();
				}
			}

			public Gdk.Pixbuf Pixbuf {
				get {
					lock (this) {
						return pixbuf;
					}
				}
			}

			public Gdk.Pixbuf ShallowCopyPixbuf ()
			{
				lock (this) {
					if (IsDisposed)
						return null;

					if (pixbuf == null)
						return null;

					return pixbuf.ShallowCopy ();
				}
			}

			~CacheEntry ()
			{
				if (!IsDisposed)
					this.Dispose ();
			}

			public void Dispose ()
			{
				lock (this) {
					if (! IsDisposed)
						cache.total_size -= this.Size;

					if (this.pixbuf != null) {
						this.pixbuf.Dispose ();

					}
					this.pixbuf = null;
					this.cache = null;
					this.Uri = null;
				}
				System.GC.SuppressFinalize (this);
			}

			public int Size {
				get {
					return pixbuf == null ? 0 : pixbuf.Width * pixbuf.Height * 3;
				}
			}
		}
	}
}
