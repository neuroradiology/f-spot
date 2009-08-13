//
// FSpot.Widgets.PhotoImageView.cs
//
// Copyright (c) 2004-2009 Novell, Inc.
//
// Author(s)
//	Larry Ewing  <lewing@novell.com>
//	Stephane Delcroix  <stephane@delcroix.org>
//
// This is free software. See COPYING for details.
//

using System;
using System.Threading;

using FSpot.Editors;
using FSpot.Utils;
using FSpot.Loaders;

using Gdk;
using Gtk;

namespace FSpot.Widgets {
	public class PhotoImageView : ImageView {
#region public API
		public PhotoImageView (IBrowsableCollection query) : this (new BrowsablePointer (query, -1))
		{
		}

		public PhotoImageView (BrowsablePointer item) : base ()
		{
			Accelerometer.OrientationChanged += HandleOrientationChanged;
			Preferences.SettingChanged += OnPreferencesChanged;

			this.item = item;
			item.Changed += HandlePhotoItemChanged;
		}

		~PhotoImageView () {
			if (progress_bar_container != null && progress_bar != null)
				progress_bar_container.Remove (progress_bar);
		}

		public BrowsablePointer Item {
			get { return item; }
		}

		IBrowsableCollection query;
		public IBrowsableCollection Query {
			get { return item.Collection; }
		}

		public Loupe Loupe {
			get { return loupe; }
		}

		public void Reload ()
		{
			if (Item == null || !Item.IsValid)
				return;
			
			HandlePhotoItemChanged (this, null);
		}

		// Zoom scaled between 0.0 and 1.0
		public double NormalizedZoom {
			get { return (Zoom - MIN_ZOOM) / (MAX_ZOOM - MIN_ZOOM); }
			set { Zoom = (value * (MAX_ZOOM - MIN_ZOOM)) + MIN_ZOOM; }
		}
		
		public event EventHandler PhotoChanged;
#endregion

#region Gtk widgetry
		protected override void OnStyleSet (Gtk.Style previous)
		{
			CheckPattern = new CheckPattern (this.Style.Backgrounds [(int)Gtk.StateType.Normal]);
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			if ((evnt.State & (ModifierType.Mod1Mask | ModifierType.ControlMask)) != 0)
				return base.OnKeyPressEvent (evnt);

			bool handled = true;
		
			// Scroll if image is zoomed in (scrollbars are visible)
			Gtk.ScrolledWindow scrolled_w = this.Parent as Gtk.ScrolledWindow;
			bool scrolled = scrolled_w != null && !this.Fit;
		
			// Go to the next/previous photo when not zoomed (no scrollbars)
			switch (evnt.Key) {
			case Gdk.Key.Up:
			case Gdk.Key.KP_Up:
			case Gdk.Key.Left:
			case Gdk.Key.KP_Left:
			case Gdk.Key.h:
			case Gdk.Key.H:
			case Gdk.Key.k:
			case Gdk.Key.K:
				if (scrolled)
					handled = false;
				else
					this.Item.MovePrevious ();
				break;
			case Gdk.Key.Page_Up:
			case Gdk.Key.KP_Page_Up:
			case Gdk.Key.BackSpace:
			case Gdk.Key.b:
			case Gdk.Key.B:
				this.Item.MovePrevious ();
				break;
			case Gdk.Key.Down:
			case Gdk.Key.KP_Down:
			case Gdk.Key.Right:
			case Gdk.Key.KP_Right:
			case Gdk.Key.j:
			case Gdk.Key.J:
			case Gdk.Key.l:
			case Gdk.Key.L:
				if (scrolled)
					handled = false;
				else
					this.Item.MoveNext ();
				break;
			case Gdk.Key.Page_Down:
			case Gdk.Key.KP_Page_Down:
			case Gdk.Key.space:
			case Gdk.Key.KP_Space:
			case Gdk.Key.n:
			case Gdk.Key.N:
				this.Item.MoveNext ();
				break;
			case Gdk.Key.Home:
			case Gdk.Key.KP_Home:
				this.Item.Index = 0;
				break;
			case Gdk.Key.r:
			case Gdk.Key.R:
				this.Item.Index = new Random().Next(0, this.Query.Count - 1);
				break;
			case Gdk.Key.End:
			case Gdk.Key.KP_End:
				this.Item.Index = this.Query.Count - 1;
				break;
			default:
				handled = false;
				break;
			}

			return handled || base.OnKeyPressEvent (evnt);
		}

		protected override void OnDestroyed ()
		{
			if (Loader != null) {
				Loader.AreaUpdated -= HandlePixbufAreaUpdated;
				Loader.AreaPrepared -= HandlePixbufPrepared;
				DisposeLoader (Loader);
			}
			base.OnDestroyed ();
		}
#endregion

#region progress bar
		Container progress_bar_container;
		ProgressBar progress_bar;
		bool? progress_bar_present = null;

