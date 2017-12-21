using Aiursoft.OSS.Data;
using Aiursoft.OSS.Models;
using Aiursoft.Pylon;
using Aiursoft.Pylon.Services.ToAPIServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Models;

namespace Aiursoft.OSS.Controllers
{
    public class SecretController : AiurController
    {
        private readonly OSSDbContext _dbContext;
        public SecretController(
            OSSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Generate(int id, string accessToken)
        {
            var app = await ApiService.ValidateAccessTokenAsync(accessToken);
            var appLocal = await _dbContext.Apps.SingleOrDefaultAsync(t => t.AppId == app.AppId);
            var file = await _dbContext.OSSFile.Include(t => t.BelongingBucket).SingleOrDefaultAsync(t => t.FileKey == id);
            if (file == null || file.BelongingBucket.BelongingAppId != appLocal.AppId)
            {
                return NotFound();
            }
            // Generate secret
            var newSecret = new Secret
            {
                Value = StringOperation.RandomString(15),
                FileId = file.FileKey
            };
            _dbContext.Secrets.Add(newSecret);
            return Protocal(ErrorType.Success, newSecret.Value);
        }
    }
}
