using gamemaster.CommandHandlers;
using gamemaster.CommandHandlers.Tote;
using gamemaster.Commands;
using gamemaster.Queries.Tote;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster.Extensions
{
    public static class PromoExtensions
    {
        public static void AddPromo(this IServiceCollection services)
        {
            services.AddSingleton<ITextCommandHandler, PromoAddTextCommandHandler>();
            services.AddSingleton<ITextCommandHandler, PromoListTextCommandHandler>();
            services.AddSingleton<ITextCommandHandler, PromoListFullTextCommandHandler>();
            services.AddSingleton<PromoCodeFindQuery>();
            services.AddSingleton<PromoActivateCommand>();
            services.AddSingleton<PromoAddCommand>();
            services.AddSingleton<PromoListQuery>();
        }
    }
}