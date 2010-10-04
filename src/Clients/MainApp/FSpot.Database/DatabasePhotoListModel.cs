//
// DatabasePhotoListModel.cs
//
// Author:
//   Mike Gemuende <mike@gemuende.de>
//
// Copyright (c) 2010 Mike Gemuende <mike@gemuende.de>
//
// This is free software. See COPYING for details.
//

using System;

using Hyena;
using Hyena.Data;
using Hyena.Data.Sqlite;
using Hyena.Collections;

using System.Text;
using System.Collections.Generic;

using FSpot;
using FSpot.Core;
using FSpot.Query;

namespace FSpot.Database
{


    public class DatabasePhotoListModel
        : BaseListModel<IPhoto>, ICacheableDatabaseModel, IPhotoListModel, IBrowsableCollection
    {
#region Private Fields

        private DatabasePhotoModelCache cache;
        private int filtered_count;

#endregion

#region Constructors

        public DatabasePhotoListModel (HyenaSqliteConnection connection, string uuid, PhotoModelProvider provider)
        {
            Selection = new Selection ();// new PhotoSelection (this);
            cache = new DatabasePhotoModelCache (connection, uuid, this, provider);
        }

#endregion


#region IPhotoListModel implementation

        public override IPhoto this[int index] {
            get {
                lock (this) {
                    return cache.GetValue (index);
                }
            }
        }

        public override int Count {
            get { return filtered_count; }
        }

        public override void Reload ()
        {
            UpdateReloadFragment ();

            cache.SaveSelection ();
            cache.Reload ();
            cache.RestoreSelection ();
            cache.UpdateAggregates ();

            filtered_count = (int)cache.Count;

            OnReloaded ();
        }

        public override void Clear ()
        {
            cache.Clear ();
            filtered_count = 0;

            OnCleared ();
        }

#endregion

#region Private Methods

        private void UpdateReloadFragment ()
        {
            StringBuilder query_builder = new StringBuilder ("FROM photos ");

            ReloadFragment = query_builder.ToString ();
        }

#endregion

#region ICacheableDatabaseModel Implementation

        public int FetchCount {
            get { return 200; }
        }

        public string ReloadFragment { get; private set; }

        public string SelectAggregates {
            get { return null; }
        }

        public string JoinTable {
            get { return null; }
        }

        public string JoinFragment {
            get { return null; }
        }

        public string JoinPrimaryKey {
            get { return null; }
        }

        public string JoinColumn {
            get { return null; }
        }

        public bool CachesJoinTableEntries {
            get { return false; }
        }

        public bool CachesValues {
            get { return false; }
        }

#endregion

#region IBrowsableCollection Compatibility

        public IPhoto[] Items {
            get {
                throw new NotSupportedException ();
            }
        }

        public void MarkChanged (int index, IBrowsableItemChanges changes)
        {
            throw new System.NotImplementedException ();
        }

        public bool Contains (IPhoto item)
        {
            DatabasePhoto photo = item as DatabasePhoto;

            if (photo == null)
                return false;

            if (photo.CacheModelId != cache.CacheId)
                return false;

            return cache.ContainsKey (photo.Id);
        }

        //public PhotoSelection Selection { get; protected set; }

        public event IBrowsableCollectionChangedHandler Changed;
        public event IBrowsableCollectionItemsChangedHandler ItemsChanged;

        public int IndexOf (IPhoto item)
        {
            return (int)cache.IndexOf (item);
        }

#endregion

    }
}
