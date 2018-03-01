using System;
using SixLabors.ImageSharp;
using System.IO;
using System.Threading.Tasks;

namespace Aiursoft.OSS.Services
{
    public class ImageCompresser
    {
        private readonly char _ = Path.DirectorySeparatorChar;
        public async Task<string> Compress(string path, string realname, int width, int height)
        {
            var CompressedFolder = Startup.StoragePath + $"{_}Compressed{_}";
            if (Directory.Exists(CompressedFolder) == false)
            {
                Directory.CreateDirectory(CompressedFolder);
            }
            var CompressedImagePath = $"{CompressedFolder}c_w{width}h{height}{realname}";
            await GetReducedImage(path, CompressedImagePath, width, height);
            return CompressedImagePath;
        }
        public async Task GetReducedImage(string sourceImage, string saveTarget, int width, int height)
        {
            await Task.Run(new Action(() =>
            {
                var image = Image.Load(sourceImage);
                image.Mutate(x => x
                    .Resize(width, height));
                image.Save(saveTarget);
            }));
        }
    }
}