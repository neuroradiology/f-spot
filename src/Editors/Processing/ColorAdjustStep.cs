//
// Fspot.Editors.Processing.ColorAdjustStep.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using FSpot.Utils;
using Gdk;

namespace FSpot.Editors.Processing {
	public class ColorAdjustStep : Step
	{
		static ColorAdjustStep ()
		{
			Pipeline.AddStep (150, new ColorAdjustStep ());
		}

		public void Process (Pipeline pipeline, Pixbuf input, out Pixbuf output)
		{
			output = input.ShallowCopy ();
		}
	}
}
