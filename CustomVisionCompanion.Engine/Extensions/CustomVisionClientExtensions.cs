using CustomVisionCompanion.Engine.Models;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomVisionCompanion.Engine.Extensions
{
    public static class CustomVisionClientExtensions
    {
        public static async Task<ImagePredictionResult> PredictImageAsync(this CustomVisionClient client, Guid projectId, Stream image, int width, int height, Guid? iterationId = null)
        {
            using (var output = await ResizeImageAsync(image, width, height))
            {
                var result = await client.PredictImageAsync(projectId, output, iterationId);
                return result;
            }
        }

        private static Task<Stream> ResizeImageAsync(Stream image, int width, int height)
        {
            // Read image from stream
            using (var output = new MagickImage(image))
            {
                // The image will be resized to fit inside the specified size.
                var size = new MagickGeometry(width, height)
                {
                    Greater = true
                };
                output.Resize(size);

                var ms = new MemoryStream();
                output.Write(ms);
                ms.Position = 0;

                return Task.FromResult(ms as Stream);
            }
        }

        //private static Task<Stream> ResizeImageAsync(Stream image, int width, int height)
        //{
        //    // Read image from stream
        //    using (var output = new MagickImage(image))
        //    {
        //        var originalPixelWidth = output.Width;
        //        var originalPixelHeight = output.Height;

        //        if (originalPixelHeight > height && originalPixelWidth > width)
        //        {
        //            var widthRatio = (double)width / originalPixelWidth;
        //            var heightRatio = (double)height / originalPixelHeight;
        //            var aspectHeight = height;
        //            var aspectWidth = width;

        //            if (originalPixelWidth > originalPixelHeight)
        //            {
        //                aspectWidth = (int)(heightRatio * originalPixelWidth);
        //            }
        //            else
        //            {
        //                aspectHeight = (int)(widthRatio * originalPixelHeight);
        //            }

        //            // The image will be resized to fit inside the specified size.
        //            var size = new MagickGeometry(aspectWidth, aspectHeight);
        //            output.Resize(size);
        //        }

        //        var ms = new MemoryStream();
        //        output.Write(ms);
        //        ms.Position = 0;

        //        return Task.FromResult(ms as Stream);
        //    }
        //}
    }
}
