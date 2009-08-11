/*
 * FSpot.Photo.cs
 *
 * Author(s):
 *	Ettore Perazzoli <ettore@perazzoli.org>
 *	Larry Ewing <lewing@gnome.org>
 *	Stephane Delcroix <stephane@delcroix.org>
 * 
 * This is free software. See COPYING for details.
 */

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Mono.Unix;

using FSpot.Utils;
using FSpot.Jobs;
using FSpot.Imaging;
using FSpot.Platform;

namespace FSpot
{
	public class Photo : DbItem, IComparable, IBrowsableItem, IBrowsableItemVersionable {
		
		PhotoChanges changes = new PhotoChanges ();
		public PhotoChanges Changes {
			get{ return changes; }
			set {
				if (value != null)
					throw new ArgumentException ("The only valid value is null");
				changes = new PhotoChanges ();
			}
		}

		// The time is always in UTC.
		private DateTime time;
		public DateTime Time {
			get { return time; }
			set {
				if (time == value)
					return;
				time = value;
				changes.TimeChanged = true;
			}
		}
	
		public string Name {
			get { return Uri.UnescapeDataString (System.IO.Path.GetFileName (VersionUri (OriginalVersionId).AbsolutePath)); }
		}
	
		//This property no longer keeps a 'directory' path, but the logical container for the image, like:
		// file:///home/bob/Photos/2007/08/23 or
		// http://www.google.com/logos
		[Obsolete ("MARKED FOR REMOVAL. no longer makes sense with versions in different Directories. Any way to get rid of this ?")]
		public string DirectoryPath {
			get { 
				System.Uri uri = VersionUri (OriginalVersionId);
				return uri.Scheme + "://" + uri.Host + System.IO.Path.GetDirectoryName (uri.AbsolutePath);
			}
		}
	
		private ArrayList tags;
		public Tag [] Tags {
			get {
				if (tags == null)
					return new Tag [0];
	
				return (Tag []) tags.ToArray (typeof (Tag));
			}
		}
	
		private bool loaded = false;
		public bool Loaded {
			get { return loaded; }
			set { 
				if (value) {
					if (DefaultVersionId != OriginalVersionId && !versions.ContainsKey (DefaultVersionId)) 
						DefaultVersionId = OriginalVersionId;	
				}
				loaded = value; 
			}
		}
	
		private string description;
		public string Description {
			get { return description; }
			set {
				if (description == value)
					return;
				description = value;
				changes.DescriptionChanged = true;
			}
		}
	
		private uint roll_id = 0;
		public uint RollId {
			get { return roll_id; }
			set {
				if (roll_id == value)
					return;
				roll_id = value;
				changes.RollIdChanged = true;
			}
		}
	
		private uint rating;
		public uint Rating {
			get { return rating; }
			set {
				if (rating == value || value < 0 || value > 5)
					return;
				rating = value;
				changes.RatingChanged = true;
			}
		}

		private string md5_sum;
		public string MD5Sum {
			get { return md5_sum; }
			set { 
				if (md5_sum == value)
				 	return;

				md5_sum = value; 
				changes.MD5SumChanged = true;
			} 
		}
	
		// Version management
		public const int OriginalVersionId = 1;

		public uint HighestVersionId {
			get {
				uint highest = 0;
				foreach (uint key in HiddenVersions.Keys)
					highest = Math.Max (highest, key);
				foreach (uint key in Versions.Keys)
					highest = Math.Max (highest, key);
				return highest;
			}
		}

		private Dictionary<uint, PhotoVersion> hidden_versions;
		private Dictionary<uint, PhotoVersion> HiddenVersions {
			get {
				if (hidden_versions == null)
					hidden_versions = new Dictionary<uint, PhotoVersion> ();
				return hidden_versions;
			}
		}
	
		private Dictionary<uint, PhotoVersion> versions = new Dictionary<uint, PhotoVersion> ();
		public IEnumerable<IBrowsableItemVersion> Versions {
			get {
				foreach (var version in versions.Values)
					yield return version;
			}
		}

