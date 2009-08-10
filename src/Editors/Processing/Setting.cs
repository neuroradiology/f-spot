//
// Fspot.Editors.Processing.Setting.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using System;

namespace FSpot.Editors.Processing {
	public class Setting : DbItem
	{
		uint version_id;

		public uint PhotoId { get; private set; }
		public uint VersionId {
			get { return version_id; }
			set {
				if (value == version_id)
					return;

				if (Id != 0 && version_id != 0)
					throw new Exception ("Can't change version on committed setting!");

				version_id = value;
			}
		}
		public string Key { get; private set; }
		public string Value { get; set; }

		public Setting (uint photo, uint version, string key, string val)
				: this (0, photo, version, key, val) { }

		public Setting (uint id, uint photo, uint version, string key, string val)
				: base (id) {
			PhotoId = photo;
			VersionId = version;
			Key = key;
			Value = val;
		}

		public bool BoolValue {
			get {
				return !IsBlank && (
							String.Compare (Value, "1") == 0
						 || String.Compare (Value, "true") == 0
					);
			}
		}

		public int IntValue {
			get { return IsBlank ? 0 : Convert.ToInt32 (Value); }
		}

		public double DoubleValue {
			get { return IsBlank ? 0.0 : Convert.ToDouble (Value); }
		}

		public bool IsBlank {
			get { return Value == null; }
		}
	}
}
