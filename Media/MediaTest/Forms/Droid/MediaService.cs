using System;
using Android.Graphics;
using System.IO;
using System.Threading.Tasks;
using Core.Services.Droid;

[assembly:Xamarin.Forms.Dependency (typeof(MediaService))]
namespace Core.Services.Droid
{
	public class MediaService : IMediaService
	{

		#region ICompressionService implementation

		public async Task<string> CompressAndResizeImageAsync (string filePath, int desiredWidth, int desiredHeight)
		{
			var newFileName = await LoadAndResizeBitmap (filePath, desiredWidth, desiredHeight);

			return newFileName;
		}

		public void DeleteImage (string filePath)
		{
			if (File.Exists (filePath))
			{
				try
				{
					File.Delete (filePath);
					// We don't really care if this explodes for a normal IO reason.
				}
				catch (UnauthorizedAccessException)
				{
				}
				catch (DirectoryNotFoundException)
				{
				}
				catch (IOException)
				{
				}
			}
		}

		#endregion

		private static int[] calculateAspectRatio (int origWidth, int origHeight, int targetWidth, int targetHeight)
		{
			int newWidth = targetWidth;
			int newHeight = targetHeight;

			// If no new width or height were specified return the original bitmap
			if (newWidth <= 0 && newHeight <= 0)
			{
				newWidth = origWidth;
				newHeight = origHeight;
			}
			// Only the width was specified
			else if (newWidth > 0 && newHeight <= 0)
			{
				newHeight = (newWidth * origHeight) / origWidth;
			}
			// only the height was specified
			else if (newWidth <= 0 && newHeight > 0)
			{
				newWidth = (newHeight * origWidth) / origHeight;
			}
			// If the user specified both a positive width and height
			// (potentially different aspect ratio) then the width or height is
			// scaled so that the image fits while maintaining aspect ratio.
			// Alternatively, the specified width and height could have been
			// kept and Bitmap.SCALE_TO_FIT specified when scaling, but this
			// would result in whitespace in the new image.
			else
			{
				double newRatio = newWidth / (double)newHeight;
				double origRatio = origWidth / (double)origHeight;

				if (origRatio > newRatio)
				{
					newHeight = (newWidth * origHeight) / origWidth;
				}
				else if (origRatio < newRatio)
				{
					newWidth = (newHeight * origWidth) / origHeight;
				}
			}

			int[] retval = new int[2];
			retval [0] = newWidth;
			retval [1] = newHeight;
			return retval;
		}

		private static int CalculateInSampleSize (BitmapFactory.Options options, int reqWidth, int reqHeight)
		{
			// Raw height and width of image
			float height = options.OutHeight;
			float width = options.OutWidth;
			double inSampleSize = 1D;

			if (height > reqHeight || width > reqWidth)
			{
				int halfHeight = (int)(height / 2);
				int halfWidth = (int)(width / 2);

				// Calculate a inSampleSize that is a power of 2 - the decoder will use a value that is a power of two anyway.
				while ((halfHeight / inSampleSize) > reqHeight && (halfWidth / inSampleSize) > reqWidth)
				{
					inSampleSize *= 2;
				}
			}

			return (int)inSampleSize;
		}

		private static async Task<string> LoadAndResizeBitmap (string fileName, int width, int height)
		{
			// First we get the the dimensions of the file on disk
			BitmapFactory.Options options = new BitmapFactory.Options {
				InPurgeable = true,
				InJustDecodeBounds = true
			};
			BitmapFactory.DecodeFile (fileName, options);

			var aspectRatio = calculateAspectRatio (options.OutWidth, options.OutHeight, width, height);

			// Now we will load the image and have BitmapFactory resize it for us.
			options.InSampleSize = CalculateInSampleSize (options, aspectRatio [0], aspectRatio [1]);
			options.InJustDecodeBounds = false;
			Bitmap resizedBitmap = await BitmapFactory.DecodeFileAsync (fileName, options);

			File.Delete (fileName);

			using (var stream = File.Open (fileName, FileMode.OpenOrCreate))
			{
				stream.Position = 0;
				await resizedBitmap.CompressAsync (Bitmap.CompressFormat.Jpeg, 92, stream);
			}

			resizedBitmap.Recycle ();

			return fileName;
		}
	}
}

