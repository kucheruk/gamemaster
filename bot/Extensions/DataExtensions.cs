using gamemaster.Db;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster.Extensions
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