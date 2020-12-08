using Killboard.API.Security;
using Killboard.Data.Models;
using Killboard.Domain.Hubs;
using Killboard.Domain.Interfaces;
using Killboard.Domain.Repositories;
using Killboard.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Killboard.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<KillboardContext>(options => options.UseSqlServer(Configuration["Killboard:Sql"]));

            services.AddSignalR();

            services.AddTransient<IMailer, Mailer>();
            services.AddTransient<IAuthorizationHandler, ApiKeyRequirementHandler>();

            services.AddAuthorization(authConfig =>
            {
                authConfig.AddPolicy("ApiKeyPolicy",
                    policyBuilder => policyBuilder
                        .AddRequirements(new ApiKeyRequirement(new[] { Configuration["Killboard:ApiKey"]})));
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin", o => o.WithOrigins("https://killboard.space", "https://localhost:44379", "https://tools.killboard.space").AllowAnyHeader());
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICharacterRepository, CharacterRepository>();
            services.AddScoped<IRegionRepository, RegionRepository>();
            services.AddScoped<IConstellationRepository, ConstellationRepository>();
            services.AddScoped<ISystemRepository, SystemRepository>();
            services.AddScoped<IStargateRepository, StargateRepository>();
            services.AddScoped<IPlanetRepository, PlanetRepository>();
            services.AddScoped<IMoonRepository, MoonRepository>();
            services.AddScoped<IAsteroidRepository, AsteroidRepository>();
            services.AddScoped<IStarRepository, StarRepository>();
            services.AddScoped<IAllianceRepository, AllianceRepository>();
            services.AddScoped<ICorporationRepository, CorporationRepository>();
            services.AddScoped<IESIService, ESIService>();

            services.AddSingleton<IKillmailService, KillmailService>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<KillmailHub>("/killmails");
            });
        }
    }
}
