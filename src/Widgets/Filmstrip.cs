/*
 * Widgets.Filmstrip.cs
 *
 * Author(s)
 * 	Stephane Delcroix  <stephane@delcroix.org>
 *
 * This is free software. See COPYING for details.
 */

//TODO:
//	* only redraw required parts on ExposeEvents (low)
//	* Handle orientation changes (low) (require gtk# changes, so I can trigger an OrientationChanged event)

using System;
using System.Collections;

using Gtk;
using Gdk;

using FSpot.Utils;
using FSpot.Platform;
using FSpot.Bling;

namespace FSpot.Widgets
{
	public class Filmstrip : EventBox, IDisposable
	{

//		public event OrientationChangedHandler OrientationChanged;
		public event EventHandler PositionChanged;

		DoubleAnimation animation;

		bool extendable = true;
		public bool Extendable {
			get { return extendable; }
			set { extendable = value; }
		}

		Orientation orientation = Orientation.Horizontal;
		public Orientation Orientation {
			get { return orientation; }
			set {
				if (orientation == value)
					return;

				throw new NotImplementedException ();
//				if (OrientationChanged != null) {
//					OrientationChangedArgs args = new OrientationChangedArgs ();
//					//args.Orientation = value;
//					//OrientationChanged (this, args);
//				}
			}
		}

		int spacing = 6;
		public int Spacing {
			get { return spacing; }
			set {
				if (value < 0)
					throw new ArgumentException ("Spacing is negative!");
				spacing = value;
			}
		}

		int thumb_offset = 17;
		public int ThumbOffset {
			get { return thumb_offset; }
			set { 
				if (value < 0)
					throw new ArgumentException ("ThumbOffset is negative!");
				thumb_offset = value;
			}
		}

		int thumb_size = 67;
		public int ThumbSize {
			get { return thumb_size; }
			set { 
				if (value < 0)
					throw new ArgumentException ("ThumbSize is negative!");
				thumb_size = value;
			}
		}

		bool squared_thumbs = false;
		public bool SquaredThumbs {
			get { return squared_thumbs; }
			set { squared_thumbs = value; }
		}

		static string [] film_100_xpm = {
		"14 100 2 1",
		" 	c None",
		".	c #000000",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		".....    .....",
		"....      ....",
		"....      ....",
		"....      ....",
		"....      ....",
		"....      ....",
		"....      ....",
		".....    .....",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		"..............",
		".....    .....",
		"....      ....",
		"....      ....",
		"....      ....",
		"....      ....",
		"....      ....",
		"....      ....",
		".....    .....",
		"..............",
		"..............",
		"..............",
		"..............",
		".............."};

		Pixbuf background_tile;
		public Pixbuf BackgroundTile {
			get {
				if (background_tile == null)
					if (orientation == Orientation.Horizontal)
						background_tile = new Pixbuf(film_100_xpm);
					else
						throw new NotImplementedException ("doesn't support Vertical orientation yet");
				return background_tile;
			}
			set { 
				if (background_tile != value && background_tile != null)
					background_tile.Dispose ();
				background_tile = value;
				BackgroundPixbuf = null;
			}
		}

		int x_offset = 2;
		public int XOffset {
			get { return x_offset; }
			set { 
				if (value < 0)
					throw new ArgumentException ("value is negative!");
				x_offset = value;
			}
		}

		int y_offset = 2;
		public int YOffset {
			get { return y_offset; }
			set { 
				if (value < 0)
					throw new ArgumentException ("value is negative!");
				y_offset = value;
			}
		}

		float x_align = 0.5f, y_align = 0.5f;
		public float XAlign {
			get { return x_align; }
			set {
				if (value < 0.0 || value > 1.0)
					throw new ArgumentException ("value is not between 0.0 and 1.0");
				x_align = value;
			}
		}

