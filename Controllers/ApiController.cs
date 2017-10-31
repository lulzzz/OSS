using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using static System.IO.Directory;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToAPIServer;
using Aiursoft.OSS.Data;
using Aiursoft.OSS.Models;
using Aiursoft.Pylon.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Models.OSS.ApiViewModels;
using Aiursoft.Pylon.Models.OSS;
using Aiursoft.Pylon.Models.API.ApiViewModels;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models.OSS.ApiAddressModels;
using Aiursoft.Pylon;
using Aiursoft.OSS.Services;

namespace Aiursoft.OSS.Controllers
{
    [AiurExceptionHandler]
    [ForceValidateModelState]
    [AiurRequireHttps]
    public class ApiController : AiurApiController
    {
        private readonly char _ = Path.DirectorySeparatorChar;
        private readonly OSSDbContext _dbContext;
        private readonly ImageCompresser _imageCompresser;
        public ApiController(
            OSSDbContext dbContext,
            ImageCompresser imageCompresser)
        {
            this._dbContext = dbContext;
            this._imageCompresser = imageCompresser;
        }

        [HttpPost]
        public async Task<JsonResult> DeleteApp(DeleteAppAddressModel model)
        {
            var app = await ApiService.ValidateAccessTokenAsync(model.AccessToken);
            if (app.AppId != model.AppId)
            {
                return Protocal(ErrorType.Unauthorized, "The app you try to delete is not the accesstoken you granted!");
            }

            var target = await _dbContext.Apps.FindAsync(app.AppId);
            if (target != null)
            {
                _dbContext.OSSFile.RemoveRange(_dbContext.OSSFile.Include(t => t.BelongingBucket).Where(t => t.BelongingBucket.BelongingAppId == target.AppId));
                _dbContext.Bucket.Delete(t => t.BelongingAppId == target.AppId);
                _dbContext.Apps.Remove(target);
                await _dbContext.SaveChangesAsync();
                return Protocal(ErrorType.Success, "Successfully deleted that app and all files.");
            }
            return Protocal(ErrorType.HasDoneAlready, "That app do not exists in our database.");
        }

        public async Task<JsonResult> ViewMyBuckets(ViewMyBucketsAddressModel model)
        {
            var app = await ApiService.ValidateAccessTokenAsync(model.AccessToken);
            var appLocal = await _dbContext.Apps.SingleOrDefaultAsync(t => t.AppId == app.AppId);
            if (appLocal == null)
            {
                appLocal = new OSSApp
                {
                    AppId = app.AppId,
                    MyBuckets = new List<Bucket>()
                };
                _dbContext.Apps.Add(appLocal);
                await _dbContext.SaveChangesAsync();
            }

            var buckets = await _dbContext
                .Bucket
                .Include(t => t.Files)
                .Where(t => t.BelongingAppId == app.AppId)
                .ToListAsync();
            buckets.ForEach(t => t.FileCount = t.Files.Count());
            var viewModel = new ViewMyBucketsViewModel
            {
                AppId = appLocal.AppId,
                Buckets = buckets,
                code = ErrorType.Success,
                message = "Successfully get your buckets!"
            };
            return Json(viewModel);
        }

        [HttpPost]
        public async Task<JsonResult> CreateBucket([FromForm]CreateBucketAddressModel model)
        {
            //Update app info
            var app = await ApiService.ValidateAccessTokenAsync(model.AccessToken);
            var appLocal = await _dbContext.Apps.Include(t => t.MyBuckets).SingleOrDefaultAsync(t => t.AppId == app.AppId);
            if (appLocal == null)
            {
                appLocal = new OSSApp
                {
                    AppId = app.AppId,
                    MyBuckets = new List<Bucket>()
                };
                _dbContext.Apps.Add(appLocal);
            }
            //Ensure not exists
            var existing = await _dbContext.Bucket.SingleOrDefaultAsync(t => t.BucketName == model.BucketName);
            if (existing != null)
            {
                return Protocal(ErrorType.NotEnoughResources, "There is one bucket already called that name!");
            }
            //Create and save to database
            var newBucket = new Bucket
            {
                BucketName = model.BucketName,
                Files = new List<OSSFile>(),
                OpenToRead = model.OpenToRead,
                OpenToUpload = model.OpenToUpload
            };
            appLocal.MyBuckets.Add(newBucket);
            await _dbContext.SaveChangesAsync();
            //Create an empty folder
            string DirectoryPath = GetCurrentDirectory() + $@"{_}Storage{_}{newBucket.BucketName}{_}";
            if (Directory.Exists(DirectoryPath) == false)
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            //return model
            var viewModel = new CreateBucketViewModel
            {
                BucketId = newBucket.BucketId,
                code = ErrorType.Success,
                message = "Successfully created your bucket!"
            };
            return Json(viewModel);
        }

