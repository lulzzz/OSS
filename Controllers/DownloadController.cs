using Aiursoft.OSS.Data;
using Aiursoft.OSS.Services;
using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.IO.Directory;

namespace Aiursoft.OSS.Controllers
{
    [AiurRequireHttps]
    [ForceValidateModelState]
    public class DownloadController : AiurController
    {
        private readonly char _ = Path.DirectorySeparatorChar;
        private readonly OSSDbContext _dbContext;
        private readonly ImageCompresser _imageCompresser;
        public DownloadController(
            OSSDbContext dbContext,
            ImageCompresser imageCompresser)
        {
            this._dbContext = dbContext;
            this._imageCompresser = imageCompresser;
        }
        [Route(template: "/{BucketName}/{FileName}.{FileExtension}")]
        public async Task<IActionResult> DownloadFile(string BucketName, string FileName, string FileExtension, string sd = "", [Range(-2, 10000)]int w = -1, [Range(-2, 10000)]int h = -1)
        {
            var targetBucket = await _dbContext.Bucket.SingleOrDefaultAsync(t => t.BucketName == BucketName);
            var targetFile = await _dbContext
                .OSSFile
                .Where(t => t.BucketId == targetBucket.BucketId)
                .SingleOrDefaultAsync(t => t.RealFileName == FileName + "." + FileExtension);

            if (targetBucket == null || targetFile == null)
                return NotFound();
            if (targetFile.BucketId != targetBucket.BucketId)
                return Unauthorized();

            // Update download times
            targetFile.DownloadTimes++;
            await _dbContext.SaveChangesAsync();

            var path = GetCurrentDirectory() + $"{_}Storage{_}{targetBucket.BucketName}{_}{targetFile.FileKey}.dat";
            try
            {
                var file = System.IO.File.ReadAllBytes(path);
                HttpContext.Response.Headers.Add("Content-Length", new FileInfo(path).Length.ToString());
                HttpContext.Response.Headers.Add("cache-control", "max-age=3600");
                // Direct download marked or unknown type
                if (!string.IsNullOrWhiteSpace(sd) || !MIME.MIMETypesDictionary.ContainsKey(FileExtension.ToLower()))
                {
                    return new FileContentResult(file, "application/octet-stream");
                }
                // Is image and compress required
                else if (StringOperation.IsImage(targetFile.RealFileName) && h > 0 && w > 0)
                {
                    return new FileContentResult(_imageCompresser.Compress(path, targetFile.RealFileName, w, h), MIME.MIMETypesDictionary[FileExtension.ToLower()]);
                }
                // Is known type
                else
                {
                    return new FileContentResult(file, MIME.MIMETypesDictionary[FileExtension.ToLower()]);
                }
            }
            catch (Exception e) when (e is DirectoryNotFoundException || e is FileNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
