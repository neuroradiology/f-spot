using Gdk;
using FSpot.Tasks;
using FSpot.Utils;

namespace FSpot.Loaders
{
	public class XdgThumbnailLoader : IPhotoLoader
	{
        ILoadable Loadable { get; set; }

		public XdgThumbnailLoader (ILoadable loadable)
		{
			Loadable = loadable;
		}

		public void ClearCache ()
		{
			XdgThumbnailSpec.RemoveThumbnail (Loadable.Uri);
		}

		public Task<Pixbuf> FindBestPreview (int width, int height)
		{
			return new WorkerThreadTask<Pixbuf> (() => {
				var size = (width > 128 || height > 128) ? ThumbnailSize.Large : ThumbnailSize.Normal;
				var pixbuf = XdgThumbnailSpec.LoadThumbnail (Loadable.Uri, size);
				return pixbuf;
			}) {
				CancelWithChildren = true
			};
		}
	}
}
