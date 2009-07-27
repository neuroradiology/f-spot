//
// Fspot/Loaders/ItemsCompletedEventArgs.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using System;

namespace FSpot.Loaders {
	public class ItemsCompletedEventArgs : EventArgs
	{
		public ImageLoaderItem Items { get; private set; }

		public ItemsCompletedEventArgs (ImageLoaderItem items) : base ()
		{
			this.Items = items;
		}
	}
}
