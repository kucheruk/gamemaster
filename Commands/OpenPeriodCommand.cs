using System.Collections.Generic;
using System.Threading.Tasks;
using gamemaster.Extensions;
using gamemaster.Models;
using Microsoft.Extensions.Logging;

namespace gamemaster.Commands
{
    public class OpenPeriodCommand
    {
        private readonly ILogger<OpenPeriodCommand> _logger;
        private readonly StoreOperationCommand _store;

        public OpenPeriodCommand(ILogger<OpenPeriodCommand> logger, StoreOperationCommand store)
        {
            _logger = logger;
            _store = store;
        }


        public async Task OpenPeriodAsync(List<AccountWithAmount> oldTotals, string newPeriod)
        {
            if (oldTotals.Count > 0)
            {
                var ops = GenerateOperations(oldTotals, newPeriod);
                var aggregated = ops.AggregateOperations();
                await _store.StoreAsync(newPeriod, Constants.SystemUser, $"Система: Открытие периода {newPeriod}",
                    aggregated);
            }
        }

        private List<AccountWithAmount> GenerateOperations(List<AccountWithAmount> oldTotals, string newPeriod)
        {
            var ops = new List<AccountWithAmount>();
            foreach (var awa in oldTotals)
            {
                if (awa.Account.UserId != Constants.CashAccount)
                {
                    ops.Add(
                        new AccountWithAmount(new Account(Constants.CashAccount, awa.Account.Currency), -awa.Amount));
                    ops.Add(awa);
                }
            }

            return ops;
        }
    }
}