        [HttpPost]
        public async Task<JsonResult> EditBucket([FromForm]EditBucketAddressModel model)
        {
            var app = await ApiService.ValidateAccessTokenAsync(model.AccessToken);
            var existing = _dbContext.Bucket.Exists(t => t.BucketName == model.NewBucketName && t.BucketId != model.BucketId);
            if (existing)
            {
                return Protocal(ErrorType.NotEnoughResources, "There is one bucket already called that name!");
            }
            var target = await _dbContext.Bucket.FindAsync(model.BucketId);
            if (target == null)
            {
                return Protocal(ErrorType.NotFound, "Not found target bucket!");
            }
            else if (target.BelongingAppId != app.AppId)
            {
                return Protocal(ErrorType.Unauthorized, "This is not your bucket!");
            }
            var oldpath = GetCurrentDirectory() + $@"{_}Storage{_}{target.BucketName}";
            var newpath = GetCurrentDirectory() + $@"{_}Storage{_}{model.NewBucketName}";
            if (oldpath != newpath)
            {
                new DirectoryInfo(oldpath).MoveTo(newpath);
            }
            target.BucketName = model.NewBucketName;
            target.OpenToRead = model.OpenToRead;
            target.OpenToUpload = model.OpenToUpload;
            await _dbContext.SaveChangesAsync();
            return Protocal(ErrorType.Success, "Successfully edited your bucket!");
        }

