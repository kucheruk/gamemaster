using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Queries.Tote
{
    public class PromoCodeFindQuery
    {
        private readonly MongoStore _ms;

        public PromoCodeFindQuery(MongoStore ms)
        {
            _ms = ms;
        }
        
        public async Task<PromoCode> FindPromoAsync(string code)
        {
            var res = await _ms.Promo.Find(a => a.Code == code)
                .FirstOrDefaultAsync();
            return res;
        }

    }
}