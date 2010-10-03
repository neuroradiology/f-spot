/*
 * IPhotoSource.cs
 *
 * Author(s):
 *  Mike Gemuende <mike@gemuende.de>
 *
 * This is free software. See COPYING for details.
 */

using System;

namespace FSpot.Core
{

    public interface IPhotoSource : ISource
    {
        IBrowsableCollection Photos { get; }

        void AddPhotos (IBrowsableCollection photos);
        void RemovePhotos (IBrowsableCollection photos);
        void DeletePhotos (IBrowsableCollection photos);

        bool CanAddPhotos { get; }
        bool CanRemovePhotos { get; }
        bool CanDeletePhotos { get; }
    }
}
