using System;
using System.Threading.Tasks;
using DicewareCore;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Bson;

namespace gamemaster.Commands
{
    public class PromoAddCommand
    {
        private readonly MongoStore _ms;

        public PromoAddCommand(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<PromoCode> AddPromoAsync(string fromUser, decimal amount,
            string currency)
        {
            var promoCode = new PromoCode
            {
                Activated = false, Amount = amount, Code = GenerateCode(), CreatedOn = DateTime.Now, FromUserId = fromUser, Id = ObjectId.GenerateNewId().ToString(), Currency = currency
            };
            await _ms.Promo.InsertOneAsync(promoCode);
            return promoCode;
        }

        private string GenerateCode()
        {
            using var dice = new Diceware();

            var pass = dice.Create(5, language: Language.English, separator: '-');
            return pass;
        }
    }
}