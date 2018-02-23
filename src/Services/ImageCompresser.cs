using System;
using SixLabors.ImageSharp;
using System.IO;

namespace Aiursoft.OSS.Services
{
    public class ImageCompresser
    {
        private readonly char _ = Path.DirectorySeparatorChar;
        public FileStream Compress(string path, string realname, int width, int height)
        {
            var CompressedFolder = Startup.StoragePath + $"{_}Compressed{_}";
            if (Directory.Exists(CompressedFolder) == false)
            {
                Directory.CreateDirectory(CompressedFolder);
            }
            var CompressedImagePath = $"{CompressedFolder}c_w{width}h{height}{realname}";
            GetReducedImage(path, CompressedImagePath, width, height);
            return File.OpenRead(CompressedImagePath);
        }
        public void GetReducedImage(string sourceImage, string saveTarget, int width, int height)
        {
            var image = Image.Load(sourceImage);
            image.Mutate(x => x
                .Resize(width, height));
            image.Save(saveTarget);
        }
    }
}