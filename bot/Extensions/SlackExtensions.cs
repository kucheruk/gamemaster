using gamemaster.CommandHandlers;
using gamemaster.CommandHandlers.Ledger;
using gamemaster.CommandHandlers.Tote;
using gamemaster.Services;
using gamemaster.Slack;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster.Extensions
{
    public static class SlackExtensions
    {
        public static void AddSlack(this IServiceCollection services)
        {
            services.AddSingleton<PlaceBetInteractionHandler>();
            services.AddSingleton<SlackApiWrapper>();
            services.AddSingleton<EmissionRequestHandler>();
            services.AddSingleton<TossACoinHandler>();
            services.AddSingleton<BalanceRequestHandler>();
            services.AddSingleton<ToteRequestHandler>();
            services.AddHttpClient<SlackResponseService>();
        }
    }
}