		public uint [] VersionIds {
			get {
				if (versions == null)
					return new uint [0];

				uint [] ids = new uint [versions.Count];
				versions.Keys.CopyTo (ids, 0);
				Array.Sort (ids);
				return ids;
			}
		}

		public PhotoVersion GetVersion (uint version_id)
		{
			return GetVersion (version_id, false);
		}

		public PhotoVersion GetVersion (uint version_id, bool include_hidden)
		{
			if (versions.ContainsKey (version_id))
				return versions [version_id];

			if (include_hidden && HiddenVersions.ContainsKey (version_id))
				return HiddenVersions [version_id];
	
			return null;
		}
	
		private uint default_version_id = OriginalVersionId;
		public uint DefaultVersionId {
			get { return default_version_id; }
			set {
				if (default_version_id == value)
					return;
				default_version_id = value;
				changes.DefaultVersionIdChanged = true;
			}
		}

		internal void AddHiddenVersion (uint version_id, System.Uri uri, string md5_sum, string name, bool is_protected, PhotoVersionType type, uint parent_version_id)
		{
			HiddenVersions [version_id] = new PhotoVersion (this, version_id, uri, md5_sum, name, is_protected, type, parent_version_id);
		}
	
		// This doesn't check if a version of that name already exists, 
		// it's supposed to be used only within the Photo and PhotoStore classes.
		internal void AddVersionUnsafely (uint version_id, System.Uri uri, string md5_sum, string name, bool is_protected, PhotoVersionType type, uint parent_version_id)
		{
			versions [version_id] = new PhotoVersion (this, version_id, uri, md5_sum, name, is_protected, type, parent_version_id);
	
			changes.AddVersion (version_id);
		}
	
		public uint AddVersion (System.Uri uri, string name)
		{
			return AddVersion (uri, name, false);
		}
	
		public uint AddVersion (System.Uri uri, string name, bool is_protected)
		{
			if (VersionNameExists (name))
				throw new ApplicationException ("A version with that name already exists");
			uint version_id = HighestVersionId + 1;
			string md5_sum = GenerateMD5 (uri);

			versions [version_id] = new PhotoVersion (this, version_id, uri, md5_sum, name, is_protected, PhotoVersionType.Simple, 0);

			changes.AddVersion (version_id);
			return version_id;
		}
	
		//FIXME: store versions next to originals. will crash on ro locations.
		private System.Uri GetUriForVersionName (string version_name, string extension)
		{
			string name_without_extension = System.IO.Path.GetFileNameWithoutExtension (Name);
	
			return new System.Uri (System.IO.Path.Combine (DirectoryPath,  name_without_extension 
						       + " (" + UriUtils.EscapeString (version_name, true, true, true) + ")" + extension));
		}
	
		public bool VersionNameExists (string version_name)
		{
            return Versions.Where ((v) => v.Name == version_name).Any ();
		}

		public System.Uri VersionUri (uint version_id)
		{
			if (!versions.ContainsKey (version_id))
				return null;
	
			PhotoVersion v = versions [version_id]; 
			if (v != null)
				return v.Uri;
	
			return null;
		}
		
		public IBrowsableItemVersion DefaultVersion {
			get {
				if (!versions.ContainsKey (DefaultVersionId))
					throw new Exception ("Something is horribly wrong, this should never happen: no default version!");
				return versions [DefaultVersionId]; 
			}
		}

