//
// FSpot.PhotoVersion.cs
//
// Author(s):
//	Ettore Perazzoli <ettore@perazzoli.org>
//	Larry Ewing <lewing@gnome.org>
//	Stephane Delcroix <stephane@delcroix.org>
//	Thomas Van Machelen <thomas.vanmachelen@gmail.com>
//	Ruben Vermeersch <ruben@savanne.be>
//
// This is free software. See COPYING for details.
//

namespace FSpot
{
	public class PhotoVersion : IBrowsableItemVersion
	{
		public string Name { get; set; }
		public IBrowsableItem Photo { get; private set; }
	
		System.Uri uri;
		public System.Uri Uri {
			get { return uri; }
			set { 
				if (value == null)
					throw new System.ArgumentNullException ("uri");
				uri = value;
			}
		}

		public string MD5Sum { get; internal set; }
		public uint VersionId { get; private set; }
		public bool IsProtected { get; private set; }
		public PhotoVersionType Type { get; set; }
		public uint ParentVersionId { get; set; }

		public uint RefCount {
			get {
				return Core.Database.Photos.VersionRefCount (this);
			}
		}
	
		public PhotoVersion (Photo photo, uint version_id, System.Uri uri,
				string md5_sum, string name, bool is_protected,
				PhotoVersionType type, uint parent_version_id)
		{
			this.Photo = photo;
			this.VersionId = version_id;
			this.Uri = uri;
			this.MD5Sum = md5_sum;
			this.Name = name;
			this.IsProtected = is_protected;
			this.Type = type;
			this.ParentVersionId = parent_version_id;
		}
	}
}
