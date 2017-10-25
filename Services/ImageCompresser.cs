using System;
using SixLabors.ImageSharp;

namespace Aiursoft.OSS.Services
{
    public class ImageCompresser
    {
        public byte[] Compress(string path)
        {
            
        }
        public void GetReducedImage(string sourceImage, string saveTarget, int Width, int Height)
        {
            var image = Image.Load(sourceImage);
            image.Mutate(x => x
                .Resize(200, 200)
                .Grayscale());
            var targetStream = System.IO.File.Create(saveTarget);
            image.SaveAsPng(targetStream);
        }
    }
}