		//FIXME: won't work on non file uris
		public uint SaveVersion (Gdk.Pixbuf buffer, bool create_version)
		{
			uint version = DefaultVersionId;
			using (ImageFile img = ImageFile.Create (DefaultVersion.Uri)) {
				// Always create a version if the source is not a jpeg for now.
				create_version = create_version || !(img is FSpot.JpegFile);
				bool original_format = img is IWritableImageFile;
	
				if (buffer == null)
					throw new ApplicationException ("invalid (null) image");
	
				string extension = original_format ? null : ".jpg";
				if (create_version)
					version = CreateDefaultModifiedVersion (DefaultVersionId, extension, false);
	
				try {
					Uri versionUri = VersionUri (version);

					if (original_format) {
						using (Stream stream = System.IO.File.OpenWrite (versionUri.LocalPath)) {
							(img as IWritableImageFile).Save (buffer, stream);
						}
					} else {
						// FIXME: There is no metadata copying yet!
						byte [] image_data = PixbufUtils.Save (buffer, "jpeg", new string [] {"quality" }, new string [] { "95" });
						using (Stream stream = System.IO.File.OpenWrite (versionUri.LocalPath)) {
							stream.Write (image_data, 0, image_data.Length);
						}

					}
					(GetVersion (version) as PhotoVersion).MD5Sum = GenerateMD5 (VersionUri (version));
					FSpot.ThumbnailGenerator.Create (versionUri).Dispose ();
					DefaultVersionId = version;
				} catch (System.Exception e) {
					System.Console.WriteLine (e);
					if (create_version)
						DeleteVersion (version);
				
					throw e;
				}
			}
			
			return version;
		}

		uint clean_hidden_versions_timeout = 0;

		public void DeleteVersion (uint version_id)
		{
			DeleteVersion (version_id, false);
		}

		public void DeleteVersion (uint version_id, bool remove_original)
		{
			if (version_id == OriginalVersionId && !remove_original)
				throw new Exception ("Cannot delete original version");
	
			changes.HideVersion (version_id);
			Versions.Remove (version_id);
			ResetDefaultVersion (version_id);

			if (clean_hidden_versions_timeout == 0) {
				clean_hidden_versions_timeout = GLib.Timeout.Add (5000, delegate () {
					clean_hidden_versions_timeout = 0;

					Core.Database.Jobs.Create (typeof (CleanHiddenVersionsJob), "");
					return true;
				});
			}
		}

		// Deletes a version without checking for refs. Use with care!
		public void FullyDeleteVersion (uint version_id, bool keep_file)
		{
			PhotoVersion version = GetVersion (version_id, true) as PhotoVersion;
			System.Uri uri =  version.Uri;
	
			if (!keep_file) {
				GLib.File file = GLib.FileFactory.NewForUri (uri);
				if (file.Exists) 
					try {
						file.Trash (null);
					} catch (GLib.GException) {
						Log.Debug ("Unable to Trash, trying to Delete");
						file.Delete ();
					}	
				try {
					ThumbnailFactory.DeleteThumbnail (uri);
				} catch {
					//ignore an error here we don't really care.
				}
			}

			if (versions.ContainsKey (version_id)) {
				versions.Remove (version_id);
				changes.RemoveVersion (version_id);
			} else if (HiddenVersions.ContainsKey (version_id)) {
				HiddenVersions.Remove (version_id);
			}

			ResetDefaultVersion (version_id);
		}

		public void DeleteHiddenVersions ()
		{
			foreach (uint version_id in HiddenVersions.Keys)
			{
				FullyDeleteVersion (version_id, false);
			}
		}

		void ResetDefaultVersion (uint version_id)
		{
			do {
				version_id --;
				if (versions.ContainsKey (version_id)) {
					DefaultVersionId = version_id;
					break;
				}
			} while (version_id > OriginalVersionId);
		}

		public uint CreateVersion (string name, uint base_version_id, bool create)
		{
			return CreateVersion (name, null, base_version_id, create, false);
		}

		public uint CreateVersion (string name, string extension, uint base_version_id, bool create)
		{
			return CreateVersion (name, extension, base_version_id, create, false);
		}
	
