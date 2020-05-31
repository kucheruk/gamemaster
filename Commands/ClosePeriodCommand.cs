using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace gamemaster
{
    public class ClosePeriodCommand
    {
        private readonly ILogger<ClosePeriodCommand> _logger;
        private readonly StoreOperationCommand _store;

        public ClosePeriodCommand(ILogger<ClosePeriodCommand> logger, StoreOperationCommand store)
        {
            _logger = logger;
            _store = store;
        }

        public async Task ClosePeriodAsync(List<AccountWithAmount> oldTotals, string oldPeriod)
        {
            if (oldTotals.Count > 0)
            {
                var ops = GenerateOperations(oldTotals, oldPeriod);
                var aggregated = ops.AggregateOperations();
                await _store.StoreAsync(oldPeriod, Constants.SystemUser, $"Система: Закрытие периода {oldPeriod}",
                    aggregated);
            }
        }

        private List<AccountWithAmount> GenerateOperations(List<AccountWithAmount> oldTotals, string oldPeriod)
        {
            var ops = new List<AccountWithAmount>();
            foreach (var awa in oldTotals)
            {
                if (awa.Account.UserId != Constants.CashAccount)
                {
                    ops.Add(new AccountWithAmount(new Account(Constants.CashAccount, awa.Account.Currency),
                        awa.Amount));
                    ops.Add(new AccountWithAmount(awa.Account, -awa.Amount));
                }
            }

            return ops;
        }
    }
}