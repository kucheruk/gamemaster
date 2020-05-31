using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace gamemaster
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