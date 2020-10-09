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

        // DONE: Deploy with docket contexts
        // DONE: Default currency = :hack:
        // DONE: Add Promo Code
        // DONE: Enter Promo Code
        // DONE: Handle Promo Code
        // DONE: Mirror messages to announcements channel
        // DONE: Total Balance with single currency
        // TODO: Help links
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppConfig>(Configuration.GetSection("App"));
            services.Configure<SlackConfig>(Configuration.GetSection("Slack"));
            services.Configure<MongoConfig>(Configuration.GetSection("Mongo"));
            services.AddMongoStorage();
            services.AddLedger();
            services.AddTote();
            services.AddPromo();
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