        public async Task<JsonResult> ViewBucketDetail(ViewBucketDetailAddressModel model)
        {
            var targetBucket = await _dbContext.Bucket.FindAsync(model.BucketId);
            if (targetBucket == null)
            {
                return Protocal(ErrorType.NotFound, "Can not find target bucket!");
            }
            var viewModel = new ViewBucketViewModel(targetBucket)
            {
                code = ErrorType.Success,
                message = "Successfully get your bucket info!",
                FileCount = await _dbContext.OSSFile.Where(t => t.BucketId == targetBucket.BucketId).CountAsync()
            };
            return Json(viewModel);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteBucket([FromForm]DeleteBucketAddressModel model)
        {
            var app = await ApiService.ValidateAccessTokenAsync(model.AccessToken);
            var bucket = await _dbContext.Bucket.FindAsync(model.BucketId);
            if (bucket.BelongingAppId != app.AppId)
            {
                return Protocal(ErrorType.Unauthorized, "The bucket you try to delete is not your app's bucket!");
            }
            _dbContext.Bucket.Remove(bucket);
            _dbContext.OSSFile.RemoveRange(_dbContext.OSSFile.Where(t => t.BucketId == bucket.BucketId));
            await _dbContext.SaveChangesAsync();
            return Protocal(ErrorType.Success, "Successfully deleted your bucket!");
        }

        public async Task<JsonResult> ViewOneFile(ViewOneFileAddressModel model)
        {
            var file = await _dbContext
                .OSSFile
                .Include(t => t.BelongingBucket)
                .SingleOrDefaultAsync(t => t.FileKey == model.FileKey);

            var path = GetCurrentDirectory() + $@"{_}Storage{_}{file.BelongingBucket.BucketName}{_}{file.FileKey}.dat";
            file.JFileSize = new FileInfo(path).Length;

            var viewModel = new ViewOneFileViewModel
            {
                File = file,
                code = ErrorType.Success,
                message = "Successfully get that file!"
            };
            return Json(viewModel);
        }

        [HttpPost]
        public async Task<JsonResult> UploadFile(CommonAddressModel model)
        {
            var app = await ApiService.ValidateAccessTokenAsync(model.AccessToken);
            //try find the target bucket
            var targetBucket = await _dbContext.Bucket.FindAsync(model.BucketId);
            if (targetBucket == null || targetBucket.BelongingAppId != app.AppId)
            {
                return Protocal(ErrorType.Unauthorized,"The bucket you try to upload is not your app's bucket!");
            }
            //try get the file from form
            var file = Request.Form.Files.First();
            if (file == null)
            {
                return Protocal(ErrorType.InvalidInput, "Please upload your file!");
            }
            var newFile = new OSSFile
            {
                RealFileName = Path.GetFileName(file.FileName.Replace(" ", "")),
                FileExtension = Path.GetExtension(file.FileName),
                BucketId = targetBucket.BucketId,
            };
            // Ensure there not exists file with the same file name.
            var exists = _dbContext.OSSFile.Exists(t => t.RealFileName == newFile.RealFileName && t.BucketId == newFile.BucketId);
            if (exists)
            {
                return Protocal(ErrorType.HasDoneAlready, "There already exists a file with that name.");
            }
            //Save to database
            _dbContext.OSSFile.Add(newFile);
            await _dbContext.SaveChangesAsync();
            //Try saving file.
            string DirectoryPath = GetCurrentDirectory() + $"{_}Storage{_}{targetBucket.BucketName}{_}";
            if (Exists(DirectoryPath) == false)
            {
                CreateDirectory(DirectoryPath);
            }
            var fileStream = new FileStream(DirectoryPath + newFile.FileKey + ".dat", FileMode.Create);
            await file.CopyToAsync(fileStream);
            fileStream.Close();
            //Return json
            return Json(new UploadFileViewModel
            {
                code = ErrorType.Success,
                FileKey = newFile.FileKey,
                message = "Successfully uploaded your file.",
                Path = newFile.GetInternetPath
            });
        }

        public async Task<JsonResult> ViewAllFiles(CommonAddressModel model)
        {
            //Analyse app
            var app = await ApiService.ValidateAccessTokenAsync(model.AccessToken);
            var bucket = await _dbContext.Bucket.FindAsync(model.BucketId);
            //Security
            if (bucket.BelongingAppId != app.AppId)
            {
                return Protocal(ErrorType.Unauthorized, "The bucket you tried to view is not that app's bucket.");
            }
            //Get all files.
            var allFiles = _dbContext.OSSFile.Include(t => t.BelongingBucket).Where(t => t.BucketId == bucket.BucketId).Take(200);
            foreach (var file in allFiles)
            {
                var path = GetCurrentDirectory() + $@"{_}Storage{_}{file.BelongingBucket.BucketName}{_}{file.FileKey}.dat";
                file.JFileSize = new FileInfo(path).Length;
            }
            var viewModel = new ViewAllFilesViewModel
            {
                AllFiles = allFiles,
                BucketId = bucket.BucketId,
                message = "Successfully get all your files of that bucket.",
                code = ErrorType.Success
            };
            return Json(viewModel);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteFile(DeleteFileAddressModel model)
        {
            //Analyse app
            var app = await ApiService.ValidateAccessTokenAsync(model.AccessToken);
            var bucket = await _dbContext.Bucket.FindAsync(model.BucketId);
            var file = await _dbContext.OSSFile.FindAsync(model.FileKey);
            if (bucket == null || file == null)
            {
                return Protocal(ErrorType.NotFound,  "We did not find that file in that bucket!");
            }
            //Security
            if (bucket.BelongingAppId != app.AppId)
            {
                return Protocal(ErrorType.Unauthorized,  "The bucket you tried is not that app's bucket.");
            }
            if (file.BucketId != bucket.BucketId)
            {
                return Protocal(ErrorType.Unauthorized, "The file and the bucket are both found but it is not in that bucket.");
            }
            //Delete file in disk
            var path = GetCurrentDirectory() + $@"{_}Storage{_}{bucket.BucketName}{_}{file.FileKey}.dat";
            System.IO.File.Delete(path);
            //Delete file in database
            _dbContext.OSSFile.Remove(file);
            await _dbContext.SaveChangesAsync();
            return Protocal(ErrorType.Success, "Successfully deleted your file!");
        }
    }
}