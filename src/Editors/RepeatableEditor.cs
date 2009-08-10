//
// FSpot.Editors.RepeatableEditor.cs
//
// Author(s)
// 	Ruben Vermeersch <ruben@savanne.be>
//
// This is free software. See COPYING for details.
//

using Gdk;
using FSpot.Utils;
using FSpot.Editors.Processing;

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
			base.LoadPhoto (photo, out photo_pixbuf, out photo_profile);
			Pipeline = new Pipeline (photo);
			SetupPipeline ();
		}

		// When this is called, the pipeline should be filled with the
		// appropriate values, chosen in the configuration widget.
		protected abstract void SetupPipeline ();

		sealed protected override void SaveEditedPhoto (Photo photo, Pixbuf pixbuf)
		{
			Pipeline.Save ();
			base.SaveEditedPhoto (photo, pixbuf);
		}

		public override EditorState CreateState ()
		{
			return new RepeatableEditorState ();
		}
	}
}
