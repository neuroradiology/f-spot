using System;
using System.Collections.Generic;

namespace FSpot.Loaders
{
    public class PhotoLoaderCache
    {
        Dictionary<ILoadable, WeakReference> Loaders = new Dictionary<ILoadable, WeakReference>();

        public IPhotoLoader RequestLoader (ILoadable photo)
        {
            WeakReference reference = null;
            IPhotoLoader loader = null;

            if (Loaders.TryGetValue (photo, out reference)) {
                loader = reference.Target as IPhotoLoader;
            }

            if (loader == null) {
                loader = new XdgThumbnailLoader (photo);
                Loaders[photo] = new WeakReference (loader);
            }

            return loader;
        }

		// FIXME: periodically purge
    }
}