		public float YAlign {
			get { return y_align; }
			set {
				if (value < 0.0 || value > 1.0)
					throw new ArgumentException ("value is not between 0.0 and 1.0");
				y_align = value;
			}
		}

		public int ActiveItem {
			get { return selection.Index; }
			set {
				if (value == selection.Index)
					return;
				if (value < 0)
					value = 0;
				if (value > selection.Collection.Count - 1)
					value = selection.Collection.Count - 1;

				selection.Index = value;
			}
		}

		double position;
		public double Position {
			get { 
				return position; 
			}
			set {
				if (value == position)
					return;
				if (value < 0)
					value = 0;
				if (value > selection.Collection.Count - 1)
					value = selection.Collection.Count - 1;

				animation.From = position;
				animation.To = value;
				animation.Restart ();

				if (PositionChanged != null)
					PositionChanged (this, EventArgs.Empty);
			}
		}

		FSpot.BrowsablePointer selection;
		DisposableCache<Uri, Pixbuf> thumb_cache;

		public Filmstrip (FSpot.BrowsablePointer selection) : this (selection, true)
		{
		}

		public Filmstrip (FSpot.BrowsablePointer selection, bool squared_thumbs) : base ()
		{
			CanFocus = true;
			this.selection = selection;
			this.selection.Changed += HandlePointerChanged;
			this.selection.Collection.Changed += HandleCollectionChanged;
			this.selection.Collection.ItemsChanged += HandleCollectionItemsChanged;
			this.squared_thumbs = squared_thumbs;
			thumb_cache = new DisposableCache<Uri, Pixbuf> (30);
			ThumbnailGenerator.Default.OnPixbufLoaded += HandlePixbufLoaded;

			animation = new DoubleAnimation (0, 0, TimeSpan.FromSeconds (4), SetPositionCore, new CubicEase (EasingMode.EaseOut));
		}
	
		int min_length = 400;
		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			base.OnSizeRequested (ref requisition);
			requisition.Width = Math.Max (min_length + 2 * x_offset, requisition.Width);
			if (min_length % BackgroundTile.Width != 0)
				requisition.Width += BackgroundTile.Width - min_length % BackgroundTile.Width;	

			requisition.Height = Math.Max (requisition.Height, BackgroundTile.Height + (2 * y_offset));
		}

		Pixbuf background_pixbuf;
		protected Pixbuf BackgroundPixbuf {
			get {
				if (background_pixbuf == null) {
					int length;
					if (Allocation.Width < min_length || !extendable)
						length = min_length;
					else
						length = Allocation.Width;

					length = length - length % BackgroundTile.Width;

					background_pixbuf = new Pixbuf (Gdk.Colorspace.Rgb, true, 8, length, BackgroundTile.Height);
					for (int i = 0; i < length; i += BackgroundTile.Width) {
						BackgroundTile.CopyArea (0, 0, BackgroundTile.Width, BackgroundTile.Height, 
								background_pixbuf, i, 0);
					}
				}
				return background_pixbuf;
			}
			set {
				if (background_pixbuf != value && background_pixbuf != null) {
					background_pixbuf.Dispose ();
					background_pixbuf = value;
				}
			}
		}

		Hashtable start_indexes;
		int filmstrip_end_pos;
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			if (evnt.Window != GdkWindow)
				return true;

			if (selection.Collection.Count == 0)
				return true;

			if (extendable && Allocation.Width >= BackgroundPixbuf.Width + (2 * x_offset) + BackgroundTile.Width)
				BackgroundPixbuf = null;

			if (extendable && Allocation.Width < BackgroundPixbuf.Width + (2 * x_offset))
				BackgroundPixbuf = null;

			int xpad = 0, ypad = 0;
			if (Allocation.Width > BackgroundPixbuf.Width + (2 * x_offset))
				xpad = (int) (x_align * (Allocation.Width - (BackgroundPixbuf.Width + (2 * x_offset))));

