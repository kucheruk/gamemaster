using Microsoft.Extensions.DependencyInjection;

namespace gamemaster
{
    public static class DataExtensions
    {
        public static void AddMongoStorage(this IServiceCollection services)
        {
            services.AddSingleton<MongoStore>();
            // services.AddSingleton<DbModelsMapper>();
        }
    }
}