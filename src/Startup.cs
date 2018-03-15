using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Aiursoft.OSS.Data;
using Aiursoft.OSS.Services;
using Aiursoft.Pylon;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;

namespace Aiursoft.OSS
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public static string StoragePath { get; set; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
            });
            services.AddDbContext<OSSDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DatabaseConnection")));

            services.AddMvc();
            services.AddSingleton<IHostedService, TimedCleaner>();
            services.AddTransient<ImageCompresser>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, OSSDbContext dbContext)
        {
            StoragePath = Configuration[nameof(StoragePath)];
            if(string.IsNullOrWhiteSpace(StoragePath))
            {
                throw new InvalidOperationException("Did not find a valid storage path!");
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseEnforceHttps();
            }

            app.UseStaticFiles();
            app.UseLanguageSwitcher();
            app.UseMvcWithDefaultRoute();
        }
    }
}
