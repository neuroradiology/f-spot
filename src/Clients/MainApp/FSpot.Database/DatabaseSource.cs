/*
 * DatabaseSource.cs
 *
 * Author(s):
 *  Mike Gemuende <mike@gemuende.de>
 *
 * This is free software. See COPYING for details.
 */

using System;

using Mono.Unix;

using FSpot.Core;


namespace FSpot.Database
{


    public class DatabaseSource : IPhotoSource
    {

#region Private Fields

        private Db database;
        private PhotoQuery query;

#endregion

#region Constructors

        public DatabaseSource (Db database)
        {
            this.database = database;

            query = new PhotoQuery (database.Photos);
        }

#endregion

#region IPhotoSource Implementation

        public string Name {
            get { return Catalog.GetString ("Photo Library"); }
        }

        public bool CanAddPhotos {
            get { return true; }
        }

        public bool CanDeletePhotos {
            get { return true; }
        }

        public bool CanRemovePhotos {
            get { return true; }
        }

        public void AddPhotos (IBrowsableCollection photos)
        {
            throw new System.NotImplementedException ();
        }

        public void DeletePhotos (IBrowsableCollection photos)
        {
            throw new System.NotImplementedException ();
        }

        public void RemovePhotos (IBrowsableCollection photos)
        {
            throw new System.NotImplementedException ();
        }

        public IBrowsableCollection Photos {
            get { return query; }
        }

#endregion


    }
}
