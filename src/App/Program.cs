using Core;
using Core.Data;
using Core.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;

namespace App
{
    public class Program
    {
        private static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<AppDbContext>();

                try
                {
                    context.Database.Migrate();
                }
                catch { }

                // load application settings from appsettings.json
                var app = services.GetRequiredService<IAppService<AppItem>>();
                AppConfig.SetSettings(app.Value);
                var userMgr = (UserManager<AppUser>)services.GetRequiredService(typeof(UserManager<AppUser>));
                if (!userMgr.Users.Any(a => a.UserName == "admin"))
                {
                    userMgr.CreateAsync(new AppUser { UserName = "admin", Email = "admin@us.com" }, "@dm1n").Wait();
                    context.Authors.Add(new Author
                    {
                        AppUserName = "admin",
                        Email = "admin@us.com",
                        DisplayName = "Administrator",
                        Avatar = "data/admin/avatar.png",
                        Bio = "<p>Something about <b>administrator</b>, maybe HTML or markdown formatted text goes here.</p><p>Should be customizable and editable from user profile.</p>",
                        IsAdmin = true,
                        Created = DateTime.UtcNow.AddDays(-120)
                    });
                    context.SaveChanges();
                }

                if (app.Value.SeedData)
                {
                    if (!userMgr.Users.Any(a => a.UserName == "demo"))
                    {
                        userMgr.CreateAsync(new AppUser { UserName = "demo", Email = "demo@us.com" }, "demo").Wait();
                    }

                    if (!context.BlogPosts.Any())
                    {
                        try
                        {
                            services.GetRequiredService<IStorageService>().Reset();
                        }
                        catch { }

                        AppData.Seed(context);
                    }
                }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

        public static void Shutdown()
        {
            cancelTokenSource.Cancel();
        }
    }
}