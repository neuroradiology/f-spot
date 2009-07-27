//
// Fspot/Loaders/ImageLoaderItem.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using System;

namespace FSpot.Loaders {
	// Different bits of data which can be extracted
	[Flags]
	public enum ImageLoaderItem {
		None        = 0,
		Thumbnail   = 1 << 0,   // A small thumbnail
		Large       = 1 << 1,   // A large image for displaying (should load reasonably fast)
		Full        = 1 << 2    // The full image, for processing purposes (potentially very slow)
	}
}