		void UpdateStatus (string text, double fraction)
		{
			if (!CheckProgressBar ())
				return;

			progress_bar.Visible = true;
			progress_bar.Fraction = fraction;
			progress_bar.Text = text;
		}

		bool CheckProgressBar ()
		{
			if (progress_bar_present != null)
				return (bool) progress_bar_present;

			// I do not like the trip through MainWindow here, can this be
			// done better?
			if (MainWindow.Toplevel == null) {
				progress_bar_present = false;
				return false;
			}

			progress_bar = new ProgressBar ();
			progress_bar.Visible = false;
			progress_bar_container = MainWindow.Toplevel.StatusContainer;
			progress_bar_container.Add (progress_bar);
			progress_bar_present = true;

			return true;
		}

		void HideStatus () {
			if (!CheckProgressBar ())
				return;

			progress_bar.Visible = false;
		}

		void HandleProgressHint (object sender, ProgressHintEventArgs args)
		{
			if (sender != Loader)
				return;

			UpdateStatus (args.Text, args.Fraction);
		}
#endregion

#region loader		
		public IImageLoader Loader { get; private set; }

		uint timer;
		bool prepared_is_new;
		ImageLoaderItem current_item;

		void DisposeLoader (IImageLoader loader)
		{
			// This can take some time for slow loaders, doing it in a thread
			// to improve responsiveness.
			Thread t = new Thread (delegate () {
				loader.Dispose ();
			});
			t.Start ();
		}

		void Load (Uri uri)
		{
			timer = Log.DebugTimerStart ();
			if (Loader != null)
				DisposeLoader (Loader);
			HideStatus ();

			current_item = ImageLoaderItem.None;

			prepared_is_new = true;
			Loader = ImageLoader.Create (uri);
			Loader.AreaPrepared += HandlePixbufPrepared;
			Loader.AreaUpdated += HandlePixbufAreaUpdated;
			Loader.ProgressHint += HandleProgressHint;
			Loader.Load (ImageLoaderItem.Thumbnail | ImageLoaderItem.Large | ImageLoaderItem.Full, HandleCompleted);
		}

		void HandlePixbufPrepared (object sender, AreaPreparedEventArgs args)
		{
			IImageLoader loader = sender as IImageLoader;
			if (loader != Loader)
				return;

			if (!ShowProgress)
				return;

			if (args.Item < current_item)
				return;

			current_item = args.Item.Largest ();

			Gdk.Pixbuf prev = Pixbuf;
			PixbufOrientation orientation = Accelerometer.GetViewOrientation (Loader.PixbufOrientation (current_item));
			ChangeImage (Loader.Pixbuf (current_item), orientation, prepared_is_new, current_item != ImageLoaderItem.Full);
			prepared_is_new = false;

			if (prev != null)
				prev.Dispose ();

			this.ZoomFit (args.ReducedResolution);
		}

		void HandlePixbufAreaUpdated (object sender, AreaUpdatedEventArgs args)
		{
			IImageLoader loader = sender as IImageLoader;
			if (loader != Loader)
				return;

			if (!ShowProgress)
				return;

			Gdk.Rectangle area = this.ImageCoordsToWindow (args.Area);
			this.QueueDrawArea (area.X, area.Y, area.Width, area.Height);
		}

		void HandleCompleted (object sender, ItemsCompletedEventArgs args)
		{
			Log.DebugTimerPrint (timer, "Loading image took {0} (" + args.Items.ToString () + ")");
			IImageLoader loader = sender as IImageLoader;
			if (loader != Loader)
				return;

			HideStatus ();

			Pixbuf prev = this.Pixbuf;
			if (current_item != args.Items.Largest ()) {
				current_item = args.Items.Largest ();
				ChangeImage (Loader.Pixbuf (current_item), Accelerometer.GetViewOrientation (Loader.PixbufOrientation (current_item)), false, false);
			}

			if (Pixbuf == null)
				LoadErrorImage ();

			progressive_display = true;

			if (prev != this.Pixbuf && prev != null)
				prev.Dispose ();
		}
#endregion
		
		protected BrowsablePointer item;
		protected Loupe loupe;
		protected Loupe sharpener;

		void HandleOrientationChanged (object sender, EventArgs e)
		{
			Reload ();
		}
		
		bool progressive_display = true;
		bool ShowProgress {
			get { return progressive_display; }
		}

