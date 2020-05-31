using System.Collections.Generic;
using System.Linq;
using gamemaster.Models;

namespace gamemaster.Extensions
{
    public static class OperationsExtensions
    {
        public static List<AccountWithAmount> ToAccountBalances(this List<JournalRecord> records)
        {
            var accounts = records.GroupBy(a => a.ToAccount())
                .ToDictionary(a => a.Key, a => a.Sum(z => z.Amount))
                .Select(a => new AccountWithAmount(a.Key, a.Value))
                .ToList();
            return accounts;
        }

        public static List<AccountWithAmount> AggregateOperations(this List<AccountWithAmount> ops)
        {
            var newOps = ops.GroupBy(a => a.Account)
                .Select(a => new AccountWithAmount(a.Key, a.Sum(v => v.Amount)))
                .Where(a => a.Amount != 0)
                .ToList();
            return newOps;
        }
    }
}