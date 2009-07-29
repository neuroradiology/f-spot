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
		static Dictionary<string, System.Type> name_table;

		static ImageLoader ()
		{
			name_table = new Dictionary<string, System.Type> ();
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
			name_table ["image/bmp"]     = name_table ["image/x-bmp"] = name_table [".bmp"];
			name_table ["image/gif"]     = name_table [".gif"];
			name_table ["image/pjpeg"]   = name_table ["image/jpeg"] = name_table ["image/jpg"] = name_table [".jpg"];
			name_table ["image/x-png"]   = name_table ["image/png"]  = name_table [".png"];
			name_table ["image/svg+xml"] = name_table [".svg"];
			name_table ["image/tiff"]    = name_table [".tiff"];
			name_table ["image/x-dcraw"] = name_table [".raw"];
			name_table ["image/x-ciff"]  = name_table [".crw"];
			name_table ["image/x-mrw"]   = name_table [".mrw"];
			name_table ["image/x-x3f"]   = name_table [".x3f"];
			name_table ["image/x-orf"]   = name_table [".orf"];
			name_table ["image/x-nef"]   = name_table [".nef"];
			name_table ["image/x-cr2"]   = name_table [".cr2"];
			name_table ["image/x-raf"]   = name_table [".raf"];

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
				GLib.FileInfo info = GLib.FileFactory.NewForUri (uri).QueryInfo ("standard::type,standard::content-type", GLib.FileQueryInfoFlags.None, null);
				if (!name_table.TryGetValue (info.ContentType, out t))
					throw new Exception ("Loader requested for unknown file type: "+extension);
			}

			loader = (IImageLoader) System.Activator.CreateInstance (t, new object[] { uri });

			return loader;
		}
	}
}