		private uint CreateVersion (string name, string extension, uint base_version_id, bool create, bool is_protected)
		{
			extension = extension ?? System.IO.Path.GetExtension (VersionUri (base_version_id).AbsolutePath);
			System.Uri new_uri = GetUriForVersionName (name, extension);
			System.Uri original_uri = VersionUri (base_version_id);
			string md5_sum = MD5Sum;
	
			if (VersionNameExists (name))
				throw new Exception ("This version name already exists");
	
			if (create) {
				GLib.File destination = GLib.FileFactory.NewForUri (new_uri);
				if (destination.Exists)
					throw new Exception (String.Format ("An object at this uri {0} already exists", new_uri.ToString ()));
	
		//FIXME. or better, fix the copy api !
				GLib.File source = GLib.FileFactory.NewForUri (original_uri);
				source.Copy (destination, GLib.FileCopyFlags.None, null, null);
	
				FSpot.ThumbnailGenerator.Create (new_uri).Dispose ();
			}

			uint version_id = HighestVersionId + 1;

			versions [version_id] = new PhotoVersion (this, version_id, new_uri, md5_sum, name, is_protected, PhotoVersionType.Simple, 0);

			changes.AddVersion (version_id);
	
			return version_id;
		}
	
		public uint CreateReparentedVersion (PhotoVersion version)
		{
			int num = 0;
			while (true) {
				num++;
				// Note for translators: Reparented is a picture becoming a version of another one
				string name = (num == 1) ? Catalog.GetString ("Reparented") : String.Format (Catalog.GetString( "Reparented ({0})"), num);
				name = String.Format (name, num);
				if (VersionNameExists (name))
					continue;
	
				Uri uri = GetUriForVersionName (name, System.IO.Path.GetExtension (version.Uri.AbsolutePath));
				uint version_id = HighestVersionId + 1;
				bool is_protected = version_id == OriginalVersionId;
				versions [version_id] = new PhotoVersion (this, version_id, uri, version.MD5Sum, name, is_protected, PhotoVersionType.Simple, 0);

				changes.AddVersion (version_id);

				Uri source_uri = version.Uri;
				Uri dest_uri = VersionUri (version_id);

				GLib.File source = GLib.FileFactory.NewForUri (source_uri);
				GLib.File dest = GLib.FileFactory.NewForUri (dest_uri);
				source.Copy (dest, GLib.FileCopyFlags.None, null, null);

				return version_id;
			}
		}
	
		public uint CreateDefaultModifiedVersion (uint base_version_id, bool create_file)
		{
			return CreateDefaultModifiedVersion (base_version_id, null, create_file);
		}

		public uint CreateDefaultModifiedVersion (uint base_version_id, string extension, bool create_file)
		{
			int num = 1;
	
			while (true) {
				string name = Catalog.GetPluralString ("Modified", 
									 "Modified ({0})", 
									 num);
				name = String.Format (name, num);
				System.Uri uri = GetUriForVersionName (name, System.IO.Path.GetExtension (VersionUri(base_version_id).GetFilename()));
				GLib.File file = GLib.FileFactory.NewForUri (uri);
	
				if (! VersionNameExists (name) && ! file.Exists)
					return CreateVersion (name, extension, base_version_id, create_file);
	
				num ++;
			}
		}
	
		public uint CreateNamedVersion (string name, string extension, uint base_version_id, bool create_file)
		{
			int num = 1;
			
			string final_name;
			while (true) {
				final_name = String.Format (
						(num == 1) ? Catalog.GetString ("Modified in {1}") : Catalog.GetString ("Modified in {1} ({0})"),
						num, name);
	
				System.Uri uri = GetUriForVersionName (final_name, System.IO.Path.GetExtension (VersionUri(base_version_id).GetFilename()));
				GLib.File file = GLib.FileFactory.NewForUri (uri);

				if (! VersionNameExists (final_name) && ! file.Exists)
					return CreateVersion (final_name, extension, base_version_id, create_file);
	
				num ++;
			}
		}
	
		public void RenameVersion (uint version_id, string new_name)
		{
			if (version_id == OriginalVersionId)
				throw new Exception ("Cannot rename original version");
	
			if (VersionNameExists (new_name))
				throw new Exception ("This name already exists");
	

			(GetVersion (version_id) as PhotoVersion).Name = new_name;
			changes.ChangeVersion (version_id);
	
			//TODO: rename file too ???
	
	//		if (System.IO.File.Exists (new_path))
	//			throw new Exception ("File with this name already exists");
	//
	//		File.Move (old_path, new_path);
	//		PhotoStore.MoveThumbnail (old_path, new_path);
		}
	
	
		// Tag management.
	
