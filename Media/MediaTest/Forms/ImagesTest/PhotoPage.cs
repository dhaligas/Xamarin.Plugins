using System;

using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Core.Services;

namespace ImagesTest
{
	public class PhotoPage : ContentPage
	{
		public PhotoPage ()
		{
			MediaFile file = null;
			var mediaService = DependencyService.Get<IMediaService> ();

			var takePhoto = new Button () {
				Text = "Take Photo"
			};

			var deletePhoto = new Button () {
				Text = "delete Photo"
			};

			deletePhoto.Clicked += (object sender, EventArgs e) =>
			{
				if (file != null)
					mediaService.DeleteImage (file.Path);
			};

			var image = new Image () {
				Aspect = Aspect.AspectFill, 
				HeightRequest = 200, 
				WidthRequest = 200
			};

			takePhoto.Clicked += async (sender, args) =>
			{
				await CrossMedia.Current.Initialize ();

				if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
				{
					await DisplayAlert ("No Camera", ":( No camera available.", "OK");
					return;
				}

				file = await CrossMedia.Current.TakePhotoAsync (new Plugin.Media.Abstractions.StoreCameraMediaOptions {
					Directory = "Sample",
					Name = "test.jpg",
				});

				if (file == null)
					return;

				await DisplayAlert ("File Location", file.Path, "OK");

				var newFileName = await mediaService.CompressAndResizeImageAsync (file.Path, 0, 1000);

				image.Source = ImageSource.FromStream (() =>
				{
					return file.GetStream ();
				});
			};


			Content = new StackLayout { 
				Children = {
					takePhoto, 
					deletePhoto,
					image
				}
			};
		}
	}
}


