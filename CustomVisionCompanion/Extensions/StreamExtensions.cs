using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CustomVisionCompanion.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<SoftwareBitmap> AsSoftwareBitmapAsync(this Stream stream)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            var softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            return softwareBitmapBGR8;
        }

        public static async Task<ImageSource> AsImageSourceAsync(this SoftwareBitmap softwareBitmap)
        {
            var bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(softwareBitmap);

            return bitmapSource;
        }

        public static async Task<byte[]> ToArrayAsync(this Stream stream)
        {
            stream.Position = 0;

            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

        public static string ToFileSize(this int value)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            for (var i = 0; i < suffixes.Length; i++)
            {
                if (value <= (Math.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(value / Math.Pow(1024, i)) + " " + suffixes[i];
                }
            }

            return ThreeNonZeroDigits(value / Math.Pow(1024, suffixes.Length - 1)) + " " + suffixes[suffixes.Length - 1];

            // Return the value formatted to include at most three
            // non-zero digits and at most two digits after the
            // decimal point. Examples:
            //         1
            //       123
            //        12.3
            //         1.23
            //         0.12
            string ThreeNonZeroDigits(double number)
            {
                if (number >= 100)
                {
                    // No digits after the decimal.
                    return number.ToString("0,0");
                }
                else if (number >= 10)
                {
                    // One digit after the decimal.
                    return number.ToString("0.0");
                }
                else
                {
                    // Two digits after the decimal.
                    return number.ToString("0.00");
                }
            }
        }
    }
}
