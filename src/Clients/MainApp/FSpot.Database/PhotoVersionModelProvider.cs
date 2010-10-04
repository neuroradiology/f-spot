//
// PhotoVersionModelProvider.cs
//
// Author:
//   Mike Gemuende <mike@gemuende.de>
// 
// Copyright (c) 2010 Mike Gemuende <mike@gemuende.de>
//
// This is free software. See COPYING for details.
//

using System;

using Hyena.Data.Sqlite;


namespace FSpot.Database
{


    internal class PhotoVersionModelProvider : SqliteModelProvider<DatabasePhotoVersion>
    {

#region Constructor

        public PhotoVersionModelProvider (HyenaSqliteConnection connection) : base (connection, "photo_versions")
        {
        }

#endregion

    }
}
