using System;
using System.Collections.Generic;

namespace FSpot.Loaders
{
    public class PhotoLoaderCache
    {
        Dictionary<string, WeakReference> Loaders = new Dictionary<string, WeakReference>();

        public IPhotoLoader RequestLoader (ILoadable photo)
        {
            var key = photo.Uri.ToString ();
            WeakReference reference = null;
            IPhotoLoader loader = null;

            if (Loaders.TryGetValue (key, out reference)) {
                loader = reference.Target as IPhotoLoader;
            }

            if (loader == null) {
                loader = new XdgThumbnailLoader (photo);
                Loaders[key] = new WeakReference (loader);
            }

            return loader;
        }
    }
}
