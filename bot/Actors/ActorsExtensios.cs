using gamemaster.Extensions;
using gamemaster.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gamemaster.Actors
{
    public static class ActorsExtensios
    {
        public static void AddActors(this IServiceCollection services)
        {
            services.AddSingleton<TotesActor>();
            services.AddSingleton<MessengerActor>();
            services.AddTransient<GamemasterSupervisor>();
            services.AddTransient<UserContextsActor>();
            services.AddTransient<UserToteContextActor>();
            services.AddSingleton<IHostedService, ActorsHostService>();
        }
    }
}