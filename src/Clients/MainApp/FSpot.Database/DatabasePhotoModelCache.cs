//
// DatabasePhotoModelCache.cs
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

    public class DatabasePhotoModelCache : SqliteModelCache<DatabasePhoto>
    {

#region Constructors

        public DatabasePhotoModelCache (HyenaSqliteConnection connection, string uuid, ICacheableDatabaseModel model, PhotoModelProvider provider) : base(connection, uuid, model, provider)
        {
        }

#endregion

    }
}
