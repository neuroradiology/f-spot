/*
 * EditorPhoto.cs
 *
 * Author(s):
 *  Mike Gemuende <mike@gemuende.de>
 *
 * This is frees software. See COPYING for details.
 */

using System;

using FSpot.Core;
using FSpot.Utils;


namespace FSpot.Gui.MetadataEditor
{
    public class EditorPhoto
    {

#region Constructors

        public EditorPhoto ()
        {
        }

#endregion

#region Photo Information

        public string Creator { get; set; }
        public string Copyright { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public uint Rating { get; set; }
        public bool WriteableMetadata { get; private set; }

#endregion

#region Read/Write Photo Information

        public static void SaveToPhoto (EditorPhoto info, IPhoto photo)
        {
            using (var metadata = Metadata.Parse (photo.DefaultVersion.Uri)) {
                if (metadata == null)
                    return;

                metadata.EnsureAvailableTags ();
                var tag = metadata.ImageTag;

                // set information to photo object
                var db_photo = photo as Photo;
                if (db_photo != null) {
                    db_photo.Description = info.Comment;
                    db_photo.Rating = info.Rating;

                    App.Instance.Database.Photos.Commit (db_photo);
                }

                // write information to file metadata
                tag.Title = info.Title;
                tag.Creator = info.Creator;
                tag.Copyright = info.Copyright;
                tag.Comment = info.Comment;
                tag.Rating = info.Rating;

                metadata.Save ();
            }
        }

        public static EditorPhoto CreateFromPhoto (IPhoto photo)
        {
            using (var metadata = Metadata.Parse (photo.DefaultVersion.Uri)) {
                if (metadata == null)
                    return null;

                var tag = metadata.ImageTag;
                var info = new EditorPhoto ();

                // get information from photo object
                info.Comment = photo.Description;
                info.Rating = photo.Rating;
                // determine information from file metadata
                info.Title = tag.Title;
                info.Creator = tag.Creator;
                info.Copyright = tag.Copyright;

                info.WriteableMetadata = metadata.Writeable;

                return info;
            }
        }

#endregion

    }
}

