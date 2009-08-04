//
// Fspot.Editors.Processing.Pipeline.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using FSpot.Utils;
using Gdk;
using System;
using System.Collections.Generic;

namespace FSpot.Editors.Processing {
	public class Pipeline : IDisposable
	{
#region Step registration
		static SortedList<uint, Step> Steps { get; set; }

		static Pipeline ()
		{
			Steps = new SortedList<uint, Step> ();
			AddStep (150, new ColorAdjustStep ());
		}

		public static void AddStep (uint order, Step step)
		{
			Steps.Add (order, step);
		}
#endregion

		public Photo Photo { get; private set; }

		public Pipeline (Photo photo)
		{
			Photo = photo;
			Settings = new Dictionary<string, Setting> ();

			SettingStore store = Core.Database.ProcessingSettings;
			foreach (Setting setting in store.GetAll (Photo.Id, Photo.DefaultVersionId)) {
				Settings.Add (setting.Key, setting);
			}
		}

		~Pipeline ()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			if (Output != null) {
				Output.Dispose ();
				Output = null;
			}

		}

#region Processing
		public Pixbuf Input { get; set; }
		public Cms.Profile InputProfile { get; set; }
		public Pixbuf Output { get; private set; }

		public void Process ()
		{
			Pixbuf input = Input.ShallowCopy ();
			Pixbuf output = null;
			foreach (Step step in Steps.Values) {
				step.Process (this, input, out output);
				input.Dispose ();
				input = output;
			}
			Output = output;
		}
#endregion

#region Settings
		Dictionary<string, Setting> Settings { get; set; }

		public void Set (Step step, string key, bool val)
		{
			Set (step, key, val ? "1" : "0");
		}

		public void Set (Step step, string key, string val)
		{
			Set (step.Name, key, val);
		}

		public void Set (string step, string key, object val)
		{
			Set (step, key, val.ToString ());
		}

		public void Set (string step, string key, string val)
		{
			key = step + ":" + key;
			if (Settings.ContainsKey (key)) {
				Settings [key].Value = val;
			} else {
				Settings.Add (key, new Setting (Photo.Id, Photo.DefaultVersionId, key, val));
			}
		}

		public Setting Get (Step step, string key)
		{
			return Get (step.Name, key);
		}

		public Setting Get (string step, string key)
		{
			key = step + ":" + key;
			Setting setting;
			if (!Settings.TryGetValue (key, out setting))
				setting = new Setting (Photo.Id, Photo.DefaultVersionId, key, null);
			return setting;
		}

		public Setting Get (string step, string key, string def)
		{
			Setting setting = Get (step, key);
			if (setting.IsBlank)
				setting.Value = def;
			return setting;
		}

		public void Save ()
		{
			foreach (Setting setting in Settings.Values) {
				Core.Database.ProcessingSettings.Commit (setting);
			}
		}
#endregion

		public void Dump ()
		{
			Log.Debug ("Dumping pipeline {0}", this);
			Log.Debug ("   Steps:");
			foreach (KeyValuePair<uint, Step> kvp in Steps) {
				Log.Debug ("      {0} - {1}", kvp.Key, kvp.Value.Name);
			}
			Log.Debug ("   Settings:");
			foreach (KeyValuePair<string, Setting> kvp in Settings) {
				Log.Debug ("      {0} - {1}", kvp.Key, kvp.Value.Value);
			}
		}
	}
}
