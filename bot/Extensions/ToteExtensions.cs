using gamemaster.Actors;
using gamemaster.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster.Extensions
{
    public static class ToteExtensions
    {
        public static void AddTote(this IServiceCollection services)
        {
            services.AddSingleton<FinishToteAmountsLogicQuery>();
            services.AddSingleton<ToteReportCountQuery>();
            services.AddSingleton<GetToteReportsQuery>();
            services.AddSingleton<SaveToteReportPointCommand>();
            services.AddSingleton<AddBetToToteCommand>();
            services.AddSingleton<GetCurrentToteForUserQuery>();
            services.AddSingleton<CreateNewToteCommand>();
            services.AddSingleton<FinishToteCommand>();
            services.AddSingleton<StartToteCommand>();
            services.AddSingleton<CancelToteCommand>();
            services.AddSingleton<AddToteOptionCommand>();
            services.AddSingleton<GetToteByIdQuery>();
            services.AddSingleton<RemoveToteOptionCommand>();
        }
    }
}