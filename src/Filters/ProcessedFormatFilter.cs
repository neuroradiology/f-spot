/*
 * Filters/ProcessedFormatFilter.cs
 *
 * Author(s)
 *   Ruben Vermeersch <ruben@savanne.be>
 *
 * This is free software. See COPYING for details.
 *
 */
using FSpot.Imaging;
using FSpot.Loaders;
using FSpot.Utils;
using Gdk;
using System;
using System.IO;

namespace FSpot.Filters {
	public class ProcessedFormatFilter : IFilter {
		public int JpegQuality { get; set; }

		public ProcessedFormatFilter ()
		{
			JpegQuality = 95;
		}

		public bool Convert (FilterRequest req)
		{
			// FIXME this should copy metadata from the original
			// even when the source is not a jpeg
			string source = req.Current.LocalPath;

			using (ImageFile img = ImageFile.Create (req.Current)) {
				Log.Debug ("Got file: {0}, RAW: {1}", req.Current, img is IRawFile);
				if (!ImageFile.IsRaw (img))
					return false;

				Pixbuf buffer = null;
				using (IImageLoader loader = ImageLoader.Create (req.Current)) {
					loader.Load (ImageLoaderItem.Full);
					buffer = loader.Full;
				}

				req.Current = req.TempUri ("jpg");
				string dest = req.Current.LocalPath;

				byte [] image_data = PixbufUtils.Save (buffer, "jpeg", new string [] {"quality" }, new string [] { JpegQuality.ToString () });
				using (Stream stream = System.IO.File.OpenWrite (dest)) {
					stream.Write (image_data, 0, image_data.Length);
				}
			}

			return true;
		}
	}
}
