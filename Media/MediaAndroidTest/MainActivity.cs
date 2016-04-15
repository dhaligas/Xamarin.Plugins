using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Content.PM;
using System.IO;

namespace MediaAndroidTest
{
	[Activity (Label = "MediaAndroidTest", MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			StartActivity (typeof(MainActivity2));
		}
	}

	[Activity (Label = "MediaAndroidTest", Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
	public class MainActivity2 : Activity
	{
		int count = 1;

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


		public static Bitmap LoadAndResizeBitmap(string fileName, int width, int height)
		{
			// First we get the the dimensions of the file on disk
			BitmapFactory.Options options = new BitmapFactory.Options
			{
				InPurgeable = true,
				InJustDecodeBounds = true
			};
			BitmapFactory.DecodeFile(fileName, options);

			// Next we calculate the ratio that we need to resize the image by
			// in order to fit the requested dimensions.
//			int outHeight = options.OutHeight;
//			int outWidth = options.OutWidth;
//			int inSampleSize = 1;
//
//			if (outHeight > height || outWidth > width)
//			{
//				inSampleSize = outWidth > outHeight
//					? outHeight / height
//					: outWidth / width;
//			}

			var aspectRatio = calculateAspectRatio (options.OutWidth, options.OutHeight, width, height);

			// Now we will load the image and have BitmapFactory resize it for us.
			options.InSampleSize = CalculateInSampleSize(options, aspectRatio[0], aspectRatio[1]);
			options.InJustDecodeBounds = false;
			Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

			var newFileName = fileName.Replace (".jpg", "_modified.jpg");

			resizedBitmap.Compress (Bitmap.CompressFormat.Png, 92, File.Open(newFileName, FileMode.CreateNew));
			resizedBitmap.Recycle ();

			Bitmap newBitmap = BitmapFactory.DecodeFile(newFileName, options);

			return newBitmap;
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.MyButton);
			var image = FindViewById<ImageView> (Resource.Id.imageView1);
			button.Click += async delegate
			{
				var media = new Plugin.Media.MediaImplementation ();
				var file = await Plugin.Media.CrossMedia.Current.TakePhotoAsync (new Plugin.Media.Abstractions.StoreCameraMediaOptions {
					Directory = "Sample",
					Name = "test.jpg",
					SaveToAlbum = false,
					DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Rear, 
					PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full
				});
				if (file == null)
					return;
				var path = file.Path;
				var stream = file.GetStream ();

				System.Diagnostics.Debug.WriteLine (path);
				System.Diagnostics.Debug.WriteLine (stream.Length);

				image.SetImageBitmap (LoadAndResizeBitmap(file.Path, 0, 1000));
				file.Dispose ();
			};

			var pick = FindViewById<Button> (Resource.Id.button1);
			pick.Click += async (sender, args) =>
			{
				var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync ();
				if (file == null)
					return;
				var path = file.Path;
				System.Diagnostics.Debug.WriteLine (path);
				image.SetImageBitmap (BitmapFactory.DecodeFile (file.Path));
				file.Dispose ();
			};

			FindViewById<Button> (Resource.Id.button2).Click += async (sender, args) =>
			{
				var media = new Plugin.Media.MediaImplementation ();
				var file = await Plugin.Media.CrossMedia.Current.TakeVideoAsync (new Plugin.Media.Abstractions.StoreVideoOptions {
					Directory = "Sample",
					Name = "test.jpg",
					SaveToAlbum = true
				});
				if (file == null)
					return;
				var path = file.Path;
				System.Diagnostics.Debug.WriteLine (path);



				file.Dispose ();
                  
			};


			FindViewById<Button> (Resource.Id.button3).Click += async (sender, args) =>
			{
				var media = new Plugin.Media.MediaImplementation ();
				var file = await Plugin.Media.CrossMedia.Current.PickVideoAsync ();
				if (file == null)
					return;

				var path = file.Path;
				System.Diagnostics.Debug.WriteLine (path);

				file.Dispose ();
			};

		}

		public override void OnRequestPermissionsResult (int requestCode, string[] permissions, Permission[] grantResults)
		{
			Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult (requestCode, permissions, grantResults);
		}
	}
}

