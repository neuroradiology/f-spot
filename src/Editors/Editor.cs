//
// FSpot.Editors.Editor.cs
//
// Author(s)
// 	Ruben Vermeersch <ruben@savanne.be>
//
// This is free software. See COPYING for details.
//

using FSpot;
using FSpot.Utils;
using FSpot.Widgets;
using FSpot.Loaders;

using Gdk;
using Gtk;

using Mono.Unix;

using System;
using System.Threading;

namespace FSpot.Editors {
	// This is the base class from which all editors inherit.
	public abstract class Editor {
		public delegate void ProcessingStartedHandler (string name, int count);
		public delegate void ProcessingStepHandler (int done);
		public delegate void ProcessingFinishedHandler ();

		public event ProcessingStartedHandler ProcessingStarted;
		public event ProcessingStepHandler ProcessingStep;
		public event ProcessingFinishedHandler ProcessingFinished;

		// Whether the user needs to select a part of the image before it can be applied.
		public bool NeedsSelection { get; protected set; }
		public bool CanHandleMultiple { get; protected set; }
		public bool HasSettings { get; protected set; }

		// Contains the current selection, the items being edited, ...
		private EditorState state;
		public EditorState State {
			get {
				if (!StateInitialized)
					throw new ApplicationException ("Editor has not been initialized yet!");

				return state;
			}
			private set { state = value; }
		}

		public bool StateInitialized {
			get { return state != null; }
		}

		// A tool can be applied if it doesn't need a selection, or if it has one.
		public bool CanBeApplied {
			get {
				Log.Debug ("{0} can be applied? {1}", this, !NeedsSelection || (NeedsSelection && State.HasSelection));
				return !NeedsSelection || (NeedsSelection && State.HasSelection);
			}
		}

		protected virtual void LoadPhoto (Photo photo, out Pixbuf photo_pixbuf, out Cms.Profile photo_profile) {
			// FIXME: We might get this value from the PhotoImageView.
			using (ImageFile img = ImageFile.Create (photo.DefaultVersion.Uri)) {
				if (State.PhotoImageView != null) {
					IImageLoader loader = State.PhotoImageView.Loader;
					loader.Load (ImageLoaderItem.Full);
					photo_pixbuf = loader.Full;
				} else {
					using (IImageLoader loader = ImageLoader.Create (photo.DefaultVersionUri)) {
						loader.Load (ImageLoaderItem.Full);
						photo_pixbuf = loader.Full;
					}
				}
				photo_profile = img.GetProfile ();
			}
		}

		// The human readable name for this action.
		public readonly string Label;

		// The label on the apply button (usually shorter than the label).
		private string apply_label = "";
		public string ApplyLabel {
			get { return apply_label == "" ? Label : apply_label; }
			protected set { apply_label = value; }
		}


		// The icon name for this action (will be loaded from the theme).
		public readonly string IconName;

		public Editor (string label, string icon_name) {
			Label = label;
			IconName = icon_name;

			// Default values for capabilities, these need to be set in
			// the subclass constructor.
			CanHandleMultiple = false;
			NeedsSelection = false;
			HasSettings = false;
		}

		// Apply the editor's action to a photo.
		public void Apply () {
			try {
				if (ProcessingStarted != null) {
					ProcessingStarted (Label, State.Items.Length);
				}
				TryApply ();
			} finally {
				if (ProcessingFinished != null) {
					ProcessingFinished ();
				}
			}
		}

		private void TryApply () {
			if (NeedsSelection && !State.HasSelection) {
				throw new Exception ("Cannot apply without selection!");
			}

			int done = 0;
			foreach (Photo photo in State.Items) {
				Pixbuf input;
				Cms.Profile input_profile;
				LoadPhoto (photo, out input, out input_profile);

				Pixbuf edited = Process (input, input_profile);
				input.Dispose ();

				bool create_version = photo.DefaultVersion.IsProtected;
				photo.SaveVersion (edited, create_version);
				photo.Changes.DataChanged = true;
				App.Instance.Database.Photos.Commit (photo);

				done++;
				if (ProcessingStep != null) {
					ProcessingStep (done);
				}
			}

			Reset ();
		}

