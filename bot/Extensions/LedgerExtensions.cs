using System;
using gamemaster.Actors;
using gamemaster.Commands;
using gamemaster.Queries.Ledger;
using gamemaster.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster.Extensions
{
    public static class LedgerExtensions
    {

        public static decimal Trim(this decimal amount)
        {
            return decimal.Round(amount, 2, MidpointRounding.ToZero);
        }
        
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