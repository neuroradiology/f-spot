/*
 * ColorEditor.cs
 *
 * Author(s)
 *	Larry Ewing <lewing@novell.com>
 * 	Ruben Vermeersch <ruben@savanne.be>
 *
 * This is free software. See COPYING for details.
 */

using FSpot;
using FSpot.ColorAdjustment;
using Gdk;
using Gtk;
using Mono.Unix;
using System;

namespace FSpot.Editors {
	class ColorEditor : RepeatableEditor {
		private Glade.XML xml;

		[Glade.Widget] private Gtk.HScale exposure_scale;
		[Glade.Widget] private Gtk.HScale temp_scale;
		[Glade.Widget] private Gtk.HScale temptint_scale;
		[Glade.Widget] private Gtk.HScale brightness_scale;
		[Glade.Widget] private Gtk.HScale contrast_scale;
		[Glade.Widget] private Gtk.HScale hue_scale;
		[Glade.Widget] private Gtk.HScale sat_scale;

		[Glade.Widget] private Gtk.SpinButton exposure_spinbutton;
		[Glade.Widget] private Gtk.SpinButton temp_spinbutton;
		[Glade.Widget] private Gtk.SpinButton temptint_spinbutton;
		[Glade.Widget] private Gtk.SpinButton brightness_spinbutton;
		[Glade.Widget] private Gtk.SpinButton contrast_spinbutton;
		[Glade.Widget] private Gtk.SpinButton hue_spinbutton;
		[Glade.Widget] private Gtk.SpinButton sat_spinbutton;

		public ColorEditor () : base (Catalog.GetString ("Adjust Colors"), "adjust-colors") {
			// FIXME: need tooltip Catalog.GetString ("Adjust the photo colors")
			HasSettings = true;
			ApplyLabel = Catalog.GetString ("Adjust");
		}

		protected override Widget CreateConfigurationWidget () {
			xml = new Glade.XML (null, "f-spot.glade", "color_editor_prefs", "f-spot");
			xml.Autoconnect (this);
			AttachInterface ();
			return xml.GetWidget ("color_editor_prefs");
		}

		private void AttachInterface () {
			temp_spinbutton.Adjustment = temp_scale.Adjustment;
			temptint_spinbutton.Adjustment = temptint_scale.Adjustment;
			exposure_spinbutton.Adjustment = exposure_scale.Adjustment;
			brightness_spinbutton.Adjustment = brightness_scale.Adjustment;
			contrast_spinbutton.Adjustment = contrast_scale.Adjustment;
			hue_spinbutton.Adjustment = hue_scale.Adjustment;
			sat_spinbutton.Adjustment = sat_scale.Adjustment;

			temp_scale.Adjustment.Value = Pipeline.Get ("ColorAdjust", "Temperature", "5000").IntValue;
			temptint_scale.Adjustment.Value = Pipeline.Get ("ColorAdjust", "Tint", "0").IntValue;
			exposure_scale.Adjustment.Value = Pipeline.Get ("ColorAdjust", "Exposure", "0.0").DoubleValue;
			brightness_scale.Adjustment.Value = Pipeline.Get ("ColorAdjust", "Brightness", "0.0").DoubleValue;
			contrast_scale.Adjustment.Value = Pipeline.Get ("ColorAdjust", "Contrast", "0.0").DoubleValue;
			hue_scale.Adjustment.Value = Pipeline.Get ("ColorAdjust", "Hue", "0.0").DoubleValue;
			sat_scale.Adjustment.Value = Pipeline.Get ("ColorAdjust", "Saturation", "0.0").DoubleValue;

			temp_spinbutton.Adjustment.ChangeValue ();
			temptint_spinbutton.Adjustment.ChangeValue ();
			exposure_spinbutton.Adjustment.ChangeValue ();
			brightness_spinbutton.Adjustment.ChangeValue ();
			contrast_spinbutton.Adjustment.ChangeValue ();
			hue_spinbutton.Adjustment.ChangeValue ();
			sat_spinbutton.Adjustment.ChangeValue ();

			exposure_scale.ValueChanged += RangeChanged;
			temp_scale.ValueChanged += RangeChanged;
			temptint_scale.ValueChanged += RangeChanged;
			brightness_scale.ValueChanged += RangeChanged;
			contrast_scale.ValueChanged += RangeChanged;
			hue_scale.ValueChanged += RangeChanged;
			sat_scale.ValueChanged += RangeChanged;
		}

		public void RangeChanged (object sender, EventArgs args) {
			UpdatePreview ();
		}

		protected override void SetupPipeline ()
		{
			Pipeline.Set ("ColorAdjust", "Temperature", (int) temp_scale.Value);
			Pipeline.Set ("ColorAdjust", "Tint", (int) temptint_scale.Value);
			Pipeline.Set ("ColorAdjust", "Exposure", (double) exposure_scale.Value);
			Pipeline.Set ("ColorAdjust", "Brightness", (double) brightness_scale.Value);
			Pipeline.Set ("ColorAdjust", "Contrast", (double) contrast_scale.Value);
			Pipeline.Set ("ColorAdjust", "Hue", (double) hue_scale.Value);
			Pipeline.Set ("ColorAdjust", "Saturation", (double) sat_scale.Value);
		}
	}
}
