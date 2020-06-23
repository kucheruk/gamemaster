using System.Collections.Generic;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Extensions;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Queries.Ledger
{
    public class GetPeriodTotalsQuery
    {
        private readonly MongoStore _ms;

        public GetPeriodTotalsQuery(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<List<AccountWithAmount>> GetTotalsAsync(string period)
        {
            var records = await _ms.Journal.Find(a => a.Period == period)
                .ToListAsync();
            var accounts = records.ToAccountBalances();
            return accounts;
        }
    }
}