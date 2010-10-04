/*
 * LabelledFieldEditorWidget.cs
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
    public class LabelledFieldEditorWidget : VBox, IFieldEditor
    {
		private bool only_writeable_files;
        private Action<Widget, EditorPhoto> update_widget;
        private Action<Widget, EditorPhoto> update_info;

        protected EditorController Controller {
            get; private set;
        }

        protected Widget Widget {
            get; private set;
        }

#region Constructors

        public LabelledFieldEditorWidget (string label, EditorController controller, Widget widget,
                                          Action<Widget, EditorPhoto> update_widget,
                                          Action<Widget, EditorPhoto> update_info)
            : this (label, controller, widget, update_widget, update_info, true)
        {
        }

        public LabelledFieldEditorWidget (string label, EditorController controller, Widget widget,
                                          Action<Widget, EditorPhoto> update_widget,
                                          Action<Widget, EditorPhoto> update_info, bool expand)
            : this (label, controller, widget, false, update_widget, update_info, expand)
        {
        }

        public LabelledFieldEditorWidget (string label, EditorController controller, Widget widget,
                                          bool only_writeable_files,
                                          Action<Widget, EditorPhoto> update_widget,
                                          Action<Widget, EditorPhoto> update_info, bool expand)
            : base (false, 3)
        {
            this.Widget = widget;
            this.Controller = controller;
            this.update_widget = update_widget;
            this.update_info = update_info;
			this.only_writeable_files = only_writeable_files;

            PackStart (new Label (label) { Xalign = 0 }, false, false, 0);

            if (expand) {
                PackStart (Widget, true, true, 0);
            } else {
                var hbox = new HBox (false, 0) {};
                hbox.PackStart (Widget, false, false, 0);
                PackStart (hbox, false, false, 0);
            }

            controller.AddEditor (this);
        }

#endregion

#region IPhotoInfoEditor Implementation

        public void UpdateEditor (EditorPhoto info)
        {
            update_widget (Widget, info);

            Widget.Sensitive = info.WriteableMetadata || ! only_writeable_files;
        }

        public void UpdateInfo (EditorPhoto info)
        {
            update_info (Widget, info);
        }

#endregion

    }
}