		protected abstract Pixbuf Process (Pixbuf input, Cms.Profile input_profile);

		protected virtual Pixbuf ProcessFast (Pixbuf input, Cms.Profile input_profile) {
			return Process (input, input_profile);
		}

		Pixbuf Original { get; set; }
		Pixbuf Preview { get; set; }

		volatile bool preview_needed = false;
		volatile Thread preview_thread = null;

		protected void UpdatePreview () {
			if (State.InBrowseMode) {
				throw new Exception ("Previews cannot be made in browse mode!");
			}

			if (State.Items.Length > 1) {
				throw new Exception ("We should have one item selected when this happened, otherwise something is terribly wrong.");
			}

			lock (this) {
				preview_needed = true;
				if (preview_thread == null) {
					preview_thread = new Thread (new ThreadStart (DoUpdatePreview));
					preview_thread.Start ();
				}
			}
		}

		void DoUpdatePreview ()
		{
			bool done = false;
			while (!done) {
				preview_needed = false;

				RenderPreview ();

				lock (this) {
					if (!preview_needed) {
						done = true;
						preview_thread = null;
					}
				}
			}
		}

		void RenderPreview ()
		{
			if (Original == null) {
				Original = State.PhotoImageView.Pixbuf;
			}

			Pixbuf old_preview = null;
			if (Preview == null) {
				int width, height;
				CalcPreviewSize (Original, out width, out height);
				Preview = Original.ScaleSimple (width, height, InterpType.Nearest);
			} else {
				// We're updating a previous preview
				old_preview = State.PhotoImageView.Pixbuf;
			}

			Pixbuf previewed = ProcessFast (Preview, null);
			State.PhotoImageView.ChangeImage (previewed, State.PhotoImageView.PixbufOrientation, false, false);
			App.Instance.Organizer.InfoBox.UpdateHistogram (previewed);

			if (old_preview != null) {
				old_preview.Dispose ();
			}
		}

		private void CalcPreviewSize (Pixbuf input, out int width, out int height) {
			int awidth = State.PhotoImageView.Allocation.Width;
			int aheight = State.PhotoImageView.Allocation.Height;
			int iwidth = input.Width;
			int iheight = input.Height;

			if (iwidth <= awidth && iheight <= aheight) {
				// Do not upscale
				width = iwidth;
				height = iheight;
			} else {
				double wratio = (double) iwidth / awidth;
				double hratio = (double) iheight / aheight;

				double ratio = Math.Max (wratio, hratio);
				width = (int) (iwidth / ratio);
				height = (int) (iheight / ratio);
			}
			//Log.Debug ("Preview size: Allocation: {0}x{1}, Input: {2}x{3}, Result: {4}x{5}", awidth, aheight, iwidth, iheight, width, height);
		}

		public void Restore () {
			if (Original != null && State.PhotoImageView != null) {
				State.PhotoImageView.ChangeImage (Original, state.PhotoImageView.PixbufOrientation, false, false);

				App.Instance.Organizer.InfoBox.UpdateHistogram (null);
			}

			Reset ();
		}

		private void Reset () {
			preview_needed = false;
			while (preview_thread != null)
				preview_thread.Join ();

			if (Preview != null) {
				Preview.Dispose ();
			}

			Preview = null;
			Original = null;
			State = null;
		}

		// Can be overriden to provide a specific configuration widget.
		// Returning null means no configuration widget.
		public virtual Widget ConfigurationWidget () {
			return null;
		}


		public virtual EditorState CreateState () {
			return new EditorState ();
		}

		public delegate void InitializedHandler ();
		public event InitializedHandler Initialized;

		public virtual void Initialize (EditorState state) {
			State = state;

			if (Initialized != null)
				Initialized ();
		}
	}
}
