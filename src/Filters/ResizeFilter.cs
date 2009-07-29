/*
 * Filters/ResizeFilter.cs
 *
 * Author(s)
 *
 *   Stephane Delcroix <stephane@delcroix.org>
 *   Larry Ewing <lewing@novell.com>
 *
 * This is free software. See COPYING for details
 *
 */
using System;
using System.IO;

using FSpot;
using FSpot.Loaders;
using FSpot.Imaging;

using Mono.Unix;

using Gdk;

namespace FSpot.Filters {
	public class ResizeFilter : IFilter
	{
		public ResizeFilter () : this (600)
		{
		}

		public ResizeFilter (int size)
		{
			Size = size;
		}

		public int Size { get; set; }

		public bool Convert (FilterRequest req)
		{
			string source = req.Current.LocalPath;
			System.Uri dest_uri = req.TempUri (System.IO.Path.GetExtension (source));
			string dest = dest_uri.LocalPath;

			using (ImageFile img = ImageFile.Create (req.Current)) {
				Pixbuf scaled;
				using (IImageLoader loader = ImageLoader.Create (req.Current)) {
					loader.Load (ImageLoaderItem.Full);

					using (Pixbuf pixbuf = loader.Full) {
						if (pixbuf.Width < Size && pixbuf.Height < Size)
							return false;

						scaled = PixbufUtils.ScaleToMaxSize (pixbuf, Size, Size, false);
					}
				}

				using (scaled) {
					string destination_extension = Path.GetExtension (dest);

					if (Path.GetExtension (source).ToLower () == Path.GetExtension (dest).ToLower () && img is IWritableImageFile) {
						using (Stream output = File.OpenWrite (dest)) {
							(img as IWritableImageFile).Save (scaled, output);
						}
					} else if (destination_extension == ".jpg") {
						// FIXME this is a bit of a nasty hack to work around
						// the lack of being able to change the path in this filter
						// and the lack of proper metadata copying yuck
						Exif.ExifData exif_data;
	
						exif_data = new Exif.ExifData (source);
						
						FSpotPixbufUtils.SaveJpeg (scaled, dest, 95, exif_data);
					} else {
						// FIXME: Metadata is lost, we need decent metadata handling!
						dest_uri = req.TempUri (".jpg");
						dest = dest_uri.LocalPath;

						byte [] image_data = PixbufUtils.Save (scaled, "jpeg", new string [] {"quality" }, new string [] { "95" });
						using (Stream stream = System.IO.File.OpenWrite (dest)) {
							stream.Write (image_data, 0, image_data.Length);
						}
					}
				}
			}

			req.Current = dest_uri;
			return true;
		}
	}
}

