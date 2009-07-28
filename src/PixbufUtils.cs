//
// FSpot.PixbufUtils.cs
//
// Author(s):
//	Ettore Perazzoli
//	Larry Ewing  <lewing@novell.com>
//	Stephane Delcroix  <stephane@declroix.org>
//
// This is free software. See COPYING for details
//

using Gdk;
using System.Collections;
using System;
using System.IO;
using System.Runtime.InteropServices;
using FSpot;
using FSpot.Utils;

public class FSpotPixbufUtils {
	static Pixbuf error_pixbuf = null;
	public static Pixbuf ErrorPixbuf {
		get {
			if (error_pixbuf == null)
				error_pixbuf = GtkUtil.TryLoadIcon (FSpot.Global.IconTheme, "f-spot-question-mark", 256, (Gtk.IconLookupFlags)0);
			return error_pixbuf;
		}
	}
	public static Pixbuf LoadingPixbuf = PixbufUtils.LoadFromAssembly ("f-spot-loading.png");
	
	public static Pixbuf TagIconFromPixbuf (Pixbuf source)
	{
		return IconFromPixbuf (source, (int) Tag.IconSize.Large);
	}

	public static Pixbuf IconFromPixbuf (Pixbuf source, int size)
	{
		Pixbuf tmp = null;
		Pixbuf icon = null;

		if (source.Width > source.Height)
			source = tmp = new Pixbuf (source, (source.Width - source.Height) /2, 0, source.Height, source.Height);
		else if (source.Width < source.Height) 
			source = tmp = new Pixbuf (source, 0, (source.Height - source.Width) /2, source.Width, source.Width);

		if (source.Width == source.Height)
			icon = source.ScaleSimple (size, size, InterpType.Bilinear);
		else
			throw new Exception ("Bad logic leads to bad accidents");

		if (tmp != null)
			tmp.Dispose ();
		
		return icon;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct FPixbufJpegMarker {
		public int type;
		public byte *data;
		public int length;
	}

	[DllImport ("libfspot")]
	static extern bool f_pixbuf_save_jpeg (IntPtr src, string path, int quality, FPixbufJpegMarker [] markers, int num_markers);

	public static void SaveJpeg (Pixbuf pixbuf, string path, int quality, Exif.ExifData exif_data)
	{
		Pixbuf temp = null;
		if (pixbuf.HasAlpha) {
			temp = PixbufUtils.Flatten (pixbuf);
			pixbuf = temp;
		}

		// The DCF spec says thumbnails should be 160x120 always
		Pixbuf thumbnail = PixbufUtils.ScaleToAspect (pixbuf, 160, 120);
		byte [] thumb_data = PixbufUtils.Save (thumbnail, "jpeg", null, null);
		thumbnail.Dispose ();

		byte [] data = new byte [0]; 
		FPixbufJpegMarker [] marker = new FPixbufJpegMarker [0];
		bool result = false;

		if (exif_data != null && exif_data.Handle.Handle != IntPtr.Zero) {
			exif_data.Data = thumb_data;

			// Most of the things we will set will be in the 0th ifd
			Exif.ExifContent content = exif_data.GetContents (Exif.Ifd.Zero);

			// reset the orientation tag the default is top/left
			content.GetEntry (Exif.Tag.Orientation).Reset ();

			// set the write time in the datetime tag
			content.GetEntry (Exif.Tag.DateTime).Reset ();

			// set the software tag
			content.GetEntry (Exif.Tag.Software).SetData (FSpot.Defines.PACKAGE + " version " + FSpot.Defines.VERSION);

			data = exif_data.Save ();
		}

		unsafe {
			if (data.Length > 0) {
				
				fixed (byte *p = data) {
					marker = new FPixbufJpegMarker [1];
					marker [0].type = 0xe1; // APP1 marker
					marker [0].data = p;
					marker [0].length = data.Length;
					
					result = f_pixbuf_save_jpeg (pixbuf.Handle, path, quality, marker, marker.Length);
				}					
			} else
				result = f_pixbuf_save_jpeg (pixbuf.Handle, path, quality, marker, marker.Length);
			
		}

		if (temp != null)
			temp.Dispose ();
		
		if (result == false)
			throw new System.Exception ("Error Saving File");
	}
}
