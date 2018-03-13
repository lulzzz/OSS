using Aiursoft.OSS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aiursoft.OSS.Services
{
    public class TimedCleaner : IHostedService, IDisposable
    {
        private IConfiguration Configuration { get; }
        private readonly ILogger _logger;
        private Timer _timer;
        private OSSDbContext _dbContext;
        private readonly char _ = Path.DirectorySeparatorChar;

        public TimedCleaner(
            IConfiguration configuration,
            ILogger<TimedCleaner> logger,
            OSSDbContext dbContext)
        {
            Configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            await AllClean();
        }

        private async Task AllClean()
        {
            var outdatedFiles = (await _dbContext.OSSFile.Include(t => t.BelongingBucket).ToListAsync())
                .Where(t => t.UploadTime + new TimeSpan(t.AliveDays, 0, 0, 0) < DateTime.Now)
                .ToList();
            foreach (var file in outdatedFiles)
            {
                var path = $@"{Configuration["StoragePath"]}{_}Storage{_}{file.BelongingBucket.BucketName}{_}{file.FileKey}.dat";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                _dbContext.OSSFile.Remove(file);
            }
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully cleaned all trash.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
