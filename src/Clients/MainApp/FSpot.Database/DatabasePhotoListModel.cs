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

            bool where_added = false;
            bool hidden_contained = false;
            foreach (var pair in Conditions) {

                IQueryCondition condition = pair.Value;

                if (condition == null)
                    continue;

//              if (condition is HiddenTag)
//                  hidden_contained = true;

                if (condition is IOrderCondition)
                    continue;

                string sql_clause = condition.SqlClause ();

                if (sql_clause == null || sql_clause.Trim () == String.Empty)
                    continue;
                query_builder.Append (where_added ? " AND " : " WHERE ");
                query_builder.Append (sql_clause);
                where_added = true;
            }

            /* if a HiddenTag condition is not explicitly given, we add one */
//          if ( ! hidden_contained) {
//              string sql_clause = HiddenTag.HideHiddenTag.SqlClause ();
//
//              if (sql_clause != null && sql_clause.Trim () != String.Empty) {
//                  query_builder.Append (where_added ? " AND " : " WHERE ");
//                  query_builder.Append (sql_clause);
//              }
//          }

            bool order_added = false;
            foreach (var pair in Conditions) {

                IQueryCondition condition = pair.Value;

                if (condition == null)
                    continue;

                if (!(condition is IOrderCondition))
                    continue;

                string sql_clause = condition.SqlClause ();

                if (sql_clause == null || sql_clause.Trim () == String.Empty)
                    continue;
                query_builder.Append (order_added ? " , " : "ORDER BY ");
                query_builder.Append (sql_clause);
                order_added = true;
            }

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
            DatabasePhoto photo = item as DatabasePhoto;

            if (photo == null)
                return -1;

            return (int) cache.IndexOf (photo);
        }


#endregion

#region PhotoQuery Methods (to be removed)

        //Query Conditions
        private Dictionary<Type, IQueryCondition> conditions;
        private Dictionary<Type, IQueryCondition> Conditions {
            get {
                if (conditions == null)
                    conditions = new Dictionary<Type, IQueryCondition> ();
                return conditions;
            }
        }

        public bool SetCondition (IQueryCondition condition)
        {
            if (condition == null)
                throw new ArgumentNullException ("condition");
            if (Conditions.ContainsKey (condition.GetType ()) && Conditions [condition.GetType ()] == condition)
                return false;
            Conditions [condition.GetType ()] = condition;
            return true;
        }

        public T GetCondition<T> () where T : IQueryCondition
        {
            IQueryCondition val;
            Conditions.TryGetValue (typeof (T), out val);
            return (T)val;
        }

        public bool UnSetCondition<T> ()
        {
            if (!Conditions.ContainsKey (typeof(T)))
                return false;
            Conditions.Remove (typeof(T));
            return true;
        }

        public void RequestReload ()
        {
            uint timer = Log.DebugTimerStart ();
            Reload ();


            if (Changed != null)
                Changed (this);

            Log.DebugTimerPrint (timer, "Reloading the query took {0}");
        }

#endregion


    }
}
