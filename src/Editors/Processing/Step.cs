//
// Fspot.Editors.Processing.Step.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using Gdk;

namespace FSpot.Editors.Processing {
	public interface Step
	{
		void Process (Pipeline pipeline, Pixbuf input, out Pixbuf output);
	}
}
