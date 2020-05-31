using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster
{
    public static class SlackCommandsExtensions
    {
        public static void AddSlackSlashCommands(this IServiceCollection services)
        {
            services.AddSingleton<EmissionRequestHandler>();
            services.AddSingleton<BalanceRequestHandler>();
            services.AddHttpClient<SlackResponseService>();
        }
    }
}