		void LoadErrorImage ()
		{
			// FIXME we should check the exception type and do something
			// like offer the user a chance to locate the moved file and
			// update the db entry, but for now just set the error pixbuf	
			Pixbuf old = Pixbuf;
			Pixbuf err = new Pixbuf (FSpotPixbufUtils.ErrorPixbuf, 0, 0,
									 FSpotPixbufUtils.ErrorPixbuf.Width,
									 FSpotPixbufUtils.ErrorPixbuf.Height);
			ChangeImage (err, PixbufOrientation.TopLeft, true, false);
			if (old != null)
				old.Dispose ();

			PixbufOrientation = PixbufOrientation.TopLeft;
			ZoomFit (false);
		}

		void HandlePhotoItemChanged (object sender, BrowsablePointerChangedEventArgs args) 
		{
			// If it is just the position that changed fall out
			if (args != null && 
			    args.PreviousItem != null &&
			    Item.IsValid &&
			    (args.PreviousIndex != item.Index) &&
			    (this.Item.Current.DefaultVersion.Uri == args.PreviousItem.DefaultVersion.Uri))
				return;

			// Don't reload if the image didn't change at all.
			if (args != null && args.Changes != null &&
			    !args.Changes.DataChanged &&
			    args.PreviousItem != null &&
			    Item.IsValid &&
			    this.Item.Current.DefaultVersion.Uri == args.PreviousItem.DefaultVersion.Uri)
				return;

			// Same image, don't load it progressively
			if (args != null &&
			    args.PreviousItem != null && 
			    Item.IsValid && 
			    Item.Current.DefaultVersion.Uri == args.PreviousItem.DefaultVersion.Uri)
				progressive_display = false;

			try {
				if (Item.IsValid) 
					Load (Item.Current.DefaultVersion.Uri);
				else
					LoadErrorImage ();
			} catch (System.Exception e) {
				Log.DebugException (e);
				LoadErrorImage ();
			}
			
			Selection = Gdk.Rectangle.Zero;

			EventHandler eh = PhotoChanged;
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		

		private void HandleLoupeDestroy (object sender, EventArgs args)
		{
			if (sender == loupe)
				loupe = null;

			if (sender == sharpener)
				sharpener = null;

		}


		public void ShowHideLoupe ()
		{
			if (loupe == null) {
				loupe = new Loupe (this);
				loupe.Destroyed += HandleLoupeDestroy;
				loupe.Show ();
			} else {
				loupe.Destroy ();	
			}
			
		}
		
		public void ShowSharpener ()
		{
			if (sharpener == null) {
				sharpener = new Sharpener (this);
				sharpener.Destroyed += HandleLoupeDestroy;
			}

			sharpener.Show ();	
		}

		void OnPreferencesChanged (object sender, NotifyEventArgs args)
		{
			LoadPreference (args.Key);
		}

		void LoadPreference (String key)
		{
			switch (key) {
			case Preferences.COLOR_MANAGEMENT_DISPLAY_PROFILE:
				Reload ();
				break;
			}
		}

		protected override void ApplyColorTransform (Pixbuf pixbuf)
		{
			Cms.Profile screen_profile;
			if (FSpot.ColorManagement.Profiles.TryGetValue (Preferences.Get<string> (Preferences.COLOR_MANAGEMENT_DISPLAY_PROFILE), out screen_profile)) 
				FSpot.ColorManagement.ApplyProfile (pixbuf, screen_profile);
		}

		bool crop_helpers = true;
		public bool CropHelpers {
			get { return crop_helpers; }
			set { 
				if (crop_helpers == value)
					return;
				crop_helpers = value;
				QueueDraw ();
			}
		}

		protected override bool OnExposeEvent (EventExpose evnt)
		{
			if (!base.OnExposeEvent (evnt))
				return false;

			if (!CanSelect || !CropHelpers || Selection == Rectangle.Zero)
				return true;

			using (Cairo.Context ctx = CairoHelper.Create (GdkWindow)) {
				ctx.SetSourceRGBA (.7, .7, .7, .8);
				ctx.SetDash (new double [] {10, 15}, 0);
				ctx.LineWidth = .8;
				for (int i=1; i<3; i++) {
					Point s = ImageCoordsToWindow (new Point (Selection.X + Selection.Width / 3 * i, Selection.Y));
					Point e = ImageCoordsToWindow (new Point (Selection.X + Selection.Width / 3 * i, Selection.Y + Selection.Height));
					ctx.MoveTo (s.X, s.Y);
					ctx.LineTo (e.X, e.Y);
					ctx.Stroke ();
				}
				for (int i=1; i<3; i++) {
					Point s = ImageCoordsToWindow (new Point (Selection.X, Selection.Y + Selection.Height / 3 * i));
					Point e = ImageCoordsToWindow (new Point (Selection.X + Selection.Width, Selection.Y + Selection.Height / 3 * i));
					ctx.MoveTo (s.X, s.Y);
					ctx.LineTo (e.X, e.Y);
					ctx.Stroke ();
				}
			}
			return true;
		}
	
	}
}
