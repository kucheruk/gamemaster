using System.Collections.Generic;
using System.Threading.Tasks;

namespace gamemaster
{
    public class EmitCurrencyCommand
    {
        private readonly StoreOperationCommand _storeOp;

        public EmitCurrencyCommand(StoreOperationCommand storeOp)
        {
            _storeOp = storeOp;
        }

        public async Task StoreEmissionAsync(string period, string user,
            string reason, AccountWithAmount awa)
        {
            await _storeOp.StoreAsync(period, user, reason,
                new List<AccountWithAmount>
                {
                    new AccountWithAmount(new Account(Constants.CashAccount, awa.Account.Currency), -awa.Amount),
                    awa
                });
        }
    }
}