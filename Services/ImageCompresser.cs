using System;
using SixLabors.ImageSharp;
using System.IO;
using static System.IO.Directory;

namespace Aiursoft.OSS.Services
{
    public class ImageCompresser
    {
        private readonly char _ = Path.DirectorySeparatorChar;
        public byte[] Compress(string path, string realname, int width, int height)
        {
            var CompressedFolder = GetCurrentDirectory() + $"{_}Storage{_}_Compressed{_}";
            if (Exists(CompressedFolder) == false)
            {
                CreateDirectory(CompressedFolder);
            }

            var realImagePath = CompressedFolder + realname;
            File.Copy(path, realImagePath, true);
            
            var CompressedImagePath = $"{CompressedFolder}c_w{width}h{height}{realname}";
            GetReducedImage(realImagePath, CompressedImagePath, width, height);
            return System.IO.File.ReadAllBytes(CompressedImagePath);
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