			if (Allocation.Height > BackgroundPixbuf.Height + (2 * y_offset))
				ypad = (int) (y_align * (Allocation.Height - (BackgroundPixbuf.Height + (2 * y_offset))));

			GdkWindow.DrawPixbuf (Style.BackgroundGC (StateType.Normal), BackgroundPixbuf, 
					0, 0, x_offset + xpad, y_offset + ypad, 
					BackgroundPixbuf.Width, BackgroundPixbuf.Height, Gdk.RgbDither.None, 0, 0);

			//drawing the icons...
			start_indexes = new Hashtable ();

			Pixbuf icon_pixbuf = new Pixbuf (Gdk.Colorspace.Rgb, true, 8, BackgroundPixbuf.Width, thumb_size);
			icon_pixbuf.Fill (0x00000000);

			Pixbuf current = GetPixbuf ((int) Math.Round (Position));
			int ref_x = (int)(icon_pixbuf.Width / 2.0 - current.Width * (Position + 0.5f - Math.Round (Position))); //xpos of the reference icon

			int start_x = ref_x;
			for (int i = (int) Math.Round (Position); i < selection.Collection.Count; i++) {
				current = GetPixbuf (i, ActiveItem == i);
				current.CopyArea (0, 0, Math.Min (current.Width, icon_pixbuf.Width - start_x) , current.Height, icon_pixbuf, start_x, 0);
				start_indexes [start_x] = i; 
				start_x += current.Width + spacing;
				if (start_x > icon_pixbuf.Width)
					break;
			}
			filmstrip_end_pos = start_x;

			start_x = ref_x;
			for (int i = (int) Math.Round (Position) - 1; i >= 0; i--) {
				current = GetPixbuf (i, ActiveItem == i);
				start_x -= (current.Width + spacing);
				current.CopyArea (Math.Max (0, -start_x), 0, Math.Min (current.Width, current.Width + start_x), current.Height, icon_pixbuf, Math.Max (start_x, 0), 0);
				start_indexes [Math.Max (0, start_x)] = i; 
				if (start_x < 0)
					break;
			}
			
			GdkWindow.DrawPixbuf (Style.BackgroundGC (StateType.Normal), icon_pixbuf,
					0, 0, x_offset + xpad, y_offset + ypad + thumb_offset,
					icon_pixbuf.Width, icon_pixbuf.Height, Gdk.RgbDither.None, 0, 0);

			icon_pixbuf.Dispose ();

