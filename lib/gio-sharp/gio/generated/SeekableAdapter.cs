// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public class SeekableAdapter : GLib.GInterfaceAdapter, GLib.Seekable {

		static SeekableIface iface;

		struct SeekableIface {
			public IntPtr gtype;
			public IntPtr itype;

			public IntPtr tell;
			public IntPtr can_seek;
			public SeekDelegate seek;
			public CanTruncateDelegate can_truncate;
			public IntPtr truncate_fn;
		}

		static SeekableAdapter ()
		{
			GLib.GType.Register (_gtype, typeof(SeekableAdapter));
			iface.seek = new SeekDelegate (SeekCallback);
			iface.can_truncate = new CanTruncateDelegate (CanTruncateCallback);
		}


		[GLib.CDeclCallback]
		delegate bool SeekDelegate (IntPtr seekable, long offset, GLib.SeekType type, IntPtr cancellable, out IntPtr error);

		static bool SeekCallback (IntPtr seekable, long offset, GLib.SeekType type, IntPtr cancellable, out IntPtr error)
		{
			error = IntPtr.Zero;

			try {
				GLib.SeekableImplementor __obj = GLib.Object.GetObject (seekable, false) as GLib.SeekableImplementor;
				bool __result = __obj.Seek (offset, type, GLib.Object.GetObject(cancellable) as GLib.Cancellable);
				return __result;
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, true);
				// NOTREACHED: above call does not return.
				throw e;
			}
		}

		[GLib.CDeclCallback]
		delegate bool CanTruncateDelegate (IntPtr seekable);

		static bool CanTruncateCallback (IntPtr seekable)
		{
			try {
				GLib.SeekableImplementor __obj = GLib.Object.GetObject (seekable, false) as GLib.SeekableImplementor;
				bool __result = __obj.CanTruncate ();
				return __result;
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, true);
				// NOTREACHED: above call does not return.
				throw e;
			}
		}
		static void Initialize (IntPtr ifaceptr, IntPtr data)
		{
			SeekableIface native_iface = (SeekableIface) Marshal.PtrToStructure (ifaceptr, typeof (SeekableIface));
			native_iface.tell = iface.tell;
			native_iface.can_seek = iface.can_seek;
			native_iface.seek = iface.seek;
			native_iface.can_truncate = iface.can_truncate;
			native_iface.truncate_fn = iface.truncate_fn;
			Marshal.StructureToPtr (native_iface, ifaceptr, false);
			GCHandle gch = (GCHandle) data;
			gch.Free ();
		}

		public SeekableAdapter ()
		{
			InitHandler = new GLib.GInterfaceInitHandler (Initialize);
		}

		SeekableImplementor implementor;

		public SeekableAdapter (SeekableImplementor implementor)
		{
			if (implementor == null)
				throw new ArgumentNullException ("implementor");
			this.implementor = implementor;
		}

		public SeekableAdapter (IntPtr handle)
		{
			this.handle = handle;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_seekable_get_type();

		private static GLib.GType _gtype = new GLib.GType (g_seekable_get_type ());

		public override GLib.GType GType {
			get {
				return _gtype;
			}
		}

		IntPtr handle;
		public override IntPtr Handle {
			get {
				if (handle != IntPtr.Zero)
					return handle;
				return implementor == null ? IntPtr.Zero : implementor.Handle;
			}
		}

		public static Seekable GetObject (IntPtr handle, bool owned)
		{
			GLib.Object obj = GLib.Object.GetObject (handle, owned);
			return GetObject (obj);
		}

		public static Seekable GetObject (GLib.Object obj)
		{
			if (obj == null)
				return null;
			else if (obj is SeekableImplementor)
				return new SeekableAdapter (obj as SeekableImplementor);
			else if (obj as Seekable == null)
				return new SeekableAdapter (obj.Handle);
			else
				return obj as Seekable;
		}

		public SeekableImplementor Implementor {
			get {
				return implementor;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern long g_seekable_tell(IntPtr raw);

		public long Position { 
			get {
				long raw_ret = g_seekable_tell(Handle);
				long ret = raw_ret;
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern bool g_seekable_can_seek(IntPtr raw);

		public bool CanSeek { 
			get {
				bool raw_ret = g_seekable_can_seek(Handle);
				bool ret = raw_ret;
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern bool g_seekable_truncate(IntPtr raw, long offset, IntPtr cancellable, out IntPtr error);

		public bool Truncate(long offset, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_seekable_truncate(Handle, offset, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern bool g_seekable_can_truncate(IntPtr raw);

		public bool CanTruncate() {
			bool raw_ret = g_seekable_can_truncate(Handle);
			bool ret = raw_ret;
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern bool g_seekable_seek(IntPtr raw, long offset, GLib.SeekType type, IntPtr cancellable, out IntPtr error);

		public bool Seek(long offset, GLib.SeekType type, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_seekable_seek(Handle, offset, type, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

#endregion
	}
}
