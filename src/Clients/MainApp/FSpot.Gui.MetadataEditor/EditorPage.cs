/*
 * EditorPage.cs
 *
 * Author(s):
 *  Mike Gemuende <mike@gemuende.de>
 *
 * This is frees software. See COPYING for details.
 */

using System;
using System.Collections.Generic;

using Gtk;


namespace FSpot.Gui.MetadataEditor
{
    public class EditorPage : VBox
    {

#region Constructors

        public EditorPage (string title)
            : base (false, 6)
        {
            Title = title;
        }

#endregion

        public string Title {
            get; private set;
        }
    }
}

