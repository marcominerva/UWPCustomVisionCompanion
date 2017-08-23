using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace CustomVisionCompanion.Services
{
    public static class MediaPicker
    {
        public static async Task<Stream> TakePhotoAsync(CameraCaptureUIMaxPhotoResolution resolution)
        {
            var captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.AllowCropping = false;
            captureUI.PhotoSettings.MaxResolution = resolution;

            var photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo != null)
            {
                return await photo.OpenStreamForReadAsync();
            }

            return null;
        }

        public static async Task<Stream> PickPhotoAsync(CameraCaptureUIMaxPhotoResolution resolution)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                int width, height;

                switch (resolution)
                {
                    case CameraCaptureUIMaxPhotoResolution.MediumXga:
                        width = 640;
                        height = 480;
                        break;

                    case CameraCaptureUIMaxPhotoResolution.Large3M:
                    default:
                        width = 1920;
                        height = 1080;
                        break;
                }

                return await ResizeImageAsync(file, width, height);
            }

            return null;
        }

        private static async Task<Stream> ResizeImageAsync(StorageFile sourceFile, int width, int height)
        {
            using (var imageStream = await sourceFile.OpenReadAsync())
            {
                var decoder = await BitmapDecoder.CreateAsync(imageStream);
                var originalPixelWidth = decoder.PixelWidth;
                var originalPixelHeight = decoder.PixelHeight;

                //do resize only if needed
                if (originalPixelHeight > height && originalPixelWidth > width)
                {
                    var resizedStream = new InMemoryRandomAccessStream();

                    //create encoder based on decoder of the source file
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);
                    var widthRatio = (double)width / originalPixelWidth;
                    var heightRatio = (double)height / originalPixelHeight;
                    var aspectHeight = (uint)height;
                    var aspectWidth = (uint)width;

                    if (originalPixelWidth > originalPixelHeight)
                    {
                        aspectWidth = (uint)(heightRatio * originalPixelWidth);
                    }
                    else
                    {
                        aspectHeight = (uint)(widthRatio * originalPixelHeight);
                    }

                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                    encoder.BitmapTransform.ScaledHeight = aspectHeight;
                    encoder.BitmapTransform.ScaledWidth = aspectWidth;

                    await encoder.FlushAsync();
                    return resizedStream.AsStreamForRead();
                }
                else
                {
                    //otherwise just use source file as thumbnail
                    return await sourceFile.OpenStreamForReadAsync();
                }
            }
        }
    }
}
