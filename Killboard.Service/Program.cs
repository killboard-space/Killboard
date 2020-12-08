using Azure.Identity;
using Killboard.Data.Models;
using Killboard.Domain.Interfaces;
using Killboard.Domain.Services;
using Killboard.Service.Services;
using Killboard.Service.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Killboard.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    if (hostContext.HostingEnvironment.IsProduction())
                    {
                        var settings = config.Build();
                        config.AddAzureAppConfiguration(options =>
                        {
                            options.Connect(settings["ConnectionStrings:AppConfig"])
                                .ConfigureKeyVault(kv =>
                                {
                                    kv.SetCredential(new DefaultAzureCredential());
                                });
                        });
                    }

                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        config.AddUserSecrets<Program>();
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<KillboardContext>(options => options.UseSqlServer(hostContext.Configuration["Killboard:Sql"]));

                    services.AddTransient<IUserService, UserService>();
                    services.AddSingleton<AllianceQueue>();
                    services.AddSingleton<AttackerQueue>();
                    services.AddSingleton<CharacterQueue>();
                    services.AddSingleton<CorporationQueue>();
                    services.AddSingleton<DroppedItemQueue>();
                    services.AddSingleton<KillboardQueue>();
                    services.AddSingleton<RefreshTokenQueue>();
                    services.AddSingleton<VictimQueue>();
                    services.AddSingleton<KillmailTableSubscription>();
                    services.AddHostedService<KillmailWorker>();
                });
    }
}
