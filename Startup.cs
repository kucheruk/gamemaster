using gamemaster.Actors;
using gamemaster.Config;
using gamemaster.Extensions;
using gamemaster.Services;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.Configure<SlackConfig>(Configuration.GetSection("Slack"));
            services.Configure<MongoConfig>(Configuration.GetSection("Mongo"));
            services.AddMongoStorage();
            services.AddLedger();
            services.AddTote();
            services.AddAppState();
            services.AddSlack();
            services.AddSingleton<MessageRouter>();
            services.AddSingleton<SlackRequestSignature>();
            services.AddTransient<GamemasterSupervisor>();
            services.AddTransient<UserContextsActor>();
            services.AddTransient<UserToteContextActor>();
            services.AddSingleton<IHostedService, ActorsHostService>();
            services.AddTransient<DbMaintenanceService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseMiddleware<DebugLoggingMiddleware>();
            app.UseMiddleware<JsonApiMiddleware>();
            // app.UseRouting();
            // app.UseAuthorization();
            // app.UseAuthentication();
            // app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}