//
// Fspot.Imaging.IWritableImageFile.cs
//
// Copyright (c) 2009 Novell, Inc.
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

namespace FSpot.Imaging {
	public interface IWritableImageFile {
		void Save (Gdk.Pixbuf pixbuf, System.IO.Stream stream);
	}
}
