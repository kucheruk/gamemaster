using Microsoft.Extensions.DependencyInjection;

namespace gamemaster
{
    public static class LedgerExtensions
    {
        public static void AddLedger(this IServiceCollection services)
        {
            services.AddSingleton<StoreOperationCommand>();
            services.AddSingleton<StoreJournalEntryCommand>();

            services.AddSingleton<OpenPeriodCommand>();
            services.AddSingleton<ClosePeriodCommand>();
            services.AddSingleton<MakeNewPeriodCommand>();
            services.AddSingleton<SetCurrentPeriodCommand>();

            services.AddSingleton<TossCurrencyCommand>();
            services.AddSingleton<EmitCurrencyCommand>();

            services.AddSingleton<GetUserBalanceQuery>();
            services.AddSingleton<GetPeriodTotalsQuery>();
            services.AddSingleton<GetCurrentPeriodQuery>();

            services.AddSingleton<CurrentPeriodService>();
            services.AddSingleton<LedgerActor>();
        }
    }
}