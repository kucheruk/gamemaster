using gamemaster.CommandHandlers.Ledger;
using gamemaster.CommandHandlers.Tote;
using gamemaster.Services;
using gamemaster.Slack;
using gamemaster.Slack.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster.Extensions
{
    public static class SlackExtensions
    {
        public static void AddSlack(this IServiceCollection services)
        {
            services.AddSingleton<SlackApiWrapper>();
            services.AddSingleton<SlackJsonHandler, SlackUrlVerificationHandler>();
            services.AddSingleton<SlackJsonHandler, SlackMessageEmptyHandler>();
            services.AddSingleton<SlackJsonHandler, SlackEventCallbackHandler>();
            
            services.AddSingleton<SlackFormHandler, SlackCommandFormHandler>();
            services.AddSingleton<SlackFormHandler, SlackInteractionFormHandler>();
            
            services.AddSingleton<SlackInteractionActionHandler, SlackFinishToteInteractionHandler>();
            services.AddSingleton<SlackInteractionActionHandler, SlackPlaceBetStartInteractionHandler>();
            services.AddSingleton<SlackInteractionActionHandler, SlackPlaceBetInteractionHandler>();
            
            
            services.AddSingleton<EmissionRequestHandler>();
            services.AddSingleton<TossACoinHandler>();
            services.AddSingleton<BalanceRequestHandler>();
            services.AddSingleton<ToteRequestHandler>();
            services.AddHttpClient<SlackResponseService>();
        }
    }
}