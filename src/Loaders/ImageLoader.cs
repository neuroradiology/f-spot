//
// Fspot.Loaders.ImageLoader.cs
//
// Copyright (c) 2009 Novell, Inc.
//
// Author(s)
//	Stephane Delcroix  <sdelcroix@novell.com>
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using FSpot.Utils;
using System;
using System.Collections.Generic;
using Gdk;

namespace FSpot.Loaders {
	public static class ImageLoader {
		static Dictionary<string, Type> name_table;
		static Dictionary<string, Type> mime_table;

		static ImageLoader ()
		{
			name_table = new Dictionary<string, Type> ();
			name_table [".svg"] = typeof (GdkImageLoader);
			name_table [".gif"] = typeof (GdkImageLoader);
			name_table [".bmp"] = typeof (GdkImageLoader);
			name_table [".jpeg"] = typeof (GdkImageLoader);
			name_table [".jpg"] = typeof (GdkImageLoader);
			name_table [".png"] = typeof (GdkImageLoader);
			name_table [".cr2"] = typeof (LibrawImageLoader);
			name_table [".nef"] = typeof (LibrawImageLoader);
			name_table [".pef"] = typeof (LibrawImageLoader);
			name_table [".raw"] = typeof (LibrawImageLoader);
			name_table [".kdc"] = typeof (LibrawImageLoader);
			name_table [".arw"] = typeof (LibrawImageLoader);
			name_table [".tiff"] = typeof (GdkImageLoader);
			name_table [".tif"] = typeof (GdkImageLoader);
			name_table [".orf"] =  typeof (LibrawImageLoader);
			name_table [".srf"] = typeof (LibrawImageLoader);
			name_table [".dng"] = typeof (LibrawImageLoader);
			name_table [".crw"] = typeof (LibrawImageLoader);
			name_table [".ppm"] = typeof (GdkImageLoader);
			name_table [".mrw"] = typeof (LibrawImageLoader);
			name_table [".raf"] = typeof (LibrawImageLoader);
			name_table [".x3f"] = typeof (LibrawImageLoader);

			// add mimetypes for fallback
			mime_table = new Dictionary<string, Type> ();
			mime_table ["image/bmp"]     = mime_table ["image/x-bmp"] = name_table [".bmp"];
			mime_table ["image/gif"]     = name_table [".gif"];
			mime_table ["image/pjpeg"]   = mime_table ["image/jpeg"] = mime_table ["image/jpg"] = name_table [".jpg"];
			mime_table ["image/x-png"]   = mime_table ["image/png"]  = name_table [".png"];
			mime_table ["image/svg+xml"] = name_table [".svg"];
			mime_table ["image/tiff"]    = name_table [".tiff"];
			mime_table ["image/x-dcraw"] = name_table [".raw"];
			mime_table ["image/x-ciff"]  = name_table [".crw"];
			mime_table ["image/x-mrw"]   = name_table [".mrw"];
			mime_table ["image/x-x3f"]   = name_table [".x3f"];
			mime_table ["image/x-orf"]   = name_table [".orf"];
			mime_table ["image/x-nef"]   = name_table [".nef"];
			mime_table ["image/x-cr2"]   = name_table [".cr2"];
			mime_table ["image/x-raf"]   = name_table [".raf"];

			//as xcf pixbufloader is not part of gdk-pixbuf, check if it's there,
			//and enable it if needed.
			foreach (Gdk.PixbufFormat format in Gdk.Pixbuf.Formats)
				if (format.Name == "xcf") {
					if (format.IsDisabled)
						format.SetDisabled (false);
					name_table [".xcf"] = typeof (GdkImageLoader);
				}
		}

		public static IImageLoader Create (Uri uri)
		{
			string path = uri.AbsolutePath;
			string extension = System.IO.Path.GetExtension (path).ToLower ();
			System.Type t;
			IImageLoader loader;

			if (!name_table.TryGetValue (extension, out t)) {
				// check if GIO can find the file, which is not the case
				// with filenames with invalid encoding
				GLib.File f = GLib.FileFactory.NewForUri (uri);
				if (f.QueryExists (null)) {
					GLib.FileInfo info = f.QueryInfo ("standard::type,standard::content-type", GLib.FileQueryInfoFlags.None, null);
					if (!mime_table.TryGetValue (info.ContentType, out t))
						throw new Exception ("Loader requested for unknown file type: "+extension);
				}
			}

			loader = (IImageLoader) System.Activator.CreateInstance (t, new object[] { uri });

			return loader;
		}

		[Obsolete ("Use Uri instead, this will deprecate soon")]
		public static bool IsAvailable (string path)
		{
			return IsAvailable (UriUtils.PathToFileUri (path));
		}

		public static bool IsAvailable (Uri uri)
		{
			string path = uri.AbsolutePath;
			string extension = System.IO.Path.GetExtension (path).ToLower ();
			return name_table.ContainsKey (extension);
		}
	}
}
