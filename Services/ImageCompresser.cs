using System;
using SixLabors.ImageSharp;
using System.IO;
using static System.IO.Directory;

namespace Aiursoft.OSS.Services
{
    public class ImageCompresser
    {
        private readonly char _ = Path.DirectorySeparatorChar;
        public byte[] Compress(string path, string realname)
        {
            var realImagePath = GetCurrentDirectory() + $"{_}Storage{_}_Compressed{_}{realname}";
            File.Copy(path, realImagePath);
            var CompressedImagePath = GetCurrentDirectory() + $"{_}Storage{_}_Compressed{_}c_{realname}";
            GetReducedImage(realImagePath, "", 200, 200);
            return System.IO.File.ReadAllBytes(CompressedImagePath);
        }
        public void GetReducedImage(string sourceImage, string saveTarget, int Width, int Height)
        {
            var image = Image.Load(sourceImage);
            image.Mutate(x => x
                .Resize(200, 200)
                .Grayscale());
            image.Save(saveTarget);
        }
    }
}