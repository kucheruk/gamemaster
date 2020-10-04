using gamemaster.Actors;
using gamemaster.Config;
using gamemaster.Extensions;
using gamemaster.Services;
using gamemaster.Slack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gamemaster
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // TODO: Deploy with docket contexts
        // TODO: Default currency = :hack:
        // TODO: Get config
        // TODO: Add Promo Code
        // TODO: Enter Promo Code
        // TODO: Handle Promo Code
        // TODO: Mirror messages to announcements channel
        // TODO: Total Balance with single currency
        // TODO: Help links
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppConfig>(Configuration.GetSection("App"));
            services.Configure<SlackConfig>(Configuration.GetSection("Slack"));
            services.Configure<MongoConfig>(Configuration.GetSection("Mongo"));
            services.AddMongoStorage();
            services.AddLedger();
            services.AddTote();
            services.AddAppState();
            services.AddSlack();
            services.AddActors();
            services.AddTransient<DbMaintenanceService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<CheckSlackSignatureMiddleware>();
            app.UseMiddleware<JsonApiMiddleware>();
        }
    }
}