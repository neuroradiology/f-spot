/*
 * DesaturateEditor.cs
 *
 * Author(s)
 * 	Ruben Vermeersch <ruben@savanne.be>
 *
 * This is free software. See COPYING for details.
 */

using FSpot;
using FSpot.ColorAdjustment;
using Gdk;
using Mono.Unix;

namespace FSpot.Editors {
    class DesaturateEditor : RepeatableEditor {
        public DesaturateEditor () : base (Catalog.GetString ("Desaturate"), "color-desaturate") {
			// FIXME: need tooltip Catalog.GetString ("Convert the photo to black and white")
			CanHandleMultiple = true;
        }

		protected override void SetupPipeline ()
		{
			Pipeline.Set ("ColorAdjust", "Temperature", 5000);
			Pipeline.Set ("ColorAdjust", "Tint", 0);
			Pipeline.Set ("ColorAdjust", "Exposure", 0.0);
			Pipeline.Set ("ColorAdjust", "Brightness", 0.0);
			Pipeline.Set ("ColorAdjust", "Contrast", 0.0);
			Pipeline.Set ("ColorAdjust", "Hue", 0.0);
			Pipeline.Set ("ColorAdjust", "Saturation", -100.0);
		}
    }
}
