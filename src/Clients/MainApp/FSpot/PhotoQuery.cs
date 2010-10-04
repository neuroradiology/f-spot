/*
 * FSpot.PhotoQuery.cs
 * 
 * Author(s):
 *  Larry Ewing  <lewing@novell.com>
 *  Stephane Delcroix  <stephane@delcroix.org>
 *
 * This is free software. See COPYING for details.
 */

using System;
using System.Collections;
using System.Collections.Generic;

using FSpot.Core;
using FSpot.Query;
using FSpot.Database;

using Hyena;
using Hyena.Data.Sqlite;

namespace FSpot {
    public class PhotoQuery : DatabasePhotoListModel {

        private Term terms;

        int count = -1;

        // Constructor
        public PhotoQuery (HyenaSqliteConnection connection, string uuid, PhotoModelProvider provider, params IQueryCondition [] conditions)
            : base (connection, uuid, provider)
        {
            SetCondition (OrderByTime.OrderByTimeDesc);

            foreach (IQueryCondition condition in conditions)
                SetCondition (condition);

            RequestReload ();
        }

        public Term Terms {
            get { return terms; }
            set {
                terms = value;
                untagged = false;
                RequestReload ();
            }
        }
    
        public DateRange Range {
            get { return GetCondition<DateRange> (); }
            set {
                if (value == null && UnSetCondition<DateRange> () || value != null && SetCondition (value))
                    RequestReload ();
            }
        }
        
        private bool untagged = false;
        public bool Untagged {
            get { return untagged; }
            set {
                if (untagged != value) {
                    untagged = value;
                    
                    if (untagged) {
                        UnSetCondition<ConditionWrapper> ();
                        UnSetCondition<HiddenTag> ();
                    }
                    
                    RequestReload ();
                }
            }
        }

        public RollSet RollSet {
            get { return GetCondition<RollSet> (); }
            set {
                if (value == null && UnSetCondition<RollSet> () || value != null && SetCondition (value))
                    RequestReload ();
            }
        }

        public RatingRange RatingRange {
            get { return GetCondition<RatingRange> (); }
            set {
                if (value == null && UnSetCondition<RatingRange>() || value != null && SetCondition (value))
                    RequestReload ();
            }
        }
        
        public HiddenTag HiddenTag {
            get { return GetCondition<HiddenTag> (); }
            set {
                if (value == null && UnSetCondition<HiddenTag>() || value != null && SetCondition (value))
                    RequestReload ();
            }
        }
        
        public ConditionWrapper TagTerm {
            get { return GetCondition<ConditionWrapper> (); }
            set {
                if (value == null && UnSetCondition<ConditionWrapper>()
                    || value != null && SetCondition (value)) {
                    
                    if (value != null) {
                        untagged = false;
                        SetCondition (HiddenTag.ShowHiddenTag);
                    } else {
                        UnSetCondition<HiddenTag> ();
                    }
                    
                    RequestReload ();
                }
            }
        }

        public OrderByTime OrderByTime {
            get { return GetCondition<OrderByTime> (); }
            set {
                if (value != null && SetCondition (value))
                    RequestReload ();
            }
        }

        public bool TimeOrderAsc {
            get { return OrderByTime.Asc; }
            set {
                if (value != OrderByTime.Asc)
                    OrderByTime = new OrderByTime (value);
            }
        }

        /*
        private int [] IndicesOf (DbItem [] dbitems)
        {
            uint timer = Log.DebugTimerStart ();
            List<int> indices = new List<int> ();
            List<uint> items_to_search = new List<uint> ();
            int cur;
            foreach (DbItem dbitem in dbitems) {
                if (reverse_lookup.TryGetValue (dbitem.Id, out cur))
                    indices.Add (cur);
                else
                    items_to_search.Add (dbitem.Id);
            }

            if (items_to_search.Count > 0)
                indices.AddRange (store.IndicesOf (temp_table, items_to_search.ToArray ()));
            Log.DebugTimerPrint (timer, "IndicesOf took {0}");
            return indices.ToArray ();
        }
         */
        public int LookupItem (System.DateTime date)
        {
            return LookupItem (date, TimeOrderAsc);
        }

        private int LookupItem (System.DateTime date, bool asc)
        {
/*          if (Count == 0)
                return -1;
            
            uint timer = Log.DebugTimerStart ();
            int low = 0;
            int high = Count - 1;
            int mid = (low + high) / 2;
            Photo current;
            while (low <= high) {
                mid = (low + high) / 2;
                if (!cache.TryGetPhoto (mid, out current))
                    //the item we're looking for is not in the cache
                    //a binary search could take up to ln2 (N/cache.SIZE) request
                    //lets reduce that number to 1
                    return store.IndexOf (temp_table, date, asc);

                int comp = this [mid].Time.CompareTo (date);
                if (!asc && comp < 0 || asc && comp > 0)
                    high = mid - 1;
                else if (!asc && comp > 0 || asc && comp < 0)
                    low = mid + 1;
                else
                    return mid;
            }
            Log.DebugTimerPrint (timer, "LookupItem took {0}");
            if (asc)
                return this[mid].Time < date ? mid + 1 : mid;
            return this[mid].Time > date ? mid + 1 : mid;
         */
            return -1;
        }

        public void Commit (int index)
        {
            Commit (new int [] {index});
        }

        public void Commit (int [] indexes)
        {
/*          List<Photo> to_commit = new List<Photo>();
            foreach (int index in indexes) {
                to_commit.Add (this [index] as Photo);
                reverse_lookup [(this [index] as Photo).Id] = index;
            }
            store.Commit (to_commit.ToArray ());*/
        }

        private void MarkChanged (object sender, DbItemEventArgs<Photo> args)
        {
/*          int [] indexes = IndicesOf (args.Items);

            if (indexes.Length > 0 && ItemsChanged != null)
                ItemsChanged (this, new BrowsableEventArgs(indexes, (args as PhotoEventArgs).Changes));*/
        }

        public void MarkChanged (int index, IBrowsableItemChanges changes)
        {
        //  MarkChanged (new int [] {index}, changes);
        }

        public void MarkChanged (int [] indexes, IBrowsableItemChanges changes)
        {
        //  ItemsChanged (this, new BrowsableEventArgs (indexes, changes));
        }
        /*
        public PhotoSelection Selection {
            get; protected set;
        }*/
    }
}
