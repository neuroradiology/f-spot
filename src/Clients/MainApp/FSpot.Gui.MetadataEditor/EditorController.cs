/*
 * EditorController.cs
 *
 * Author(s):
 *  Mike Gemuende <mike@gemuende.de>
 *
 * This is frees software. See COPYING for details.
 */

using System;
using System.Collections.Generic;

using Hyena;

using FSpot.Core;
using FSpot.Utils;


namespace FSpot.Gui.MetadataEditor
{
    public class EditorController
    {

#region Private Fields

        private Dictionary<IPhoto, EditorPhoto> properties = new Dictionary<IPhoto, EditorPhoto> ();
        private List<IFieldEditor> editors = new List<IFieldEditor> ();

#endregion

#region Constructors

        public EditorController (IBrowsableCollection collection)
        {
            Collection = collection;
            Pointer = new BrowsablePointer (Collection, -1);

            Pointer.Changed += (o, e) => {
                SelectPhoto (Pointer.Current);
            };
        }

#endregion

#region Public Interface

        public IBrowsableCollection Collection {
            get; private set;
        }

        public BrowsablePointer Pointer {
            get; private set;
        }

        public void AddEditor (IFieldEditor editor)
        {
            editors.Add (editor);
        }

        public IEnumerable<EditorPhoto> PhotoInfos {
            get {
                foreach (var photo in Collection.Items) {
                    yield return properties [photo];
                }
            }
        }

        public bool Load (Func<int, int, bool> loaded)
        {
            int i = 0;
            foreach (var photo in Collection.Items) {

                properties.Add (photo, EditorPhoto.CreateFromPhoto (photo));

                if ( ! loaded (i, Collection.Count))
                    return false;

                i++;
            }

            return true;
        }

        public bool Store (Func<int, int, bool> stored)
        {
            UpdateInfo ();

            int i = 0;
            foreach (var photo in Collection.Items) {

                EditorPhoto.SaveToPhoto (properties [photo], photo);

                if ( ! stored (i, Collection.Count)) {
                    Log.DebugFormat ("Callback aborted.");
                    return false;
                }

                i++;
            }

            return true;
        }

#endregion

#region Controller Logic

        private EditorPhoto SelectedPhotoInfo {
            get; set;
        }

        private void SelectPhoto (IPhoto photo)
        {
            UpdateInfo ();

            SelectedPhotoInfo = properties [photo];

            UpdateEditors ();
        }

        private void UpdateInfo ()
        {
            if (SelectedPhotoInfo == null)
                return;

            foreach (var editor in editors)
                editor.UpdateInfo (SelectedPhotoInfo);
        }

        private void UpdateEditors ()
        {
            if (SelectedPhotoInfo == null)
                return;

            foreach (var editor in editors)
                editor.UpdateEditor (SelectedPhotoInfo);
        }

#endregion

    }
}

