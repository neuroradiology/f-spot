/*
 * BasicEditorPage.cs
 *
 * Author(s):
 *  Mike Gemuende <mike@gemuende.de>
 *
 * This is frees software. See COPYING for details.
 */

using System;

using Mono.Unix;

using Gtk;

using FSpot.Widgets;


namespace FSpot.Gui.MetadataEditor
{
    public class BasicEditorPage : EditorPage
    {
        public BasicEditorPage (EditorController controller)
            : base (Catalog.GetString ("Basic Photo Info"))
        {
            PackStart (new LabelledSyncEditorWidget (Catalog.GetString ("Creator:"),
                            Catalog.GetString ("Set this Creator to all Photos"),
                            controller, new Entry (), true,
                            (widget, info) => { (widget as Entry).Text = info.Creator ?? String.Empty; },
                            (widget, info) => { info.Creator = (widget as Entry).Text; }),
                       false, false, 3);

            PackStart (new LabelledSyncEditorWidget (Catalog.GetString ("Copyright:"),
                            Catalog.GetString ("Set this Copyright to all Photos"),
                            controller, new Entry (), true,
                            (widget, info) => { (widget as Entry).Text = info.Copyright ?? String.Empty; },
                            (widget, info) => { info.Copyright = (widget as Entry).Text; }),
                       false, false, 3);

            PackStart (new HSeparator (), false, false, 6);

            PackStart (new LabelledSyncEditorWidget (Catalog.GetString ("Title:"),
                            Catalog.GetString ("Set this Title to all Photos"),
                            controller, new Entry (), true,
                            (widget, info) => { (widget as Entry).Text = info.Title ?? String.Empty; },
                            (widget, info) => { info.Title = (widget as Entry).Text; }),
                       false, false, 3);

            PackStart (new LabelledSyncEditorWidget (Catalog.GetString ("Comment:"),
                            Catalog.GetString ("Set this Comment to all Photos"),
                            controller, new Entry (), false,
                            (widget, info) => { (widget as Entry).Text = info.Comment ?? String.Empty; },
                            (widget, info) => { info.Comment = (widget as Entry).Text; }),
                       false, false, 3);

            PackStart (new LabelledFieldEditorWidget (Catalog.GetString ("Rating:"),
                            controller, new RatingEntry (), false,
                            (widget, info) => { (widget as RatingEntry).Value = (int) info.Rating; },
                            (widget, info) => { info.Rating = (uint) (widget as RatingEntry).Value; },
                            false),
                       false, false, 0);

            PackStart (new VBox (), true, true, 0);
        }
    }
}

