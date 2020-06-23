using gamemaster.CommandHandlers;
using gamemaster.CommandHandlers.Tote;
using gamemaster.Commands;
using gamemaster.Queries.Tote;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster.Extensions
{
    public static class ToteExtensions
    {
        public static void AddTote(this IServiceCollection services)
        {
            services.AddSingleton<ITextCommandHandler, ToteStartTextCommandHandler>();
            services.AddSingleton<ITextCommandHandler, ToteFinishTextCommandHandler>();
            services.AddSingleton<ITextCommandHandler, NewToteTextCommandHandler>();
            services.AddSingleton<ITextCommandHandler, ToteCancelTextCommandHandler>();
            services.AddSingleton<ITextCommandHandler, ToteReportTextCommandHandler>();
            services.AddSingleton<ITextCommandHandler, ToteHelpTextCommandHandler>();
            services.AddSingleton<ITextCommandHandler, ToteAddOptionTextCommandHandler>();
            services.AddSingleton<ITextCommandHandler, ToteRemoveOptionTextCommandHandler>();
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