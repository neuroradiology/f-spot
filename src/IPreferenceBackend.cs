/*
 * FSpot.IPreferenceBackend.cs
 *
 * Author(s):
 *	Stephane Delcroix  <stephane@delcroix.org>
 *
 * This is free software. See COPYING for details.
 */

namespace FSpot
{
	public class NotifyEventArgs : System.EventArgs
	{
		string key;
		public string Key {
			get { return key; }
		}

		object val;
		public object Value {
			get { return val; }
		}

		public NotifyEventArgs (string key, object val)
		{
			this.key = key;
			this.val = val;
		}
	}

	public class NoSuchKeyException : System.Exception
	{
		public NoSuchKeyException () : base ()
		{
		}

		public NoSuchKeyException (string key) : base (key)
		{
		}
	}

	public delegate void NotifyChangedHandler (object sender, NotifyEventArgs args);

	public interface IPreferenceBackend
	{
		object Get (string key);
		void Set (string key, object value);	
		void AddNotify (string key, NotifyChangedHandler handler);
	}
}
