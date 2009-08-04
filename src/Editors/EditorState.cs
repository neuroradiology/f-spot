//
// FSpot.Editors.EditorState.cs
//
// Author(s)
// 	Ruben Vermeersch <ruben@savanne.be>
//
// This is free software. See COPYING for details.
//

using Gdk;
using FSpot.Widgets;

namespace FSpot.Editors {
	public class EditorState {
		// The area selected by the user.
		public Rectangle Selection { get; set; }

		// The images selected by the user.
		public IBrowsableItem [] Items { get; set; }

		// The view, into which images are shown (null if we are in the browse view).
		public PhotoImageView PhotoImageView { get; set; }

		// Has a portion of the image been selected?
		public bool HasSelection {
			get { return Selection != Rectangle.Zero; }
		}

		// Is the user in browse mode?
		public bool InBrowseMode {
			get { return PhotoImageView == null; }
		}
	}
}
