//
// Fspot/Loaders/AreaPreparedEventArgs.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using System;

namespace FSpot.Loaders {
	public class AreaPreparedEventArgs : EventArgs
	{
		public ImageLoaderItem Item { get; private set; }

		public AreaPreparedEventArgs (ImageLoaderItem item) : base ()
		{
			this.Item = item;
		}
	}
}
