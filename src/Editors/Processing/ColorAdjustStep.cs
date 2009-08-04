//
// Fspot.Editors.Processing.ColorAdjustStep.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using FSpot.ColorAdjustment;
using FSpot.Utils;
using Gdk;

namespace FSpot.Editors.Processing {
	public class ColorAdjustStep : Step
	{
		public string Name {
			get { return "ColorAdjust"; }
		}

		public void Process (Pipeline pipeline, Pixbuf input, out Pixbuf output)
		{
			if (pipeline.Get (this, "Temperature").IsBlank) {
				output = input.ShallowCopy ();
				return;
			}

			Cms.ColorCIEXYZ src_wp;
			Cms.ColorCIEXYZ dest_wp;

			int temp = pipeline.Get (this, "Temperature").IntValue;
			int tint = pipeline.Get (this, "Tint").IntValue;
			Cms.Profile input_profile = pipeline.InputProfile;
			double exposure = pipeline.Get (this, "Exposure").DoubleValue;
			double brightness = pipeline.Get (this, "Brightness").DoubleValue;
			double contrast = pipeline.Get (this, "Contrast").DoubleValue;
			double hue = pipeline.Get (this, "Hue").DoubleValue;
			double saturation = pipeline.Get (this, "Saturation").DoubleValue;

			src_wp = Cms.ColorCIExyY.WhitePointFromTemperature (5000).ToXYZ ();
			dest_wp = Cms.ColorCIExyY.WhitePointFromTemperature (temp).ToXYZ ();
			Cms.ColorCIELab dest_lab = dest_wp.ToLab (src_wp);
			dest_lab.a += tint;
			dest_wp = dest_lab.ToXYZ (src_wp);

			FullColorAdjustment adjust = new FullColorAdjustment (input, input_profile,
					exposure, brightness, contrast, hue, saturation, src_wp, dest_wp);
			output = adjust.Adjust ();
		}
	}
}
