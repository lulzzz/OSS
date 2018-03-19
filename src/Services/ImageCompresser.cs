using System;
using SixLabors.ImageSharp;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Aiursoft.OSS.Services
{
    public class ImageCompresser
    {
        private readonly IConfiguration _configuration;

        public ImageCompresser(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> Compress(string path, string realname, int width, int height)
        {
            var CompressedFolder = _configuration["StoragePath"] + $"{Path.DirectorySeparatorChar}Compressed{Path.DirectorySeparatorChar}";
            if (Directory.Exists(CompressedFolder) == false)
            {
                Directory.CreateDirectory(CompressedFolder);
            }
            var CompressedImagePath = $"{CompressedFolder}oss_compressed_w{width}h{height}{realname}";
            await GetReducedImage(path, CompressedImagePath, width, height);
            return CompressedImagePath;
        }
        public async Task GetReducedImage(string sourceImage, string saveTarget, int width, int height)
        {
            var sourceFileInfo = new FileInfo(sourceImage);
            if (File.Exists(saveTarget))
            {
                if (new FileInfo(saveTarget).LastWriteTime > sourceFileInfo.LastWriteTime)
                {
                    return;
                }
            }
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