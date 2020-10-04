using System.Collections.Generic;
using System.Threading.Tasks;
using gamemaster.Config;
using gamemaster.Db;
using gamemaster.Extensions;
using gamemaster.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace gamemaster.Queries.Ledger
{
    public class GetUserBalanceQuery
    {
        private readonly MongoStore _ms;
        private readonly IOptions<AppConfig> _app;

        public GetUserBalanceQuery(MongoStore ms, IOptions<AppConfig> app)
        {
            _ms = ms;
            _app = app;
        }

        public async Task<List<AccountWithAmount>> GetAsync(string period, string userId,
            string currency, bool isAdmin)
        {
            var where = Builders<JournalRecord>.Filter;
            var cond = where.Eq(a => a.Period, period);
            if (!isAdmin)
            {
                cond &= where.Eq(a => a.UserId, userId);
            }

            if (_app.Value.LimitToDefaultCurrency)
            {
                cond &= where.Eq(a => a.Currency, _app.Value.DefaultCurrency);
            }
            else if (!string.IsNullOrEmpty(currency))
            {
                cond &= where.Eq(a => a.Currency, currency);
            }

            var recs = await _ms.Journal.Find(cond).ToListAsync();
            return recs.ToAccountBalances();
        }

    }
}