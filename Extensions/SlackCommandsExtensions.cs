using gamemaster.CommandHandlers;
using gamemaster.Services;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster.Extensions
{
    public static class SlackCommandsExtensions
    {
        public static void AddSlackSlashCommands(this IServiceCollection services)
        {
            services.AddSingleton<EmissionRequestHandler>();
            services.AddSingleton<TossACoinHandler>();
            services.AddSingleton<BalanceRequestHandler>();
            services.AddHttpClient<SlackResponseService>();
        }
    }
}