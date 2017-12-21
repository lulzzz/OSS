using Aiursoft.OSS.Data;
using Aiursoft.OSS.Models.DownloadAddressModels;
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
        public async Task<IActionResult> DownloadFile(DownloadFileAddressModel model)
        {
            var targetBucket = await _dbContext.Bucket.SingleOrDefaultAsync(t => t.BucketName == model.BucketName);
            var targetFile = await _dbContext
                .OSSFile
                .Where(t => t.BucketId == targetBucket.BucketId)
                .SingleOrDefaultAsync(t => t.RealFileName == model.FileName + "." + model.FileExtension);

            if (targetBucket == null || targetFile == null || !targetBucket.OpenToRead)
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
                if (!string.IsNullOrWhiteSpace(model.sd) || !MIME.MIMETypesDictionary.ContainsKey(model.FileExtension.ToLower()))
                {
                    return new FileContentResult(file, "application/octet-stream");
                }
                // Is image and compress required
                else if (StringOperation.IsImage(targetFile.RealFileName) && model.h > 0 && model.w > 0)
                {
                    return new FileContentResult(_imageCompresser.Compress(path, targetFile.RealFileName, model.w, model.h), MIME.MIMETypesDictionary[model.FileExtension.ToLower()]);
                }
                // Is known type
                else
                {
                    return new FileContentResult(file, MIME.MIMETypesDictionary[model.FileExtension.ToLower()]);
                }
            }
            catch (Exception e) when (e is DirectoryNotFoundException || e is FileNotFoundException)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> FromSecret(FromSecretAddressModel model)
        {
            var secret = await _dbContext
                .Secrets
                .Include(t => t.File)
                .SingleOrDefaultAsync(t => t.Value == model.sec);
            if (secret == null || secret.Used)
            {
                return NotFound();
            }
            secret.Used = true;
            secret.UseTime = DateTime.Now;
            secret.UserIpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            await _dbContext.SaveChangesAsync();
            var bucket = await _dbContext
                .Bucket
                .SingleOrDefaultAsync(t => t.BucketId == secret.File.BucketId);

            var path = GetCurrentDirectory() + $"{_}Storage{_}{bucket.BucketName}{_}{secret.File.FileKey}.dat";
            try
            {
                var file = System.IO.File.ReadAllBytes(path);
                HttpContext.Response.Headers.Add("Content-Length", new FileInfo(path).Length.ToString());
                HttpContext.Response.Headers.Add("cache-control", "max-age=3600");
                // Direct download marked or unknown type
                if (!string.IsNullOrWhiteSpace(model.sd) || !MIME.MIMETypesDictionary.ContainsKey(secret.File.FileExtension.Trim('.').ToLower()))
                {
                    return new FileContentResult(file, "application/octet-stream");
                }
                // Is image and compress required
                else if (StringOperation.IsImage(secret.File.RealFileName) && model.h > 0 && model.w > 0)
                {
                    return new FileContentResult(_imageCompresser.Compress(path, secret.File.RealFileName, model.w, model.h), MIME.MIMETypesDictionary[secret.File.FileExtension.Trim('.').ToLower()]);
                }
                // Is known type
                else
                {
                    return new FileContentResult(file, MIME.MIMETypesDictionary[secret.File.FileExtension.Trim('.').ToLower()]);
                }
            }
            catch (Exception e) when (e is DirectoryNotFoundException || e is FileNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