		// This doesn't check if the tag is already there, use with caution.
		public void AddTagUnsafely (Tag tag)
		{
			if (tags == null)
				tags = new ArrayList ();
	
			tags.Add (tag);
			changes.AddTag (tag);
		}
	
		// This on the other hand does, but is O(n) with n being the number of existing tags.
		public void AddTag (Tag tag)
		{
			if (!HasTag (tag))
				AddTagUnsafely (tag);
		}
	
		public void AddTag (Tag []taglist)
		{
			/*
			 * FIXME need a better naming convention here, perhaps just
			 * plain Add.
			 */
			foreach (Tag tag in taglist)
				AddTag (tag);
		}	
	
		public void RemoveTag (Tag tag)
		{
			if (!HasTag (tag))
				return;

			tags.Remove (tag);
			changes.RemoveTag (tag);
		}
	
		public void RemoveTag (Tag []taglist)
		{	
			foreach (Tag tag in taglist)
				RemoveTag (tag);
		}	
	
		public void RemoveCategory (IList<Tag> taglist)
		{
			foreach (Tag tag in taglist) {
				Category cat = tag as Category;
	
				if (cat != null)
					RemoveCategory (cat.Children);
	
				RemoveTag (tag);
			}
		}
	
		public bool HasTag (Tag tag)
		{
			if (tags == null)
				return false;
	
			return tags.Contains (tag);
		}
	
		//
		// MD5 Calculator
		//
		private static System.Security.Cryptography.MD5 md5_generator;

		private static System.Security.Cryptography.MD5 MD5Generator {
			get {
				if (md5_generator == null)
				 	md5_generator = new System.Security.Cryptography.MD5CryptoServiceProvider ();

				return md5_generator;
			} 
		}

		private static IDictionary<System.Uri, string> md5_cache = new Dictionary<System.Uri, string> ();

		public static void ResetMD5Cache () {
			if (md5_cache != null)	
				md5_cache.Clear (); 
		}

		public static string GenerateMD5 (System.Uri uri)
		{
		 	try {
			 	if (md5_cache.ContainsKey (uri))
				 	return md5_cache [uri];

				using (Gdk.Pixbuf pixbuf = ThumbnailGenerator.Create (uri))
				{
					byte[] serialized = GdkUtils.Serialize (pixbuf);
					byte[] md5 = MD5Generator.ComputeHash (serialized);
					string md5_string = Convert.ToBase64String (md5);

					md5_cache.Add (uri, md5_string);
					return md5_string;
				}
			} catch (Exception e) {
			 	Log.DebugException (String.Format ("Failed to create MD5Sum for Uri: {0}\n", uri), e);
			}

			return string.Empty; 
		}


		// Constructor
		public Photo (uint id, long unix_time, System.Uri uri, string md5_sum)
			: base (id)
		{
			if (uri == null)
				throw new System.ArgumentNullException ("uri");
	
			time = DbUtils.DateTimeFromUnixTime (unix_time);
	
			description = String.Empty;
			rating = 0;
			this.md5_sum = md5_sum;
	
			// Note that the original version is never stored in the photo_versions table in the
			// database.
			AddVersionUnsafely (OriginalVersionId, uri, md5_sum, Catalog.GetString ("Original"), true, PhotoVersionType.Simple, 0);
		}

#region IComparable implementation

		// IComparable 
		public int CompareTo (object obj) {
			if (this.GetType () == obj.GetType ()) {
				return this.Compare((Photo)obj);
			} else if (obj is DateTime) {
				return this.time.CompareTo ((DateTime)obj);
			} else {
				throw new Exception ("Object must be of type Photo");
			}
		}

		public int CompareTo (Photo photo)
		{
			int result = Id.CompareTo (photo.Id);
			
			if (result == 0)
				return 0;
			else 
				return (this as IBrowsableItem).Compare (photo);
		}

#endregion
	}
}
