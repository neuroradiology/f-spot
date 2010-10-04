/*
 * LabelledSyncEditorWidget.cs
 *
 * Author(s):
 *  Mike Gemuende <mike@gemuende.de>
 *
 * This is frees software. See COPYING for details.
 */

using System;

using Gtk;


namespace FSpot.Gui.MetadataEditor
{
    public class LabelledSyncEditorWidget : LabelledFieldEditorWidget
    {
        public LabelledSyncEditorWidget (string label, string sync_button_tooltip, EditorController controller,
                                         Widget widget,
                                         Action<Widget, EditorPhoto> update_widget,
                                         Action<Widget, EditorPhoto> update_info)
            : this (label, sync_button_tooltip, controller, widget, false, update_widget, update_info)
        {
        }

        public LabelledSyncEditorWidget (string label, string sync_button_tooltip, EditorController controller,
                                         Widget widget, bool only_writeable_files,
                                         Action<Widget, EditorPhoto> update_widget,
                                         Action<Widget, EditorPhoto> update_info)
            : base (label, controller, new HBox (false, 8),
                    (w, info) => { update_widget (widget, info); },
                    (w, info) => { update_info (widget, info); })
        {
            (Widget as HBox).PackStart (widget, true, true, 0);

            var sync_button = new FieldEditorSyncButton (controller, this);
            sync_button.TooltipText = sync_button_tooltip;

            (Widget as HBox).PackStart (sync_button, false, false, 0);
        }
    }
}

