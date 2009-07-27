//
// Fspot/Loaders/ImageLoaderItemExtensions.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using System;

namespace FSpot.Loaders {
	public static class ImageLoaderItemExtensions {
		public static bool Contains (this ImageLoaderItem item, ImageLoaderItem target)
		{
			return (item & target) == target;
		}

		public static ImageLoaderItem Largest (this ImageLoaderItem items)
		{
			if (items.Contains (ImageLoaderItem.Full))
				return ImageLoaderItem.Full;
			if (items.Contains (ImageLoaderItem.Large))
				return ImageLoaderItem.Large;
			if (items.Contains (ImageLoaderItem.Thumbnail))
				return ImageLoaderItem.Thumbnail;
			return ImageLoaderItem.None;
		}
	}
}
