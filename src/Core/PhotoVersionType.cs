//
// Fspot.PhotoVersionType.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

namespace FSpot
{
	// Denotes the type of a photo version.
	//
	// WARNING: Do not change the values of the enum values, unless you migrate
	// the database along!
	public enum PhotoVersionType
	{
		// A simple bitmap
		Simple = 0,

		// A version that's calculated using the associated processing
		// settings. Obtaining the final version is a matter of taking the
		// parent version and applying the needed processing steps.
		Processable = 1,

		// Invisible version, will be deleted as soon as there are no
		// references anymore. Upon deletion, versions are kept around as
		// hidden when a processable version references it as its parent
		// version.
		Hidden = 2
	}
}
