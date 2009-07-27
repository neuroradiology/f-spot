//
// Fspot/Loaders/AreaUpdatedEventArgs.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//	Stephane Delcroix  <sdelcroix@novell.com>
//
//
// This is free software. See COPYING for details
//

using Gdk;
using System;

namespace FSpot.Loaders {
	public class AreaUpdatedEventArgs : EventArgs
	{
		public ImageLoaderItem Item { get; private set; }
		public Rectangle Area { get; private set; }

		public AreaUpdatedEventArgs (ImageLoaderItem item, Rectangle area) : base ()
		{
			this.Item = item;
			this.Area = area;
		}
	}
}
