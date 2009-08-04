//
// Fspot.Editors.Processing.SettingStore.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using Banshee.Database;
using Mono.Data.SqliteClient;
using System;
using System.Collections.Generic;

namespace FSpot.Editors.Processing {
	public class SettingStore : DbStore<Setting>
	{
		public SettingStore (QueuedSqliteDatabase database, bool is_new) : base (database, false)
		{
			if (!is_new && Database.TableExists ("processing_settings"))
				return;

			Database.ExecuteNonQuery (
				"CREATE TABLE processing_settings (\n" +
				"	id  INTEGER PRIMARY KEY NOT NULL,\n" +
				"	photo_id	INTEGER NOT NULL,\n" +
				"	version_id	INTEGER NOT NULL,\n" +
				"	key			TEXT NOT NULL,\n" +
				"	value		TEXT NOT NULL\n" +
				")");
		}

		public override Setting Get (uint id)
		{
			Setting setting = null;
			SqliteDataReader reader = Database.Query(new DbCommand ("SELECT * FROM processing_settings WHERE id = :id", "id", id));

			if (reader.Read ()) {
				setting = new Setting (id,
									   Convert.ToUInt32 (reader ["photo_id"]),
									   Convert.ToUInt32 (reader ["version_id"]),
									   reader ["key"].ToString (),
									   reader ["value"].ToString ());
			}

			return setting;
		}

		public List<Setting> GetAll (uint photo_id, uint version_id)
		{
			List<Setting> settings = new List<Setting> ();
			SqliteDataReader reader = Database.Query(new DbCommand ("SELECT * FROM processing_settings WHERE photo_id = :photo_id AND version_id = :version_id", "photo_id", photo_id, "version_id", version_id));

			while (reader.Read ()) {
				Setting setting = new Setting (
									   Convert.ToUInt32 (reader ["id"]),
									   Convert.ToUInt32 (reader ["photo_id"]),
									   Convert.ToUInt32 (reader ["version_id"]),
									   reader ["key"].ToString (),
									   reader ["value"].ToString ());
				settings.Add (setting);
			}

			return settings;
		}

		public override void Remove (Setting setting)
		{
			Database.ExecuteNonQuery (new DbCommand ("DELETE FROM processing_settings WHERE id = :id", "id", setting.Id));
		}

		public void RemoveAll (uint photo_id, uint version_id)
		{
			Database.ExecuteNonQuery (new DbCommand ("DELETE FROM processing_settings WHERE photo_id = :photo_id AND version_id = :version_id", "photo_id", photo_id, "version_id", version_id));
		}

		public override void Commit (Setting setting)
		{
			if (setting.Id == 0) {
				Database.ExecuteNonQuery (
						new DbCommand (
							"INSERT INTO processing_settings (photo_id, version_id, key, value) " +
							"VALUES (:photo_id, :version_id, :key, :value)",
							"photo_id", setting.PhotoId,
							"version_id", setting.VersionId,
							"key", setting.Key,
							"value", setting.Value
							)
						);
			} else {
				Database.ExecuteNonQuery (
						new DbCommand (
							"UPDATE processing_settings " +
							"SET value = :value " +
							"WHERE id = :id ",
							"value", setting.Value,
							"id", setting.Id
							)
						);
			}
		}
	}
}
