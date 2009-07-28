//
// FSpot.Utils.PixbufExtensions.cs
//
// Author(s)
// 	Ruben Vermeersch <ruben@savanne.be>
//
// This is free software. See COPYING for details.
//

using Gdk;
using System;
using System.Runtime.InteropServices;

namespace FSpot.Utils
{
	public static class PixbufExtensions
	{
		public static Pixbuf ShallowCopy (this Pixbuf pixbuf)
		{
			Pixbuf result = new Pixbuf (pixbuf, 0, 0, pixbuf.Width, pixbuf.Height);
			result.CopyThumbnailOptionsFrom (pixbuf);
			return result;
		}

		//
		// FIXME this is actually not public api and we should do a verison check,
		// but frankly I'm irritated that it isn't public so I don't much care.
		//
		[DllImport("libgdk_pixbuf-2.0-0.dll")]
		static extern bool gdk_pixbuf_set_option(IntPtr raw, string key, string value);

		public static bool SetOption(this Gdk.Pixbuf pixbuf, string key, string value)
		{

			if (value != null)
				return gdk_pixbuf_set_option(pixbuf.Handle, key, value);
			else
				return false;
		}

		public static void CopyThumbnailOptionsFrom (this Gdk.Pixbuf dest, Gdk.Pixbuf src)
		{
			if (src != null && dest != null) {
				dest.SetOption ("tEXt::Thumb::URI", src.GetOption ("tEXt::Thumb::URI"));
				dest.SetOption ("tEXt::Thumb::MTime", src.GetOption ("tEXt::Thumb::MTime"));
			}
		}
	}
}
