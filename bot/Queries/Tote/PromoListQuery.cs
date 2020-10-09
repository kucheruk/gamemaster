using System.Collections.Generic;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Bson;
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

        public async Task<List<PromoCode>> ListPromosAsync(bool activatedOnly = true)
        {
            var query = activatedOnly
                ? _ms.Promo
                    .Find(a => a.Activated == false)
                : _ms.Promo.Find(new BsonDocument());
            var res = await query
                .ToListAsync();
            return res;
        }
    }
}