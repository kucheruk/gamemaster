using gamemaster.Commands;
using gamemaster.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster.Extensions
{
    public static class AppStateExtensions
    {
        public static void AddAppState(this IServiceCollection services)
        {
            services.AddSingleton<GetAppStateQuery>();
            services.AddSingleton<SetAppStateCommand>();
        }
    }
}