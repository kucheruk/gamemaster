using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace gamemaster
{
    public class GetUserBalanceQuery
    {
        private readonly MongoStore _ms;

        public GetUserBalanceQuery(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<List<AccountWithAmount>> GetAsync(string period, string userId)
        {
            var recs = await _ms.Journal.Find(a => a.Period == period && a.UserId == userId).ToListAsync();
            return recs.ToAccountBalances();
        }
    }
}