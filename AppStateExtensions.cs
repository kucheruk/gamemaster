using Microsoft.Extensions.DependencyInjection;

namespace gamemaster
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