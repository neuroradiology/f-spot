//
// DatabasePhotoVersion.cs
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

using Hyena;
using Hyena.Data.Sqlite;

using FSpot;
using FSpot.Core;


namespace FSpot.Database
{

    public class DatabasePhotoVersion : IPhotoVersion
    {

#region Constructor

        public DatabasePhotoVersion ()
        {
        }

#endregion

#region Database Columns

        [DatabaseColumn("photo_id", Constraints = DatabaseColumnConstraints.PrimaryKey)]
        public int PhotoId { get; private set; }

        [DatabaseColumn("version_id")]
        public int VersionId { get; private set; }

        [DatabaseColumn("time")]
        public System.DateTime Time { get; private set; }

        [DatabaseColumn("base_uri")]
        private string BaseUriDb {
            get {
                if (BaseUri == null)
                    return null;
                return BaseUri.AbsoluteUri;
            }
            set {
                if (value == null) {
                    BaseUri = null;
                    return;
                }

                BaseUri = new SafeUri (value);
            }
        }

        [DatabaseColumn("filename")]
        public string Filename { get; private set; }

        [DatabaseColumn("import_md5")]
        public string ImportMD5 { get; private set; }

        [DatabaseColumn("protected")]
        public bool IsProtected { get; private set; }

#endregion

#region Remaining IPhotoVersion Implementation

        public string Name {
            get { return Filename; }
        }

        public SafeUri BaseUri { get; private set; }

        public SafeUri Uri {
            get { return BaseUri.Append (Filename); }
            set {
                if (value == null)
                    throw new ArgumentNullException ();

                BaseUri = value.GetBaseUri ();
                Filename = value.GetFilename ();
            }
        }

#endregion

    }
}
