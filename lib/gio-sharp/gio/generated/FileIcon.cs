// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public class FileIcon : GLib.Object, GLib.Icon, GLib.LoadableIcon {

		[Obsolete]
		protected FileIcon(GLib.GType gtype) : base(gtype) {}
		public FileIcon(IntPtr raw) : base(raw) {}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_file_icon_new(IntPtr file);

		public FileIcon (GLib.File file) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (FileIcon)) {
				ArrayList vals = new ArrayList();
				ArrayList names = new ArrayList();
				if (file != null) {
					names.Add ("file");
					vals.Add (new GLib.Value (file));
				}
				CreateNativeObject ((string[])names.ToArray (typeof (string)), (GLib.Value[])vals.ToArray (typeof (GLib.Value)));
				return;
			}
			Raw = g_file_icon_new(file == null ? IntPtr.Zero : file.Handle);
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_file_icon_get_file(IntPtr raw);

		[GLib.Property ("file")]
		public GLib.File File {
			get  {
				IntPtr raw_ret = g_file_icon_get_file(Handle);
				GLib.File ret = GLib.FileAdapter.GetObject (raw_ret, false);
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_file_icon_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = g_file_icon_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
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

		[DllImport("libgio-2.0-0.dll")]
		static extern void g_loadable_icon_load_async(IntPtr raw, int size, IntPtr cancellable, GLibSharp.AsyncReadyCallbackNative cb, IntPtr user_data);

		public void LoadAsync(int size, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb) {
			GLibSharp.AsyncReadyCallbackWrapper cb_wrapper = new GLibSharp.AsyncReadyCallbackWrapper (cb);
			cb_wrapper.PersistUntilCalled ();
			g_loadable_icon_load_async(Handle, size, cancellable == null ? IntPtr.Zero : cancellable.Handle, cb_wrapper.NativeDelegate, IntPtr.Zero);
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_loadable_icon_load(IntPtr raw, int size, IntPtr type, IntPtr cancellable, out IntPtr error);

		public GLib.InputStream Load(int size, string type, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			IntPtr raw_ret = g_loadable_icon_load(Handle, size, GLib.Marshaller.StringToPtrGStrdup(type), cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			GLib.InputStream ret = GLib.Object.GetObject(raw_ret) as GLib.InputStream;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_loadable_icon_load_finish(IntPtr raw, IntPtr res, IntPtr type, out IntPtr error);

		public GLib.InputStream LoadFinish(GLib.AsyncResult res, string type) {
			IntPtr error = IntPtr.Zero;
			IntPtr raw_ret = g_loadable_icon_load_finish(Handle, res == null ? IntPtr.Zero : res.Handle, GLib.Marshaller.StringToPtrGStrdup(type), out error);
			GLib.InputStream ret = GLib.Object.GetObject(raw_ret) as GLib.InputStream;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

#endregion
	}
}
