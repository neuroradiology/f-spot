// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLibSharp {

	using System;
	using System.Runtime.InteropServices;

#region Autogenerated code
	[GLib.CDeclCallback]
	internal delegate bool FileReadMoreCallbackNative(IntPtr file_contents, long file_size, IntPtr callback_data);

	internal class FileReadMoreCallbackInvoker {

		FileReadMoreCallbackNative native_cb;
		IntPtr __data;
		GLib.DestroyNotify __notify;

		~FileReadMoreCallbackInvoker ()
		{
			if (__notify == null)
				return;
			__notify (__data);
		}

		internal FileReadMoreCallbackInvoker (FileReadMoreCallbackNative native_cb) : this (native_cb, IntPtr.Zero, null) {}

		internal FileReadMoreCallbackInvoker (FileReadMoreCallbackNative native_cb, IntPtr data) : this (native_cb, data, null) {}

		internal FileReadMoreCallbackInvoker (FileReadMoreCallbackNative native_cb, IntPtr data, GLib.DestroyNotify notify)
		{
			this.native_cb = native_cb;
			__data = data;
			__notify = notify;
		}

		internal GLib.FileReadMoreCallback Handler {
			get {
				return new GLib.FileReadMoreCallback(InvokeNative);
			}
		}

		bool InvokeNative (string file_contents, long file_size)
		{
			IntPtr native_file_contents = GLib.Marshaller.StringToPtrGStrdup (file_contents);
			bool result = native_cb (native_file_contents, file_size, __data);
			GLib.Marshaller.Free (native_file_contents);
			return result;
		}
	}

	internal class FileReadMoreCallbackWrapper {

		public bool NativeCallback (IntPtr file_contents, long file_size, IntPtr callback_data)
		{
			try {
				bool __ret = managed (GLib.Marshaller.Utf8PtrToString (file_contents), file_size);
				if (release_on_call)
					gch.Free ();
				return __ret;
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
				return false;
			}
		}

		bool release_on_call = false;
		GCHandle gch;

		public void PersistUntilCalled ()
		{
			release_on_call = true;
			gch = GCHandle.Alloc (this);
		}

		internal FileReadMoreCallbackNative NativeDelegate;
		GLib.FileReadMoreCallback managed;

		public FileReadMoreCallbackWrapper (GLib.FileReadMoreCallback managed)
		{
			this.managed = managed;
			if (managed != null)
				NativeDelegate = new FileReadMoreCallbackNative (NativeCallback);
		}

		public static GLib.FileReadMoreCallback GetManagedDelegate (FileReadMoreCallbackNative native)
		{
			if (native == null)
				return null;
			FileReadMoreCallbackWrapper wrapper = (FileReadMoreCallbackWrapper) native.Target;
			if (wrapper == null)
				return null;
			return wrapper.managed;
		}
	}
#endregion
}
