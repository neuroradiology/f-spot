// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public class ThemedIcon : GLib.Object, GLib.Icon {

		[Obsolete]
		protected ThemedIcon(GLib.GType gtype) : base(gtype) {}
		public ThemedIcon(IntPtr raw) : base(raw) {}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_themed_icon_new(IntPtr iconname);

		public ThemedIcon (string iconname) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (ThemedIcon)) {
				throw new InvalidOperationException ("Can't override this constructor.");
			}
			IntPtr native_iconname = GLib.Marshaller.StringToPtrGStrdup (iconname);
			Raw = g_themed_icon_new(native_iconname);
			GLib.Marshaller.Free (native_iconname);
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_themed_icon_new_from_names(IntPtr iconnames, int len);

		public static ThemedIcon NewFromNames(string iconnames)
		{
			ThemedIcon result = new ThemedIcon (g_themed_icon_new_from_names(GLib.Marshaller.StringToPtrGStrdup(iconnames), System.Text.Encoding.UTF8.GetByteCount (iconnames)));
			return result;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_themed_icon_new_with_default_fallbacks(IntPtr iconname);

		public static ThemedIcon NewWithDefaultFallbacks(string iconname)
		{
			IntPtr native_iconname = GLib.Marshaller.StringToPtrGStrdup (iconname);
			ThemedIcon result = new ThemedIcon (g_themed_icon_new_with_default_fallbacks(native_iconname));
			GLib.Marshaller.Free (native_iconname);
			return result;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_themed_icon_get_names(IntPtr raw);

		[GLib.Property ("names")]
		public string[] Names {
			get  {
				IntPtr raw_ret = g_themed_icon_get_names(Handle);
				string[] ret = GLib.Marshaller.NullTermPtrToStringArray (raw_ret, false);
				return ret;
			}
		}

		[GLib.Property ("use-default-fallbacks")]
		public bool UseDefaultFallbacks {
			get {
				GLib.Value val = GetProperty ("use-default-fallbacks");
				bool ret = (bool) val;
				val.Dispose ();
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern void g_themed_icon_append_name(IntPtr raw, IntPtr iconname);

		public void AppendName(string iconname) {
			IntPtr native_iconname = GLib.Marshaller.StringToPtrGStrdup (iconname);
			g_themed_icon_append_name(Handle, native_iconname);
			GLib.Marshaller.Free (native_iconname);
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_themed_icon_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = g_themed_icon_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern void g_themed_icon_prepend_name(IntPtr raw, IntPtr iconname);

		public void PrependName(string iconname) {
			IntPtr native_iconname = GLib.Marshaller.StringToPtrGStrdup (iconname);
			g_themed_icon_prepend_name(Handle, native_iconname);
			GLib.Marshaller.Free (native_iconname);
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_icon_to_string(IntPtr raw);

		public override string ToString() {
			IntPtr raw_ret = g_icon_to_string(Handle);
			string ret = GLib.Marshaller.PtrToStringGFree(raw_ret);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern bool g_icon_equal(IntPtr raw, IntPtr icon2);

		public bool Equal(GLib.Icon icon2) {
			bool raw_ret = g_icon_equal(Handle, icon2 == null ? IntPtr.Zero : icon2.Handle);
			bool ret = raw_ret;
			return ret;
		}

#endregion
	}
}
