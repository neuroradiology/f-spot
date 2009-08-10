//
// FSpot.Editors.RepeatableEditorState.cs
//
// Author(s)
// 	Ruben Vermeersch <ruben@savanne.be>
//
// This is free software. See COPYING for details.
//

using Gdk;
using FSpot.Widgets;
using FSpot.Editors.Processing;

namespace FSpot.Editors {
	public class RepeatableEditorState : EditorState {
		// The processing pipeline
		Pipeline pipeline;
		public Pipeline Pipeline {
			get {
				if (pipeline == null)
					pipeline = new Pipeline (Items [0] as Photo);
				return pipeline;
			}
			set {
				pipeline = value;
			}
		}
	}
}
