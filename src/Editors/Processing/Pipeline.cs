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
	public class Pipeline
	{
#region Step registration
		static SortedList<uint, Step> Steps { get; set; }

		static Pipeline ()
		{
			Steps = new SortedList<uint, Step> ();
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

#region Processing
		public Pixbuf Input { get; set; }
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

		public void Set (string key, bool val)
		{
			Set (key, val ? "1" : "0");
		}

		public void Set (string key, string val)
		{
			if (Settings.ContainsKey (key)) {
				Settings [key].Value = val;
			} else {
				Settings.Add (key, new Setting (Photo.Id, Photo.DefaultVersionId, key, val));
			}
		}

		public Setting Get (string key)
		{
			Setting setting;
			if (!Settings.TryGetValue (key, out setting))
				setting = new Setting (Photo.Id, Photo.DefaultVersionId, key, null);
			return setting;
		}

		public void Save ()
		{
			foreach (Setting setting in Settings.Values) {
				Core.Database.ProcessingSettings.Commit (setting);
			}
		}
#endregion
	}
}
