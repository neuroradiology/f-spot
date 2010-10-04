/*
 * InfoEditorPresetWidget.cs
 *
 * Author(s):
 *  Mike Gemuende <mike@gemuende.de>
 *
 * This is frees software. See COPYING for details.
 */

using System;
using System.Collections.Generic;

using Mono.Unix;

using Gtk;


namespace FSpot.Gui.MetadataEditor
{
    public class InfoEditorPresetWidget : HBox
    {
        private ComboBox preset_combo;

        List<IFieldEditor> editors = new List<IFieldEditor> ();

        public InfoEditorPresetWidget (string label, EditorController controller)
            : base (false, 8)
        {
            PackStart (new Label (label), false, false, 0);

            preset_combo = new ComboBox ();
            PackStart (preset_combo, true, true, 0);

            var save_button = new Button ();

            Image image = new Image (Stock.Save, IconSize.Menu);
            save_button.Add (image);
            save_button.Relief = ReliefStyle.None;
            save_button.TooltipText = Catalog.GetString ("Save Current Values as Preset");
            save_button.Clicked += (o, a) => { SavePresetDialog (); };

            PackStart (save_button, false, false, 0);
        }


        public void AddEditor (IFieldEditor editor)
        {
            editors.Add (editor);
        }

        private EditorPhoto ReadPreset ()
        {
            var info = new EditorPhoto ();

            foreach (var editor in editors)
                editor.UpdateInfo (info);

            return info;
        }

        private void SavePreset (string preset_name)
        {

        }

        private void SavePresetDialog ()
        {
            var dialog = new EnterPresetNameDialog ();

            var result = dialog.Run ();

            if (result == (int) ResponseType.Ok)
                SavePreset (dialog.PresetName);

            dialog.Destroy ();
        }

        private class EnterPresetNameDialog : Gtk.Dialog
        {
            private Entry name_entry;

            public string PresetName {
                get { return name_entry.Text; }
            }

            public EnterPresetNameDialog () : base ()
            {
                Title = Catalog.GetString ("Enter Preset Name");
                SetSizeRequest (300, 100);

                var inner_vbox = new VBox (false, 3);
                VBox.Add (inner_vbox);

                inner_vbox.PackStart (new Label (Catalog.GetString ("Preset Name:")) { Xalign = 0.0f }, false, false, 3);
                inner_vbox.PackStart (name_entry = new Entry () {}, false, false, 3);

                VBox.ShowAll ();

                AddActionWidget (new Button ("gtk-cancel") { Visible = true }, ResponseType.Cancel);
                AddActionWidget (new Button ("gtk-save") { Visible = true }, ResponseType.Ok);
            }
        }
    }
}

