/*
 * FSpot.ThumbnailGenerator.cs
 *
 * Author(s)
 * 	Larry Ewing  <lewing@novell.com>
 *
 * This is free software. See COPYING for details.
 */

using System;
using System.IO;
using FSpot.Utils;
using FSpot.Loaders;
using FSpot.Platform;

using Mono.Unix.Native;
using GFileInfo = GLib.FileInfo;

namespace FSpot {
	public class ThumbnailGenerator : ImageLoaderThread {

		static public ThumbnailGenerator Default = new ThumbnailGenerator ();
		
		public const string ThumbMTime = "tEXt::Thumb::MTime";
		public const string ThumbUri = "tEXt::Thumb::URI";
		public const string ThumbImageWidth = "tEXt::Thumb::Image::Width";
		public const string ThumbImageHeight = "tEXt::Thumb::Image::Height"; 

		public static Gdk.Pixbuf Create (Uri uri)
		{
			using (IImageLoader loader = ImageLoader.Create (uri)) {
				loader.Load (ImageLoaderItem.Thumbnail);
				Gdk.Pixbuf thumb = loader.Thumbnail;
				Save (thumb, uri);
				return thumb;
			}
		}
		
		private static void Save (Gdk.Pixbuf image, Uri uri)
		{
			try {
				ThumbnailCache.Default.RemoveThumbnailForUri (uri);
			} finally {
				ThumbnailFactory.SaveThumbnail (image, uri);
			}
		}

		protected override void EmitLoaded (System.Collections.Queue results)
		{
			base.EmitLoaded (results);
			
			foreach (RequestItem r in results) {
				if (r.result != null)
					r.result.Dispose ();
			}
				
		}

		public override void Request (Uri uri, int order, ImageLoaderItem item)
		{
			if (ThumbnailFactory.ThumbnailExists (uri) && ThumbnailFactory.ThumbnailIsRecent (uri))
				return;

			base.Request (uri, order, item);
		}

		protected override void ProcessRequest (RequestItem request)
		{
			try {
				base.ProcessRequest (request);

				Gdk.Pixbuf image = request.result;
				if (image != null)
					Save (image, request.uri);

				System.Threading.Thread.Sleep (75);
			} catch (System.Exception e) {
				System.Console.WriteLine (e.ToString ());
			}
		}

	}
}
