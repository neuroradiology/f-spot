using Gdk;
using FSpot.Tasks;

namespace FSpot.Loaders
{
	public interface IPhotoLoader
	{
		/// <summary>
		///    Used to remove all cached data when a file is delete.
		/// </summary>
		void ClearCache ();

		Task<Pixbuf> FindBestPreview (int width, int height);
	}
}
