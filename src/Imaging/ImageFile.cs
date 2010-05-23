using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using FSpot.Utils;
using FSpot.Imaging;
using Mono.Unix;
using Mono.Unix.Native;
using Gdk;

using GFileInfo = GLib.FileInfo;

namespace FSpot {
	public class ImageFormatException : ApplicationException {
		public ImageFormatException (string msg) : base (msg)
		{
		}
	}

	public class ImageFile : IDisposable {
		protected Uri uri;

		static Dictionary<string, Type> name_table;
		static Dictionary<string, Type> mime_table;

		public ImageFile (string path) 
		{
			this.uri = UriUtils.PathToFileUri (path);
		}
		
		public ImageFile (Uri uri)
		{
			this.uri = uri;
		}

		~ImageFile ()
		{
			Dispose ();
		}
		
		protected Stream Open ()
		{
			Log.Debug ("open uri = {0}", uri.ToString ());
//			if (uri.IsFile)
//				return new FileStream (uri.LocalPath, FileMode.Open);
			return new GLib.GioStream (GLib.FileFactory.NewForUri (uri).Read (null));
		}

		public static void AddExtension (string extension, Type type)
		{
			name_table [extension] = type;
		}

		static ImageFile ()
		{
			name_table = new Dictionary<string, Type> ();
			name_table [".svg"] = typeof (FSpot.Svg.SvgFile);
			name_table [".gif"] = typeof (ImageFile);
			name_table [".bmp"] = typeof (ImageFile);
			name_table [".pcx"] = typeof (ImageFile);
			name_table [".jpeg"] = typeof (JpegFile);
			name_table [".jpg"] = typeof (JpegFile);
			name_table [".png"] = typeof (FSpot.Png.PngFile);
			name_table [".cr2"] = typeof (FSpot.Tiff.TiffFile);
			name_table [".nef"] = typeof (FSpot.Tiff.NefFile);
			name_table [".pef"] = typeof (FSpot.Tiff.NefFile);
			name_table [".raw"] = typeof (FSpot.Tiff.NefFile);
			name_table [".kdc"] = typeof (FSpot.Tiff.NefFile);
			name_table [".arw"] = typeof (FSpot.Tiff.NefFile);
			name_table [".rw2"] = typeof (FSpot.Tiff.NefFile);
			name_table [".tiff"] = typeof (FSpot.Tiff.TiffFile);
			name_table [".tif"] = typeof (FSpot.Tiff.TiffFile);
			name_table [".orf"] =  typeof (FSpot.Tiff.NefFile);
			name_table [".srf"] = typeof (FSpot.Tiff.NefFile);
			name_table [".dng"] = typeof (FSpot.Tiff.DngFile);
			name_table [".crw"] = typeof (FSpot.Ciff.CiffFile);
			name_table [".ppm"] = typeof (FSpot.Pnm.PnmFile);
			name_table [".mrw"] = typeof (FSpot.Mrw.MrwFile);
			name_table [".raf"] = typeof (FSpot.Raf.RafFile);
			name_table [".x3f"] = typeof (FSpot.X3f.X3fFile);

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
					name_table [".xcf"] = typeof (ImageFile);
				}
		}

		public Uri Uri {
			get { return this.uri; }
		}

		public PixbufOrientation Orientation {
			get { return GetOrientation (); }
		}

		public virtual string Description
		{
			get { return null; }
		}

		protected Gdk.Pixbuf TransformAndDispose (Gdk.Pixbuf orig)
		{
			if (orig == null)
				return null;

			Gdk.Pixbuf rotated = FSpot.Utils.PixbufUtils.TransformOrientation (orig, this.Orientation);

			orig.Dispose ();
			
			return rotated;
		}

		public virtual PixbufOrientation GetOrientation () 
		{
			return PixbufOrientation.TopLeft;
		}
		
		// FIXME this need to have an intent just like the loading stuff.
		public virtual Cms.Profile GetProfile ()
		{
			return null;
		}
		
		public virtual System.DateTime Date 
		{
			get {
				// FIXME mono uses the file change time (ctime) incorrectly
				// as the creation time so we try to work around that slightly
				GFileInfo info = GLib.FileFactory.NewForUri (uri).QueryInfo ("time::modified,time::created", GLib.FileQueryInfoFlags.None, null);
				DateTime write = NativeConvert.ToDateTime ((long)info.GetAttributeULong ("time::modified"));
				DateTime create = NativeConvert.ToDateTime ((long)info.GetAttributeULong ("time::created"));

				if (create < write)
					return create;
				else 
					return write;
			}
		}

		[Obsolete ("use Create (System.Uri) instead")]
		public static ImageFile Create (string path)
		{
			return Create (UriUtils.PathToFileUri (path));
		}

		public static ImageFile Create (Uri uri)
		{
			string path = uri.AbsolutePath;
			string extension = System.IO.Path.GetExtension (path).ToLower ();
			Type t = (Type) name_table [extension];

			if (!name_table.TryGetValue (extension, out t)) {
				GLib.FileInfo info = GLib.FileFactory.NewForUri (uri).QueryInfo ("standard::type,standard::content-type", GLib.FileQueryInfoFlags.None, null);
				if (!mime_table.TryGetValue (info.ContentType, out t))
					t = null;
			}

			ImageFile img;

			if (t != null)
				img = (ImageFile) System.Activator.CreateInstance (t, new object[] { uri });
			else 
				img = new ImageFile (uri);

			return img;
		}
		
		// FIXME these are horrible hacks to get a temporary name
		// with the right extension for ImageFile until we use the mime data
		// properly.  It is here to make sure we can find the places that use
		// this hack
		public static string TempPath (string name)
		{
			return TempPath (name, System.IO.Path.GetExtension (name));
		}
		
		public static string TempPath (string name, string extension)
		{
			string temp = System.IO.Path.GetTempFileName ();
			string imgtemp = temp + "." + extension;

			System.IO.File.Move (temp, imgtemp);

			return imgtemp;
		}

		public void Dispose ()
		{
			Close ();
			System.GC.SuppressFinalize (this);
		}

		protected virtual void Close ()
		{
		}

		public static bool IsJpeg (ImageFile image)
		{
			return image is JpegFile;
		}

		public static bool IsJpeg (Uri uri)
		{
			return IsJpeg (ImageFile.Create (uri));
		}

		public static bool IsRaw (ImageFile image)
		{
			return image is IRawFile;
		}

		public static bool IsRaw (Uri uri)
		{
			return IsRaw (ImageFile.Create (uri));
		}
	} 
}
