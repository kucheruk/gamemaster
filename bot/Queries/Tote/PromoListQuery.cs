using System.Collections.Generic;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Queries.Tote
{
    public class PromoListQuery
    {
        private readonly MongoStore _ms;

        public PromoListQuery(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<List<PromoCode>> ListPromosAsync()
        {
            var res = await _ms.Promo.Find(a => a.Activated == false)
                .ToListAsync();
            return res;
        }
    }
}