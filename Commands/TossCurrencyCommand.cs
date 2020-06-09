using System.Collections.Generic;
using System.Threading.Tasks;
using gamemaster.Models;

namespace gamemaster.Commands
{
    public class TossCurrencyCommand
    {
        private readonly StoreOperationCommand _store;

        public TossCurrencyCommand(StoreOperationCommand store)
        {
            _store = store;
        }

        public async Task<OperationDescription> TransferAsync(string period, string from,
            string to,
            decimal amount, string currency)
        {
            var fromAcc = new Account(from, currency);
            var toAcc = new Account(to, currency);
            var ops = new List<AccountWithAmount>
            {
                new AccountWithAmount(fromAcc, -amount),
                new AccountWithAmount(toAcc, amount)
            };
            return await _store.StoreAsync(period, from, "Перевод по запросу пользователя", ops);
        }
    }
}