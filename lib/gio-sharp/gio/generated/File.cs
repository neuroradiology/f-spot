// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;

#region Autogenerated code
	public interface File : GLib.IWrapper {

		void CreateAsync(GLib.FileCreateFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		string Path { 
			get;
		}
		bool ReplaceContentsFinish(GLib.AsyncResult res, string new_etag);
		bool Move(GLib.File destination, GLib.FileCopyFlags flags, GLib.Cancellable cancellable, GLib.FileProgressCallback progress_callback);
		GLib.FileMonitor Monitor(GLib.FileMonitorFlags flags, GLib.Cancellable cancellable);
		GLib.File GetChild(string name);
		bool QueryExists(GLib.Cancellable cancellable);
		GLib.FileEnumerator EnumerateChildrenFinish(GLib.AsyncResult res);
		void MountEnclosingVolume(GLib.MountMountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool MakeDirectory(GLib.Cancellable cancellable);
		GLib.FileOutputStream ReplaceFinish(GLib.AsyncResult res);
		void QueryFilesystemInfoAsync(string attributes, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void SetDisplayNameAsync(string display_name, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileOutputStream CreateFinish(GLib.AsyncResult res);
		GLib.FileAttributeInfoList QueryWritableNamespaces(GLib.Cancellable cancellable);
		bool EjectMountableWithOperationFinish(GLib.AsyncResult result);
		bool SetAttributesFromInfo(GLib.FileInfo info, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		string UriScheme { 
			get;
		}
		void StopMountable(GLib.MountUnmountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool MakeDirectoryWithParents(GLib.Cancellable cancellable);
		bool StartMountableFinish(GLib.AsyncResult result);
		void EnumerateChildrenAsync(string attributes, GLib.FileQueryInfoFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool SetAttributesFinish(GLib.AsyncResult result, GLib.FileInfo info);
		void ReplaceAsync(string etag, bool make_backup, GLib.FileCreateFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.File Dup();
		GLib.Mount FindEnclosingMount(GLib.Cancellable cancellable);
		bool MountEnclosingVolumeFinish(GLib.AsyncResult result);
		bool EjectMountableFinish(GLib.AsyncResult result);
		bool SetAttributeUint64(string attribute, ulong value, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		bool LoadPartialContentsFinish(GLib.AsyncResult res, string contents, out ulong length, string etag_out);
		GLib.FileIOStream ReplaceReadwrite(string etag, bool make_backup, GLib.FileCreateFlags flags, GLib.Cancellable cancellable);
		void QueryInfoAsync(string attributes, GLib.FileQueryInfoFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool SetAttributeInt64(string attribute, long value, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		GLib.File ResolveRelativePath(string relative_path);
		GLib.File MountMountableFinish(GLib.AsyncResult result);
		GLib.FileOutputStream Create(GLib.FileCreateFlags flags, GLib.Cancellable cancellable);
		bool HasUriScheme(string uri_scheme);
		GLib.FileInputStream ReadFinish(GLib.AsyncResult res);
		GLib.FileEnumerator EnumerateChildren(string attributes, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		bool SupportsThreadContexts();
		void StartMountable(GLib.DriveStartFlags flags, GLib.MountOperation start_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void MountMountable(GLib.MountMountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool LoadContentsFinish(GLib.AsyncResult res, string contents, out ulong length, string etag_out);
		void OpenReadwriteAsync(int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void LoadPartialContentsAsync(GLib.Cancellable cancellable, GLib.FileReadMoreCallback read_more_callback, GLib.AsyncReadyCallback cb);
		bool StopMountableFinish(GLib.AsyncResult result);
		void ReplaceContentsAsync(string contents, string etag, bool make_backup, GLib.FileCreateFlags flags, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void CopyAsync(GLib.File destination, GLib.FileCopyFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.FileProgressCallback progress_callback, GLib.AsyncReadyCallback cb);
		void EjectMountableWithOperation(GLib.MountUnmountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void UnmountMountableWithOperation(GLib.MountUnmountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool MakeSymbolicLink(string symlink_value, GLib.Cancellable cancellable);
		bool SetAttributeString(string attribute, string value, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		bool UnmountMountableWithOperationFinish(GLib.AsyncResult result);
		bool Copy(GLib.File destination, GLib.FileCopyFlags flags, GLib.Cancellable cancellable, GLib.FileProgressCallback progress_callback);
		bool SetAttributeByteString(string attribute, string value, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		GLib.FileOutputStream AppendToFinish(GLib.AsyncResult res);
		bool CopyAttributes(GLib.File destination, GLib.FileCopyFlags flags, GLib.Cancellable cancellable);
		void PollMountable(GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool PollMountableFinish(GLib.AsyncResult result);
		GLib.FileInfo QueryInfo(string attributes, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		bool Delete(GLib.Cancellable cancellable);
		bool Equal(GLib.File file2);
		GLib.File SetDisplayNameFinish(GLib.AsyncResult res);
		void SetAttributesAsync(GLib.FileInfo info, GLib.FileQueryInfoFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileType QueryFileType(GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		GLib.AppInfo QueryDefaultHandler(GLib.Cancellable cancellable);
		GLib.FileInfo QueryInfoFinish(GLib.AsyncResult res);
		GLib.FileInputStream Read(GLib.Cancellable cancellable);
		bool SetAttribute(string attribute, GLib.FileAttributeType type, IntPtr value_p, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		GLib.FileInfo QueryFilesystemInfo(string attributes, GLib.Cancellable cancellable);
		GLib.File Parent { 
			get;
		}
		string ParsedName { 
			get;
		}
		GLib.FileOutputStream Replace(string etag, bool make_backup, GLib.FileCreateFlags flags, GLib.Cancellable cancellable);
		string Uri { 
			get;
		}
		GLib.FileInfo QueryFilesystemInfoFinish(GLib.AsyncResult res);
		void ReadAsync(int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		string GetRelativePath(GLib.File descendant);
		GLib.FileAttributeInfoList QuerySettableAttributes(GLib.Cancellable cancellable);
		bool LoadContents(GLib.Cancellable cancellable, string contents, out ulong length, string etag_out);
		void LoadContentsAsync(GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void CreateReadwriteAsync(GLib.FileCreateFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileIOStream ReplaceReadwriteFinish(GLib.AsyncResult res);
		bool UnmountMountableFinish(GLib.AsyncResult result);
		string Basename { 
			get;
		}
		bool SetAttributeInt32(string attribute, int value, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		GLib.FileIOStream CreateReadwrite(GLib.FileCreateFlags flags, GLib.Cancellable cancellable);
		void EjectMountable(GLib.MountUnmountFlags flags, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		void AppendToAsync(GLib.FileCreateFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.Mount FindEnclosingMountFinish(GLib.AsyncResult res);
		void ReplaceReadwriteAsync(string etag, bool make_backup, GLib.FileCreateFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool HasPrefix(GLib.File prefix);
		bool IsNative { 
			get;
		}
		void FindEnclosingMountAsync(int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.File GetChildForDisplayName(string display_name);
		GLib.FileIOStream CreateReadwriteFinish(GLib.AsyncResult res);
		GLib.FileIOStream OpenReadwriteFinish(GLib.AsyncResult res);
		GLib.File SetDisplayName(string display_name, GLib.Cancellable cancellable);
		GLib.FileIOStream OpenReadwrite(GLib.Cancellable cancellable);
		bool ReplaceContents(string contents, string etag, bool make_backup, GLib.FileCreateFlags flags, string new_etag, GLib.Cancellable cancellable);
		bool SetAttributeUint32(string attribute, uint value, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		GLib.FileOutputStream AppendTo(GLib.FileCreateFlags flags, GLib.Cancellable cancellable);
		bool Trash(GLib.Cancellable cancellable);
		void UnmountMountable(GLib.MountUnmountFlags flags, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool CopyFinish(GLib.AsyncResult res);
#region Customized extensions
#line 1 "File.custom"
// File.custom - customizations to GLib.File
//
// Authors: Stephane Delcroix  <stephane@delcroix.org>
//
// Copyright (C) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

bool Exists
{
	get;
}

bool Delete();

#endregion
	}

	[GLib.GInterface (typeof (FileAdapter))]
	public interface FileImplementor : GLib.IWrapper {

		GLib.File Dup ();
		uint Hash ();
		bool Equal (GLib.File file2);
		bool IsNative { get; }
		bool HasUriScheme (string uri_scheme);
		string UriScheme { get; }
		string Basename { get; }
		string Path { get; }
		string Uri { get; }
		string ParseName { get; }
		GLib.File Parent { get; }
		bool PrefixMatches (GLib.File file);
		string GetRelativePath (GLib.File descendant);
		GLib.File ResolveRelativePath (string relative_path);
		GLib.File GetChildForDisplayName (string display_name);
		GLib.FileEnumerator EnumerateChildren (string attributes, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		void EnumerateChildrenAsync (string attributes, GLib.FileQueryInfoFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileEnumerator EnumerateChildrenFinish (GLib.AsyncResult res);
		GLib.FileInfo QueryInfo (string attributes, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		void QueryInfoAsync (string attributes, GLib.FileQueryInfoFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileInfo QueryInfoFinish (GLib.AsyncResult res);
		GLib.FileInfo QueryFilesystemInfo (string attributes, GLib.Cancellable cancellable);
		void QueryFilesystemInfoAsync (string attributes, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileInfo QueryFilesystemInfoFinish (GLib.AsyncResult res);
		GLib.Mount FindEnclosingMount (GLib.Cancellable cancellable);
		void FindEnclosingMountAsync (int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.Mount FindEnclosingMountFinish (GLib.AsyncResult res);
		GLib.File SetDisplayName (string display_name, GLib.Cancellable cancellable);
		void SetDisplayNameAsync (string display_name, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.File SetDisplayNameFinish (GLib.AsyncResult res);
		GLib.FileAttributeInfoList QuerySettableAttributes (GLib.Cancellable cancellable);
		void QuerySettableAttributesAsync ();
		void QuerySettableAttributesFinish ();
		GLib.FileAttributeInfoList QueryWritableNamespaces (GLib.Cancellable cancellable);
		void QueryWritableNamespacesAsync ();
		void QueryWritableNamespacesFinish ();
		bool SetAttribute (string attribute, GLib.FileAttributeType type, IntPtr value_p, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		bool SetAttributesFromInfo (GLib.FileInfo info, GLib.FileQueryInfoFlags flags, GLib.Cancellable cancellable);
		void SetAttributesAsync (GLib.FileInfo info, GLib.FileQueryInfoFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool SetAttributesFinish (GLib.AsyncResult result, GLib.FileInfo info);
		GLib.FileInputStream ReadFn (GLib.Cancellable cancellable);
		void ReadAsync (int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileInputStream ReadFinish (GLib.AsyncResult res);
		GLib.FileOutputStream AppendTo (GLib.FileCreateFlags flags, GLib.Cancellable cancellable);
		void AppendToAsync (GLib.FileCreateFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileOutputStream AppendToFinish (GLib.AsyncResult res);
		GLib.FileOutputStream Create (GLib.FileCreateFlags flags, GLib.Cancellable cancellable);
		void CreateAsync (GLib.FileCreateFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileOutputStream CreateFinish (GLib.AsyncResult res);
		GLib.FileOutputStream Replace (string etag, bool make_backup, GLib.FileCreateFlags flags, GLib.Cancellable cancellable);
		void ReplaceAsync (string etag, bool make_backup, GLib.FileCreateFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileOutputStream ReplaceFinish (GLib.AsyncResult res);
		bool DeleteFile (GLib.Cancellable cancellable);
		void DeleteFileAsync ();
		void DeleteFileFinish ();
		bool Trash (GLib.Cancellable cancellable);
		void TrashAsync ();
		void TrashFinish ();
		bool MakeDirectory (GLib.Cancellable cancellable);
		void MakeDirectoryAsync ();
		void MakeDirectoryFinish ();
		bool MakeSymbolicLink (string symlink_value, GLib.Cancellable cancellable);
		void MakeSymbolicLinkAsync ();
		void MakeSymbolicLinkFinish ();
		bool Copy (GLib.File destination, GLib.FileCopyFlags flags, GLib.Cancellable cancellable, GLib.FileProgressCallback progress_callback);
		void CopyAsync (GLib.File destination, GLib.FileCopyFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.FileProgressCallback progress_callback, GLib.AsyncReadyCallback cb);
		bool CopyFinish (GLib.AsyncResult res);
		bool Move (GLib.File destination, GLib.FileCopyFlags flags, GLib.Cancellable cancellable, GLib.FileProgressCallback progress_callback);
		void MoveAsync ();
		void MoveFinish ();
		void MountMountable (GLib.MountMountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.File MountMountableFinish (GLib.AsyncResult result);
		void UnmountMountable (GLib.MountUnmountFlags flags, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool UnmountMountableFinish (GLib.AsyncResult result);
		void EjectMountable (GLib.MountUnmountFlags flags, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool EjectMountableFinish (GLib.AsyncResult result);
		void MountEnclosingVolume (GLib.MountMountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool MountEnclosingVolumeFinish (GLib.AsyncResult result);
		GLib.FileMonitor MonitorDir (GLib.FileMonitorFlags flags, GLib.Cancellable cancellable);
		GLib.FileMonitor MonitorFile (GLib.FileMonitorFlags flags, GLib.Cancellable cancellable);
		GLib.FileIOStream OpenReadwrite (GLib.Cancellable cancellable);
		void OpenReadwriteAsync (int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileIOStream OpenReadwriteFinish (GLib.AsyncResult res);
		GLib.FileIOStream CreateReadwrite (GLib.FileCreateFlags flags, GLib.Cancellable cancellable);
		void CreateReadwriteAsync (GLib.FileCreateFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileIOStream CreateReadwriteFinish (GLib.AsyncResult res);
		GLib.FileIOStream ReplaceReadwrite (string etag, bool make_backup, GLib.FileCreateFlags flags, GLib.Cancellable cancellable);
		void ReplaceReadwriteAsync (string etag, bool make_backup, GLib.FileCreateFlags flags, int io_priority, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		GLib.FileIOStream ReplaceReadwriteFinish (GLib.AsyncResult res);
		void StartMountable (GLib.DriveStartFlags flags, GLib.MountOperation start_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool StartMountableFinish (GLib.AsyncResult result);
		void StopMountable (GLib.MountUnmountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool StopMountableFinish (GLib.AsyncResult result);
		void UnmountMountableWithOperation (GLib.MountUnmountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool UnmountMountableWithOperationFinish (GLib.AsyncResult result);
		void EjectMountableWithOperation (GLib.MountUnmountFlags flags, GLib.MountOperation mount_operation, GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool EjectMountableWithOperationFinish (GLib.AsyncResult result);
		void PollMountable (GLib.Cancellable cancellable, GLib.AsyncReadyCallback cb);
		bool PollMountableFinish (GLib.AsyncResult result);
	}
#endregion
}
