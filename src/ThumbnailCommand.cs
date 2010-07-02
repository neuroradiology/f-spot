using System;
using Gtk;
using FSpot;
using FSpot.Utils;
using FSpot.Tasks;
using FSpot.UI.Dialog;

public class ThumbnailCommand {
	
	private Gtk.Window parent_window;

	public ThumbnailCommand (Gtk.Window parent_window)
	{
		this.parent_window = parent_window;
	}

	public bool Execute (Photo [] photos)
	{
		ProgressDialog progress_dialog = null;
		if (photos.Length > 1) {
			progress_dialog = new ProgressDialog (Mono.Unix.Catalog.GetString ("Updating Thumbnails"),
							      ProgressDialog.CancelButtonType.Stop,
							      photos.Length, parent_window);
		}

		int count = 0;
		foreach (Photo p in photos) {
			if (progress_dialog != null
			    && progress_dialog.Update (String.Format (Mono.Unix.Catalog.GetString ("Updating picture \"{0}\""), p.Name)))
				break;

			foreach (uint version_id in p.VersionIds) {
                GenerateThumbnail (p.GetVersion (version_id));
			}
			
			count++;
		}

		if (progress_dialog != null)
			progress_dialog.Destroy ();

		return true;
	}

    void GenerateThumbnail (ILoadable item)
    {
        var loader = App.Instance.Loaders.RequestLoader (item);
        var preview_task = loader.FindBestPreview (256, 256);
        var task = new Task<bool> (() => { preview_task.Result.Dispose (); return false; }, TaskPriority.Background);
        preview_task.ContinueWith (task);
    }
}
