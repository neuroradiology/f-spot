//
// BrowsablePointer.cs
//
// Author:
//   Stephane Delcroix <sdelcroix@novell.com>
//   Larry Ewing <lewing@novell.com>
//
// Copyright (C) 2005-2008 Novell, Inc.
// Copyright (C) 2008 Stephane Delcroix
// Copyright (C) 2005-2006 Larry Ewing
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace FSpot.Core
{

	public class BrowsablePointer
	{
		IBrowsableCollection collection;
		IPhoto item;
		int index;
		public event EventHandler<BrowsablePointerChangedEventArgs> Changed;

		public BrowsablePointer (IBrowsableCollection collection, int index)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");

			this.collection = collection;
			this.Index = index;
			item = Current;

			collection.Changed += HandleCollectionChanged;
			collection.ItemsChanged += HandleCollectionItemsChanged;
		}

		public IBrowsableCollection Collection {
			get { return collection; }
		}

		public IPhoto Current {
			get {
				if (!this.IsValid)
					return null;
				else
					return collection [index];
			}
		}

		private bool Valid (int val)
		{
			return val >= 0 && val < collection.Count;
		}

		public bool IsValid {
			get { return Valid (this.Index); }
		}

		public void MoveFirst ()
		{
			Index = 0;
		}

		public void MoveLast ()
		{
			Index = collection.Count - 1;
		}

		public void MoveNext ()
		{
			MoveNext (false);
		}

		public void MoveNext (bool wrap)
		{
			int val = Index;

			val++;
			if (!Valid (val))
				val = wrap ? 0 : Index;

			Index = val;
		}

		public void MovePrevious ()
		{
			MovePrevious (false);
		}

		public void MovePrevious (bool wrap)
		{
			int val = Index;

			val--;
			if (!Valid (val))
				val = wrap ? collection.Count - 1 : Index;

			Index = val;
		}

		public int Index {
			get { return index; }
			set {
				if (index != value) {
					SetIndex (value);
				}
			}
		}

		private void SetIndex (int value)
		{
			SetIndex (value, null);
		}

		private void SetIndex (int value, IBrowsableItemChanges changes)
		{
			BrowsablePointerChangedEventArgs args = new BrowsablePointerChangedEventArgs (Current, index, changes);

			index = value;
			item = Current;

			if (Changed != null)
				Changed (this, args);
		}

		protected void HandleCollectionItemsChanged (IBrowsableCollection collection,
							     BrowsableEventArgs event_args)
		{
			foreach (int item in event_args.Items)
				if (item == Index)
					SetIndex (Index, event_args.Changes);
		}

		protected void HandleCollectionChanged (IBrowsableCollection collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");
			int old_location = Index;
			int next_location = collection.IndexOf (item);

			if (old_location == next_location) {
				if (! Valid (next_location))
					SetIndex (0, null);

				return;
			}

			if (Valid (next_location))
				SetIndex (next_location);
			else if (Valid (old_location))
				SetIndex (old_location);
			else
				SetIndex (0);
		}
	}
}