			return true;
		}

		protected override bool OnScrollEvent (EventScroll args)
		{
			float shift = 1f;
			if ((args.State & Gdk.ModifierType.ShiftMask) > 0)
				shift = 6f;

			switch (args.Direction) {
			case ScrollDirection.Up:
			case ScrollDirection.Right:
				Position = animation.To - shift;
				return true;
			case Gdk.ScrollDirection.Down:
			case Gdk.ScrollDirection.Left:
				Position = animation.To + shift;
				return true;
			}
			return false;
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey ek)
		{
			switch (ek.Key) {
			case Gdk.Key.Page_Down:
			case Gdk.Key.Down:
			case Gdk.Key.Right:
				ActiveItem ++;
				return true;
				
			case Gdk.Key.Page_Up:
			case Gdk.Key.Up:
			case Gdk.Key.Left:
				ActiveItem --;
				return true;
			}
			return false;
		}

		protected virtual void SetPositionCore (double position)
		{
			if (this.position == position)
				return;
			if (position < 0)
				position = 0;
			if (position > selection.Collection.Count - 1)
				position = selection.Collection.Count - 1;


			this.position = position;
			QueueDraw ();
		}

		void HandlePointerChanged (object sender, BrowsablePointerChangedEventArgs args)
		{
			Position = ActiveItem;
		}

		void HandleCollectionChanged (IBrowsableCollection coll)
		{
			Position = ActiveItem;
			QueueDraw ();
		}

		void HandleCollectionItemsChanged (IBrowsableCollection coll, BrowsableEventArgs args)
		{
			if (!args.Changes.DataChanged)
				return;
			foreach (int item in args.Items)
				thumb_cache.TryRemove ((selection.Collection [item]).DefaultVersionUri);

			//FIXME call QueueDrawArea
			QueueDraw ();
		}

		void HandlePixbufLoaded (ImageLoaderThread pl, Uri uri, int order, Pixbuf p) {
			if (!thumb_cache.Contains (uri)) {
				return;
			}
			
			//FIXME use QueueDrawArea
			//FIXME only invalidate if displayed
			QueueDraw ();
			

		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			if(evnt.Button != 1 || evnt.X > filmstrip_end_pos) 
				return false;
			HasFocus = true;
			int pos = -1;
			foreach (int key in start_indexes.Keys)
				if (key <= evnt.X && key > pos)
					pos = key;
			ActiveItem = (int)start_indexes [pos];
			return true;
		}
 
 		protected Pixbuf GetPixbuf (int i)
		{
			return GetPixbuf (i, false);
		}

 		protected virtual Pixbuf GetPixbuf (int i, bool highlighted)
		{
			Pixbuf current;
			Uri uri = (selection.Collection [i]).DefaultVersionUri;
			try {
				current = PixbufUtils.ShallowCopy (thumb_cache.Get (uri));
			} catch (IndexOutOfRangeException) {
				current = null;
			}

			if (current == null) {
				try {
					ThumbnailGenerator.Default.Request ((selection.Collection [i]).DefaultVersionUri, 0, 256, 256);

					if (SquaredThumbs) {
						using (Pixbuf p = ThumbnailFactory.LoadThumbnail (uri)) {
							current = PixbufUtils.IconFromPixbuf (p, ThumbSize);
						}
					} else 
						current = ThumbnailFactory.LoadThumbnail (uri, -1, ThumbSize);
					thumb_cache.Add (uri, current);
				} catch {
					try {
						current = FSpot.Global.IconTheme.LoadIcon ("gtk-missing-image", ThumbSize, (Gtk.IconLookupFlags)0);
					} catch {
						current = null;
					}
					thumb_cache.Add (uri, null);
				}

			}
			
			//FIXME
			if (FSpot.ColorManagement.IsEnabled) {
				current = current.Copy ();
				FSpot.ColorManagement.ApplyScreenProfile (current);
			}
			
			if (!highlighted)
				return current;

			Pixbuf highlight = new Pixbuf (Gdk.Colorspace.Rgb, true, 8, current.Width, current.Height);
			Gdk.Color color = Style.Background (StateType.Selected);
			uint ucol = (uint)((uint)color.Red / 256 << 24 ) + ((uint)color.Green / 256 << 16) + ((uint)color.Blue / 256 << 8) + 255;
			highlight.Fill (ucol);
			current.CopyArea (1, 1, current.Width - 2, current.Height - 2, highlight, 1, 1);	
			return highlight;
		}

		~Filmstrip ()
		{
			Log.Debug ("Finalizer called on {0}. Should be Disposed", GetType ());		
			Dispose (false);
		}
			
		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
			System.GC.SuppressFinalize (this);
		}

		bool is_disposed = false;
		protected virtual void Dispose (bool disposing)
		{
			if (is_disposed)
				return;
			if (disposing) {
				this.selection.Changed -= HandlePointerChanged;
				this.selection.Collection.Changed -= HandleCollectionChanged;
				this.selection.Collection.ItemsChanged -= HandleCollectionItemsChanged;
				ThumbnailGenerator.Default.OnPixbufLoaded -= HandlePixbufLoaded;
				if (background_pixbuf != null)
					background_pixbuf.Dispose ();
				if (background_tile != null)
					background_tile.Dispose ();
				thumb_cache.Dispose ();
			}
			//Free unmanaged resources

			is_disposed = true;
		}
	}
}
