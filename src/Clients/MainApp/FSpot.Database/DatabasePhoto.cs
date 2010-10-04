//
// DatabasePhoto.cs
//
// Author:
//   Mike Gemuende <mike@gemuende.de>
// 
// Copyright (c) 2010 Mike Gemuende <mike@gemuende.de>
//
// This is free software. See COPYING for details.
//

using System;
using System.Collections.Generic;

using Hyena.Data;
using Hyena.Data.Sqlite;

using FSpot;
using FSpot.Core;


namespace FSpot.Database
{

    /// <summary>
    ///    This class implements a Photo which is stored in the database. It is for using
    ///    it with a SqliteModelProvider.
    /// </summary>
    public class DatabasePhoto : IPhoto, ICacheableItem
    {

#region Constructor

        public DatabasePhoto ()
        {
        }

#endregion

#region Database Columns

        [DatabaseColumn("id", Constraints = DatabaseColumnConstraints.PrimaryKey)]
        public int Id { get; private set; }

        [DatabaseColumn("time")]
        public System.DateTime Time { get; private set; }

        [DatabaseColumn("base_uri")]
        private string BaseUri { get; set; }

        [DatabaseColumn("filename")]
        private string Filename { get; set; }

        [DatabaseColumn("description")]
        public string Description { get; private set; }

        [DatabaseColumn("roll_id")]
        public int RollId { get; private set; }

        [DatabaseColumn("default_version_id")]
        public int DefaultVersionId { get; private set; }

        [DatabaseColumn("rating")]
        public int DbRating { get; private set; }

#endregion

#region Remaining IPhoto Implementation

        public Tag[] Tags {
            get { return tags.ToArray (); }
        }

        public IPhotoVersion DefaultVersion {
            get {
                foreach (var version in Versions) {
                    if ((version as DatabasePhotoVersion).VersionId == DefaultVersionId)
                        return version;
                }
                return versions[0];
                throw new Exception (String.Format ("No default Version, something is horrible wrong (photo id {0}, default version {1} but {2})", Id, (versions[0] as DatabasePhotoVersion).VersionId, DefaultVersionId));
                //return null;
            }
        }

        public IEnumerable<IPhotoVersion> Versions {
            get {
                foreach (IPhotoVersion version in versions)
                    yield return version;
            }
        }

        public string Name {
            get { return Filename; }
        }

        public uint Rating {
            //get { return (uint) (DbRating ?? 0); }
            get { return (uint)(DbRating); }
        }

#endregion

#region Remaining IBrowsableItem Implementation

        private List<DatabasePhotoVersion> versions;
        internal List<DatabasePhotoVersion> VersionList {
           get { return versions ?? (versions = new List<DatabasePhotoVersion> ()); }
        }

        private List<Tag> tags;
        internal List<Tag> TagList {
            get { return tags ?? (tags = new List<Tag> ()); }
        }

#endregion

#region ICachableItem Implementation

        public object CacheEntryId { get; set; }

        public long CacheModelId { get; set; }

#endregion

    }
}
