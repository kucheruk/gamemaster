using System.Collections.Generic;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Extensions;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Queries
{
    public class GetUserBalanceQuery
    {
        private readonly MongoStore _ms;

        public GetUserBalanceQuery(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<List<AccountWithAmount>> GetAsync(string period)
        {
            var recs = await _ms.Journal.Find(a => a.Period == period).ToListAsync();
            return recs.ToAccountBalances();
        }

        public async Task<List<AccountWithAmount>> GetAsync(string period, string userId)
        {
            var recs = await _ms.Journal.Find(a => a.Period == period && a.UserId == userId).ToListAsync();
            return recs.ToAccountBalances();
        }

        public async Task<List<AccountWithAmount>> GetAsync(string period, string userId, string currency)
        {
            var recs = await _ms.Journal.Find(a => a.Period == period && a.UserId == userId && a.Currency == currency)
                .ToListAsync();
            return recs.ToAccountBalances();
        }
    }
}