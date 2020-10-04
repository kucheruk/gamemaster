using System;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Commands
{
    public class PromoActivateCommand
    {
        private readonly MongoStore _ms;

        public PromoActivateCommand(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<bool> ActivatePromoAsync(string userId, string promoCode)
        {
            var upd = Builders<PromoCode>.Update;
            var s = await _ms.Promo.Find(a => a.Code == promoCode).FirstOrDefaultAsync();
            if (s == null || s.Activated)
            {
                return false;
            }
            await _ms.Promo.UpdateOneAsync(a => a.Id == s.Id,
                upd.Set(a => a.Activated, true)
                    .Set(a => a.ActivatedOn, DateTime.Now)
                    .Set(a => a.ToUserId, userId));
            return true;

        }
    }
}