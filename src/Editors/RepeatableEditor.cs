//
// FSpot.Editors.RepeatableEditor.cs
//
// Author(s)
// 	Ruben Vermeersch <ruben@savanne.be>
//
// This is free software. See COPYING for details.
//

using FSpot.Editors.Processing;
using FSpot.Loaders;
using FSpot.Platform;
using FSpot.Utils;
using Gdk;
using System;

namespace FSpot.Editors {
	public abstract class RepeatableEditor : Editor {
		Pipeline pipeline;
		protected Pipeline Pipeline {
			get {
				return (State as RepeatableEditorState).Pipeline;
			}
			set {
				(State as RepeatableEditorState).Pipeline = value;
			}
		}

		public RepeatableEditor (string label, string icon_name)
				: base (label, icon_name)
		{
		}

		sealed protected override Pixbuf Process (Pixbuf input, Cms.Profile input_profile)
		{
			Pipeline.Input = input;
			Pipeline.InputProfile = input_profile;
			SetupPipeline ();
			Pipeline.Process ();
			return Pipeline.Output.ShallowCopy ();
		}

		sealed protected override void LoadPhoto (Photo photo, out Pixbuf photo_pixbuf, out Cms.Profile photo_profile)
		{
			// Figure out the original version to process
			PhotoVersion version = photo.DefaultVersion;
			uint parent_version = version.Type == PhotoVersionType.Processable ? version.ParentVersionId : version.VersionId;
			Uri uri = photo.VersionUri (parent_version);

			if (parent_version == photo.DefaultVersionId) {
				base.LoadPhoto (photo, out photo_pixbuf, out photo_profile);
			} else {
				using (ImageFile img = ImageFile.Create (uri)) {
					using (IImageLoader loader = ImageLoader.Create (uri)) {
						loader.Load (ImageLoaderItem.Full);
						photo_pixbuf = loader.Full;
					}
					photo_profile = img.GetProfile ();
				}
			}

			Pipeline = new Pipeline (photo);
			SetupPipeline ();
		}

		// When this is called, the pipeline should be filled with the
		// appropriate values, chosen in the configuration widget.
		protected abstract void SetupPipeline ();

		sealed protected override void SaveEditedPhoto (Photo photo, Pixbuf pixbuf)
		{
			// Create a new version if the original is protected...
			bool create_version = photo.DefaultVersion.IsProtected;

			// Or if it's not a Processable version.
			create_version |= photo.DefaultVersion.Type != PhotoVersionType.Processable;

			uint parent_version_id = photo.DefaultVersionId;
			uint saved_version = photo.SaveVersion (pixbuf, create_version);
			if (create_version) {
				photo.DefaultVersion.Type = PhotoVersionType.Processable;
				photo.DefaultVersion.ParentVersionId = parent_version_id;
				photo.Changes.ChangeVersion (photo.DefaultVersionId);
			}
			Pipeline.Save (saved_version);

			photo.Changes.DataChanged = true;
			Core.Database.Photos.Commit (photo);
			ThumbnailFactory.DeleteThumbnail (photo.DefaultVersionUri);
		}

		public override EditorState CreateState ()
		{
			return new RepeatableEditorState ();
		}

		bool have_full_preview = false;
		sealed protected override Pixbuf GetPreviewInput ()
		{
			have_full_preview = false;

			// Figure out the original version to process
			Photo photo = State.Items [0] as Photo;
			PhotoVersion version = photo.DefaultVersion;
			uint parent_version = version.Type == PhotoVersionType.Processable ? version.ParentVersionId : version.VersionId;
			Uri uri = photo.VersionUri (parent_version);

			// Load the large size blocking
			IImageLoader loader = ImageLoader.Create (uri);
			loader.Load (ImageLoaderItem.Large);
			Pixbuf large = loader.Large;

			// Load the full size in the background
			loader.Load (ImageLoaderItem.Full, HandleFullPreviewLoaded);

			if (large == null)
				return ScalePreviewInput (Original);
			else
				return ScalePreviewInput (large);
		}

		private void HandleFullPreviewLoaded (object sender, ItemsCompletedEventArgs args)
		{
			IImageLoader loader = sender as IImageLoader;

			if (have_full_preview)
				return;

			if (!args.Items.Contains (ImageLoaderItem.Full))
				return;

			Pixbuf full = loader.Full;
			if (full == null) {
				loader.Dispose ();
				return;
			}

			if (!StateInitialized) {
				full.Dispose ();
				loader.Dispose ();
				return;
			}

			Pixbuf old_preview = Preview;
			Preview = ScalePreviewInput (full);
			if (old_preview != null)
				old_preview.Dispose ();

			have_full_preview = true;
			full.Dispose ();
			loader.Dispose ();

			UpdatePreview ();
		}
	}
}
