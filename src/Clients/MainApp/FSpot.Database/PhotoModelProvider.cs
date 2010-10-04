//
// PhotoModelProvider.cs
//
// Author:
//   Mike Gemuende <mike@gemuende.de>
//
// Copyright (c) 2010 Mike Gemuende <mike@gemuende.de>
//
// This is free software. See COPYING for details.
//

using System;
using System.Data;
using System.Text;
using System.Collections.Generic;

using Hyena;
using Hyena.Data.Sqlite;


namespace FSpot.Database
{


    public class PhotoModelProvider : SqliteModelProvider<DatabasePhoto>
    {

#region Private Fields

        private PhotoVersionModelProvider photo_versions;

#endregion

#region Constructor

        public PhotoModelProvider (HyenaSqliteConnection connection) : base (connection, "photos")
        {
            photo_versions = new PhotoVersionModelProvider (connection);
        }

#endregion

#region Public Methods

        /// <summary>
        ///    This method loads a bunch of photos at once. It is more efficient than loading each one, because
        ///    The photo versions can be also loaded at once.
        /// </summary>
        /// <remarks>
        ///    This method is mainly used to provide efficient loading of photos for a DatabasePhotoModelCache.
        /// </remarks>
        public IEnumerable<DatabasePhoto> LoadAll (IDataReader reader)
        {
            Dictionary<int, DatabasePhoto> photo_lookup = new Dictionary<int, DatabasePhoto> ();
            List<DatabasePhoto> photo_list = new List<DatabasePhoto> ();
            StringBuilder photo_id_query = new StringBuilder ("photo_id IN (");

            bool first = true;
            while (reader.Read ()) {
                DatabasePhoto photo = base.Load (reader);

                photo_lookup.Add (photo.Id, photo);
                photo_list.Add (photo);

                if ( ! first)
                    photo_id_query.Append (", ");

                photo_id_query.Append (photo.Id);

                first = false;
            }

            photo_id_query.Append (")");

            // Add Versions
            foreach (DatabasePhotoVersion version in photo_versions.FetchAllMatching (photo_id_query.ToString ())) {
                photo_lookup [version.PhotoId].VersionList.Add (version);
            }

            // Add Tags
            IDataReader tag_reader =
                Connection.Query (String.Format ("SELECT photo_id, tag_id FROM photo_tags WHERE {0}",
                                               photo_id_query.ToString ()));

            using (tag_reader) {
                while (tag_reader.Read ()) {
                    int photo_id = Convert.ToInt32 (tag_reader ["photo_id"]);
                    int tag_id = Convert.ToInt32 (tag_reader ["tag_id"]);

                    photo_lookup [photo_id].TagList.Add (App.Instance.Database.Tags.Get ((uint) tag_id));
                }
            }

            return photo_list;
        }

#endregion

#region Protected Methods

        protected void Populate (DatabasePhoto photo)
        {
            foreach (DatabasePhotoVersion version in photo_versions.FetchAllMatching ("photo_id = ?", photo.Id)) {
                photo.VersionList.Add (version);
            }

            if (photo.VersionList.Count == 0)
                Log.DebugFormat ("No versions for photo ({0}) found.", photo.Id);

            IDataReader tag_reader =
                Connection.Query ("SELECT photo_id, tag_id FROM photo_tags WHERE photo_id = ?", photo.Id);

            using (tag_reader) {
                while (tag_reader.Read ()) {
                    int tag_id = Convert.ToInt32 (tag_reader ["tag_id"]);

                    photo.TagList.Add (App.Instance.Database.Tags.Get ((uint) tag_id));
                }
            }
        }

#endregion

#region Override SqliteModelProvider Behavior

        public override DatabasePhoto Load (IDataReader reader)
        {
            DatabasePhoto photo = base.Load (reader);
            Populate (photo);

            return photo;
        }

        public override void Delete (IEnumerable<DatabasePhoto> photos)
        {
            List<DatabasePhotoVersion> versions_to_delete = new List<DatabasePhotoVersion> ();

            foreach (DatabasePhoto photo in photos) {
                foreach (DatabasePhotoVersion version in photo.Versions)
                    versions_to_delete.Add (version);
            }

            base.Delete (photos);
            photo_versions.Delete (versions_to_delete);
        }

        public override void Delete (long id)
        {
            base.Delete (id);

            // FIXME: this only works, because the photo id is marked as primary key
            photo_versions.Delete (id);
        }

#endregion

    }
}
